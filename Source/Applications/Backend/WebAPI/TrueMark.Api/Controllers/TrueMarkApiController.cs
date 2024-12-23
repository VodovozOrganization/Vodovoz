using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Api.Options;
using TrueMark.Api.Responses;
using TrueMark.Contracts;
using TrueMark.Contracts.Requests;
using TrueMark.Contracts.Responses;
using IAuthorizationService = TrueMark.Api.Services.Authorization.IAuthorizationService;

namespace TrueMark.Api.Controllers;

//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[action]")]

public class TrueMarkApiController : ControllerBase
{
	private readonly ILogger<TrueMarkApiController> _logger;
	private readonly HttpClient _httpClient;
	private readonly IAuthorizationService _authorizationService;
	private readonly IOptions<TrueMarkApiOptions> _options;
	private readonly IOptions<TestOrganizationInfo> _testOrganizationInfoOptions;
	private readonly OrganizationCertificate _organizationCertificate;

	public TrueMarkApiController(
		IHttpClientFactory httpClientFactory,
		IAuthorizationService authorizationService,
		IOptions<TrueMarkApiOptions> options,
		IOptions<TestOrganizationInfo> testOrganizationInfoOptions,
		ILogger<TrueMarkApiController> logger)
	{
		if(httpClientFactory is null)
		{
			throw new ArgumentNullException(nameof(httpClientFactory));
		}

		_logger = logger
			?? throw new ArgumentNullException(nameof(logger));
		_authorizationService = authorizationService
			?? throw new ArgumentNullException(nameof(authorizationService));
		_options = options
			?? throw new ArgumentNullException(nameof(options));
		_testOrganizationInfoOptions = testOrganizationInfoOptions ?? throw new ArgumentNullException(nameof(testOrganizationInfoOptions));
		_httpClient = httpClientFactory.CreateClient("truemark-external");

		_organizationCertificate = options.Value.OrganizationCertificates.FirstOrDefault();
	}

	[HttpGet]
	public async Task<TrueMarkRegistrationResultDto> ParticipantRegistrationForWaterAsync(string inn)
	{
		throw new NotImplementedException();

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
		throw new NotImplementedException();

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
				"Ошибка при получении статуса регистрации в ЧЗ: Http code {ResponseStatusCode}, причина {ResponseReasonPhrase}",
				response.StatusCode,
				response.ReasonPhrase);

			return null;
		}

		string responseBody = await response.Content.ReadAsStringAsync();
		var registrationResult = JsonSerializer.Deserialize<IList<ParticipantRegistrationDto>>(responseBody);

		return registrationResult;
	}

	[HttpPost]
	public async Task<ProductInstancesInfoResponse> RequestProductInstanceInfo(
		[FromServices] IRequestClient<ProductInstanceInfoRequest> requestClient,
		IEnumerable<string> identificationCodes,
		CancellationToken cancellationToken)
	{
		var instanceStatuses = new List<ProductInstanceStatus>();

		var ownerInn = _testOrganizationInfoOptions.Value.Inn;
		var ownerName = _testOrganizationInfoOptions.Value.Name;

		foreach(var code in identificationCodes)
		{
			var instanceStatus = new ProductInstanceStatus
			{
				IdentificationCode = code,
				Status = ProductInstanceStatusEnum.Introduced,
				OwnerInn = ownerInn,
				OwnerName = ownerName,
			};

			instanceStatuses.Add(instanceStatus);
		}

		var response = new ProductInstancesInfoResponse
		{
			InstanceStatuses = instanceStatuses
		};

		return response;

		//var identificationCodesArray = identificationCodes.ToArray();

		//var errorMessage = new StringBuilder();

		//string bearerToken;

		//try
		//{
		//	bearerToken = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint, _organizationCertificate.Inn);
		//}
		//catch(Exception e)
		//{
		//	const string tokenRequestFailedMessage = "Ошибка получения токена авторизации в Честном знаке";

		//	_logger.LogError(e, tokenRequestFailedMessage);

		//	return new ProductInstancesInfoResponse
		//	{
		//		ErrorMessage = tokenRequestFailedMessage
		//	};
		//}

		//List<Task<Response<ProductInstanceInfoResponse>>> requestsTasks = CreateRequestsTasks(requestClient, identificationCodesArray, bearerToken, cancellationToken);

		//var productInstancesInfoResponses = new List<ProductInstanceStatus>();

		//try
		//{
		//	var results = await Task.WhenAll(requestsTasks);

		//	return ProcessResponses(errorMessage, productInstancesInfoResponses, results.Select(r => r.Message));
		//}
		//catch
		//{
		//	var partiallyProcessedResults = new List<ProductInstanceInfoResponse>();

		//	foreach(var requestTask in requestsTasks)
		//	{
		//		if(requestTask.Status != TaskStatus.Canceled)
		//		{
		//			if(requestTask.Exception != null)
		//			{
		//				var indexOfFaultedTask = requestsTasks.IndexOf(requestTask);

		//				if(indexOfFaultedTask != -1 && identificationCodesArray.Length < indexOfFaultedTask)
		//				{
		//					_logger.LogError(
		//					requestTask.Exception,
		//					"Request of code #{Code} was faulted with exception",
		//					identificationCodesArray[indexOfFaultedTask]);
		//				}

		//				errorMessage.AppendLine(requestTask.Exception.Message);
		//			}
		//			else
		//			{
		//				partiallyProcessedResults.Add(requestTask.Result.Message);
		//			}
		//		}
		//		else
		//		{
		//			var indexOfCancelledTask = requestsTasks.IndexOf(requestTask);

		//			if(indexOfCancelledTask != -1 && identificationCodesArray.Length < indexOfCancelledTask)
		//			{
		//				_logger.LogWarning(
		//					"Request of code #{Code} was cancelled",
		//					identificationCodesArray[indexOfCancelledTask]);
		//			}
		//		}
		//	}

		//	if(requestsTasks.Any(rt => rt.Status == TaskStatus.Canceled))
		//	{
		//		errorMessage.AppendLine("Часть запросов была отменена");
		//	}

		//	return ProcessResponses(errorMessage, productInstancesInfoResponses, partiallyProcessedResults);
		//}
	}

	[HttpGet, Produces(MediaTypeNames.Text.Plain)]
	public async Task<string> Login(string certificateThumbPrint, string inn)
	{
		throw new NotImplementedException();

		return await _authorizationService.Login(certificateThumbPrint, inn);
	}

	[HttpGet]
	public async Task<byte[]> Sign(string data, bool isDeatchedSign, string certificateThumbPrint, string inn)
	{
		throw new NotImplementedException();
		var currentCert = _options.Value.OrganizationCertificates.SingleOrDefault(c => c.CertificateThumbPrint == certificateThumbPrint && c.Inn == inn);

		return await _authorizationService.CreateAttachedSignedCmsWithStore2012_256(data, isDeatchedSign, currentCert.CertPath, currentCert.CertPwd);
	}

	[HttpGet]
	public async Task<GetTrueMarkApiTokenResponse> GetTrueMarkApiToken()
	{
		throw new NotImplementedException();
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

	private static List<Task<Response<ProductInstanceInfoResponse>>> CreateRequestsTasks(
		IRequestClient<ProductInstanceInfoRequest> requestClient,
		IEnumerable<string> identificationCodes,
		string token,
		CancellationToken cancellationToken)
	{
		var requestsTasks = new List<Task<Response<ProductInstanceInfoResponse>>>();

		foreach(var code in identificationCodes)
		{
			requestsTasks.Add(requestClient.GetResponse<ProductInstanceInfoResponse>(
				new ProductInstanceInfoRequest
				{
					Bearer = token,
					ProductCode = code
				},
				cancellationToken,
				RequestTimeout.After(s: 10)));
		}

		return requestsTasks;
	}

	private static ProductInstancesInfoResponse ProcessResponses(
		StringBuilder errorMessage,
		List<ProductInstanceStatus> productInstancesInfoResponses,
		IEnumerable<ProductInstanceInfoResponse> results)
	{
		const string baseErrorMessage = "Не удалось получить данные о статусах экземпляров товаров.";

		foreach(var result in results)
		{
			if(string.IsNullOrWhiteSpace(result.ErrorMessage))
			{
				productInstancesInfoResponses.Add(result.InstanceStatus);
			}
			else
			{
				errorMessage.AppendLine(result.ErrorMessage);
			}
		}

		return new ProductInstancesInfoResponse
		{
			InstanceStatuses = productInstancesInfoResponses,
			ErrorMessage = errorMessage.Length > 0 ? baseErrorMessage + errorMessage.ToString() : ""
		};
	}
}
