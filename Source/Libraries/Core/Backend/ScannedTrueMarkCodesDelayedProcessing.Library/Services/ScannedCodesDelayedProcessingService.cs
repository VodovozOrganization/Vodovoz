using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Services.TrueMark;

namespace ScannedTrueMarkCodesDelayedProcessing.Library.Services
{
	public class ScannedCodesDelayedProcessingService
	{
		private readonly ILogger<ScannedCodesDelayedProcessingService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListItemTrueMarkProductCodesProcessingService _routeListItemTrueMarkProductCodesProcessingService;
		private readonly IGenericRepository<DriversScannedTrueMarkCode> _driversScannedCodesRepository;
		private readonly IGenericRepository<RouteListItemEntity> _routeListItemRepostory;
		private readonly IGenericRepository<OrderItemEntity> _orderItemRepository;
		private readonly IGenericRepository<OrderEntity> _orderEntityRepository;
		private readonly IGenericRepository<OrderEdoRequest> _orderEdoRequestRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly MessageService _messageService;

		public ScannedCodesDelayedProcessingService(
			ILogger<ScannedCodesDelayedProcessingService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListItemTrueMarkProductCodesProcessingService routeListItemTrueMarkProductCodesProcessingService,
			IGenericRepository<DriversScannedTrueMarkCode> driversScannedCodesRepository,
			IGenericRepository<RouteListItemEntity> routeListItemRepostory,
			IGenericRepository<OrderItemEntity> orderItemRepository,
			IGenericRepository<OrderEntity> orderEntityRepository,
			IGenericRepository<OrderEdoRequest> orderEdoRequestRepository,
			IOrderRepository orderRepository,
			MessageService messageService)
		{
			_logger =
				logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWorkFactory =
				unitOfWorkFactory ?? throw new System.ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListItemTrueMarkProductCodesProcessingService =
				routeListItemTrueMarkProductCodesProcessingService ?? throw new System.ArgumentNullException(nameof(routeListItemTrueMarkProductCodesProcessingService));
			_driversScannedCodesRepository =
				driversScannedCodesRepository ?? throw new System.ArgumentNullException(nameof(driversScannedCodesRepository));
			_routeListItemRepostory =
				routeListItemRepostory ?? throw new System.ArgumentNullException(nameof(routeListItemRepostory));
			_orderItemRepository =
				orderItemRepository ?? throw new System.ArgumentNullException(nameof(orderItemRepository));
			_orderEntityRepository =
				orderEntityRepository ?? throw new ArgumentNullException(nameof(orderEntityRepository));
			_orderEdoRequestRepository =
				orderEdoRequestRepository ?? throw new ArgumentNullException(nameof(orderEdoRequestRepository));
			_orderRepository =
				orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_messageService =
				messageService ?? throw new ArgumentNullException(nameof(messageService));
		}

		public async Task ProcessScannedCodesAsync(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ScannedCodesDelayedProcessingService)))
			{
				var scannedCodes = _driversScannedCodesRepository
					.Get(uow, x => x.IsProcessingCompleted == false)
					.ToArray();

				var routeListAddresses = _routeListItemRepostory
					.Get(uow, x => scannedCodes.Select(y => y.RouteListAddressId).Contains(x.Id))
					.ToArray();

				var orderItems = _orderItemRepository
					.Get(uow, x => scannedCodes.Select(y => y.OrderItemId).Contains(x.Id))
					.ToArray();

				var orders = _orderEntityRepository
					.Get(uow, x => orderItems.Select(y => y.Order.Id).Contains(x.Id))
					.ToArray();

				var edoRequests = _orderEdoRequestRepository
					.Get(uow, x => orders.Select(y => y.Id).Contains(x.Order.Id))
					.ToArray();

				var scannedCodesData = _driversScannedCodesRepository
					.Get(uow, x => x.IsProcessingCompleted == false)
					.GroupBy(x => new { x.OrderItemId, x.RouteListAddressId })
					.ToDictionary(x => x.Key, x => x.ToList());

				foreach(var codesData in scannedCodesData)
				{
					var orderItemId = codesData.Key.OrderItemId;

					var routeListAddress =
						routeListAddresses
						.FirstOrDefault(x => x.Id == codesData.Key.RouteListAddressId)
						?? throw new Exception($"Не найдена строка маршрутного листа с ID {codesData.Key.RouteListAddressId}");

					var bottlesCodes = codesData.Value
						.Where(x => !x.IsDefective)
						.ToList();

					var defectiveCodes = codesData.Value
						.Where(x => x.IsDefective)
						.ToList();

					var addBottlesCodesResult =
						await _routeListItemTrueMarkProductCodesProcessingService.AddProductCodesToRouteListItemNoCodeStatusCheck(
							uow,
							routeListAddress,
							orderItemId,
							bottlesCodes.Select(x => x.RawCode),
							SourceProductCodeStatus.New,
							ProductCodeProblem.None);

					if(addBottlesCodesResult.IsSuccess)
					{
						foreach(var code in bottlesCodes)
						{
							code.IsProcessingCompleted = true;
							await uow.SaveAsync(code, cancellationToken: cancellationToken);
						}
					}

					var addDefectiveCodesResult =
						await _routeListItemTrueMarkProductCodesProcessingService.AddProductCodesToRouteListItemNoCodeStatusCheck(
							uow,
							routeListAddress,
							orderItemId,
							defectiveCodes.Select(x => x.RawCode),
							SourceProductCodeStatus.Problem,
							ProductCodeProblem.Defect);

					if(addDefectiveCodesResult.IsSuccess)
					{
						foreach(var code in defectiveCodes)
						{
							code.IsProcessingCompleted = true;
							await uow.SaveAsync(code, cancellationToken: cancellationToken);
						}
					}
				}

				var newEdoRequests = new List<OrderEdoRequest>();

				foreach(var order in orders)
				{
					var isAllDriversScannedCodesInOrderProcessed =
						await _orderRepository.IsAllDriversScannedCodesInOrderProcessed(uow, order.Id, cancellationToken);

					if(isAllDriversScannedCodesInOrderProcessed && !edoRequests.Any(x => x.Order.Id == order.Id && x.DocumentType == EdoDocumentType.UPD))
					{
						var orderRouteListItems = routeListAddresses
							.Where(x => x.Order.Id == order.Id)
							.FirstOrDefault();

						var edoRequest = CreateEdoRequests(uow, order, orderRouteListItems);

						newEdoRequests.Add(edoRequest);
					}
				}

				await uow.CommitAsync(cancellationToken);

				foreach(var newEdoRequest in newEdoRequests)
				{
					await _messageService.PublishEdoRequestCreatedEvent(newEdoRequest.Id);
				}
			}
		}

		private OrderEdoRequest CreateEdoRequests(IUnitOfWork uow, OrderEntity order, RouteListItemEntity routeListAddress)
		{
			var edoRequest = new OrderEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Driver,
				DocumentType = EdoDocumentType.UPD,
				Order = order,
			};

			foreach(var code in routeListAddress.TrueMarkCodes)
			{
				edoRequest.ProductCodes.Add(code);
			}

			uow.Save(edoRequest);

			return edoRequest;
		}
	}
}
