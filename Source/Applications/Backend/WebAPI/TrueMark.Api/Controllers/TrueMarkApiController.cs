using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Api.Contracts.Dto;
using TrueMark.Api.Contracts.Requests;
using TrueMark.Api.Contracts.Responses;
using TrueMark.Api.Extensions;
using TrueMark.Api.Options;
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
	private readonly ILogger<TrueMarkApiController> _logger;
	private readonly HttpClient _httpClient;
	private readonly IAuthorizationService _authorizationService;
	private readonly IOptions<TrueMarkApiOptions> _options;
	private readonly OrganizationCertificate _organizationCertificate;

	public TrueMarkApiController(
		IHttpClientFactory httpClientFactory,
		IAuthorizationService authorizationService,
		IOptions<TrueMarkApiOptions> options,
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
		_httpClient = httpClientFactory.CreateClient("truemark-external");

		_organizationCertificate = options.Value.OrganizationCertificates.FirstOrDefault();
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
		IEnumerable<string> identificationCodes,
		CancellationToken cancellationToken)
	{
		var requestsTasks = new List<Task<ProductInstancesInfoResponse>>();
		var count = identificationCodes.Count();
		var processed = 0;

		while(processed < count)
		{
			var portion = identificationCodes.Skip(processed).Take(1000);
			processed += 1000;
			var portionRequestTask = RequestCodesStatuses(portion, cancellationToken);
			requestsTasks.Add(portionRequestTask);
		}

		var result = new List<ProductInstanceStatus>();

		try
		{
			var responses = await Task.WhenAll(requestsTasks);

			foreach(var response in responses)
			{
				if(!string.IsNullOrEmpty(response.ErrorMessage))
				{
					return new ProductInstancesInfoResponse
					{
						ErrorMessage = response.ErrorMessage,
						NoCodesFound = response.NoCodesFound
					};
				}

				result.AddRange(response.InstanceStatuses);
			}

			return new ProductInstancesInfoResponse
			{
				InstanceStatuses = result
			};
		}
		catch(Exception ex)
		{
			return new ProductInstancesInfoResponse
			{
				ErrorMessage = ex.Message
			};
		}
	}

	private async Task<ProductInstancesInfoResponse> RequestCodesStatuses(
		IEnumerable<string> identificationCodes,
		CancellationToken cancellationToken
		)
	{
		var uri = $"cises/info";

		var errorMessage = new StringBuilder();
		errorMessage.AppendLine("Не удалось получить данные о статусах экземпляров товаров.");

		try
		{
			var content = JsonSerializer.Serialize(identificationCodes.ToArray());
			var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

			var token = await _authorizationService.Login(
				_organizationCertificate.CertificateThumbPrint,
				_organizationCertificate.Inn
			);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var response = await _httpClient.PostAsync(uri, httpContent, cancellationToken);

			if(!response.IsSuccessStatusCode)
			{
				return new ProductInstancesInfoResponse
				{
					ErrorMessage = errorMessage.AppendLine($"{response.StatusCode} {response.ReasonPhrase}").ToString(),
					NoCodesFound = response.StatusCode == HttpStatusCode.NotFound
				};
			}

			string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
			var cisesInformation = JsonSerializer.Deserialize<IList<CisInfoRoot>>(responseBody);
			_logger.LogTrace("ResponseBody: {ResponseBody}", responseBody);

			var productInstancesInfo = cisesInformation.Select(cisesInformation =>
				new ProductInstanceStatus
				{
					IdentificationCode = cisesInformation.CisInfo.RequestedCis,
					Gtin = cisesInformation.CisInfo.Gtin,
					Status = cisesInformation.CisInfo.Status.ToProductInstanceStatusEnum(),
					GeneralPackageType = Enum.TryParse<GeneralPackageType>(cisesInformation.CisInfo.GeneralPackageType, true, out var generalPackageType) ? generalPackageType : null,
					PackageType = Enum.TryParse<PackageType>(cisesInformation.CisInfo.PackageType, true, out var packageType) ? packageType : null,
					Childs = cisesInformation.CisInfo.Childs ?? Enumerable.Empty<string>(),
					ParentId = cisesInformation.CisInfo.Parent,
					OwnerInn = cisesInformation.CisInfo.OwnerInn,
					OwnerName = cisesInformation.CisInfo.OwnerName,
					ProducedDate = DateTime.TryParse(cisesInformation.CisInfo.ProducedDate, out var producedDate) ? producedDate : null,
					ExpirationDate = DateTime.TryParse(cisesInformation.CisInfo.ExpirationDate, out var expirationDate) ? expirationDate : null
				}
			);

			_logger.LogInformation("Проверены статусы кодов в кол-ве: {CodesCount} шт.", productInstancesInfo.Count());

			return new ProductInstancesInfoResponse
			{
				InstanceStatuses = new List<ProductInstanceStatus>(productInstancesInfo)
			};
		}
		catch(Exception e)
		{
			_logger.LogError(e, errorMessage.ToString());

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
		return await SignDocument(data, isDeatchedSign, certificateThumbPrint, inn);
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

	/// <summary>
	/// Отправка документа вывода из оборота
	/// </summary>
	/// <param name="documentData">Данные документа</param>
	/// <param name="cancellationToken">Токен отмены</param>
	/// <returns>GUID документа в ЧЗ</returns>
	[HttpPost]
	public async Task<IActionResult> SendIndividualAccountingWithdrawalDocument([FromBody] SendDocumentDataRequest documentData, CancellationToken cancellationToken)
	{
		var uri = $"lk/documents/create?pg=water";

		var document = documentData.Document;
		var inn = documentData.Inn;

		try
		{
			var certificateThumbPrint = GetCertificateThumbPrintByInn(inn);

			var sign = await SignDocument(document, true, certificateThumbPrint, inn);

			var documentToSend = new SendDocumentDto
			{
				DocumentFormat = "MANUAL",
				Type = "LK_RECEIPT",
				ProductDocument = Convert.ToBase64String(Encoding.UTF8.GetBytes(document)),
				Signature = Convert.ToBase64String(sign)
			};

			var httpContent = new StringContent(JsonSerializer.Serialize(documentToSend), Encoding.UTF8, "application/json");

			var token = await _authorizationService.Login(certificateThumbPrint, inn);
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var responseResult = await _httpClient.PostAsync(uri, httpContent, cancellationToken);
			responseResult.EnsureSuccessStatusCode();

			var documentId = await responseResult.Content.ReadAsStringAsync(cancellationToken);

			return new ContentResult
			{
				Content = documentId,
				ContentType = "text/plain",
				StatusCode = StatusCodes.Status200OK
			};
		}
		catch(Exception e)
		{
			_logger.LogError(e, "Ошибка при отправке документа вывода из оборота");

			return new ContentResult
			{
				Content = $"Не удалось выполнить отправку документа вывода из оборота. Ошибка: {e.Message}",
				ContentType = "text/plain",
				StatusCode = StatusCodes.Status500InternalServerError
			};
		}
	}

	private async Task<byte[]> SignDocument(string data, bool isDeatchedSign, string certificateThumbPrint, string inn)
	{
		var currentCert = _options.Value.OrganizationCertificates.SingleOrDefault(c => c.CertificateThumbPrint == certificateThumbPrint && c.Inn == inn);

		return await _authorizationService.CreateAttachedSignedCmsWithStore2012_256(data, isDeatchedSign, currentCert.CertPath, currentCert.CertPwd);
	}

	private string GetCertificateThumbPrintByInn(string inn) =>
		_options.Value.OrganizationCertificates.Where(c => c.Inn == inn).Select(x => x.CertificateThumbPrint).SingleOrDefault();

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
