using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.RobotMia.Api.Exceptions;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Requests.V1;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер заказов
	/// </summary>
	public class OrderController : VersionedController
	{
		private readonly IOrderService _orderService;
		private readonly IIncomingCallCallService _incomingCallService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="orderService"></param>
		/// <param name="incomingCallService"></param>
		public OrderController(
			ILogger<ApiControllerBase> logger,
			IOrderService orderService,
			IIncomingCallCallService incomingCallService)
			: base(logger)
		{
			_orderService = orderService
				?? throw new ArgumentNullException(nameof(orderService));
			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
		}

		/// <summary>
		/// Создание заказа
		/// </summary>
		/// <param name="postOrderRequest"></param>
		/// <param name="unitOfWork"></param>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> PostAsync(
			CreateOrderRequest postOrderRequest,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallService.GetCallByIdAsync(postOrderRequest.CallId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {postOrderRequest.CallId}", statusCode: StatusCodes.Status400BadRequest);
			}

			if(unitOfWork.GetById<Counterparty>(postOrderRequest.CounterpartyId) is not Counterparty counterparty)
			{
				return Problem($"Должен быть указан идентификатор контрагента, указано значение: {postOrderRequest.CounterpartyId}", statusCode: StatusCodes.Status400BadRequest);
			}

			if(unitOfWork.GetById<DeliveryPoint>(postOrderRequest.DeliveryPointId) is not DeliveryPoint deliveryPoint
				|| deliveryPoint.Counterparty.Id != postOrderRequest.CounterpartyId)
			{
				return Problem($"Должен быть указан идентификатор существующей точки доставки указанного контрагента, указано значение: {postOrderRequest.DeliveryPointId}", statusCode: StatusCodes.Status400BadRequest);
			}

			if(postOrderRequest.DeliveryDate is null
				|| postOrderRequest.DeliveryIntervalId is null
				|| counterparty.PersonType == Core.Domain.Clients.PersonType.legal && postOrderRequest.SignatureType is null
				|| postOrderRequest.ContactPhone is null
				|| postOrderRequest.PaymentType is null
				|| postOrderRequest.CallBeforeArrivalMinutes is null
				|| postOrderRequest.BottlesReturn is null
				|| !postOrderRequest.SaleItems.Any())
			{
				try
				{
					var incompleteOrderResult = await _orderService.CreateIncompleteOrderAsync(postOrderRequest);

					if(incompleteOrderResult.IsSuccess)
					{
						call.CreatedOrderId = incompleteOrderResult.Value;

						unitOfWork.Save(call);
						unitOfWork.Commit();
						return NoContent();
					}

					var errorsCombined = string.Join(", ", incompleteOrderResult.Errors.Select(x => x.Message));

					_logger.LogError("Ошибка создания незавершенного заказа: {Errors}", errorsCombined);
					return Problem("Не удалось создать незавершенный заказ", statusCode: StatusCodes.Status400BadRequest);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка создания незавершенного заказа: {Message}", ex.Message);
					return Problem("Не удалось создать незавершенный заказ", statusCode: StatusCodes.Status400BadRequest);
				}
			}

			try
			{
				var createdOrderResult = await _orderService.CreateAndAcceptOrderAsync(postOrderRequest);

				if(createdOrderResult.IsFailure)
				{
					_logger.LogError("Ошибка создания заказа: {Errors}", string.Join(", ", createdOrderResult.Errors.Select(x => x.Message)));
					return Problem(string.Join(", ", createdOrderResult.Errors.Select(x => x.Message)), statusCode: StatusCodes.Status400BadRequest);
				}

				var createdOrderId = createdOrderResult.Value;

				_logger.LogInformation("Создан заказ #{OrderId}", createdOrderId);

				call.CreatedOrderId = createdOrderId;

				unitOfWork.Save(call);
				unitOfWork.Commit();
				return NoContent();
			}
			catch(NomenclatureNotFoundException nnfe)
			{
				_logger.LogError(nnfe, "Ошибка создания заказа: {Message}", nnfe.Message);

				return Problem($"Не удалось создать заказ: {nnfe.Message}", statusCode: StatusCodes.Status400BadRequest);
			}
			catch(NomenclatureSaleUnavailableException nsue)
			{
				_logger.LogError(nsue, "Ошибка создания заказа: {Message}", nsue.Message);

				return Problem($"Не удалось создать заказ: {nsue.Message}", statusCode: StatusCodes.Status400BadRequest);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка создания заказа: {Message}", ex.Message);

				return Problem("Не удалось создать заказ", statusCode: StatusCodes.Status400BadRequest);
			}
		}

		/// <summary>
		/// Вычисление цены заказа
		/// </summary>
		/// <param name="calculatePriceRequest"></param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpPost("CalculatePrice")]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CalculatePriceResponse))]
		public async Task<IActionResult> CalculatePriceAsync(
			CalculatePriceRequest calculatePriceRequest,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallService.GetCallByIdAsync(calculatePriceRequest.CallId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {calculatePriceRequest.CallId}", statusCode: StatusCodes.Status400BadRequest);
			}

			return _orderService
				.GetOrderAndDeliveryPrices(calculatePriceRequest)
				.Match<CalculatePriceResponse, IActionResult>(
					response => Ok(response),
					errors => Problem(string.Join(", ", errors.Select(x => x.Message)), statusCode: StatusCodes.Status400BadRequest));
		}
	}
}
