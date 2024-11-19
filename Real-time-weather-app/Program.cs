using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public abstract class WeatherServiceBase
{
    protected string ApiKey = "f02c9dfb8ce297d27f9e20c7718f81bc"; // API Key
    protected string BaseUrl = "https://api.openweathermap.org/data/2.5/weather?";

    public abstract Task<WeatherData> GetWeatherAsync(string city);
}

public class WeatherService : WeatherServiceBase
{
    public override async Task<WeatherData> GetWeatherAsync(string city)
    {
        using (var client = new HttpClient())
        {
            var url = $"{BaseUrl}q={city}&appid={ApiKey}&units=metric";
            try
            {
                var response = await client.GetStringAsync(url);
                var weatherResponse = JsonConvert.DeserializeObject<WeatherApiResponse>(response);
                return weatherResponse.WeatherData;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
                throw;
            }
        }
    }
}

public class WeatherData
{
    public required Main Main { get; set; }
    public required Wind Wind { get; set; }
    public required string Name { get; set; }
}

public class Main
{
    public float Temp { get; set; }
    public float Humidity { get; set; }
}

public class Wind
{
    public float Speed { get; set; }
}

public class WeatherApiResponse
{
    [JsonProperty("cod")]
    public required int Cod { get; set; }
    [JsonProperty("message")]
    public required string Message { get; set; }
    [JsonProperty("main")]
    public required Main Main { get; set; }
    [JsonProperty("wind")]
    public required Wind Wind { get; set; }
    [JsonProperty("name")]
    public required string Name { get; set; }
    [JsonProperty("weather")]
    public required WeatherData WeatherData { get; set; }
}

public class CityNotFoundException : Exception
{
    public CityNotFoundException(string message) : base(message) { }
}

public class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter city name: ");
        var city = Console.ReadLine();

        WeatherServiceBase weatherService = new WeatherService();

        try
        {
            var weatherData = await weatherService.GetWeatherAsync(city);

            if (weatherData == null)
            {
                Console.WriteLine($"No data of {city} found in database.");
                return;
            }

            DisplayWeather(weatherData);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching data: {ex.Message}");
            throw;
        }
    }

    private static void DisplayWeather(WeatherData weatherData)
    {
        Console.WriteLine($"Weather in {weatherData.Name}:");
        Console.WriteLine($"Temperature: {weatherData.Main.Temp}°C");
        Console.WriteLine($"Humidity: {weatherData.Main.Humidity}%");
        Console.WriteLine($"Wind Speed: {weatherData.Wind.Speed} m/s");
    }
}
