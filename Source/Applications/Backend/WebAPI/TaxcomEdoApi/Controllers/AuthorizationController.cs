using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaxcomEdoApi.Library.Config;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
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
		public async Task<IActionResult> Login(CancellationToken cancellationToken)
		{
			try
			{
				var response = await _authorizationService.LoginAsync(
					_apiOptions.Login,
					_apiOptions.Password,
					cancellationToken: cancellationToken);
				
				return Ok(response);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при попытке получения токена");
				return Problem();
			}
		}
		
		[HttpPost]
		public async Task<IActionResult> CertificateLogin(CancellationToken cancellationToken)
		{
			//25e358964a844d0eab9a624bb877ab7120710cf4a9c84f728de51e3c65b507d2
			try
			{
				var response = await _authorizationService.CertificateLoginAsync(_certificate.RawData, cancellationToken);
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
