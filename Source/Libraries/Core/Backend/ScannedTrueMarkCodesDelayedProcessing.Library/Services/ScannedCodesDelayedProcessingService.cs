using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using ScannedTrueMarkCodesDelayedProcessing.Library.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Results;
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

				_logger.LogInformation("Обработка отсканированных кодов ЧЗ, количество: {Count}", scannedCodesData.Count());

				await AddScannedCodesToRouteListItems(uow, scannedCodesData, cancellationToken);

				var newEdoRequests = await CreateEdoRequests(uow, scannedCodesData, cancellationToken);

				if(uow.HasChanges)
				{
					await uow.CommitAsync(cancellationToken);
				}

				foreach(var newEdoRequest in newEdoRequests)
				{
					await _messageService.PublishEdoRequestCreatedEvent(newEdoRequest.Id);
				}

				_logger.LogInformation("Обработка отсканированных кодов ЧЗ завершена");
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
				var bottlesCodes = routeListItemScannedCode.Value
				.Where(x => !x.IsDefective)
				.ToList();

				var addBottlesCodesResult = await AddBottlesCodesToRouteListItem(
					routeListItemScannedCode.Key.RouteListAddress.Id,
					routeListItemScannedCode.Key.OrderItem.Id,
					bottlesCodes.Select(x => x.RawCode),
					cancellationToken);
				
				var defectiveCodes = routeListItemScannedCode.Value
					.Where(x => x.IsDefective)
					.ToList();

				var addDefectiveCodesResult = await AddDefectiveCodesToRouteListItem(
					routeListItemScannedCode.Key.RouteListAddress.Id,
					routeListItemScannedCode.Key.OrderItem.Id,
					defectiveCodes.Select(x => x.RawCode),
					cancellationToken);

				foreach(var code in bottlesCodes)
				{
					code.IsProcessingCompleted = addBottlesCodesResult.IsSuccess;
					code.IsProcessingError = addBottlesCodesResult.IsFailure;
					await uow.SaveAsync(code, cancellationToken: cancellationToken);
				}

				foreach(var code in defectiveCodes)
				{
					code.IsProcessingCompleted = addDefectiveCodesResult.IsSuccess;
					code.IsProcessingError = addDefectiveCodesResult.IsFailure;
					await uow.SaveAsync(code, cancellationToken: cancellationToken);
				}
			}
		}

		private async Task<Result> AddBottlesCodesToRouteListItem(
			int routeListAddressId,
			int orderItemId,
			IEnumerable<string> scannedCodes,
			CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(AddBottlesCodesToRouteListItem)))
			{
				try
				{
					var routeListAddress = uow.GetById<RouteListItemEntity>(routeListAddressId);

					var addCodesResult =
						await _routeListItemTrueMarkProductCodesProcessingService.AddProductCodesToRouteListItemNoCodeStatusCheck(
							uow,
							routeListAddress,
							orderItemId,
							scannedCodes,
							SourceProductCodeStatus.New,
							ProductCodeProblem.None);

					if(addCodesResult.IsSuccess)
					{
						await uow.CommitAsync(cancellationToken);
					}

					return addCodesResult;
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						"Ошибка при добавлении кодов бутылей к адресу маршрутного листа {RouteListAddressId}, строка закза {OrderItemId}",
						routeListAddressId,
						orderItemId);

					return Result.Failure(DriversScannedCodes.AddingCodeToRouteListAddressError);
				}
			}
		}

		private async Task<Result> AddDefectiveCodesToRouteListItem(
			int routeListAddressId,
			int orderItemId,
			IEnumerable<string> scannedCodes,
			CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(AddBottlesCodesToRouteListItem)))
			{
				try
				{
					var routeListAddress = uow.GetById<RouteListItemEntity>(routeListAddressId);

					var addCodesResult =
						await _routeListItemTrueMarkProductCodesProcessingService.AddProductCodesToRouteListItemNoCodeStatusCheck(
							uow,
							routeListAddress,
							orderItemId,
							scannedCodes,
							SourceProductCodeStatus.Problem,
							ProductCodeProblem.Defect);

					if(addCodesResult.IsSuccess)
					{
						await uow.CommitAsync(cancellationToken);
					}

					return addCodesResult;
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						"Ошибка при добавлении кодов дефектных бутылей к адресу маршрутного листа {RouteListAddressId}, строка закза {OrderItemId}",
						routeListAddressId,
						orderItemId);

					return Result.Failure(DriversScannedCodes.AddingCodeToRouteListAddressError);
				}
			}
		}

		private async Task<IEnumerable<OrderEdoRequest>> CreateEdoRequests(IUnitOfWork uow,
			IEnumerable<DriversScannedCodeDataNode> scannedCodesData,
			CancellationToken cancellationToken)
		{
			var routeListAddresses = scannedCodesData
				.Select(x => x.RouteListAddress)
				.Distinct()
				.ToList();

			var newEdoRequests = new List<OrderEdoRequest>();

			foreach(var routeListAddress in routeListAddresses)
			{
				var orderScannedCodes = scannedCodesData
					.Where(x => x.Order.Id == routeListAddress.Order.Id)
					.Select(x => x.DriversScannedCode)
					.ToList();

				newEdoRequests.AddRange(
					await CreateEdoRequests(uow, routeListAddress, orderScannedCodes, cancellationToken));
			}

			return newEdoRequests;
		}

		private async Task<IEnumerable<OrderEdoRequest>> CreateEdoRequests(
			IUnitOfWork uow,
			RouteListItemEntity routeListAddress,
			IEnumerable<DriversScannedTrueMarkCode> orderDriversScannedCodes,
			CancellationToken cancellationToken)
		{
			var newEdoRequests = new List<OrderEdoRequest>();
			var order = routeListAddress.Order;

			var isAllDriversScannedCodesInOrderProcessed =
					orderDriversScannedCodes.All(x => x.IsProcessingCompleted);

			var existingEdoRequests = await _edoDocflowRepository
				.GetOrderEdoRequestsByOrderId(uow, order.Id, cancellationToken);

			var isOrderEdoRequestExists = existingEdoRequests
				.Any(x => x.Order.Id == order.Id && x.DocumentType == EdoDocumentType.UPD);

			var isOrderOnClosingStatus = _orderRepository.GetOnClosingOrderStatuses().Contains(order.OrderStatus);

			if(isAllDriversScannedCodesInOrderProcessed
				&& !isOrderEdoRequestExists
				&& isOrderOnClosingStatus)
			{
				_logger.LogInformation(
					"Создание заявки на ЭДО для заказа {OrderId}, адрес {RouteListAddressId}",
					routeListAddress.Order.Id,
					routeListAddress.Id);

				var edoRequest = CreateEdoRequest(uow, routeListAddress);

				newEdoRequests.Add(edoRequest);
			}

			return newEdoRequests;
		}

		private OrderEdoRequest CreateEdoRequest(IUnitOfWork uow, RouteListItemEntity routeListAddress)
		{
			var edoRequest = new OrderEdoRequest
			{
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Driver,
				DocumentType = EdoDocumentType.UPD,
				Order = routeListAddress.Order,
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
