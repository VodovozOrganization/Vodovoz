using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace CustomerOrdersApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RecomendationsController : SignatureControllerBase
	{
		private readonly ICustomerOrdersService _customerOrdersService;

		public RecomendationsController(
			ILogger<RecomendationsController> logger,
			ICustomerOrdersService customerOrdersService)
			: base(logger)
		{
			_customerOrdersService = customerOrdersService
				?? throw new ArgumentNullException(nameof(customerOrdersService));
		}

		[HttpGet]
		public async Task<IActionResult> GetRecommendations(GetRecomendationsDto getRecomendationsDto, CancellationToken cancellationToken)
		{
			var sourceName = getRecomendationsDto.Source.GetEnumTitle();

			try
			{
				Logger.LogInformation(
					"Поступил запрос от {Source} на создание заявки на звонок c подписью {Signature}, проверяем...",
					sourceName,
					getRecomendationsDto.Signature);

				if(!_customerOrdersService.ValidateRequestRecomendationsSignature(getRecomendationsDto, out var generatedSignature))
				{
					return InvalidSignature(getRecomendationsDto.Signature, generatedSignature);
				}

				Logger.LogInformation("Подпись валидна, получаем рекомендации");

				return (await _customerOrdersService.GetRecomendations(getRecomendationsDto, cancellationToken))
					.Match<IEnumerable<RecomendationItemDto>, Exception, IActionResult>(
						recomendations => Ok(recomendations),
						error =>
						{
							Logger.LogError(error, "Ошибка при запросе рекомендаций: {Message}",
								error.Message);

							return Problem(error.Message);
						});
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при сохранении заявки на звонок контакта {Phone} от {ErpCounterpartyId}",
					getRecomendationsDto.ErpCounterpartyId,
					sourceName);

				return Problem();
			}
		}
	}
}
