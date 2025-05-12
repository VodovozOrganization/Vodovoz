using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.Dto;
using WarehouseApi.Contracts.Responses.V1;
using WarehouseApi.Library.Extensions;

namespace WarehouseApi.Library.Services
{
	internal sealed class SelfDeliveryService : ISelfDeliveryService
	{
		private ILogger<SelfDeliveryService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IGenericRepository<SelfDeliveryDocument> _selfDeliveryDocumentRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

		public SelfDeliveryService(
			ILogger<SelfDeliveryService> logger,
			IUnitOfWork unitOfWork,
			IGenericRepository<Order> orderRepository,
			IGenericRepository<SelfDeliveryDocument> selfDeliveryDocumentRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
			_selfDeliveryDocumentRepository = selfDeliveryDocumentRepository
				?? throw new ArgumentNullException(nameof(selfDeliveryDocumentRepository));
			_trueMarkWaterCodeService = trueMarkWaterCodeService
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
		}

		public async Task<Result<GetSelfDeliveryResponse>> GetSelfDeliveryDocument(int id, CancellationToken cancellationToken)
		{
			var documents = await _selfDeliveryDocumentRepository
				.GetAsync(_unitOfWork, x => x.Id == id, 1, cancellationToken);

			if(!documents.Any())
			{
				return Vodovoz.Errors.Documents.SelfDeliveryDocument.NotFound;
			}

			return documents.FirstOrDefault()?.ToDtoApiV1();
		}

		/// <inheritdoc/>
		public Result<OrderDto> GetSelfDeliveryOrder(int orderId)
		{
			var order = _orderRepository
				.Get(
					_unitOfWork,
					x => x.Id == orderId,
					1)
				.FirstOrDefault();

			var validationResult = ValidateSelfDeliveryOrderToProcess(order);

			if(validationResult.IsFailure)
			{
				return Result.Failure<OrderDto>(validationResult.Errors);
			}

			var nomenclatures = order.OrderItems
				.Select(x => x.Nomenclature)
				.ToArray();

			var dto = order.ToApiDtoV1(nomenclatures);

			dto.DocType = DocumentSourceType.Invoice;

			return dto;
		}

		/// <inheritdoc/>
		public async Task<Result> AddTrueMarkCode(
			int orderId,
			string scannedCode,
			CancellationToken cancellationToken)
		{
			var order = _orderRepository
				.Get(
					_unitOfWork,
					x => x.Id == orderId,
					1)
				.FirstOrDefault();

			var validationResult = ValidateSelfDeliveryOrderToProcess(order);

			if(validationResult.IsFailure)
			{
				return validationResult;
			}

			if(!(_selfDeliveryDocumentRepository
				.Get(_unitOfWork, x => x.Order.Id == orderId, 1)
				.FirstOrDefault() is SelfDeliveryDocument selfDeliveryDocument))
			{
				selfDeliveryDocument = new SelfDeliveryDocument
				{
					Order = order,
				};
			}

			var code = await _trueMarkWaterCodeService
				.GetTrueMarkCodeByScannedCode(_unitOfWork, scannedCode, cancellationToken);

			_unitOfWork.Save(selfDeliveryDocument);

			var waterItems = selfDeliveryDocument.Order.OrderItems
				.Where(x => x.Nomenclature.IsAccountableInTrueMark)
				.ToArray();

			foreach(var item in waterItems)
			{
				var addResult = selfDeliveryDocument.AddItem(item);

				if(addResult.IsFailure)
				{
					return Result.Failure(addResult.Errors);
				}
			}

			return Result.Success();
		}

		public Result ValidateSelfDeliveryOrderToProcess(Order order)
		{
			if(order is null)
			{
				_logger.LogWarning($"Заказ не найден.");
				return Vodovoz.Errors.Orders.Order.NotFound;
			}

			if(!order.SelfDelivery)
			{
				_logger.LogWarning($"Заказ с id {order.Id} не является самовывозом.");
				return Vodovoz.Errors.Orders.Order.IsNotSelfDelivery;
			}

			if(order.OrderStatus != OrderStatus.Accepted)
			{
				_logger.LogWarning($"Заказ с id {order.Id} не может быть обработан, так как его статус {order.OrderStatus}");
				return Vodovoz.Errors.Orders.Order.CantEdit;
			}

			return Result.Success();
		}

		public Task<Result<SelfDeliveryDocument>> GetSelfDeliveryDocumentByOrderId(int? orderId)
		{
			throw new NotImplementedException();
		}

		public Task<Result<bool>> CreateDocument()
		{
			throw new NotImplementedException();
		}

		public Task<Result<bool>> AddCodes(IEnumerable<string> codesToAdd)
		{
			throw new NotImplementedException();
		}

		public Task<Result<bool>> ChangeCodes(IEnumerable<string> codesToChange)
		{
			throw new NotImplementedException();
		}

		public Task<Result<bool>> RemoveCodes(IEnumerable<string> codesToDelete)
		{
			throw new NotImplementedException();
		}

		public void EndLoad()
		{
			throw new NotImplementedException();
		}
	}
}
