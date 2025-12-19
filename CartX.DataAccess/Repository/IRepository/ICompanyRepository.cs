using CartX.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CartX.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company obj);
    }
}
