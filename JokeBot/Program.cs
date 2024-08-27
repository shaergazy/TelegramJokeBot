using JokeBot;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Telegram.Bot;
using Telegram.Bot.Types;

internal class Program
{
    static ITelegramBotClient bot = new TelegramBotClient("telegram_api_key");
    private static string userLanguage = "ru"; 
    private static void Main(string[] args)
    {
        bot.StartReceiving(Update, Error);
        Console.ReadLine();
    }

    private static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var message = update.Message;
        if (message == null)
            return;

        if (message.Text == "/start")
        {
            await bot.SendTextMessageAsync(message.Chat.Id, GetWelcomeMessage());
            await SendJokeAsync(message);
        }
        else if (message.Text.ToLower() == "/english")
        {
            userLanguage = "en";
            await bot.SendTextMessageAsync(message.Chat.Id, "You have selected English. Send 'ha' to get a joke.");
        }
        else if (message.Text.ToLower() == "/russian")
        {
            userLanguage = "ru";
            await bot.SendTextMessageAsync(message.Chat.Id, "Вы выбрали русский. Отправьте 'ха', чтобы получить анекдот.");
        }
        else if (message.Text.ToLower().Contains("ha") || message.Text.ToLower().Contains("ха"))
        {
            await SendJokeAsync(message);
        }
    }

    public static async Task SendJokeAsync(Message message)
    {
        string jokeText = await GetJokeAsync(userLanguage);
        await bot.SendTextMessageAsync(message.Chat.Id, jokeText);
    }

    public static async Task<string> GetJokeAsync(string language)
    {
        if (language == "en")
        {
            var joke = await GetJoke<Joke>("https://v2.jokeapi.dev/joke/Dark");
            if (joke.type == "single")
            {
                return joke.joke;
            }
            else
            {
                return $"{joke.setup}\n{joke.delivery}";
            }
        }
        else
        {
            return GetJokeFromDatabase();
        }
    }

    public static async Task<T> GetJoke<T>(string url)
    {
        using (var client = new HttpClient { BaseAddress = new Uri(url) })
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync("").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Error = delegate (object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                {
                    args.ErrorContext.Handled = true;
                }
            });
        }
    }

    private static string GetJokeFromDatabase()
    {
        using (var context = new JokesDbContext())
        {
            var random = new Random();
            int count = context.Jokes.Count();

            if (count == 0)
            {
                return "No jokes available in the database.";
            }

            int randomIndex = random.Next(count);
            var joke = context.Jokes.Skip(randomIndex).Take(1).FirstOrDefault();

            return joke?.Content ?? "No joke found.";
        }
    }

    private static string GetWelcomeMessage()
    {
        return "Привет! Я ваш бот для шуток. Если вам скучно и вы хотите посмеяться, просто введите 'ха' или 'ha', и я расскажу вам шутку.\n\nИнструкция:\n1. Найдите удобное место, потому что шутки настолько смешные, что могут вызвать падение со стула.\n2. Убедитесь, что рядом никого нет, чтобы не напугать людей своим громким смехом.\n3. Введите 'ха' или 'ha'.\n4. Выберите язык шуток: введите /russian для русских шуток или /english для английских шуток.\n5. Приготовьтесь держаться за живот от смеха!\n\nПусть смех начнется! 😂";
    }

    private static async Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        Console.WriteLine(exception.ToString());
    }
}
