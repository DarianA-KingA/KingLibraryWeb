
using KingLibraryWeb.DataAccess.Data;
using KingLibraryWeb.DataAccess.Repository.IRepository;
using KingLibraryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingLibraryWeb.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationContext _db;
        public OrderDetailRepository(ApplicationContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
