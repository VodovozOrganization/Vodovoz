using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PayPageAPI.Models;
using System;
using System.Diagnostics;

namespace PayPageAPI.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IAvangardFastPaymentModel _avangardFastPaymentModel;
		private readonly IPayViewModelFactory _payViewModelFactory;

		public HomeController(
			ILogger<HomeController> logger,
			IAvangardFastPaymentModel avangardFastPaymentModel,
			IPayViewModelFactory payViewModelFactory)
		{
			_logger = logger;
			_avangardFastPaymentModel = avangardFastPaymentModel ?? throw new ArgumentNullException(nameof(avangardFastPaymentModel));
			_payViewModelFactory = payViewModelFactory ?? throw new ArgumentNullException(nameof(payViewModelFactory));
		}

		[Route("~/{fastPaymentGuid:guid}")]
		[HttpGet]
		public IActionResult Index(Guid fastPaymentGuid)
		{
			_logger.LogInformation("Поступил запрос на открытие главной страницы");

			var fastPayment = _avangardFastPaymentModel.GetFastPaymentByGuid(fastPaymentGuid);

			if(fastPayment == null)
			{
				_logger.LogError($"Запрос пришел с несуществующим платежом: guid {fastPaymentGuid}");
				return new NotFoundResult();
			}
			
			_logger.LogInformation($"Загружаем страницу с сессией: {fastPayment.Ticket}");
			return View(_payViewModelFactory.CreateNewPayViewModel(fastPayment));
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
