using AutoMapper;
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
        private IMapper _mapper;
        private IVATDBContext vatContext;

        public ProductDataContext(IVATDBContext vatContext, IMapper mapper)
        {
            this.connection = vatContext.GetConnection();
            this.vatContext = vatContext;
            _mapper = mapper;
        }
        public async Task<bool> DeleteProduct(int productId)
        {
            using (var connection = vatContext.GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var query = "DELETE FROM Products WHERE ProductId = @ProductId";

                        var parameters = new { ProductId = productId };

                        int rowsAffected = await connection.ExecuteAsync(query, parameters, transaction);

                        transaction.Commit();

                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            var query = "SELECT * FROM Products";

            var products = await connection.QueryAsync<Product>(query);

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductById(int productId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ProductID", productId, DbType.Int32);

            var sql = "SELECT * FROM Products WHERE ProductId=@ProductId";
            var product= await connection.QuerySingleOrDefaultAsync<Product>(sql, parameters);
            
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateUpdateProduct(ProductDto productDto)
        {
            using(connection)
            {
                Product product = _mapper.Map<ProductDto, Product>(productDto);
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var update = "UPDATE Products SET Name=@Name, Price=@Price, Description=@Description, ImageUrl=@ImageUrl, CategoryName=@CategoryName WHERE ProductId=@ProductId";
                        var create = "INSERT INTO Products (Name, CategoryName, Description, ImageUrl, Price) VALUES (@Name, @CategoryName, @Description, @ImageUrl, @Price); SELECT CAST(SCOPE_IDENTITY() as int);";

                        var parameters = new DynamicParameters();
                        parameters.Add("@ProductId", product.ProductId, DbType.Int32);
                        parameters.Add("@Name", product.Name, DbType.String);
                        parameters.Add("@CategoryName", product.CategoryName, DbType.String);
                        parameters.Add("@Description", product.Description, DbType.String);
                        parameters.Add("@ImageUrl", product.ImageUrl, DbType.String);
                        parameters.Add("@Price", product.Price, DbType.Int32);

                        if (product.ProductId > 0)
                        {
                            await connection.ExecuteAsync(update, parameters, transaction);
                        }
                        else
                        {
                            var newProductId = await connection.ExecuteScalarAsync<int>(create, parameters, transaction);
                            product.ProductId = newProductId;
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                    return _mapper.Map<Product, ProductDto>(product);
                }
            }
        }
    }
}
