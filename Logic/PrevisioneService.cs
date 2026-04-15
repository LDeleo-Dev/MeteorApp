using System.Net.Http;
using System.Text.Json;

namespace AppMeteoMAUI.Logic
{
    public class PrevisioneGiorno
    {
        public string Giorno { get; set; } = "-";
        public double Min { get; set; }
        public double Max { get; set; }
        public double Pioggia { get; set; }
        public string IconaMeteo
        {
            get
            {
                if (Max < 5) return "weather_cold.png";
                if (Max < 18) return "weather_cloudy.png";
                if (Max < 28) return "weather_sun.png";
                return "weather_hot.png";
            }
        }
    }

    public class PrevisioneService
    {
        private readonly HttpClient _http = new();

        public async Task<List<PrevisioneGiorno>?> GetPrevisione5Giorni(double lat, double lon)
        {
            string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&longitude={lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}&daily=temperature_2m_max,temperature_2m_min,precipitation_sum&forecast_days=5&timezone=auto";
            try
            {
                var respMsg = await _http.GetAsync(url);
                if (!respMsg.IsSuccessStatusCode)
                    return null;
                var resp = await respMsg.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(resp);
                var root = doc.RootElement;
                if (!root.TryGetProperty("daily", out var daily))
                    return null;
                var dates = daily.GetProperty("time").EnumerateArray();
                var tmax = daily.GetProperty("temperature_2m_max").EnumerateArray();
                var tmin = daily.GetProperty("temperature_2m_min").EnumerateArray();
                var prec = daily.GetProperty("precipitation_sum").EnumerateArray();
                var culture = new System.Globalization.CultureInfo("it-IT");
                var lista = new List<PrevisioneGiorno>();
                while (dates.MoveNext() && tmax.MoveNext() && tmin.MoveNext() && prec.MoveNext())
                {
                    string dataIso = dates.Current.GetString() ?? "-";
                    string giornoFormattato = "-";
                    if (DateTime.TryParse(dataIso, out var dt))
                    {
                        string nomeGiorno = culture.DateTimeFormat.GetDayName(dt.DayOfWeek);
                        nomeGiorno = char.ToUpper(nomeGiorno[0], culture) + nomeGiorno.Substring(1);
                        giornoFormattato = $"{nomeGiorno} {dt.Day}";
                    }
                    lista.Add(new PrevisioneGiorno
                    {
                        Giorno = giornoFormattato,
                        Min = tmin.Current.GetDouble(),
                        Max = tmax.Current.GetDouble(),
                        Pioggia = prec.Current.GetDouble()
                    });
                }
                return lista;
            }
            catch
            {
                return null;
            }
        }
    }
}
