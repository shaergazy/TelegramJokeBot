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
        var message = update.Message;
        if(message.Text.ToLower().Contains("ha") || message.Text.ToLower().Contains("ха") || message.Text == "/start")
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
                    .GetAsync("https://v2.jokeapi.dev/joke/Dark")
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