using EdoService.Dto;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Vodovoz.Services;

namespace EdoService.Services
{
	public class TrueApiService : ITrueApiService
	{
		private readonly IEdoLogger _edoLogger;
		private static HttpClient _httpClient;
		private readonly IEdoSettings _edoSettings;
		private readonly IAuthorizationService _authorizationService;

		public TrueApiService(IAuthorizationService authorizationService, IEdoSettings edoSettings)
		{
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

			var logger = LogManager.GetCurrentClassLogger();
			_edoLogger = new EdoLogger(logger);

			_httpClient = new HttpClient()
			{
				BaseAddress = new Uri(edoSettings.TrueApiBaseAddressUri),
				Timeout = TimeSpan.FromSeconds(5)
			};

			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<string> Login() => await _authorizationService.Login();

		public async Task<bool> ParticipantsAsync(string inn, string productGroup) //7729076804
		{
			//var token = Login(); Жду ЭЦП

			var uri = $"{_edoSettings.TrueApiParticipantsUri}?inns={inn}";
			var request = new HttpRequestMessage(HttpMethod.Get, uri);
			var response = await _httpClient.SendAsync(request);

			if(response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				var registrationResult = JsonSerializer.Deserialize<IEnumerable<TrueApiRegistrationDto>>(responseBody);

				if(registrationResult != null)
				{
					var registration = registrationResult.FirstOrDefault();

					var isRegisteredForProductGroup = registration != null 
					                                  && registration.IsRegistered 
					                                  && registration.ProductGroups.Contains(productGroup);

					return isRegisteredForProductGroup;
				}
			}

			_edoLogger.LogError(response);

			return false;
		}
	}
}
