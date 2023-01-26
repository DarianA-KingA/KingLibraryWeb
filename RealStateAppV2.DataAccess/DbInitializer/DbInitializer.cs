using KingLibraryWeb.DataAccess.Data;
using KingLibraryWeb.DataAccess.Repository.IRepository;
using KingLibraryWeb.Models;
using KingLibraryWeb.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingLibraryWeb.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationContext _db;
        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void Initialize()
        {
            //migrations if they are not applied
            try 
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch { }
            //create role if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();                
            }
            if (!_roleManager.RoleExistsAsync(SD.Role_User_Comp).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();
            }
            if (!_roleManager.RoleExistsAsync(SD.Role_Employee).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            }
            if (!_roleManager.RoleExistsAsync(SD.Role_User_Indi).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();
            }
            //if admin user is not created
            var user = _userManager.FindByEmailAsync("admin@dotnet.com").GetAwaiter().GetResult();
            if (user == null) 
            {
                _userManager.CreateAsync(new ApplicationUser 
                { 
                    UserName = "admin@dotnet.com",
                    Email= "admin@dotnet.com",
                    Name = "John Doe",
                    PhoneNumber = "1234567890",
                    StreetAddress = "123 Ave",
                    State="LA",
                    PostalCode = "90020",
                    City="Miami"
                },"Admin123*").GetAwaiter().GetResult();
                var createdUser = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@dotnet.com");
                _userManager.AddToRoleAsync(createdUser, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;

        }
    }
}
