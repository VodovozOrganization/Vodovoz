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
		/*
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
		}*/

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
						new ProductInstanceStatus { 
							IdentificationCode = x.CisInfo.RequestedCis, 
							Status = GetStatus(x.CisInfo.Status) 
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

		/*public async Task<ProductInstancesInfo> Send()
		{
			var uri = $"cises/info";

			IEnumerable<string> identificationCodes = new string[] { "0104602009723261215QDn,QsJEgpZ\"" };

			//var token = await _authorizationService.Login(_organizationCertificate.CertificateThumbPrint);
			var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJwcm9kdWN0X2dyb3VwX2luZm8iOlt7Im5hbWUiOiJ3YXRlciIsInN0YXR1cyI6IjUiLCJ0eXBlcyI6WyJUUkFERV9QQVJUSUNJUEFOVCIsIldIT0xFU0FMRVIiXX1dLCJ1c2VyX3N0YXR1cyI6IkFDVElWRSIsImlubiI6Ijc4MTY0NTMyOTQiLCJwaWQiOjExMDAwMDczMTYwLCJjbGllbnRfaWQiOiJjcnB0LXNlcnZpY2UiLCJhdXRob3JpdGllcyI6WyJST0xFX0FETUlOIiwiQ1JQVC1GQUNBREUuUFJPRklMRS1DT05UUk9MTEVSLkNPTVBBTlkuQ1JFQVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuUkVBR0dSRUdBVElPTi5SRUFEIiwiQ1JQVC1GQUNBREUuQVBQLVVTRVItQ09OVFJPTExFUi5MSVNULUFDVElWRS5SRUFEIiwiQ1JQVC1LTS1PUkRFUlMuT1JERVItRkFDQURFLUNPTlRST0xMRVIuUkVBRElORy1CWS1PUEVSQVRPUi5SRUFEIiwiQ1JQVC1MSy1ET0MtQVBJLkFQUC1VU0VSLUNPTlRST0xMRVIuV1JJVEUiLCJDUlBULUxLLURPQy1BUEkuQkxPQ0tJTkctQ09OVFJPTExFUi5DUkVBVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5MS19HVElOX1JFQ0VJUFRfQ0FOQ0VMLlJFQUQiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5PU1RfQ09NUExFVEVfREVTQ1JJUFRJT04uQ1JFQVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuV1JJVEUtT0ZGLkNSRUFURSIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkNPTlRSQUNULUNPTU1JU1NJT05JTkcuQ1JFQVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuSU5ESVZJRFVBTElaQVRJT04uQ1JFQVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuT1NUX0RFU0NSSVBUSU9OLlJFQUQiLCJDUlBULUZBQ0FERS5DSVMtQ09OVFJPTExFUi5TRUFSQ0guUkVBRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLldSSVRFLU9GRi5SRUFEIiwiQ1JQVC1LTS1PUkRFUlMuTEFCRUwtVEVNUExBVEUtRkFDQURFLUNPTlRST0xMRVIuVVBEQVRJTkcuV1JJVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5TSElQLUNST1NTQk9SREVSLkNSRUFURSIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkxQX0NBTkNFTF9TSElQTUVOVF9DUk9TU0JPUkRFUi5DUkVBVEUiLCJDUlBULUtNLU9SREVSUy5PUkRFUi1GQUNBREUtQ09OVFJPTExFUi5SRUFESU5HLUJZLVNVWi5SRUFEIiwiQ1JQVC1LTS1PUkRFUlMuT1JERVItRkFDQURFLUNPTlRST0xMRVIuT1JERVJTLUZST00tU1VaLkNSRUFURSIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLktNLUNBTkNFTC5DUkVBVEUiLCJDUlBULUxLLURPQy1BUEkuQVBQLVVTRVItQ09OVFJPTExFUi5DUkVBVEUiLCJDUlBULUtNLU9SREVSUy5MQUJFTC1URU1QTEFURS1GQUNBREUtQ09OVFJPTExFUi5ET1dOTE9BRElORy5ET1dOTE9BRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLlJFUE9SVF9SRVdFSUdISU5HLkNSRUFURSIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLlJFQUdHUkVHQVRJT04uQ1JFQVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuTFBfUkVUVVJOLlJFQUQiLCJDUlBULUtNLU9SREVSUy5TVVotUkVHSVNUUlktRkFDQURFLUNPTlRST0xMRVIuUkVBRElORy1CWS1PUEVSQVRPUi5SRUFEIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuTEtfR1RJTl9SRUNFSVBULkNSRUFURSIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkxLX0dUSU5fUkVDRUlQVF9DQU5DRUwuQ1JFQVRFIiwiQ1JQVC1LTS1PUkRFUlMuTEFCRUwtVEVNUExBVEUtRkFDQURFLUNPTlRST0xMRVIuVVBEQVRJTkctREVGQVVMVC5XUklURSIsIkNSUFQtTEstRE9DLUFQSS5SRVNVTUUtQUNDRVNTLUNPTlRST0xMRVIuQ1JFQVRFIiwiQ1JQVC1LTS1PUkRFUlMuTEFCRUwtVEVNUExBVEUtRkFDQURFLUNPTlRST0xMRVIuREVMRVRJTkcuREVMRVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuS00tQVBQTElFRC1DQU5DRUwuQ1JFQVRFIiwiQ1JQVC1LTS1PUkRFUlMuTEFCRUwtVEVNUExBVEUtRkFDQURFLUNPTlRST0xMRVIuQ1JFQVRJTkctREVGQVVMVC5DUkVBVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5JTkRJVklEVUFMSVpBVElPTi5SRUFEIiwiQ1JQVC1LTS1PUkRFUlMuTEFCRUwtVEVNUExBVEUtRkFDQURFLUNPTlRST0xMRVIuU1RBVFVTLUNIQU5HRS5XUklURSIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkNST1NTQk9SREVSLUFDQ0VQVEFOQ0UuQ1JFQVRFIiwiQ1JQVC1LTS1PUkRFUlMuT1BFUkFUT1IuV1JJVEUiLCJDUlBULUtNLU9SREVSUy5MQUJFTC1URU1QTEFURS1GQUNBREUtQ09OVFJPTExFUi5SRUFESU5HLURFRkFVTFRTLUJZLVNVWi5SRUFEIiwiQ1JQVC1LTS1PUkRFUlMuT1JERVItRkFDQURFLUNPTlRST0xMRVIuUkVBRElORy5SRUFEIiwiRUxLLVJFR0lTVFJBVElPTi5XUklURSIsIkNSUFQtS00tT1JERVJTLlBBUlRJQ0lQQU5ULU9SLU9QRVJBVE9SLlJFQUQiLCJDUlBULUtNLU9SREVSUy5MQUJFTC1URU1QTEFURS1GQUNBREUtQ09OVFJPTExFUi5SRUFESU5HLUJZLVNVWi5SRUFEIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuRUFTLUNST1NTQk9SREVSLUVYUE9SVC5DUkVBVEUiLCJDUlBULUtNLU9SREVSUy5TVVouV1JJVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5PU1RfREVTQ1JJUFRJT04uQ1JFQVRFIiwiQ1JQVC1MSy1ET0MtQVBJLkRSQUZULkFETUlOSVNUUkFUSU9OIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuU0hJUC1DUk9TU0JPUkRFUi5SRUFEIiwiQ1JQVC1LTS1PUkRFUlMuTEFCRUwtVEVNUExBVEUtRkFDQURFLUNPTlRST0xMRVIuREVMRVRJTkctREVGQVVMVC5ERUxFVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5TSElQTUVOVC5DUkVBVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5MT0FOLkNSRUFURSIsIkNSUFQtS00tT1JERVJTLk9SREVSLUZBQ0FERS1DT05UUk9MTEVSLlNVWi1FVkVOVFMuQ1JFQVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuUkVDRUlQVC5DUkVBVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5ESVNBR0dSRUdBVElPTi5SRUFEIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuTFBfSU5UUk9EVUNFX09TVC5DUkVBVEUiLCJFTEstUkVHSVNUUkFUSU9OLkNSRUFURSIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkxLX0dUSU5fUkVDRUlQVC5SRUFEIiwiQ1JQVC1GQUNBREUuQ0lTLUNPTlRST0xMRVIuUkVQT1JULkRPV05MT0FEIiwiQ1JQVC1GQUNBREUuQVBQLVVTRVItQ09OVFJPTExFUi5MSVNULVJFTU9WRUQuUkVBRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkNST1NTQk9SREVSLlJFQUQiLCJDUlBULUxLLURPQy1BUEkuQVBQLVVTRVItQ09OVFJPTExFUi5ERUxFVEUiLCJDUlBULUtNLU9SREVSUy5MQUJFTC1URU1QTEFURS1GQUNBREUtQ09OVFJPTExFUi5ET1dOTE9BRElORy1CWS1PUEVSQVRPUi5ET1dOTE9BRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkFHR1JFR0FUSU9OLlJFQUQiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5DUk9TU0JPUkRFUi5DUkVBVEUiLCJDUlBULUtNLU9SREVSUy5PUkRFUi1GQUNBREUtQ09OVFJPTExFUi5DUkVBVElORy1EUkFGVC5DUkVBVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5SRU1BUktJTkcuUkVBRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLklOREktQ09NTUlTU0lPTklORy5DUkVBVEUiLCJDUlBULUtNLU9SREVSUy5TVEFUSVNUSUNTLUZBQ0FERS1DT05UUk9MTEVSLlJFQURJTkctUEFSVElDSVBBTlQtU1RBVElTVElDUy5SRUFEIiwiRUxLLVBST0ZJTEUuUkVBRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkFDQ0VQVEFOQ0UuQ1JFQVRFIiwiQ1JQVC1MSy1ET0MtQVBJLkFERC1DRVJULUNPTlRST0xMRVIuQ1JFQVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuU0hJUE1FTlQuUkVBRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkVBUy1DUk9TU0JPUkRFUi1FWFBPUlQuUkVBRCIsIkNSUFQtRkFDQURFLk1BUktFRC1QUk9EVUNUUy1DT05UUk9MTEVSLlJFQUQiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5BR0dSRUdBVElPTi5DUkVBVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5ESVNBR0dSRUdBVElPTi5DUkVBVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5DT01NSVNTSU9OSU5HLkNSRUFURSIsIkNSUFQtRkFDQURFLk1BUktFRC1QUk9EVUNUUy1DT05UUk9MTEVSLkFETUlOSVNUUkFUSU9OIiwiQ1JQVC1LTS1PUkRFUlMuUEFSVElDSVBBTlQuV1JJVEUiLCJDUlBULUtNLU9SREVSUy5MQUJFTC1URU1QTEFURS1GQUNBREUtQ09OVFJPTExFUi5DUkVBVElORy5DUkVBVEUiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5DUk9TU0JPUkRFUi1FWFBPUlQuQ1JFQVRFIiwiQ1JQVC1LTS1PUkRFUlMuT1JERVItRkFDQURFLUNPTlRST0xMRVIuQ1JFQVRJTkcuQ1JFQVRFIiwiQ1JQVC1GQUNBREUuUFJPRklMRS1DT05UUk9MTEVSLkNPTVBBTlkuUkVBRCIsIkNSUFQtRkFDQURFLlBBUlRJQ0lQQU5ULUNPTlRST0xMRVIuR0VULUJZLUlOTi5SRUFEIiwiRUxLLVJFR0lTVFJBVElPTi5SRUFEIiwiQ1JQVC1LTS1PUkRFUlMuT1JERVItRkFDQURFLUNPTlRST0xMRVIuTU9ESUZZSU5HLURSQUZULldSSVRFIiwiQ1JQVC1LTS1PUkRFUlMuUEFSVElDSVBBTlQtT1ItT1BFUkFUT1IuV1JJVEUiLCJDUlBULUtNLU9SREVSUy5MQUJFTC1URU1QTEFURS1GQUNBREUtQ09OVFJPTExFUi5SRUFESU5HLUJZLU9QRVJBVE9SLlJFQUQiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5MUF9DQU5DRUxfU0hJUE1FTlRfQ1JPU1NCT1JERVIuUkVBRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLkFDQ0VQVEFOQ0UuUkVBRCIsIkNSUFQtRkFDQURFLkRPQy1DT05UUk9MTEVSLk9TVF9DT01QTEVURV9ERVNDUklQVElPTi5SRUFEIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuTFBfSU5UUk9EVUNFX09TVC5SRUFEIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuUkVQT1JUX1JFV0VJR0hJTkcuUkVBRCIsIkNSUFQtRkFDQURFLkNJUy1DT05UUk9MTEVSLlJFUE9SVC5SRUFEIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuSU1QT1JULUNPTU1JU1NJT05JTkcuQ1JFQVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuUkVNQVJLSU5HLkNSRUFURSIsIkNSUFQtS00tT1JERVJTLlNVWi1SRUdJU1RSWS1GQUNBREUtQ09OVFJPTExFUi5SRUFESU5HLlJFQUQiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5MT0FOLlJFQUQiLCJDUlBULUZBQ0FERS5ET0MtQ09OVFJPTExFUi5MUF9SRVRVUk4uQ1JFQVRFIiwiQ1JQVC1LTS1PUkRFUlMuTEFCRUwtVEVNUExBVEUtRkFDQURFLUNPTlRST0xMRVIuUkVBRElORy5SRUFEIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuQ09NTUlTU0lPTklORy5SRUFEIiwiQ1JQVC1LTS1PUkRFUlMuT1JERVItRkFDQURFLUNPTlRST0xMRVIuTU9ESUZZSU5HLldSSVRFIiwiQ1JQVC1GQUNBREUuRE9DLUNPTlRST0xMRVIuQ1JPU1NCT1JERVItRVhQT1JULlJFQUQiLCJST0xFX09SR1_QntC_0YLQvtCy0LDRjyDRgtC-0YDQs9C-0LLQu9GPIiwiUk9MRV9PUkdf0KPRh9Cw0YHRgtC90LjQuiDQvtCx0L7RgNC-0YLQsCIsIlJPTEVfT1JHX1RSQURFX1BBUlRJQ0lQQU5UIiwiUk9MRV9PUkdfV0hPTEVTQUxFUiIsIklOTl83ODE2NDUzMjk0Il0sImZ1bGxfbmFtZSI6ItCY0YHRgNCw0YTQuNC70L7QstCwINCY0YDQuNC90LAg0JDQu9C10LrRgdC10LXQstC90LAiLCJzY29wZSI6WyJ0cnVzdGVkIl0sImlkIjoxMTAwMDQ2NDczOSwiZXhwIjoxNjc1MTc4MzA3LCJvcmdhbmlzYXRpb25fc3RhdHVzIjoiUkVHSVNURVJFRCIsImp0aSI6IjQxNmQxNWZhLTU3Y2QtNDQzMC05NDU0LTEwYzgwNjhiMmY0MyJ9.ex4RLx3JSXTwP3ERL5lu0DQ-71YifEL7Yb-3pkg8x6U";
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
					var cisesInformation = JsonSerializer.Deserialize<IList<CisInfo>>(responseBody);
					_logger.LogInformation($"responseBody: {responseBody}");

					var productInstancesInfo = cisesInformation.Select(x =>
						new ProductInstanceStatus
						{
							IdentificationCode = x.RequestedCis,
							Status = GetStatus(x.Status)
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
		}*/

		private ProductInstanceStatusEnum GetStatus(string statusName)
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
					throw new InvalidOperationException($"Не известный статус экземпляра продукта: {statusName}");
			}
		}
	}
}
