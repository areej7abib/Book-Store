using E_commerce.DataAccess.Repository.IRepository;
using Ecommerce.DataAccess.Data;

namespace E_commerce.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext context;
        public ICategoryRepository Category {  get; private set; }
        public IApplicationRepository Application {  get; private set; }
        public IOrderHeaderRepository OrderHeader {  get; private set; }
        public IOrderDetailRepository OrderDetail {  get; private set; }
        public ICompanyRepository Company {  get; private set; }
        public IShoppingCartRepository ShoppingCart {  get; private set; }

        public IProductRepository Product { get; private set; }

        public UnitOfWork(ApplicationContext context)
        {
            this.context = context;
            Category = new CategryRepository(context);
            Product = new ProductRepository(context);
            ShoppingCart = new ShoppingCartRepository(context);
            Company = new CompanyRepository(context);
            OrderDetail = new OrderDetailRepository(context);
            OrderHeader = new OrderHeaderRepository(context);
            Application = new ApplicationRepository(context);
        }

        public void Save()
        {
           context.SaveChanges();
        }
    }
}
