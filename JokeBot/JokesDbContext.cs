using Microsoft.EntityFrameworkCore;

namespace JokeBot
{
    public class JokesDbContext : DbContext
    {
        public DbSet<JokeModel> Jokes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.; Database=JokeDb;Integrated Security=false;User ID=sa;Password=Vrysmplpswd1!;TrustServerCertificate=True");
        }
    }
}
