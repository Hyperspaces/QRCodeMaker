using Microsoft.EntityFrameworkCore;

namespace QRCodeMaker.Dao
{
    class HLContext : DbContext
    {
        public HLContext()
        {
        }

        public HLContext(DbContextOptions<HLContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=MLXXB193;Initial Catalog=Local;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HomeLetter>().ToTable("HomeLetter");
        }

        public DbSet<HomeLetter> HomeLetter { get; set; }
    }
}
