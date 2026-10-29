using BitrixApi.Contracts.Dto.Responses;
using BitrixApi.Library.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Errors.Clients;
using Vodovoz.Errors.Orders;

namespace BitrixApi.Controllers.V1
{
	/// <summary>
	/// Контроллер заказов
	/// </summary>
	public class OrdersController : VersionedController
	{
		private readonly IOrdersService _ordersService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="ordersService">Сервис получения заказов</param>
		public OrdersController(
			ILogger<OrdersController> logger,
			IOrdersService ordersService)
			: base(logger)
		{
			_ordersService = ordersService ?? throw new ArgumentNullException(nameof(ordersService));
		}

		/// <summary>
		/// Получение номеров заказов контрагента по номеру телефона,
		/// созданных начиная с указанной даты и не находящихся в отмененных статусах
		/// </summary>
		/// <param name="phone">Номер телефона в формате 7XXXXXXXXXX</param>
		/// <param name="startDate">Дата, начиная с которой ищутся заказы (по дате создания заказа)</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Dto ответа с идентификаторами найденных заказов через запятую</returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetOrdersResponse))]
		public async Task<IActionResult> GetOrders(
			[FromQuery, Required, RegularExpression(@"^7\d{10}$",
				ErrorMessage = "Номер телефона должен быть в формате 7XXXXXXXXXX")] string phone,
			[FromQuery, Required] DateTime startDate,
			CancellationToken cancellationToken)
		{
			try
			{
				_logger.LogInformation(
					"Поступил запрос заказов. Phone: {Phone}. StartDate: {StartDate}",
					phone,
					startDate);

				var result = await _ordersService.GetOrdersByPhoneNumberFromDate(phone, startDate, cancellationToken);

				return MapResult(result, result =>
				{
					if(result.IsSuccess)
					{
						return StatusCodes.Status200OK;
					}

					var firstError = result.Errors.First();

					if(firstError == CounterpartyErrors.NotFound)
					{
						return StatusCodes.Status404NotFound;
					}

					if(firstError == OrderErrors.NotFoundByPhoneAndStartDate)
					{
						return StatusCodes.Status404NotFound;
					}

					return StatusCodes.Status400BadRequest;
				});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении заказов: {ExceptionMessage}", ex.Message);

				return Problem(
					ex.Message,
					statusCode: StatusCodes.Status500InternalServerError,
					title: "Произошла ошибка при получении заказов",
					instance: Request.Path);
			}
		}
	}
}
