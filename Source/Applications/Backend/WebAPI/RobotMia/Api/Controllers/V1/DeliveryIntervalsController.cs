using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.RobotMia.Api.Extensions.Mapping;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер интервалов доставки
	/// </summary>
	public class DeliveryIntervalsController : VersionedController
	{
		private readonly IGenericRepository<DeliveryPoint> _deliveryPointRepository;
		private readonly IIncomingCallCallService _incomingCallService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="deliveryPointRepository"></param>
		/// <param name="districtRepository"></param>
		/// <param name="incomingCallService"></param>
		public DeliveryIntervalsController(
			ILogger<ApiControllerBase> logger,
			IGenericRepository<DeliveryPoint> deliveryPointRepository,
			IGenericRepository<District> districtRepository,
			IIncomingCallCallService incomingCallService)
			: base(logger)
		{
			_deliveryPointRepository = deliveryPointRepository
				?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
		}

		/// <summary>
		/// Получение доступных интервалов доставки
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <param name="deliveryPointId">Идентификатор точки доставки</param>
		/// <param name="deliveryDate">Дата доставки</param>
		/// <param name="nomenclatureIds"></param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DeliveryIntervalDto>))]
		public async Task<IActionResult> GetAsync(
			[FromQuery(Name = "call_id"), Required] Guid callId,
			[FromQuery(Name = "delivery_point_id"), Required] int deliveryPointId,
			[FromQuery(Name = "delivery_date")] DateTime? deliveryDate,
			[FromQuery(Name = "nomenclature_ids"), Required] IEnumerable<int> nomenclatureIds,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallService.GetCallByIdAsync(callId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {callId}", statusCode: StatusCodes.Status400BadRequest);
			}

			District district = null;

			var availableDeliverySchedules = new List<DeliverySchedule>();

			district = _deliveryPointRepository
				.Get(unitOfWork, dp => dp.Id == deliveryPointId)
				.FirstOrDefault()
				?.District;

			if(district is null)
			{
				return Problem("Не удается определить принадлежность точки доставки району доставки", statusCode: StatusCodes.Status404NotFound);
			}

			var schedules = new List<DeliveryIntervalDto>();

			if(deliveryDate is null)
			{
				var startDate = DateTime.Today;

				var endDate = DateTime.Today.AddDays(6);

				for(var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
				{
					schedules.AddRange(district
						.GetAvailableDeliveryScheduleRestrictionsByDeliveryDate(currentDate)
						.OrderBy(s => s.DeliverySchedule.DeliveryTime)
						.Select(r => r.DeliverySchedule.MapToDeliveryIntervalDtoV1(currentDate)));
				}
			}
			else
			{
				schedules.AddRange(district
					.GetAvailableDeliveryScheduleRestrictionsByDeliveryDate(deliveryDate.Value)
					.OrderBy(s => s.DeliverySchedule.DeliveryTime)
					.Select(r => r.DeliverySchedule.MapToDeliveryIntervalDtoV1(deliveryDate.Value)));

			}

			return Ok(schedules);
		}
	}
}
