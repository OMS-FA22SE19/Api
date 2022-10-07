using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence
{
    public sealed class ApplicationDbContextInitialiser
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            //// Default roles
            var administratorRole = new ApplicationRole("Administrator");

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await _roleManager.CreateAsync(administratorRole);
            }

            // Default users
            var adminRole = await _roleManager.FindByNameAsync("Administrator");
            var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost", FullName = "Administrator", RoleId = adminRole.Id };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Administrator1!");
            }

            //// Default data

            var customerRole = new ApplicationRole("Customer");
            if (_roleManager.Roles.All(r => r.Name != customerRole.Name))
            {
                await _roleManager.CreateAsync(customerRole);
            }

            // Default users
            var cusRole = await _roleManager.FindByNameAsync("Customer");
            var customer = new ApplicationUser { UserName = "defaultCustomer", Email = "longpnhse150499@fpt.edu.vn", PhoneNumber = "0939758999", FullName = "Default Customer", RoleId = cusRole.Id };

            if (_userManager.Users.All(u => u.UserName != customer.UserName))
            {
                await _userManager.CreateAsync(customer, "abc123");
            }
        }
    }
}
