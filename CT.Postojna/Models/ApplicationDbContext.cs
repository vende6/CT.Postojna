using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CT.Postojna.Models;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.Id).HasMaxLength(63);
            entity.Property(user => user.NormalizedUserName).HasMaxLength(191);
            entity.Property(user => user.NormalizedEmail).HasMaxLength(191);
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.Property(role => role.Id).HasMaxLength(63);
            entity.Property(role => role.NormalizedName).HasMaxLength(191);
        });

        builder.Entity<IdentityUserClaim<string>>()
            .Property(claim => claim.UserId).HasMaxLength(63);
        builder.Entity<IdentityRoleClaim<string>>()
            .Property(claim => claim.RoleId).HasMaxLength(63);

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.Property(login => login.LoginProvider).HasMaxLength(63);
            entity.Property(login => login.ProviderKey).HasMaxLength(63);
            entity.Property(login => login.UserId).HasMaxLength(63);
        });

        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.Property(userRole => userRole.UserId).HasMaxLength(63);
            entity.Property(userRole => userRole.RoleId).HasMaxLength(63);
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.Property(token => token.UserId).HasMaxLength(63);
            entity.Property(token => token.LoginProvider).HasMaxLength(63);
            entity.Property(token => token.Name).HasMaxLength(63);
        });
    }
}
