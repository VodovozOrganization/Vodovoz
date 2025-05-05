using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;
using WarehouseApi.Contracts.Dto;
using WarehouseApi.Library.Extensions;

namespace WarehouseApi.Library.Services
{
	public sealed class OrderService
	{
		private ILogger<OrderService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGenericRepository<Order> _orderRepository;

		public OrderService(
			ILogger<OrderService> logger,
			IUnitOfWork unitOfWork)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public Result<OrderDto> GetOrder(int id)
		{
			var order = _orderRepository
				.Get(
					_unitOfWork,
					x => x.Id == id,
					1)
				.FirstOrDefault();

			if(order is null)
			{
				_logger.LogWarning($"Заказ с id {id} не найден.");
				return Vodovoz.Errors.Orders.Order.NotFound;
			}

			var nomenclatures = order.OrderItems
				.Select(x => x.Nomenclature)
				.ToArray();

			var dto = order.ToApiDto(nomenclatures);

			if(order.SelfDelivery)
			{
				dto.DocType = DocumentSourceType.Invoice;
			}
			else
			{
				dto.DocType = DocumentSourceType.CarLoadDocument;
			}

			return dto;
		}
	}
}
