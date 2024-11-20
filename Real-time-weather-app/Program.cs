using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public abstract class WeatherServiceBase
{
    protected string ApiKey = "f02c9dfb8ce297d27f9e20c7718f81bc"; // API Key
    protected string BaseUrl = "https://api.openweathermap.org/data/2.5/weather?";

    public abstract Task<WeatherData?> GetWeatherAsync(string city);
}

public class WeatherService : WeatherServiceBase
{
    public override async Task<WeatherData?> GetWeatherAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentNullException(nameof(city), "City name cannot be null or empty.");
        }

        using (var client = new HttpClient())
        {
            var url = $"{BaseUrl}q={city}&appid={ApiKey}&units=metric";
            try
            {
                var response = await client.GetAsync(url);

                // Check if the response is successful
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new CityNotFoundException($"City '{city}' was not found in the weather database. Please check the spelling and try again.");
                    }

                    throw new HttpRequestException($"Error fetching weather data: {response.StatusCode} - {response.ReasonPhrase}");
                }

                // Read and deserialize the response
                var responseData = await response.Content.ReadAsStringAsync();
                var weatherResponse = JsonConvert.DeserializeObject<WeatherApiResponse>(responseData);

                if (weatherResponse == null || weatherResponse.Cod != 200)
                {
                    throw new CityNotFoundException($"City '{city}' was not found in the weather database.");
                }

                return new WeatherData
                {
                    Main = weatherResponse.Main,
                    Wind = weatherResponse.Wind,
                    Name = weatherResponse.Name
                };
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

public class WeatherApiResponse
{
    [JsonProperty("cod")]
    public int Cod { get; set; }
    [JsonProperty("message")]
    public string Message { get; set; }
    [JsonProperty("main")]
    public Main Main { get; set; }
    [JsonProperty("wind")]
    public Wind Wind { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
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

        if (string.IsNullOrWhiteSpace(city))
        {
            Console.WriteLine("City name cannot be empty. Please try again.");
            return;
        }

        WeatherServiceBase weatherService = new WeatherService();

        try
        {
            var weatherData = await weatherService.GetWeatherAsync(city);

            if (weatherData == null)
            {
                Console.WriteLine($"No data for '{city}' found in the database.");
                return;
            }

            DisplayWeather(weatherData);
        }
        catch (CityNotFoundException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error fetching data: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
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
