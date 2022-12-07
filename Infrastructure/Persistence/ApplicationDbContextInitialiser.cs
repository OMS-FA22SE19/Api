using Core.Entities;
using Core.Interfaces;
using Infrastructure.Common;
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
        private readonly IUnitOfWork _unitOfWork;

        public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
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
            var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost", FullName = "Administrator" };

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
            var customer = new ApplicationUser { UserName = "defaultCustomer", Email = "longpnhse150499@fpt.edu.vn", PhoneNumber = "0939758999", FullName = "Default Customer" };

            if (_userManager.Users.All(u => u.UserName != customer.UserName))
            {
                await _userManager.CreateAsync(customer, "abc123");
                await _userManager.AddToRolesAsync(customer, new[] { customerRole.Name });
            }

            var restaurantOwnerRole = new IdentityRole("Restaurant Owner");
            if (_roleManager.Roles.All(r => r.Name != restaurantOwnerRole.Name))
            {
                await _roleManager.CreateAsync(restaurantOwnerRole);
            }

            var StaffRole = new IdentityRole("Staff");
            if (_roleManager.Roles.All(r => r.Name != StaffRole.Name))
            {
                await _roleManager.CreateAsync(StaffRole);
            }

            var ChefRole = new IdentityRole("Chef");
            if (_roleManager.Roles.All(r => r.Name != ChefRole.Name))
            {
                await _roleManager.CreateAsync(ChefRole);
            }

            var topic = new Topic { Name = "Staff" };
            var topicResult = await _unitOfWork.TopicRepository.GetAsync(r => r.Name.Equals(topic.Name));
            if (topicResult is null)
            {
                await _unitOfWork.TopicRepository.InsertAsync(topic);
                await _unitOfWork.CompleteAsync(default);
            }
        }
    }
}
