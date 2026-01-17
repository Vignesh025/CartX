using CartX.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CartX.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
        public void Update(ApplicationUser applicationUser);
    }
}
