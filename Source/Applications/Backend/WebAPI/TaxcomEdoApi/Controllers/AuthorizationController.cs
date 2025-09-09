using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Services;

namespace TaxcomEdoApi.Controllers
{
	public class AuthorizationController : ControllerBase
	{
		private readonly ILogger<AuthorizationController> _logger;
		private readonly IEdoAuthorizationService _authorizationService;
		private readonly X509Certificate2 _certificate;
		private readonly TaxcomEdoApiOptions _apiOptions;

		//Добавить авторизацию
		public AuthorizationController(
			ILogger<AuthorizationController> logger,
			IEdoAuthorizationService authorizationService,
			X509Certificate2 certificate,
			IOptions<TaxcomEdoApiOptions> apiOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			_certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
			_apiOptions = (apiOptions ?? throw new ArgumentNullException(nameof(apiOptions))).Value;
		}

		[HttpGet]
		public async Task<IActionResult> Login()
		{
			try
			{
				var response = await _authorizationService.Login(_apiOptions.Login, _apiOptions.Password);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при попытке получения токена");
				return Problem();
			}
		}
		
		[HttpPost]
		public async Task<IActionResult> CertificateLogin()
		{
			try
			{
				var response = await _authorizationService.CertificateLogin(_certificate.RawData);
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при попытке получения токена");
				return Problem();
			}
		}
	}
}
