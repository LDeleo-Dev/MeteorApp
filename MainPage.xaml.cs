using AppMeteoMAUI.Logic;
using System.Text.Json;
using AppMeteoMAUI.Logic;

namespace AppMeteoMAUI;

public partial class MainPage : ContentPage
{
	private readonly MeteoService _meteoService = new();


	private readonly PrevisioneService _previsioneService = new();

	public MainPage()
	{
		InitializeComponent();
	}

	private List<ResultWithForecast> _lastResults = new();

	private class ResultWithForecast
	{
		public string Nome { get; set; } = string.Empty;
		public double? Temp { get; set; }
		public double? Wind { get; set; }
		public string? Info { get; set; }
		public double? Lat { get; set; }
		public double? Lon { get; set; }
		public List<Logic.PrevisioneGiorno>? Previsione { get; set; }
		public string IconaMeteo
		{
			get
			{
				if (Temp == null) return "weather_unknown.png";
				if (Temp < 5) return "weather_cold.png";
				if (Temp < 18) return "weather_cloudy.png";
				if (Temp < 28) return "weather_sun.png";
				return "weather_hot.png";
			}
		}
	}

	private async void OnSearchClicked(object sender, EventArgs e)
	{
		var input = CityEntry.Text?.Trim();
		if (string.IsNullOrWhiteSpace(input))
		{
			await DisplayAlert("Errore", "Inserisci almeno una città.", "OK");
			return;
		}
		var cities = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (cities.Length == 0)
		{
			await DisplayAlert("Errore", "Nessuna città valida trovata.", "OK");
			return;
		}
		ResultsView.ItemsSource = null;
		SearchBarGrid.IsVisible = false;
		ToolbarItems.Clear();
		ToolbarItems.Add(new ToolbarItem { Text = "+", Order = ToolbarItemOrder.Primary, Priority = 0, Command = new Command(() => ShowSearchBar()) });

		var tasks = cities.Select(async city =>
		{
			var res = await _meteoService.GetWeatherAsync(city);
			string info = res.Errore != null
				? $"Errore: {res.Errore}"
				: $"Temperatura: {res.Temp:0.#} °C\nVento: {res.Wind:0.#} km/h";
			double? lat = null, lon = null;
			List<Logic.PrevisioneGiorno>? previsione = null;
			if (res.Errore == null)
			{
				string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1&language=it&format=json";
				using var http = new HttpClient();
				var geoRespMsg = await http.GetAsync(geoUrl);
				if (geoRespMsg.IsSuccessStatusCode)
				{
					var geoResp = await geoRespMsg.Content.ReadAsStringAsync();
					using var geoDoc = JsonDocument.Parse(geoResp);
					var geoRoot = geoDoc.RootElement;
					if (geoRoot.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
					{
						var loc = results[0];
						lat = loc.GetProperty("latitude").GetDouble();
						lon = loc.GetProperty("longitude").GetDouble();
						previsione = await _previsioneService.GetPrevisione5Giorni(lat.Value, lon.Value);
					}
				}
			}
			return new ResultWithForecast { Nome = city, Temp = res.Temp, Wind = res.Wind, Info = info, Lat = lat, Lon = lon, Previsione = previsione };
		});
		var results = (await Task.WhenAll(tasks)).OrderByDescending(r => r.Temp ?? double.MinValue).ToList();
		_lastResults = results;
		ResultsView.ItemsSource = results;

	}

	private void ShowSearchBar()
	{
		SearchBarGrid.IsVisible = true;
		ToolbarItems.Clear();
	}
		private async void OnShowForecastClicked(object sender, EventArgs e)
		{
			if (sender is Button btn && btn.BindingContext is ValueTuple<string, double?, double?, string?, double?, double?> ctx)
			{
				var (nome, _, _, _, lat, lon) = ctx;
				if (lat == null || lon == null)
				{
					await DisplayAlert("Errore", "Coordinate non disponibili per questa città.", "OK");
					return;
				}
			// ...existing code...
			// (Funzioni non più usate rimosse)
			}
	}
}
