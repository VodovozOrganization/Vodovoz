using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.Admin.Client
{
	public class AdminClient
    {
		private readonly string _url = "pacs/settings";
		private readonly HttpClient _httpClient = new HttpClient();
		private readonly ILogger<AdminClient> _logger;
		private readonly IPacsSettings _pacsSettings;

		public AdminClient(ILogger<AdminClient> logger, IPacsSettings pacsSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));
		}

		public async Task SetSettings(DomainSettings settings)
		{
			var uri = $"{_pacsSettings.AdministratorApiUrl}/{_url}/set";
			var content = new StringContent(JsonSerializer.Serialize(settings), Encoding.UTF8, "application/json");
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.AdministratorApiKey);

			try
			{
				await _httpClient.PostAsync(uri, content);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при установке новых настроек");
				throw;
			}
		}

		public async Task<DomainSettings> GetSettings()
		{
			var uri = $"{_pacsSettings.AdministratorApiUrl}/{_url}/get";
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("ApiKey", _pacsSettings.AdministratorApiKey);

			try
			{
				var response = await _httpClient.GetAsync(uri);
				if(response.IsSuccessStatusCode)
				{
					var responseBody = await response.Content.ReadAsStringAsync();
					var registrationResult = JsonSerializer.Deserialize<DomainSettings>(responseBody, 
						new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
					return registrationResult;
				}
				else
				{
					throw new InvalidOperationException($"Code: {response.StatusCode}. {response.ReasonPhrase}");
				}
				
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при установке новых настроек");
				throw;
			}
		}
	}
}
