using ProductModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductInterface
{
    public interface IProductDataContext
    {
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> GetProductById(int productId);
        Task<Product> CreateUpdateProduct(Product product);
        Task<bool> DeleteProduct(int productId);
    }
}
