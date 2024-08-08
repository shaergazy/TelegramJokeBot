using JokeBot;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Telegram.Bot;
using Telegram.Bot.Types;
internal class Program
{
    static ITelegramBotClient bot = new TelegramBotClient("telegram_api_key");
    private static void Main(string[] args)
    {
        bot.StartReceiving(Update, Error);
        Console.ReadLine();
    }

    private static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        string welcomeMessage = "Привет! Я ваш бот для шуток. Если вам скучно и вы хотите посмеяться, просто введите 'ха' или 'ha', и я расскажу вам шутку.\n\nИнструкция:\n1. Найдите удобное место, потому что шутки настолько смешные, что могут вызвать падение со стула.\n2. Убедитесь, что рядом никого нет, чтобы не напугать людей своим громким смехом.\n3. Введите 'ха' или 'ha'.\n4. Приготовьтесь держаться за живот от смеха!\n\nПусть смех начнется! 😂";

        var message = update.Message;
        if (message == null)
            return;
        if (message.Text == "/start")
        {
            await bot.SendTextMessageAsync(message.Chat.Id, welcomeMessage);
            await SendJokeAsync(message);
        }
        else if (message.Text.ToLower().Contains("ha") || message.Text.ToLower().Contains("ха"))
        {
            await SendJokeAsync(message);
        }
    }

    public static async Task SendJokeAsync(Message message)
        {
            var joke = GetJoke<Joke>().Result;
            if (joke.type == "single")
            {
                await bot.SendTextMessageAsync(message.Chat.Id, joke.joke);
                return;
            }
            else
            {
                await bot.SendTextMessageAsync(message.Chat.Id, joke.setup);
                await Task.Delay(3000);
                await bot.SendTextMessageAsync(message.Chat.Id, joke.delivery);
                return;
            }
            
        }
    }

    public static async Task<T> GetJoke<T>()
    {

            using (var client = new HttpClient { BaseAddress = new Uri("https://v2.jokeapi.dev/joke/Dark") })
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client
                    .GetAsync("https://v2.jokeapi.dev/joke/Dark,Programming")
                    .ConfigureAwait(false);

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

    private static async Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}