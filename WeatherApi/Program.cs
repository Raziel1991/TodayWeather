using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;


namespace WeatherApi
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Get API key from Class
            string fileKeyPath = "weatherKey.txt";
            GetApiKey getApiKey = new GetApiKey(fileKeyPath);
            string apiKey = getApiKey.ReadAPIKey();
            

            string cityName = "Toronto";
            int limit = 5;
            string apiUrlLatLon = $"http://api.openweathermap.org/geo/1.0/direct?q={cityName}&limit={limit}&appid={apiKey}";
            string apiUrlWeather25;
            string apiUrlWeather30;
            CityLocation selectedCity;            
            string responseBodyLocation;
            string responseBodyWeather;
            string responseBodyWeather30;


            // Deserialize Location Latitude Longitud JSON array 
            responseBodyLocation = await GetWeatherApiResponse(apiUrlLatLon);
            List<CityLocation> locations = JsonSerializer.Deserialize<List<CityLocation>>(responseBodyLocation);

            //Set required city// in future change the city dynamically by location or multiple menu selection
            selectedCity = locations[1];

            apiUrlWeather30 = $"https://api.openweathermap.org/data/3.0/onecall?lat={selectedCity.lat}&lon={selectedCity.lon}&exclude=minutely,daily&appid={apiKey}&units=metric";
            apiUrlWeather25 = $"https://api.openweathermap.org/data/2.5/weather?lat={selectedCity.lat}&lon={selectedCity.lon}&appid={apiKey}&units=metric";


            //Deserialize city location Weather 2.5
            responseBodyWeather = await GetWeatherApiResponse(apiUrlWeather25);
            CityWeather cityWeather = JsonSerializer.Deserialize<CityWeather>(responseBodyWeather);

            ////Deserialize city location Weather 3.0
            responseBodyWeather30 = await GetWeatherApiResponse(apiUrlWeather30);
            CityWeatherHourly cityWeather30 = JsonSerializer.Deserialize<CityWeatherHourly>(responseBodyWeather30);

            cityWeather.cityLocation = selectedCity;


            // Print weather details 2.5
            Console.WriteLine($"Location: {cityWeather.name}");
            Console.WriteLine($"Current weather: {cityWeather.main.temp}");
            Console.WriteLine($"Feels like: {cityWeather.main.feels_like}");
            Console.WriteLine();

            // Print weather details 3.0
            Console.WriteLine($"Weather details 3.0");
            Console.WriteLine($"Latitude: {cityWeather30.lat}");
            Console.WriteLine($"Longitude: {cityWeather30.lon}");
            Console.WriteLine($"Wind Speed: {cityWeather30.current.wind_speed}");
            Console.WriteLine($"Date and Time: {cityWeather30.GetRealTimeDate(cityWeather30.timezone_offset, cityWeather30.current.dt)}");
            Console.WriteLine();


            //cityWeather30.GetWeatherHourlyDetails(cityWeather30, 50);
            cityWeather30.GetSelectedHourWeather(23);



            Console.ReadLine();
        }

        public static async Task<string> GetWeatherApiResponse(string apiUrl)
        {
            string responseBody = ""; // Declared once outside the try block
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync(); // Assigned without redeclaration
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"General Error: {e}");
                }
            }
            return responseBody;
        }
    }
}


// Convert from the fetch Json to c# object 
class CityLocation
{
    public string name { get; set; }
    public string country { get; set; }
    public string state { get; set; }
    public double lat { get; set; }
    public double lon { get; set; }

    public CityLocation(string name, string country, string state, double lat, double lon)
    {
        this.name = name;
        this.country = country;
        this.state = state;
        this.lat = lat;
        this.lon = lon;
    }
}


// Classes for the 2.5 api call 
class CityWeather
{
    public string name { get; set; }
    public int timezone { get; set; }

    public WeatherMain main { get; set; }
    public CityLocation cityLocation { get; set; }
    public CityWeather(string name, int timezone, WeatherMain main)
    {
        this.name = name;
        this.timezone = timezone;
        this.main = main;
        //Console.WriteLine($"City of {name} weather created with current tempeture of {this.main.temp}");
    }
}
class WeatherMain
{
    public double temp { get; set; }
    public double feels_like { get; set; }
    public double temp_min { get; set; }
    public double temp_max { get; set; }
    public int pressure { get; set; }
    public int humidity { get; set; }

    public WeatherMain(double temp, double feels_like, double temp_min, double temp_max, int pressure, int humidity)
    {
        this.temp = temp;
        this.feels_like = feels_like;
        this.temp_min = temp_min;
        this.temp_max = temp_max;
        this.pressure = pressure;
        this.humidity = humidity;
    }
}


// classes for 3.0 json format 
class CityWeatherHourly
{
    public double lon { get; set; }
    public double lat { get; set; }
    public long timezone_offset { get; set; }
    public CityWeatherCurrent current { get; set; }
    public List<CityWeatherCurrent> hourly { get; set; }
    
    public CityWeatherHourly(double lon, double lat, CityWeatherCurrent current, List<CityWeatherCurrent> hourly)
    {
        this.lon = lon;
        this.lat = lat;
        this.current = current;
        this.hourly = hourly;
    }
    public DateTime GetRealTimeDate(long timezone_offset, long dt)
    {
        return DateTimeOffset.FromUnixTimeSeconds(dt + timezone_offset).DateTime;
    }

    public void GetWeatherHourlyDetails(CityWeatherHourly cityWeather30, int selectedHours)
    {
        for (int i = 0; i < selectedHours; i++)
        {
            Console.WriteLine($"NEW {i + 1}");
            Console.WriteLine($"Date and Time: {cityWeather30.GetRealTimeDate(cityWeather30.timezone_offset, cityWeather30.hourly[i].dt)}");
            Console.WriteLine($"Temperature: {cityWeather30.hourly[i].temp}");
            Console.WriteLine($"Feels like: {cityWeather30.hourly[i].feels_like}");
            Console.WriteLine($"Humidity: {cityWeather30.hourly[i].humidity}");
            Console.WriteLine($"Wind Speed: {cityWeather30.hourly[i].wind_speed}");
            Console.WriteLine();
        }
    }

    public void GetSelectedHourWeather(int selectedHour)
    {
        foreach (var item in hourly)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(item.dt + timezone_offset);
            DateTime dateTime = dateTimeOffset.DateTime;

            if (dateTime.Hour == selectedHour)
            {
                Console.WriteLine($"Weather details for {dateTime}:");
                Console.WriteLine($"Temperature: {item.temp}");
                Console.WriteLine($"Feels like: {item.feels_like}");
                Console.WriteLine($"Humidity: {item.humidity}");
                Console.WriteLine($"Wind Speed: {item.wind_speed}");
                Console.WriteLine();
                break; 
            }
        }
    }
}

class CityWeatherCurrent
{
    public long dt { get; set; }
    public double temp { get; set; }
    public double feels_like { get; set; }
    public int humidity { get; set; }
    public double wind_speed { get; set; }

    public CityWeatherCurrent(long dt, double temp, double feels_like, int humidity, double wind_speed)
    {
        this.dt = dt;
        this.temp = temp;
        this.feels_like = feels_like;
        this.humidity = humidity;
        this.wind_speed = wind_speed;

    }
}

