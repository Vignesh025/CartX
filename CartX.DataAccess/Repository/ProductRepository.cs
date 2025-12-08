using CartX.DataAccess.Data;
using CartX.DataAccess.Repository.IRepository;
using CartX.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CartX.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {
            _db.Products.Update(obj);
        }
    }
}
