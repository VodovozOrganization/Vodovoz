using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TrueMarkApi.Library.Dto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IAuthorizationService = TrueMarkApi.Services.Authorization.IAuthorizationService;
using TrueMarkApi.Dto;
using TrueMarkApi.Dto.Participants;

namespace TrueMarkApi.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ApiController]
	[Route("api/[controller]")]
	
	public class TrueMarkApiController : ControllerBase
	{
		private readonly IAuthorizationService _authorizationService;
		private static HttpClient _httpClient;
		private readonly ILogger<TrueMarkApiController> _logger;
		private readonly OrganizationCertificate _organizationCertificate;

		public TrueMarkApiController(
			IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			IAuthorizationService authorizationService,
			ILogger<TrueMarkApiController> logger)
		{
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			var apiSection = (configuration ?? throw new ArgumentNullException(nameof(configuration))).GetSection("Api");

			_httpClient = httpClientFactory.CreateClient();
			_httpClient.BaseAddress = new Uri(apiSection.GetValue<string>("ExternalTrueApiBaseUrl"));
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var organizationsCertificateSection = apiSection.GetSection("OrganizationCertificates");
			_organizationCertificate = organizationsCertificateSection.Get<OrganizationCertificate[]>().ToArray().FirstOrDefault();

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}
		
		[HttpGet]
		[Route("/api/ParticipantRegistrationForWater")]
		public async Task<TrueMarkResponseResultDto> ParticipantRegistrationForWaterAsync(string inn)
		{
			var uri = $"participants?inns={inn}";

			var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			StringBuilder errorMessage = new StringBuilder();
			errorMessage.AppendLine("Не удалось получить статус регистрации учатниска.");

			try
			{
				var response = await _httpClient.GetAsync(uri);

				if(response.IsSuccessStatusCode)
				{
					string responseBody = await response.Content.ReadAsStringAsync();
					var registrationResult = JsonSerializer.Deserialize<IList<ParticipantRegistrationDto>>(responseBody).FirstOrDefault();

					if(!string.IsNullOrWhiteSpace(registrationResult.ErrorMessage))
					{
						return new TrueMarkResponseResultDto
						{
							ErrorMessage = registrationResult.ErrorMessage
						};
					}

					if(!registrationResult.IsRegisteredForWater)
					{
						return new TrueMarkResponseResultDto
						{
							ErrorMessage = "Участник зарегистрирован в Честном Знаке, но нет регистрации по группе товаров \"Вода\"!"
						};
					}

					return new TrueMarkResponseResultDto
					{
						RegistrationStatusString = registrationResult.Status
					};

				}

				return new TrueMarkResponseResultDto
				{
					ErrorMessage = errorMessage.AppendLine($"{response.StatusCode} {response.ReasonPhrase}").ToString()
				};
			}
			catch(Exception e)
			{
				_logger.LogError(e, errorMessage.ToString());

				return new TrueMarkResponseResultDto
				{
					ErrorMessage = errorMessage.AppendLine(e.Message).ToString()

				};
			}
		}

		[HttpPost]
		[Route("/api/Participants")]
		public async Task<IList<ParticipantRegistrationDto>> ParticipantsAsync(IList<string> inns)
		{
			if(!inns.Any())
			{
				return null;
			}

			var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint);
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

		[HttpPost]
		[Route("/api/RequestProductInstanceInfo")]
		public async Task<ProductInstancesInfo> GetProductInstanceInfo([FromBody]IEnumerable<string> identificationCodes)
		{
			var uri = $"cises/info";

			var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			_logger.LogInformation($"token: {token}");

			StringBuilder errorMessage = new StringBuilder();
			errorMessage.AppendLine("Не удалось получить данные о статусах экземпляров товаров.");

			try
			{
				string content = JsonSerializer.Serialize(identificationCodes.ToArray());
				HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(uri, httpContent);
				if(response.IsSuccessStatusCode)
				{
					string responseBody = await response.Content.ReadAsStringAsync();
					var cisesInformation = JsonSerializer.Deserialize<IList<CisInfoRoot>>(responseBody);
					_logger.LogInformation($"responseBody: {responseBody}");

					var productInstancesInfo = cisesInformation.Select(x =>
						new ProductInstanceStatus
						{
							IdentificationCode = x.CisInfo.RequestedCis,
							Status = GetStatus(x.CisInfo.Status),
							OwnerInn = x.CisInfo.OwnerInn,
							OwnerName = x.CisInfo.OwnerName
						}
					);

					return new ProductInstancesInfo
					{
						InstanceStatuses = new List<ProductInstanceStatus>(productInstancesInfo)
					};
				}

				return new ProductInstancesInfo
				{
					ErrorMessage = errorMessage.AppendLine($"{response.StatusCode} {response.ReasonPhrase}").ToString()
				};
			}
			catch(Exception e)
			{
				_logger.LogError(e, errorMessage.ToString());

				return new ProductInstancesInfo
				{
					ErrorMessage = errorMessage.AppendLine(e.Message).ToString()
				};
			}
		}
		private ProductInstanceStatusEnum? GetStatus(string statusName)
		{
			switch(statusName)
			{
				case "EMITTED":
					return ProductInstanceStatusEnum.Emitted;
				case "APPLIED":
					return ProductInstanceStatusEnum.Applied;
				case "APPLIED_PAID":
					return ProductInstanceStatusEnum.AppliedPaid;
				case "INTRODUCED":
					return ProductInstanceStatusEnum.Introduced;
				case "WRITTEN_OFF":
					return ProductInstanceStatusEnum.WrittenOff;
				case "RETIRED":
					return ProductInstanceStatusEnum.Retired;
				case "WITHDRAWN":
					return ProductInstanceStatusEnum.Withdrawn;
				case "DISAGGREGATION":
					return ProductInstanceStatusEnum.Disaggregation;
				case "DISAGGREGATED":
					return ProductInstanceStatusEnum.Disaggregated;
				case "APPLIED_NOT_PAID":
					return ProductInstanceStatusEnum.AppliedNotPaid;
				default:
					return null;
			}
		}
	}
}
