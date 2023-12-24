using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Models;
using WebApp.Models;

namespace WebApp.Db;

public class AppDbContext : IdentityDbContext
{
    private readonly IConfiguration _configuration;

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
    {
        this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Database.EnsureCreated();
    }

    public DbSet<AppUser>? AppUsers { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_configuration.GetConnectionString("SqliteConnection"));
            optionsBuilder
                .UseLoggerFactory(
                        LoggerFactory
                            .Create(builder =>
                                builder.AddConsole().SetMinimumLevel(LogLevel.Error))
                    );
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // rename MS Identity tables
        builder.Entity<IdentityUser>().ToTable("User");
        builder.Entity<IdentityRole>().ToTable("Role");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRole");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserToken");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("UserRoleClaim");


        // seed default Roles
        var adminRoleId = Guid.NewGuid().ToString().ToLower();
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole(AppBaseConstants.AdminRole)
            {
                Id = adminRoleId,
                NormalizedName = AppBaseConstants.AdminRole.ToUpper()
            },
            new IdentityRole(AppBaseConstants.UserRole)
            {
                NormalizedName = AppBaseConstants.UserRole.ToUpper()
            }
        );


        // seed default user
        var adminUserName = "admin@webapp.io";
        var adminUserId = Guid.NewGuid().ToString().ToLower();
        var hasher = new PasswordHasher<IdentityUser>();
        var adminUser = new AppUser()
        {
            Id = adminUserId,
            UserName = adminUserName,
            NormalizedUserName = adminUserName.ToUpper(),
            Email = adminUserName,
            NormalizedEmail = adminUserName.ToUpper(),
            EmailConfirmed = true,
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "123");

        builder.Entity<AppUser>().HasData(adminUser);


        // seed defaul Admin Role for default user
        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = adminRoleId,
            UserId = adminUserId
        });
    }
}