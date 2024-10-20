using E_commerce.DataAccess.Repository.IRepository;
using E_Commerce.Models;
using Ecommerce.DataAccess.Data;


namespace E_commerce.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationContext context;

        public ProductRepository(ApplicationContext context):base(context) 
        {
            this.context = context;
        }
        public void update(Product obj)
        {
           Product? product = context.Products.FirstOrDefault(e=>e.Id == obj.Id);
            if (product != null)
            {
                product.Title = obj.Title;
                product.ISBN = obj.ISBN;
                product.Author = obj.Author;
                product.Description = obj.Description;
                product.Price = obj.Price;
                product.CategoryId = obj.CategoryId;
                product.ListPrice = obj.ListPrice;
                product.Price = obj.Price;
                product.Price100 = obj.Price100;
                product.Price50 = obj.Price50;
                if (obj.ImageUrl != null)
                {
                    product.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
