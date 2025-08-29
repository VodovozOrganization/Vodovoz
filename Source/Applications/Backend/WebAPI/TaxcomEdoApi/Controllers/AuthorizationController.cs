using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TaxcomEdoApi.Controllers
{
	public class AuthorizationController : ControllerBase
	{
		private readonly ILogger<AuthorizationController> _logger;

		public AuthorizationController(ILogger<AuthorizationController> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpGet]
		public IActionResult Login()
		{
			try
			{
				var response = _client.Login();
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при попытке регистрации");
				return Problem();
			}
		}
		
		[HttpPost]
		public IActionResult CertificateLogin()
		{
			try
			{
				var response = _client.CertificateLogin();
				return response;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Произошла ошибка при попытке получения токена");
				return Problem();
			}
		}
	}
}
