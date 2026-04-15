using System.Net.Http;
using System.Text.Json;
using System.Collections.Concurrent;

namespace AppMeteoMAUI.Logic
{
    public static class WeatherCache
    {
        private static readonly ConcurrentDictionary<string, (DateTimeOffset, string)> _cache = new();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public static bool TryGet(string key, out string? weatherJson)
        {
            weatherJson = null;
            if (_cache.TryGetValue(key, out var entry))
            {
                if (DateTimeOffset.UtcNow - entry.Item1 < CacheDuration)
                {
                    weatherJson = entry.Item2;
                    return true;
                }
                else
                {
                    _cache.TryRemove(key, out _);
                }
            }
            return false;
        }

        public static void Set(string key, string weatherJson)
        {
            _cache[key] = (DateTimeOffset.UtcNow, weatherJson);
        }
    }

    public class MeteoService
    {
        private readonly HttpClient _http = new();

        public async Task<(double? Temp, double? Wind, string? Time, string? Errore)> GetWeatherAsync(string city)
        {
            try
            {
                string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1&language=it&format=json";
                var geoRespMsg = await _http.GetAsync(geoUrl);
                if (!geoRespMsg.IsSuccessStatusCode)
                    return (null, null, null, $"Errore HTTP {geoRespMsg.StatusCode}");
                var geoResp = await geoRespMsg.Content.ReadAsStringAsync();
                using var geoDoc = JsonDocument.Parse(geoResp);
                var geoRoot = geoDoc.RootElement;
                if (!geoRoot.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                    return (null, null, null, "Città non trovata");
                var loc = results[0];
                double lat = loc.GetProperty("latitude").GetDouble();
                double lon = loc.GetProperty("longitude").GetDouble();
                string cacheKey = $"{lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                string weatherResp;
                if (!WeatherCache.TryGet(cacheKey, out weatherResp))
                {
                    string weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&current_weather=true&timezone=auto";
                    var weatherRespMsg = await _http.GetAsync(weatherUrl);
                    if (!weatherRespMsg.IsSuccessStatusCode)
                        return (null, null, null, $"Errore HTTP {weatherRespMsg.StatusCode}");
                    weatherResp = await weatherRespMsg.Content.ReadAsStringAsync();
                    WeatherCache.Set(cacheKey, weatherResp);
                }
                using var weatherDoc = JsonDocument.Parse(weatherResp);
                var weatherRoot = weatherDoc.RootElement;
                var current = weatherRoot.GetProperty("current_weather");
                double temp = current.GetProperty("temperature").GetDouble();
                double wind = current.GetProperty("windspeed").GetDouble();
                string time = current.GetProperty("time").GetString() ?? "-";
                return (temp, wind, time, null);
            }
            catch (Exception ex)
            {
                return (null, null, null, $"Errore: {ex.Message}");
            }
        }
    }
}
