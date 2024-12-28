using CryptoPro.Security.Cryptography;
using CryptoPro.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;
using TaxcomEdo.Api.Dto;

namespace TaxcomEdo.Api.Controllers;

[ApiController]
[Route("api/[action]")]

public class TaxcomEdoApiController : ControllerBase
{
	#region вынести в настройки

	// TODO Через композ замапить каталог с сертификатами
	private const string _testCertPath = "/etc/vodovoz/certs/M...shi.pfx";
	private const string _testCertPassword = "";

	private const string _integratorID = "Fun...._.......-....-....-....-........B4E4";

	#endregion вынести в настройки

	private readonly ILogger<TaxcomEdoApiController> _logger;
	private readonly CpX509Certificate2 _cert;
	private HttpClient _httpClient;

	public TaxcomEdoApiController(
		IHttpClientFactory httpClientFactory,
		ILogger<TaxcomEdoApiController> logger)
	{
		if(httpClientFactory is null)
		{
			throw new ArgumentNullException(nameof(httpClientFactory));
		}

		_httpClient = httpClientFactory.CreateClient();
		_httpClient.BaseAddress = new Uri("https://api.taxcom.ru/v1.3/");

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		_cert = new CpX509Certificate2(_testCertPath, _testCertPassword, X509KeyStorageFlags.EphemeralKeySet);
	}

	private byte[] DecryptMsg(byte[] encodedEnvelopedCms, CpX509Certificate2 cert)
	{
		var envelopedCms = new CpEnvelopedCms();
		envelopedCms.Decode(encodedEnvelopedCms);
		envelopedCms.Decrypt(new CpX509Certificate2Collection(cert));

		return envelopedCms.ContentInfo.Content;
	}

	[HttpGet]
	public async Task<string> Login()
	{
		var rawCertData = _cert.GetRawCertData();

		var content = new ByteArrayContent(rawCertData);

		_httpClient.DefaultRequestHeaders.Clear();
		_httpClient.DefaultRequestHeaders.Add("Integrator-Id", _integratorID);
		_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pkcs7-mime"));

		var responseMessage = await _httpClient.PostAsync("API/CertificateLogin", content);

		// действует 5 мин, реализовать кеш на 5 мин
		var encryptedToken = await responseMessage.Content.ReadAsByteArrayAsync();

		var tokenRaw = DecryptMsg(encryptedToken, _cert);

		var token = Encoding.UTF8.GetString(tokenRaw);

		return token;
	}

	private void PrepareHttpClient(string token)
	{
		_httpClient.DefaultRequestHeaders.Clear();
		_httpClient.DefaultRequestHeaders.Add("Assistant-Key", token);
		_httpClient.DefaultRequestHeaders.Add("Integrator-Id", _integratorID);
		_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pkcs7-mime"));
	}

	[HttpGet]
	public async Task<IActionResult> GetContactListUpdates(string token, DateTime? lastCheckContactsUpdates, EdoContactStateCode? contactState)
	{
		PrepareHttpClient(token);

		string url = $"API/GetContactListUpdates?date={lastCheckContactsUpdates}&status={contactState}";
		var contactUpdatesRaw = await _httpClient.GetStreamAsync(url);
		var contactListSerializer = new XmlSerializer(typeof(ContactList));
		var contactUpdates = contactListSerializer.Deserialize(contactUpdatesRaw) as ContactList;

		return Ok(contactUpdates);
	}
}
