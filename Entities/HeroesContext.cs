namespace Entities
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class HeroesContext : DbContext
    {
        public HeroesContext()
            : base("name=HeroesContext")
        {
        }

        public virtual DbSet<CombatMode> CombatModes { get; set; }
        public virtual DbSet<Creature> Creatures { get; set; }
        public virtual DbSet<Hero> Heroes { get; set; }
        public virtual DbSet<History> Histories { get; set; }
        public virtual DbSet<Race> Races { get; set; }
        public virtual DbSet<Type> Types { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<History>()
                .HasMany(e => e.Users)
                .WithMany(e => e.Histories)
                .Map(m => m.ToTable("HistoryToUsers").MapLeftKey("HistoryId").MapRightKey("UserId"));

            modelBuilder.Entity<Type>()
                .Property(e => e.Name)
                .IsFixedLength();

            modelBuilder.Entity<Type>()
                .HasMany(e => e.Heroes)
                .WithOptional(e => e.Type1)
                .HasForeignKey(e => e.Type);
        }
    }
}
