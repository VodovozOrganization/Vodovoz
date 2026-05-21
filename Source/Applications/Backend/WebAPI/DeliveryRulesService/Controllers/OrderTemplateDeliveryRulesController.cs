using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeliveryRulesService.Cache;
using DeliveryRulesService.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using VodovozBusiness.Services.Sale;

namespace DeliveryRulesService.Controllers
{
	[ApiController]
	[Route("DeliveryRules/[action]")]
	public class OrderTemplateDeliveryRulesController : DistrictController
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public OrderTemplateDeliveryRulesController(
			ILogger<OrderTemplateDeliveryRulesController> logger,
			IUnitOfWorkFactory uowFactory,
			IDeliveryRepository deliveryRepository,
			DistrictCacheService districtCacheService,
			IDistrictRulesService districtRulesService) : base(logger, deliveryRepository, districtCacheService, districtRulesService)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}
		
		[HttpGet]
		public async Task<IActionResult> GetOrderTemplateDeliveryRulesAsync(
			decimal latitude,
			decimal longitude,
			CancellationToken cancellationToken)
		{
			Logger.LogInformation(
				"Поступил запрос на получение правил доставки для шаблона автозаказа. Широта {Latitude}, долгота {Longitude}",
				latitude,
				longitude);

			try
			{
				var date = DateTime.Now;
				using var uow = _uowFactory.CreateWithoutRoot();
				var district = await GetDistrictAsync(uow, latitude, longitude, cancellationToken);

				if(district != null)
				{
					Logger.LogInformation("Получен район {District}. Заполняем данные для шаблона автозаказа", district.DistrictName);
					
					var orderTemplateScheduleRestrictions = (from WeekDayName weekDay in Enum.GetValues(typeof(WeekDayName))
						where weekDay != WeekDayName.Today
						let scheduleRestrictions = DistrictRulesService.GetScheduleRestrictions(district, weekDay, date, false)
						let extendedScheduleRestrictions = DistrictRulesService.ReorderScheduleRestrictions(scheduleRestrictions)
							.Select(x => new ExtendedScheduleRestrictionDto
							{
								Id = x.Id,
								ScheduleRestriction = x.Name
							})
							.ToList()
						select OrderTemplateDeliveryRuleDto.Create(weekDay, extendedScheduleRestrictions)).ToList();

					return Ok(OrderTemplateDeliveryRulesDto.Create(orderTemplateScheduleRestrictions));
				}
			}
			catch(Exception e)
			{
				Logger.LogError(e, "Произошла ошибка при получении правил доставки для шаблона автозаказа");
				return Problem("Неизвестная ошибка, пожалуйста, попробуйте позднее", statusCode: 500);
			}

			return Problem("Неизвестный район доставки", statusCode: 404);
		}
	}
}
