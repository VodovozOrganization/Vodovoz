using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using TrueApi.Dto.Participants;
using TrueApi.Services;

namespace TrueApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TrueApiController : ControllerBase
	{
		private readonly IAuthorizationService _authorizationService;
		private static HttpClient _httpClient;
		private readonly ILogger<TrueApiController> _logger;

		public TrueApiController(
			IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			IAuthorizationService authorizationService,
			ILogger<TrueApiController> logger)
		{
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			var apiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("Api");

			_httpClient = httpClientFactory.CreateClient();
			_httpClient.BaseAddress = new Uri(apiSection.GetValue<string>("BaseUrl"));
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpGet]
		[Route("/api/ParticipantRegistrationByProductGroup")]
		public async Task<bool> ParticipantRegistrationByProductGroupAsync(string inn, string productGroup)
		{
			var uri = $"participants?inns={inn}";

			var token = await _authorizationService.Login();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var response = await _httpClient.GetAsync(uri);

			if(response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				var registrationResult = JsonSerializer.Deserialize<IEnumerable<ParticipantRegistrationDto>>(responseBody);

				if(registrationResult != null)
				{
					var registration = registrationResult.FirstOrDefault(r=>r.IsRegistered && r.ProductGroups.Contains(productGroup));

					return registration != null;
				}
			}

			_logger.LogError($"Ошибка при получении статуса регистрации в ЧЗ: Http code {response.StatusCode}, причина {response.ReasonPhrase}");

			return false;
		}

		[HttpPost]
		[Route("/api/Participants")]
		public async Task<IList<ParticipantRegistrationDto>> ParticipantsAsync(IList<string> inns)
		{
			if(!inns.Any())
			{
				return null;
			}

			var token = await _authorizationService.Login();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			
			var innString = string.Join("&inns=", inns);
			
			var uri = $"participants?inns={innString}";
			
			var response = await _httpClient.GetAsync(uri);

			if(response.IsSuccessStatusCode)
			{
				string responseBody = await response.Content.ReadAsStringAsync();
				var registrationResult = JsonSerializer.Deserialize<IList<ParticipantRegistrationDto>>(responseBody);

				return registrationResult;
			}

			_logger.LogError($"Ошибка при получении статуса регистрации в ЧЗ: Http code {response.StatusCode}, причина {response.ReasonPhrase}");

			return null;
		}
	}
}
