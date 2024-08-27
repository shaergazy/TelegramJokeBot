using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

class Program
{
    static async Task Main(string[] args)
    {
        var dbContext = new JokeDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        for (int i = 92; i > 0; i--)
        {
            var url = $"http://anecdotica.ru/categories/4/{i}";
            var jokes = await GetJokesFromUrl(url);

            foreach (var joke in jokes)
            {
                dbContext.Jokes.Add(new Joke { Content = joke });
            }

            Console.WriteLine("Jokes have been saved to the database.");
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task<string[]> GetJokesFromUrl(string url)
    {
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(url);
        var jokeNodes = doc.DocumentNode.SelectNodes("//div[@class='item_text']");

        return jokeNodes?.Select(node => node.InnerText.Trim()).ToArray() ?? new string[0];
    }
}

public class Joke
{
    public int Id { get; set; }
    public string Content { get; set; }
}

public class JokeDbContext : DbContext
{
    public DbSet<Joke> Jokes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=.; Database=JokeDb;Integrated Security=false;User ID=sa;Password=Vrysmplpswd1!;TrustServerCertificate=True");
    }
}