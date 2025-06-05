using Edo.Transport;
using FluentNHibernate.Conventions;
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
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
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
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly IEdoDocflowRepository _edoDocflowRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IGenericRepository<TrueMarkProductCode> _productCodeRepository;
		private readonly MessageService _messageService;

		public ScannedCodesDelayedProcessingService(
			ILogger<ScannedCodesDelayedProcessingService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListItemTrueMarkProductCodesProcessingService routeListItemTrueMarkProductCodesProcessingService,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			IEdoDocflowRepository edoDocflowRepository,
			IOrderRepository orderRepository,
			IGenericRepository<TrueMarkProductCode> productCodeRepository,
			MessageService messageService)
		{
			_logger =
				logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWorkFactory =
				unitOfWorkFactory ?? throw new System.ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListItemTrueMarkProductCodesProcessingService =
				routeListItemTrueMarkProductCodesProcessingService ?? throw new System.ArgumentNullException(nameof(routeListItemTrueMarkProductCodesProcessingService));
			_trueMarkWaterCodeService =
				trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_edoDocflowRepository =
				edoDocflowRepository ?? throw new ArgumentNullException(nameof(edoDocflowRepository));
			_orderRepository =
				orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_productCodeRepository =
				productCodeRepository ?? throw new ArgumentNullException(nameof(productCodeRepository));
			_messageService =
				messageService ?? throw new ArgumentNullException(nameof(messageService));
		}

		public async Task ProcessScannedCodesAsync(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ScannedCodesDelayedProcessingService)))
			{
				var scannedCodesData = await _edoDocflowRepository
					.GetAllNotProcessedDriversScannedCodesData(uow, cancellationToken);

				if(!scannedCodesData.Any())
				{
					_logger.LogInformation("Нет отсканированных кодов ЧЗ для обработки");
					return;
				}

				_logger.LogInformation("Обработка отсканированных кодов ЧЗ, количество: {Count}", scannedCodesData.Count());

				await CheckScannedCodesAndAddToRouteListItems(uow, scannedCodesData, cancellationToken);

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

		private async Task CheckScannedCodesAndAddToRouteListItems(
			IUnitOfWork uow,
			IEnumerable<DriversScannedCodeDataNode> scannedCodesData,
			CancellationToken cancellationToken)
		{
			var routeListItemScannedCodes = scannedCodesData
				.GroupBy(x => new { x.RouteListAddress, x.OrderItem })
				.ToDictionary(x => x.Key, x => x.Select(c => c.DriversScannedCode).ToList());

			foreach(var routeListItemScannedCode in routeListItemScannedCodes)
			{
				var scannedTrueMarkAnyCodesDataResult = await GetTrueMarkCodesByScannedCodes(
				routeListItemScannedCode.Value,
				cancellationToken);

				if(scannedTrueMarkAnyCodesDataResult.IsFailure)
				{
					foreach(var driversScannedCode in routeListItemScannedCode.Value)
					{
						driversScannedCode.DriversScannedTrueMarkCodeStatus = DriversScannedTrueMarkCodeStatus.Error;
						driversScannedCode.DriversScannedTrueMarkCodeError = DriversScannedTrueMarkCodeError.TrueMarkApiRequestError;

						await uow.SaveAsync(driversScannedCode, cancellationToken: cancellationToken);
					}

					continue;
				}

				var scannedTrueMarkAnyCodesData = scannedTrueMarkAnyCodesDataResult.Value;

				scannedTrueMarkAnyCodesData =
					await RemoveScannedCodesDuplicates(uow, scannedTrueMarkAnyCodesData, cancellationToken);

				scannedTrueMarkAnyCodesData =
					await RemoveNotTrueMarkCodes(uow, scannedTrueMarkAnyCodesData, cancellationToken);

				scannedTrueMarkAnyCodesData =
					await RemoveLowLevelCodesScannedDuplicates(uow, scannedTrueMarkAnyCodesData, cancellationToken);

				scannedTrueMarkAnyCodesData =
					await RemoveExistingTransportGroupCodesAndCheckIdentificationCodes(uow, scannedTrueMarkAnyCodesData, cancellationToken);

				await AddCodesToRouteListItem(
					uow,
					scannedTrueMarkAnyCodesData,
					routeListItemScannedCode.Key.RouteListAddress,
					routeListItemScannedCode.Key.OrderItem.Id,
					cancellationToken);
			}
		}

		private async Task<IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode>> RemoveScannedCodesDuplicates(
			IUnitOfWork uow,
			IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode> codesData,
			CancellationToken cancellationToken)
		{
			var codesToRemove = codesData
				.GroupBy(x => x.Key.RawCode)
				.Where(g => g.Count() > 1)
				.Select(g => g.Last())
				.ToList();

			foreach(var codeData in codesToRemove)
			{
				var driversScannedCode = codeData.Key;

				_logger.LogInformation(
					"Выбрасываем отсканированный водителем кода ЧЗ {ScannedCodeId} т.к. один и тот же код был отсканирован несколько раз",
					driversScannedCode.Id);

				driversScannedCode.DriversScannedTrueMarkCodeStatus = DriversScannedTrueMarkCodeStatus.Error;
				driversScannedCode.DriversScannedTrueMarkCodeError = DriversScannedTrueMarkCodeError.Duplicate;

				await uow.SaveAsync(driversScannedCode, cancellationToken: cancellationToken);

				codesData.Remove(codeData.Key);
			}

			return codesData;
		}

		private async Task<IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode>> RemoveNotTrueMarkCodes(
			IUnitOfWork uow,
			IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode> codesData,
			CancellationToken cancellationToken)
		{
			var codesToRemove = codesData
				.Where(c => c.Value is null)
				.ToList();

			foreach(var codeData in codesToRemove)
			{
				var driversScannedCode = codeData.Key;

				_logger.LogInformation(
					"Выбрасываем отсканированный водителем код ЧЗ {ScannedCodeId} из-за отсутствия сведений о коде в ЧЗ",
					driversScannedCode.Id);

				driversScannedCode.DriversScannedTrueMarkCodeStatus = DriversScannedTrueMarkCodeStatus.Error;
				driversScannedCode.DriversScannedTrueMarkCodeError = DriversScannedTrueMarkCodeError.NotTrueMarkCode;

				await uow.SaveAsync(driversScannedCode, cancellationToken: cancellationToken);

				codesData.Remove(codeData.Key);
			}

			return codesData;
		}

		private async Task<IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode>> RemoveExistingTransportGroupCodesAndCheckIdentificationCodes(
			IUnitOfWork uow,
			IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode> codesData,
			CancellationToken cancellationToken)
		{
			var codesToRemove = new List<DriversScannedTrueMarkCode>();

			foreach(var codeData in codesData)
			{
				var driversScannedCode = codeData.Key;
				var trueMarkAnyCode = codeData.Value;

				var getSavedTrueMarkCodeResult =
					_trueMarkWaterCodeService.GetSavedTrueMarkAnyCodesByScannedCodes(uow, driversScannedCode.RawCode);

				if(getSavedTrueMarkCodeResult.IsFailure)
				{
					continue;
				}

				if(trueMarkAnyCode.IsTrueMarkTransportCode || trueMarkAnyCode.IsTrueMarkWaterGroupCode)
				{
					codesToRemove.Add(driversScannedCode);
					continue;
				}

				codesData[driversScannedCode] = getSavedTrueMarkCodeResult.Value;
			}

			foreach(var codeToRemove in codesToRemove)
			{
				_logger.LogInformation(
					"Выбрасываем отсканированный водителем код ЧЗ {ScannedCodeId} из-за наличия кода в базе",
					codeToRemove.Id);

				codeToRemove.DriversScannedTrueMarkCodeStatus = DriversScannedTrueMarkCodeStatus.Error;
				codeToRemove.DriversScannedTrueMarkCodeError = DriversScannedTrueMarkCodeError.Duplicate;

				await uow.SaveAsync(codeToRemove, cancellationToken: cancellationToken);

				codesData.Remove(codeToRemove);
			}

			return codesData;
		}

		private async Task<IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode>> RemoveLowLevelCodesScannedDuplicates(
			IUnitOfWork uow,
			IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode> codesData,
			CancellationToken cancellationToken)
		{
			var driverScannedCodesTrueMarkCodes = new Dictionary<DriversScannedTrueMarkCode, IEnumerable<TrueMarkAnyCode>>();

			foreach(var codeDataToCheck in codesData)
			{
				var allTrueMarkCodes = codeDataToCheck.Value.Match(
					transportCode => transportCode.GetAllCodes().ToArray(),
					groupCode => groupCode.GetAllCodes().ToArray(),
					waterCode => new TrueMarkAnyCode[] { waterCode });

				driverScannedCodesTrueMarkCodes.Add(codeDataToCheck.Key, allTrueMarkCodes);
			}

			foreach(var codeData in codesData)
			{
				var containsCodeFunc = TrueMarkAnyCodeListContainsCodeFunc(codeData.Value);

				var otherTrueMarkAnyCodes = driverScannedCodesTrueMarkCodes
					.Where(x => x.Key.Id != codeData.Key.Id)
					.SelectMany(x => x.Value)
					.ToList();

				var isCodeHasScannedHighLevelCode = otherTrueMarkAnyCodes
					.Any(containsCodeFunc);

				if(!isCodeHasScannedHighLevelCode)
				{
					continue;
				}

				var driversScannedCodeToRemove = codeData.Key;

				_logger.LogInformation(
					"Выбрасываем отсканированный водителем код ЧЗ {ScannedCodeId} из-за наличия отсканированного кода более высокого уровня кода",
					driversScannedCodeToRemove.Id);

				driversScannedCodeToRemove.DriversScannedTrueMarkCodeStatus = DriversScannedTrueMarkCodeStatus.Error;
				driversScannedCodeToRemove.DriversScannedTrueMarkCodeError = DriversScannedTrueMarkCodeError.HighLevelCodesScanned;

				await uow.SaveAsync(driversScannedCodeToRemove, cancellationToken: cancellationToken);

				codesData.Remove(codeData.Key);
			}

			return codesData;
		}

		private Func<TrueMarkAnyCode, bool> TrueMarkAnyCodeListContainsCodeFunc(TrueMarkAnyCode code)
		{
			return c => code.Match(
				transportCode => c.IsTrueMarkTransportCode
					&& c.TrueMarkTransportCode.RawCode == transportCode.RawCode,
				groupCode => c.IsTrueMarkWaterGroupCode
					&& c.TrueMarkWaterGroupCode.GTIN == groupCode.GTIN
					&& c.TrueMarkWaterGroupCode.SerialNumber == groupCode.SerialNumber,
				waterCode => c.IsTrueMarkWaterIdentificationCode
					&& c.TrueMarkWaterIdentificationCode.GTIN == waterCode.GTIN
					&& c.TrueMarkWaterIdentificationCode.SerialNumber == waterCode.SerialNumber);
		}


		private async Task AddCodesToRouteListItem(
			IUnitOfWork uow,
			IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode> codesData,
			RouteListItemEntity routeListAddress,
			int orderItemId,
			CancellationToken cancellationToken)
		{
			foreach(var codeData in codesData)
			{
				var driversScannedCode = codeData.Key;
				var trueMarkAnyCode = codeData.Value;

				var isCodeAlreadyExists = false;
				var isCodeHasDuplicate = false;
				int codeDuplicatesCount = 0;

				var newCodeStatus = driversScannedCode.IsDefective ? SourceProductCodeStatus.Problem : SourceProductCodeStatus.New;
				var newCodeProblem = driversScannedCode.IsDefective ? ProductCodeProblem.Defect : ProductCodeProblem.None;

				try
				{
					if(trueMarkAnyCode.IsTrueMarkWaterIdentificationCode)
					{
						var identificationCode = trueMarkAnyCode.TrueMarkWaterIdentificationCode;
						isCodeAlreadyExists = identificationCode.Id > 0;

						if(isCodeAlreadyExists)
						{
							var codeDuplicates = GetProductCodesHavingRequiredResultCodeIds(uow, identificationCode.Id);
							isCodeHasDuplicate = codeDuplicates.Any();
							codeDuplicatesCount = codeDuplicates.Count();
						}

						if(isCodeHasDuplicate)
						{
							newCodeStatus = SourceProductCodeStatus.Problem;
							newCodeProblem = ProductCodeProblem.Defect;
						}
					}

					await _routeListItemTrueMarkProductCodesProcessingService.AddTrueMarkAnyCodeToRouteListItemNoCodeStatusCheck(
						uow,
						routeListAddress,
						orderItemId,
						trueMarkAnyCode,
						newCodeStatus,
						newCodeProblem);

					driversScannedCode.DriversScannedTrueMarkCodeStatus = DriversScannedTrueMarkCodeStatus.Succeed;
					driversScannedCode.DriversScannedTrueMarkCodeError = DriversScannedTrueMarkCodeError.None;
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						"Ошибка при добавлении отсканированного водителем кода ЧЗ {ScannedCodeId} к адресу маршрутного листа {RouteListAddressId}, строка закза {OrderItemId}",
						driversScannedCode.Id,
						routeListAddress.Id,
						orderItemId);

					driversScannedCode.DriversScannedTrueMarkCodeStatus = DriversScannedTrueMarkCodeStatus.Error;
					driversScannedCode.DriversScannedTrueMarkCodeError = DriversScannedTrueMarkCodeError.Exception;
				}
				finally
				{
					await uow.SaveAsync(driversScannedCode, cancellationToken: cancellationToken);
				}
			}
		}

		private async Task<Result<IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode>>> GetTrueMarkCodesByScannedCodes(
			IEnumerable<DriversScannedTrueMarkCode> driversScannedCodes,
			CancellationToken cancellationToken)
		{
			var rawScannedCodes = driversScannedCodes.Select(x => x.RawCode).Distinct().ToList();
			var trueMarkAnyCodesDataResult = await _trueMarkWaterCodeService.GetTrueMarkAnyCodesByScannedCodes(rawScannedCodes, cancellationToken);

			if(trueMarkAnyCodesDataResult.IsFailure)
			{
				return Result.Failure<IDictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode>>(trueMarkAnyCodesDataResult.Errors);
			}

			var driversScannedCodesData = new Dictionary<DriversScannedTrueMarkCode, TrueMarkAnyCode>();
			var trueMarkAnyCodesData = trueMarkAnyCodesDataResult.Value;

			foreach(var driversScannedCode in driversScannedCodes)
			{
				if(trueMarkAnyCodesData.TryGetValue(driversScannedCode.RawCode, out var trueMarkAnyCode))
				{
					driversScannedCodesData.Add(driversScannedCode, trueMarkAnyCode);
				}
			}

			return driversScannedCodesData;
		}

		private IEnumerable<TrueMarkProductCode> GetProductCodesHavingRequiredResultCodeIds(IUnitOfWork uow, int resultCodeId) =>
			_productCodeRepository
			.Get(uow, x => resultCodeId == x.ResultCode.Id)
			.Distinct()
			.ToList();

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
					orderDriversScannedCodes.All(x => x.DriversScannedTrueMarkCodeStatus != DriversScannedTrueMarkCodeStatus.None);

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
