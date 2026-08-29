using Microsoft.EntityFrameworkCore ;
using coFind.Domain.Entities ;

namespace coFind.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users {get; set;}
        public DbSet<Offer> Offers {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Offer>()
            .HasOne(o => o.Owner)
            .WithMany(u => u.Offers)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}