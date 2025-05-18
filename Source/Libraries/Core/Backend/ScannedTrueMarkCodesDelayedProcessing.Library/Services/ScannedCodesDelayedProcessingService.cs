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
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.EntityRepositories.Edo;
using VodovozBusiness.Services.TrueMark;

namespace ScannedTrueMarkCodesDelayedProcessing.Library.Services
{
	public class ScannedCodesDelayedProcessingService
	{
		private readonly ILogger<ScannedCodesDelayedProcessingService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListItemTrueMarkProductCodesProcessingService _routeListItemTrueMarkProductCodesProcessingService;
		private readonly IEdoDocflowRepository _edoDocflowRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly MessageService _messageService;

		public ScannedCodesDelayedProcessingService(
			ILogger<ScannedCodesDelayedProcessingService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListItemTrueMarkProductCodesProcessingService routeListItemTrueMarkProductCodesProcessingService,
			IEdoDocflowRepository edoDocflowRepository,
			IOrderRepository orderRepository,
			MessageService messageService)
		{
			_logger =
				logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWorkFactory =
				unitOfWorkFactory ?? throw new System.ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListItemTrueMarkProductCodesProcessingService =
				routeListItemTrueMarkProductCodesProcessingService ?? throw new System.ArgumentNullException(nameof(routeListItemTrueMarkProductCodesProcessingService));
			_edoDocflowRepository =
				edoDocflowRepository ?? throw new ArgumentNullException(nameof(edoDocflowRepository));
			_orderRepository =
				orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_messageService =
				messageService ?? throw new ArgumentNullException(nameof(messageService));
		}

		public async Task ProcessScannedCodesAsync(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ScannedCodesDelayedProcessingService)))
			{
				var scannedCodesData = await _edoDocflowRepository
					.GetAllNotProcessedDriversScannedCodesData(uow, cancellationToken);

				await AddScannedCodesToRouteListItems(uow, scannedCodesData, cancellationToken);

				await CreateEdoRequests(uow, scannedCodesData, cancellationToken);
			}
		}

		private async Task AddScannedCodesToRouteListItems(
			IUnitOfWork uow,
			IEnumerable<DriversScannedCodeDataNode> scannedCodesData,
			CancellationToken cancellationToken)
		{
			var routeListItemScannedCodes = scannedCodesData
				.GroupBy(x => new { x.RouteListAddress, x.OrderItem })
				.ToDictionary(x => x.Key, x => x.Select(c => c.DriversScannedCode).ToList());

			foreach(var routeListItemScannedCode in routeListItemScannedCodes)
			{
				await AddBottlesCodesToRouteListItems(
					uow,
					routeListItemScannedCode.Key.RouteListAddress,
					routeListItemScannedCode.Key.OrderItem.Id,
					routeListItemScannedCode.Value,
					cancellationToken);

				await AddDefectiveCodesToRouteListItems(
					uow,
					routeListItemScannedCode.Key.RouteListAddress,
					routeListItemScannedCode.Key.OrderItem.Id,
					routeListItemScannedCode.Value,
					cancellationToken);
			}
		}

		private async Task AddBottlesCodesToRouteListItems(
			IUnitOfWork uow,
			RouteListItemEntity routeListAddress,
			int orderItemId,
			IEnumerable<DriversScannedTrueMarkCode> driversScannedCodes,
			CancellationToken cancellationToken)
		{

			var bottlesCodes = driversScannedCodes
				.Where(x => !x.IsDefective)
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
		}

		private async Task AddDefectiveCodesToRouteListItems(
			IUnitOfWork uow,
			RouteListItemEntity routeListAddress,
			int orderItemId,
			IEnumerable<DriversScannedTrueMarkCode> driversScannedCodes,
			CancellationToken cancellationToken)
		{

			var defectiveCodes = driversScannedCodes
				.Where(x => x.IsDefective)
				.ToList();

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

		private async Task CreateEdoRequests(IUnitOfWork uow,
			IEnumerable<DriversScannedCodeDataNode> scannedCodesData,
			CancellationToken cancellationToken)
		{
			var routeListItemOrders = scannedCodesData.
				GroupBy(x => x.RouteListAddress)
				.ToDictionary(x => x.Key, x => x.Select(c => c.Order).Distinct().ToList());

			var newEdoRequests = new List<OrderEdoRequest>();

			foreach(var routeListItemOrder in routeListItemOrders)
			{
				newEdoRequests.AddRange(
					await CreateEdoRequests(uow, routeListItemOrder.Key, routeListItemOrder.Value, cancellationToken));
			}

			await uow.CommitAsync(cancellationToken);

			foreach(var newEdoRequest in newEdoRequests)
			{
				await _messageService.PublishEdoRequestCreatedEvent(newEdoRequest.Id);
			}
		}

		private async Task<IEnumerable<OrderEdoRequest>> CreateEdoRequests(IUnitOfWork uow, RouteListItemEntity routeListAddress, IEnumerable<OrderEntity> orders, CancellationToken cancellationToken)
		{
			var newEdoRequests = new List<OrderEdoRequest>();

			foreach(var order in orders)
			{
				var isAllDriversScannedCodesInOrderProcessed =
					await _orderRepository.IsAllDriversScannedCodesInOrderProcessed(uow, order.Id, cancellationToken);

				var existingEdoRequests = await _edoDocflowRepository
					.GetOrderEdoRequestsByOrderId(uow, order.Id, cancellationToken);

				var isOrderEdoRequestExists = existingEdoRequests
					.Any(x => x.Order.Id == order.Id && x.DocumentType == EdoDocumentType.UPD);

				var isOrderOnClosingStatus = _orderRepository.GetOnClosingOrderStatuses().Contains(order.OrderStatus);

				if(isAllDriversScannedCodesInOrderProcessed
					&& !isOrderEdoRequestExists
					&& isOrderOnClosingStatus)
				{
					var edoRequest = CreateEdoRequest(uow, order, routeListAddress);

					newEdoRequests.Add(edoRequest);
				}
			}

			return newEdoRequests;
		}

		private OrderEdoRequest CreateEdoRequest(IUnitOfWork uow, OrderEntity order, RouteListItemEntity routeListAddress)
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
