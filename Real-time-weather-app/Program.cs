using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class WeatherData
{
    public Main Main { get; set; }
    public Wind Wind { get; set; }
    public string Name { get; set; }
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

public class WeatherService
{
    private readonly string _apiKey = "4c38819a4e74211b5e2edb9aac6216e6"; // Your OpenWeather API key
    private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5/weather?";

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        using (var client = new HttpClient())
        {
            var url = $"{_baseUrl}q={city}&appid={_apiKey}&units=metric";
            var response = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<WeatherData>(response);
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter city name: ");
        var city = Console.ReadLine();

        var weatherService = new WeatherService();
        var weatherData = await weatherService.GetWeatherAsync(city);

        Console.WriteLine($"Weather in {weatherData.Name}:");
        Console.WriteLine($"Temperature: {weatherData.Main.Temp}°C");
        Console.WriteLine($"Humidity: {weatherData.Main.Humidity}%");
        Console.WriteLine($"Wind Speed: {weatherData.Wind.Speed} m/s");
    }
}
