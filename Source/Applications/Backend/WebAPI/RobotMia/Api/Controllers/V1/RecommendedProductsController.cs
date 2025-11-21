using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods.Recomendations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Requests.V1;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер рекомендованных товаров
	/// </summary>
	public class RecomendationsController : VersionedController
	{
		private readonly IIncomingCallCallService _incomingCallService;
		private readonly IRecomendationService _recomendationService;
		private readonly IGenericRepository<Counterparty> _counterpartyRepository;
		private readonly IGenericRepository<DeliveryPoint> _deliveryPointRepository;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="incomingCallService"></param>
		/// <param name="recomendationService"></param>
		/// <param name="counterpartyrepository"></param>
		/// <param name="deliveryPointRepository"></param>
		public RecomendationsController(
			ILogger<RecomendationsController> logger,
			IIncomingCallCallService incomingCallService,
			IRecomendationService recomendationService,
			IGenericRepository<Counterparty> counterpartyrepository,
			IGenericRepository<DeliveryPoint> deliveryPointRepository)
			: base(logger)
		{

			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
			_recomendationService = recomendationService
				?? throw new ArgumentNullException(nameof(recomendationService));
			_counterpartyRepository = counterpartyrepository
				?? throw new ArgumentNullException(nameof(counterpartyrepository));
			_deliveryPointRepository = deliveryPointRepository
				?? throw new ArgumentNullException(nameof(deliveryPointRepository));
		}

		/// <summary>
		/// Получение списка рекомендованных товаров
		/// </summary>
		/// <param name="getRecomendationsRequest"></param>
		/// <param name="unitOfWork"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RecomendationItemDto>))]
		public async Task<IActionResult> GetAsync(
			GetRecomendationsRequest getRecomendationsRequest,
			[FromServices] IUnitOfWork unitOfWork,
			CancellationToken cancellationToken)
		{
			var call = await _incomingCallService.GetCallByIdAsync(getRecomendationsRequest.CallId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {getRecomendationsRequest.CallId}", statusCode: StatusCodes.Status400BadRequest);
			}

			var counterparty = _counterpartyRepository.GetFirstOrDefault(
				unitOfWork,
				x => x.Id == getRecomendationsRequest.CounterpartyId);

			if(counterparty is null)
			{
				return Problem("Контрагент не найден", statusCode: StatusCodes.Status404NotFound);
			}

			var deliveryPoint = _deliveryPointRepository.GetFirstOrDefault(
				unitOfWork,
				x => x.Id == getRecomendationsRequest.DeliveryPointId
					&& x.Counterparty.Id == getRecomendationsRequest.CounterpartyId);

			if(deliveryPoint is null)
			{
				return Problem("Точка доставки не найдена", statusCode: StatusCodes.Status404NotFound);
			}

			var recomendationtems = await _recomendationService
				.GetRecomendationItemsForRobot(
					unitOfWork,
					counterparty.PersonType,
					deliveryPoint.RoomType,
					getRecomendationsRequest.AddedNomenclatureIds ?? Enumerable.Empty<int>(),
					cancellationToken);

			return Ok(recomendationtems);
		}
	}
}
