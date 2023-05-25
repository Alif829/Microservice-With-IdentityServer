using ConnectionGateway;
using Dapper;
using Microsoft.Extensions.Configuration;
using ProductInterface;
using ProductModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductDataContext
{
    public class ProductDataContext:IProductDataContext
    {
        private  IDbConnection connection;
        private readonly IVATDBContext vatContext;

        public ProductDataContext(IConfiguration configuration)
        {
            this.connection = vatContext.GetConnection();
        }

        public async Task<Product> CreateUpdateProduct(Product product)
        {
            var update = "UPDATE Products SET Name=@Name,Price=@Price," +
                      "Description=@Description,ImageUrl=@ImageUrl,CategoryName=@CategoryName WHERE ProductID=@ProductID";

            var create = "INSERT INTO Products (Name,CategoryName,Description,ImageUrl,Price) " +
                "                        VALUES(@Name,@CategoryName,@Description,@ImageUrl,@Price);" +
                                        "SELECT CAST(SCOPE_IDENTITY() as int); ";

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyID", product.ProductId, DbType.Int32);
            parameters.Add("@Name", product.Name, DbType.String);
            parameters.Add("@CategoryName", product.CategoryName, DbType.String);
            parameters.Add("@Description", product.Description, DbType.String);
            parameters.Add("@ImageUrl", product.ImageUrl, DbType.String);
            parameters.Add("@Price", product.Price, DbType.Int32);


            if (product.ProductId>0)
            {
                await connection.ExecuteAsync(update, parameters);
            }
            else
            {
                await connection.ExecuteAsync(create, parameters);
            }
            return product;
        }

        public async Task<bool> DeleteProduct(int productId)
        {
                var query = "DELETE FROM Products WHERE ProductId = @ProductId"; 

                var parameters = new { ProductId = productId };

                int rowsAffected = await connection.ExecuteAsync(query, parameters);

                return rowsAffected > 0;
        }


        Task<IEnumerable<Product>> IProductDataContext.GetProducts()
        {
                var query = "SELECT * FROM Products";

                var products = connection.QueryAsync<Product>(query);

                return products;
        }

        Task<Product> IProductDataContext.GetProductById(int productId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ProductID", productId, DbType.Int32);

            var sql = "SELECT * FROM Products WHERE ProductID=@ProductID";

            return connection.QuerySingleOrDefaultAsync<Product>(sql, parameters);
        }
    }
}
