using CartX.DataAccess.Data;
using CartX.DataAccess.Repository.IRepository;
using CartX.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CartX.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationUser applicationUser)
        {
            _db.ApplicationUsers.Update(applicationUser);
        }
    }
}
