using System.Data.Entity;

namespace Entities
{

    public partial class HeroesContext : DbContext
    {
        public HeroesContext()
            : base("name=HeroesContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
