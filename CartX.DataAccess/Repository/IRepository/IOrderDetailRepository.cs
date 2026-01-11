using CartX.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CartX.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository : IRepository<OrderDetail>
    {
        void Update(OrderDetail obj);
    }
}
