
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
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private ApplicationContext _db;
        public CompanyRepository(ApplicationContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Company obj)
        {
            _db.Companies.Update(obj);
        }
    }
}
