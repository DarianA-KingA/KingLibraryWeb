
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
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private ApplicationContext _db;
        public CoverTypeRepository(ApplicationContext db) : base(db)
        {
            _db = db;
        }
        public void Update(CoverType obj)
        {
            _db.CoverType.Update(obj);
        }
    }
}
