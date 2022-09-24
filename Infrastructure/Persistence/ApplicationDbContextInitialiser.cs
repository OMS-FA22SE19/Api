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
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
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
            var administratorRole = new IdentityRole("Administrator");

            if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await _roleManager.CreateAsync(administratorRole);
            }

            // Default users
            var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Administrator1!");
                await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
            }

            //// Default data

            var customerRole = new IdentityRole("Customer");
            if (_roleManager.Roles.All(r => r.Name != customerRole.Name))
            {
                await _roleManager.CreateAsync(customerRole);
            }

            // Default users
            var customer = new ApplicationUser { UserName = "defaultCustomer", Email = "longpnhse150499@fpt.edu.vn", PhoneNumber = "0939758999" };

            if (_userManager.Users.All(u => u.UserName != customer.UserName))
            {
                await _userManager.CreateAsync(customer, "abc123");
                await _userManager.AddToRolesAsync(customer, new[] { customerRole.Name });
            }
        }
    }
}
