﻿using Core.Common.Interfaces;
using Core.Entities;
using Duende.IdentityServer.EntityFramework.Options;
using Infrastructure.Common;
using Infrastructure.Persistence.Interceptors;
using MediatR;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Infrastructure.Persistence
{
    public sealed class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>, IApplicationDbContext, IDisposable
    {
        private readonly IMediator _mediator;
        private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions,
            IMediator mediator,
            AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
            : base(options, operationalStoreOptions)
        {
            _mediator = mediator;
            _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        }

        public DbSet<AdminSetting> AdminSettings => Set<AdminSetting>();
        public DbSet<Topic> Topics => Set<Topic>();
        public DbSet<UserTopic> UserTopics => Set<UserTopic>();
        public DbSet<UserDeviceToken> UserDeviceTokens => Set<UserDeviceToken>();
        public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
        public DbSet<Core.Entities.Type> Types => Set<Core.Entities.Type>();
        public DbSet<Food> Foods => Set<Food>();
        public DbSet<FoodType> FoodTypes => Set<FoodType>();
        public DbSet<CourseType> CourseTypes => Set<CourseType>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<MenuFood> MenuFoods => Set<MenuFood>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<Billing> Billings => Set<Billing>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<ReservationTable> ReservationTables => Set<ReservationTable>();
        public DbSet<Table> Tables => Set<Table>();
        public DbSet<TableType> TableTypes => Set<TableType>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.Entity<ApplicationUser>(e =>
            {
                e.HasIndex(x => x.PhoneNumber)
                .IsUnique();
            });
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _mediator.DispatchDomainEvents(this);

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
