using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Получение истории заказов контрагента
	/// </summary>
	public class CounterpartyOrderHistoryService : ICounterpartyOrderHistoryService
	{
		private readonly ILogger<CounterpartyOrderHistoryService> _logger;
		private readonly ICounterpartyIdentificationQueries _queries;

		public CounterpartyOrderHistoryService(
			ILogger<CounterpartyOrderHistoryService> logger,
			ICounterpartyIdentificationQueries queries)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_queries = queries ?? throw new ArgumentNullException(nameof(queries));
		}

		/// <inheritdoc />
		public async Task<CounterpartyOrderHistoryDto> GetAsync(
			CounterpartyOrderHistoryRequestDto request,
			CancellationToken cancellationToken)
		{
			if(request == null || request.ErpCounterpartyId <= 0)
			{
				return new CounterpartyOrderHistoryDto
				{
					Status = CounterpartySearchStatus.Error,
					ErrorDescription = "Не указан идентификатор контрагента"
				};
			}

			_logger.LogInformation(
				"Запрос истории заказов контрагента {CounterpartyId}",
				request.ErpCounterpartyId);

			var counterpartyExists = await _queries.CounterpartyExistsAsync(
				request.ErpCounterpartyId,
				cancellationToken);

			if(!counterpartyExists)
			{
				return new CounterpartyOrderHistoryDto
				{
					Status = CounterpartySearchStatus.NotFound,
					ErpCounterpartyId = request.ErpCounterpartyId,
					ErrorDescription = "Контрагент не найден"
				};
			}

			var orders = await _queries.GetOrdersAsync(
				request.ErpCounterpartyId,
				cancellationToken);

			var orderDtos = new List<CounterpartyOrderDto>();

			foreach(var order in orders)
			{
				orderDtos.Add(new CounterpartyOrderDto
				{
					OrderId = order.Id,
					DeliveryDate = order.DeliveryDate,
					OrderStatus = order.OrderStatus,
					PaymentStatus = order.OrderPaymentStatus,
					Status = CounterpartyOrderStatusMapper.GetStatusName(
						order.OrderStatus,
						order.OrderPaymentStatus),
					IsCanceled = CounterpartyOrderStatusMapper.IsCanceled(order.OrderStatus),
					IsDelivered = CounterpartyOrderStatusMapper.IsDelivered(order.OrderStatus),
					IsPaid = CounterpartyOrderStatusMapper.IsPaid(order.OrderPaymentStatus)
				});
			}

			return new CounterpartyOrderHistoryDto
			{
				Status = CounterpartySearchStatus.Found,
				ErpCounterpartyId = request.ErpCounterpartyId,
				Orders = orderDtos
			};
		}
	}
}
