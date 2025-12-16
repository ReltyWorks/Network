using Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth
{
    /// <summary>
    /// DB 에 쿼리하기위한 흐름을 가지고있음 (DB Facade 등)
    /// </summary>
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// DB 에서 User 엔티티 쿼리 루트
        /// </summary>
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // GameDbContext 타입이 정의된 어셈블리에서 IEntityTypeConfiguration 전부 찾아서 적용
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDbContext).Assembly);
        }
    }
}
