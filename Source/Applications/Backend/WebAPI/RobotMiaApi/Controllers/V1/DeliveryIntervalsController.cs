using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Extensions.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Presentation.WebApi.Common;

namespace RobotMiaApi.Controllers.V1
{
	/// <summary>
	/// Контроллер интервалов доставки
	/// </summary>
	public class DeliveryIntervalsController : VersionedController
	{
		private readonly IGenericRepository<DeliveryPoint> _deliveryPointRepository;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="deliveryPointRepository"></param>
		/// <param name="districtRepository"></param>
		public DeliveryIntervalsController(
			ILogger<ApiControllerBase> logger,
			IGenericRepository<DeliveryPoint> deliveryPointRepository,
			IGenericRepository<District> districtRepository)
			: base(logger)
		{
			_deliveryPointRepository = deliveryPointRepository
				?? throw new ArgumentNullException(nameof(deliveryPointRepository));
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
		public IActionResult Get(
			[FromQuery(Name = "call_id"), Required] Guid callId,
			[FromQuery(Name = "delivery_point_id"), Required] int deliveryPointId,
			[FromQuery(Name = "delivery_date"), Required] DateTime deliveryDate,
			[FromQuery(Name = "nomenclature_ids"), Required] IEnumerable<int> nomenclatureIds,
			[FromServices] IUnitOfWork unitOfWork)
		{
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

			var schedules = district.GetAvailableDeliveryScheduleRestrictionsByDeliveryDate(deliveryDate)
				.OrderBy(s => s.DeliverySchedule.DeliveryTime)
				.Select(r => r.DeliverySchedule)
				.ToList();

			return Ok(schedules.MapToDeliveryIntervalDtoV1(deliveryDate));
		}
	}
}
