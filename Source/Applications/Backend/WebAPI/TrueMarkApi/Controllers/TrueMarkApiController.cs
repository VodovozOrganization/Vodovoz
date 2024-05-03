using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
using TrueMarkApi.Dto;
using TrueMarkApi.Dto.Participants;
using TrueMarkApi.Library.Dto;
using TrueMarkApi.Responses;
using IAuthorizationService = TrueMarkApi.Services.Authorization.IAuthorizationService;

namespace TrueMarkApi.Controllers
{
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ApiController]
	[Route("api/[action]")]
	
	public class TrueMarkApiController : ControllerBase
	{
		private readonly IAuthorizationService _authorizationService;
		private static HttpClient _httpClient;
		private readonly ILogger<TrueMarkApiController> _logger;
		private readonly OrganizationCertificate _organizationCertificate;

		public TrueMarkApiController(
			IConfiguration configuration,
			IAuthorizationService authorizationService,
			HttpClient httpClient,
			ILogger<TrueMarkApiController> logger)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

			var apiSection = configuration.GetSection("Api");
			var organizationsCertificateSection = apiSection.GetSection("OrganizationCertificates");
			_organizationCertificate = organizationsCertificateSection.Get<OrganizationCertificate[]>().ToArray().FirstOrDefault();
		}
		
		[HttpGet]
		public async Task<TrueMarkResponseResultDto> ParticipantRegistrationForWaterAsync(string inn)
		{
			var uri = $"participants?inns={inn}";

			var errorMessage = new StringBuilder();
			errorMessage.AppendLine("Не удалось получить статус регистрации учатниска.");

			var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint, _organizationCertificate.Inn);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			try
			{
				var response = await _httpClient.GetAsync(uri);

				if(!response.IsSuccessStatusCode)
				{
					return new TrueMarkResponseResultDto
					{
						ErrorMessage = errorMessage.AppendLine($"{response.StatusCode} {response.ReasonPhrase}").ToString()
					};

				}

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
		public async Task<IList<ParticipantRegistrationDto>> ParticipantsAsync(IList<string> inns)
		{
			if(!inns.Any())
			{
				return null;
			}

			var innString = string.Join("&inns=", inns);

			var uri = $"participants?inns={innString}";

			var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint, _organizationCertificate.Inn);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var response = await _httpClient.GetAsync(uri);

			if(!response.IsSuccessStatusCode)
			{
				_logger.LogError($"Ошибка при получении статуса регистрации в ЧЗ: Http code {response.StatusCode}, причина {response.ReasonPhrase}");

				return null;
			}

			string responseBody = await response.Content.ReadAsStringAsync();
			var registrationResult = JsonSerializer.Deserialize<IList<ParticipantRegistrationDto>>(responseBody);

			return registrationResult;
		}

		[HttpPost]
		public async Task<ProductInstancesInfo> RequestProductInstanceInfoAsync(IEnumerable<string> identificationCodes)
		{
			var uri = $"cises/info";

			var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint, _organizationCertificate.Inn);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var errorMessage = new StringBuilder();
			errorMessage.AppendLine("Не удалось получить данные о статусах экземпляров товаров.");

			try
			{
				string content = JsonSerializer.Serialize(identificationCodes.ToArray());
				HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(uri, httpContent);

				if(!response.IsSuccessStatusCode)
				{
					return new ProductInstancesInfo
					{
						ErrorMessage = errorMessage.AppendLine($"{response.StatusCode} {response.ReasonPhrase}").ToString()
					};
				}

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
			catch(Exception e)
			{
				_logger.LogError(e, errorMessage.ToString());

				return new ProductInstancesInfo
				{
					ErrorMessage = errorMessage.AppendLine(e.Message).ToString()
				};
			}
		}

		[HttpGet]
		public async Task<GetTrueMarkApiTokenResponse> GetTrueMarkApiToken()
		{
			try
			{
				var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint, _organizationCertificate.Inn);

				return new GetTrueMarkApiTokenResponse
				{
					Token = token,
				};
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при запросе ключа четсного знака: {ExceptionMessage}", e.Message);

				return new GetTrueMarkApiTokenResponse
				{
					ErrorMessage = e.Message
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
