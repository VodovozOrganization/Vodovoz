using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrueMark.Api.Options;
using TrueMark.Api.Responses;
using TrueMark.Contracts;
using TrueMark.Contracts.Requests;
using TrueMark.Contracts.Responses;
using IAuthorizationService = TrueMark.Api.Services.Authorization.IAuthorizationService;

namespace TrueMark.Api.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[action]")]

public class TrueMarkApiController : ControllerBase
{
	private readonly IAuthorizationService _authorizationService;
	private readonly IOptions<TrueMarkApiOptions> _options;
	private static HttpClient _httpClient;
	private readonly ILogger<TrueMarkApiController> _logger;
	private readonly OrganizationCertificate _organizationCertificate;

	public TrueMarkApiController(
		IConfiguration configuration,
		IHttpClientFactory httpClientFactory,
		IAuthorizationService authorizationService,
		IOptions<TrueMarkApiOptions> options,
		ILogger<TrueMarkApiController> logger)
	{
		_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
		_options = options ?? throw new ArgumentNullException(nameof(options));
		_httpClient = httpClientFactory.CreateClient();
		_httpClient.BaseAddress = new Uri(options.Value.ExternalTrueMarkBaseUrl);
		_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		_organizationCertificate = options.Value.OrganizationCertificates.FirstOrDefault();

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpGet]
	public async Task<TrueMarkRegistrationResultDto> ParticipantRegistrationForWaterAsync(string inn)
	{
		var uri = $"participants?inns={inn}";

		var errorMessage = new StringBuilder();
		errorMessage.AppendLine("Не удалось получить статус регистрации учатниска.");

		try
		{
			var response = await _httpClient.GetAsync(uri);

			if(!response.IsSuccessStatusCode)
			{
				return new TrueMarkRegistrationResultDto
				{
					ErrorMessage = errorMessage.AppendLine($"{response.StatusCode} {response.ReasonPhrase}").ToString()
				};
			}

			string responseBody = await response.Content.ReadAsStringAsync();
			var registrationResult = JsonSerializer.Deserialize<IList<ParticipantRegistrationDto>>(responseBody).FirstOrDefault();

			if(!string.IsNullOrWhiteSpace(registrationResult.ErrorMessage))
			{
				return new TrueMarkRegistrationResultDto
				{
					ErrorMessage = registrationResult.ErrorMessage
				};
			}

			if(!registrationResult.IsRegisteredForWater)
			{
				return new TrueMarkRegistrationResultDto
				{
					ErrorMessage = "Участник зарегистрирован в Честном Знаке, но нет регистрации по группе товаров \"Вода\"!"
				};
			}

			return new TrueMarkRegistrationResultDto
			{
				RegistrationStatusString = registrationResult.Status
			};
		}
		catch(Exception e)
		{
			_logger.LogError(e, errorMessage.ToString());

			return new TrueMarkRegistrationResultDto
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

		var response = await _httpClient.GetAsync(uri);

		if(!response.IsSuccessStatusCode)
		{
			_logger.LogError(
				$"Ошибка при получении статуса регистрации в ЧЗ: Http code {response.StatusCode}, причина {response.ReasonPhrase}");

			return null;
		}

		string responseBody = await response.Content.ReadAsStringAsync();
		var registrationResult = JsonSerializer.Deserialize<IList<ParticipantRegistrationDto>>(responseBody);

		return registrationResult;
	}

	[HttpPost]
	public async Task<ProductInstancesInfoResponse> RequestProductInstanceInfo(
		[FromServices] IRequestClient<ProductInstanceInfoRequest> requestClient,
		[FromBody] IEnumerable<string> identificationCodes)
	{
		var errorMessage = new StringBuilder();
		errorMessage.AppendLine("Не удалось получить данные о статусах экземпляров товаров.");

		try
		{
			var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint, _organizationCertificate.Inn);

			var requestsTasks = new List<Task<Response<ProductInstanceInfoResponse>>>();

			foreach(var code in identificationCodes)
			{
				requestsTasks.Add(requestClient.GetResponse<ProductInstanceInfoResponse>(new ProductInstanceInfoRequest
				{
					Bearer = token,
					ProductCode = code
				}));
			}

			var results = await Task.WhenAll(requestsTasks); // Таймаут, добавить таймаут на стороне сообщений в Rabbit

			var productInstancesInfoResponses = new List<ProductInstanceStatus>();

			foreach(var result in results)
			{
				if(string.IsNullOrWhiteSpace(result.Message.ErrorMessage))
				{
					productInstancesInfoResponses.Add(result.Message.InstanceStatus);
				}
				else
				{
					errorMessage.AppendLine(result.Message.ErrorMessage);
				}
			}

			return new ProductInstancesInfoResponse // Поправить обработку на стороне клиента
			{
				InstanceStatuses = productInstancesInfoResponses,
				ErrorMessage = errorMessage.ToString() // проверить, что клиент корректно обработает
			};
		}
		catch(Exception e)
		{
			_logger.LogError(e, "Error: {CombinedErrorMessage}", errorMessage.ToString());

			return new ProductInstancesInfoResponse
			{
				ErrorMessage = errorMessage.AppendLine(e.Message).ToString()
			};
		}
	}

	[HttpGet, Produces(MediaTypeNames.Text.Plain)]
	public async Task<string> Login(string certificateThumbPrint, string inn)
	{
		return await _authorizationService.Login(certificateThumbPrint, inn);
	}

	[HttpGet]
	public async Task<byte[]> Sign(string data, bool isDeatchedSign, string certificateThumbPrint, string inn)
	{
		var currentCert = _options.Value.OrganizationCertificates.SingleOrDefault(c => c.CertificateThumbPrint == certificateThumbPrint && c.Inn == inn);

		return await _authorizationService.CreateAttachedSignedCmsWithStore2012_256(data, isDeatchedSign, currentCert.CertPath, currentCert.CertPwd);
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
		catch(Exception e)
		{
			_logger.LogError(e, "Произошла ошибка при запросе ключа четсного знака: {ExceptionMessage}", e.Message);

			return new GetTrueMarkApiTokenResponse
			{
				ErrorMessage = e.Message
			};
		}
	}
}
