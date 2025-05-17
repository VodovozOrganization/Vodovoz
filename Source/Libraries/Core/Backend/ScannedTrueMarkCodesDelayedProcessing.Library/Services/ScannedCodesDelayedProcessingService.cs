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

				var scannedCodes = scannedCodesData
					.Select(x => x.DriversScannedCode)
					.ToArray();

				var routeListAddresses = scannedCodesData
					.Select(x => x.RouteListAddress)
					.ToArray();

				var orderItems = scannedCodesData
					.Select(x => x.OrderItem)
					.ToArray();

				var orders = scannedCodesData
					.Select(x => x.Order)
					.ToArray();

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
