using DriverAPI.Library.Converters;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Models.TrueMark;
using Vodovoz.Services;

namespace DriverAPI.Library.Models
{
	public class OrderModel : IOrderModel
	{
		private readonly ILogger<OrderModel> _logger;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly OrderConverter _orderConverter;
		private readonly IDriverApiParametersProvider _webApiParametersProvider;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly ISmsPaymentModel _aPISmsPaymentModel;
		private readonly ISmsPaymentServiceAPIHelper _smsPaymentServiceAPIHelper;
		private readonly IFastPaymentsServiceAPIHelper _fastPaymentsServiceApiHelper;
		private readonly IUnitOfWork _uow;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly QRPaymentConverter _qrPaymentConverter;
		private readonly IFastPaymentModel _fastPaymentModel;
		private readonly int _maxClosingRating = 5;
		private readonly PaymentType[] _smsAndQRNotPayable = new PaymentType[] { PaymentType.PaidOnline, PaymentType.Barter, PaymentType.ContractDocumentation };
		private readonly IOrderParametersProvider _orderParametersProvider;

		public OrderModel(
			ILogger<OrderModel> logger,
			IOrderRepository orderRepository,
			IRouteListRepository routeListRepository,
			IRouteListItemRepository routeListItemRepository,
			OrderConverter orderConverter,
			IDriverApiParametersProvider webApiParametersProvider,
			IComplaintsRepository complaintsRepository,
			ISmsPaymentModel aPISmsPaymentModel,
			ISmsPaymentServiceAPIHelper smsPaymentServiceAPIHelper,
			IFastPaymentsServiceAPIHelper fastPaymentsServiceApiHelper,
			IUnitOfWork unitOfWork,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			QRPaymentConverter qrPaymentConverter,
			IFastPaymentModel fastPaymentModel,
			IOrderParametersProvider orderParametersProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_webApiParametersProvider = webApiParametersProvider ?? throw new ArgumentNullException(nameof(webApiParametersProvider));
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_aPISmsPaymentModel = aPISmsPaymentModel ?? throw new ArgumentNullException(nameof(aPISmsPaymentModel));
			_smsPaymentServiceAPIHelper = smsPaymentServiceAPIHelper ?? throw new ArgumentNullException(nameof(smsPaymentServiceAPIHelper));
			_fastPaymentsServiceApiHelper = fastPaymentsServiceApiHelper ?? throw new ArgumentNullException(nameof(fastPaymentsServiceApiHelper));
			_uow = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_qrPaymentConverter = qrPaymentConverter ?? throw new ArgumentNullException(nameof(qrPaymentConverter));
			_fastPaymentModel = fastPaymentModel ?? throw new ArgumentNullException(nameof(fastPaymentModel));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		/// <summary>
		/// Получение заказа в требуемом формате из заказа программы ДВ (использует функцию ниже)
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>APIOrder</returns>
		public OrderDto Get(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ {orderId} не найден");
			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_uow, vodovozOrder);

			var order = _orderConverter.ConvertToAPIOrder(
				vodovozOrder,
				routeListItem.CreationDate,
				_aPISmsPaymentModel.GetOrderSmsPaymentStatus(orderId),
				_fastPaymentModel.GetOrderFastPaymentStatus(orderId, vodovozOrder.OnlineOrder));
			order.OrderAdditionalInfo = GetAdditionalInfo(vodovozOrder);

			return order;
		}

		/// <summary>
		/// Получение списка заказов в требуемом формате из заказов программы ДВ по списку идентификаторов
		/// </summary>
		/// <param name="orderIds">Список идентификаторов заказов</param>
		/// <returns>IEnumerable APIOrder</returns>
		public IEnumerable<OrderDto> Get(int[] orderIds)
		{
			var result = new List<OrderDto>();
			var vodovozOrders = _orderRepository.GetOrders(_uow, orderIds);

			foreach(var vodovozOrder in vodovozOrders)
			{
				var smsPaymentStatus = _aPISmsPaymentModel.GetOrderSmsPaymentStatus(vodovozOrder.Id);
				var qrPaymentStatus = _fastPaymentModel.GetOrderFastPaymentStatus(vodovozOrder.Id, vodovozOrder.OnlineOrder);
				var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(_uow, vodovozOrder);
				var order = _orderConverter.ConvertToAPIOrder(vodovozOrder, routeListItem.CreationDate, smsPaymentStatus, qrPaymentStatus);
				order.OrderAdditionalInfo = GetAdditionalInfo(vodovozOrder);
				result.Add(order);
			}

			return result;
		}

		/// <summary>
		/// Получение типов оплаты на которые можно изменить тип оплаты заказа переданного в аргументе
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>IEnumerable APIPaymentType</returns>
		public IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ {orderId} не найден");

			return GetAvailableToChangePaymentTypes(vodovozOrder);
		}

		/// <summary>
		/// Получение типов оплаты на которые можно изменить тип оплаты заказа переданного в аргументе
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns>IEnumerable<see cref="PaymentDtoType"/></returns>
		public IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Order order)
		{
			var availablePaymentTypes = new List<PaymentDtoType>();

			bool paid = _fastPaymentModel.GetOrderFastPaymentStatus(order.Id) == Vodovoz.Domain.FastPayments.FastPaymentStatus.Performed;

			if(order.PaymentType == PaymentType.Cash)
			{
				availablePaymentTypes.Add(PaymentDtoType.Terminal);
				availablePaymentTypes.Add(PaymentDtoType.DriverApplicationQR);
			}

			if(order.PaymentType == PaymentType.Terminal)
			{
				availablePaymentTypes.Add(PaymentDtoType.Cash);
				availablePaymentTypes.Add(PaymentDtoType.DriverApplicationQR);
			}

			if(order.PaymentType == PaymentType.DriverApplicationQR
				|| (order.PaymentType == PaymentType.SmsQR && !paid))
			{
				availablePaymentTypes.Add(PaymentDtoType.Cash);
				availablePaymentTypes.Add(PaymentDtoType.Terminal);
			}

			return availablePaymentTypes;
		}

		/// <summary>
		/// Получение дополнительной информации для заказа по идентификатору
		/// </summary>
		/// <param name="orderId">Идентификатор заказа</param>
		/// <returns>APIOrderAdditionalInfo</returns>
		public OrderAdditionalInfoDto GetAdditionalInfo(int orderId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ {orderId} не найден");

			return GetAdditionalInfo(vodovozOrder);
		}

		/// <summary>
		/// Получение дополнительной информации для заказа из заказа программы ДВ
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns>APIOrderAdditionalInfo</returns>
		public OrderAdditionalInfoDto GetAdditionalInfo(Order order)
		{
			return new OrderAdditionalInfoDto
			{
				AvailablePaymentTypes = GetAvailableToChangePaymentTypes(order),
				CanSendSms = CanSendSmsForPayment(order, _aPISmsPaymentModel.GetOrderSmsPaymentStatus(order.Id)),
				CanReceiveQRCode = CanReceiveQRCodeForPayment(order),
			};
		}

		/// <summary>
		/// Проверка возможности отправки СМС для оплаты
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <param name="smsPaymentStatus">Статус оплаты СМС</param>
		/// <returns></returns>
		private bool CanSendSmsForPayment(Order order, SmsPaymentStatus? smsPaymentStatus)
		{
			return !_smsAndQRNotPayable.Contains(order.PaymentType) && order.OrderSum > 0;
		}

		/// <summary>
		/// Проверка возможности отправки QR-кода для оплаты
		/// </summary>
		/// <param name="order">Заказ программы ДВ</param>
		/// <returns></returns>
		private bool CanReceiveQRCodeForPayment(Order order)
		{
			return !_smsAndQRNotPayable.Contains(order.PaymentType) && order.OrderSum > 0;
		}

		public void ChangeOrderPaymentType(int orderId, PaymentType paymentType, Employee driver)
		{
			if(driver is null)
			{
				throw new ArgumentNullException(nameof(driver));
			}

			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId)
				?? throw new DataNotFoundException(nameof(orderId), $"Заказ {orderId} не найден");

			if(vodovozOrder.OrderStatus != OrderStatus.OnTheWay)
			{
				throw new InvalidOperationException($"Нельзя изменить тип оплаты для заказа: {orderId}, заказ не в пути.");
			}

			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался сменить тип оплаты заказа {OrderId} водителя {DriverId}",
					driver.Id,
					orderId,
					routeList.Driver.Id);
				throw new InvalidOperationException("Нельзя сменить тип оплаты заказа другого водителя");
			}

			vodovozOrder.PaymentType = paymentType;
			_uow.Save(vodovozOrder);
			_uow.Commit();
		}

		public void CompleteOrderDelivery(DateTime actionTime, Employee driver, IDriverCompleteOrderInfo completeOrderInfo)
		{
			var orderId = completeOrderInfo.OrderId;
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				throw new ArgumentOutOfRangeException(nameof(orderId), $"Заказ не найден: {orderId}");
			}

			if(routeList is null)
			{
				_logger.LogWarning("МЛ для заказа: {OrderId} не найден", orderId);
				throw new ArgumentOutOfRangeException(nameof(orderId), $"МЛ для заказа: {orderId} не найден");
			}

			if(routeListAddress is null)
			{
				_logger.LogWarning("Адрес МЛ для заказа: {OrderId} не найден", orderId);
				throw new ArgumentOutOfRangeException(nameof(orderId), $"Адрес МЛ для заказа: {orderId} не найден");
			}

			if(routeList.Driver.Id != driver.Id)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался завершить заказ {OrderId} водителя {DriverId}",
					driver.Id, orderId, routeList.Driver.Id);
				throw new InvalidOperationException("Нельзя завершить заказ другого водителя");
			}

			if(routeList.Status != RouteListStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, МЛ не в пути", orderId);
				throw new ArgumentOutOfRangeException(nameof(orderId), $"Нельзя завершить заказ: {orderId}, МЛ не в пути");
			}

			if(routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				_logger.LogWarning("Нельзя завершить заказ: {OrderId}, адрес МЛ не в пути", orderId);
				throw new ArgumentOutOfRangeException(nameof(orderId), $"Нельзя завершить заказ: {orderId}, адрес МЛ не в пути");
			}

			SaveScannedCodes(actionTime, completeOrderInfo);

			routeListAddress.DriverBottlesReturned = completeOrderInfo.BottlesReturnCount;

			routeList.ChangeAddressStatus(_uow, routeListAddress.Id, RouteListItemStatus.Completed);

			if(completeOrderInfo.Rating < _maxClosingRating)
			{
				var complaintReason = _complaintsRepository.GetDriverComplaintReasonById(_uow, completeOrderInfo.DriverComplaintReasonId);
				var complaintSource = _complaintsRepository.GetComplaintSourceById(_uow, _webApiParametersProvider.ComplaintSourceId);
				var reason = complaintReason?.Name ?? completeOrderInfo.OtherDriverComplaintReasonComment;

				var complaint = new Complaint
				{
					ComplaintSource = complaintSource,
					ComplaintType = ComplaintType.Driver,
					Order = vodovozOrder,
					DriverRating = completeOrderInfo.Rating,
					DeliveryPoint = vodovozOrder.DeliveryPoint,
					CreationDate = actionTime,
					ChangedDate = actionTime,
					Driver = driver,
					CreatedBy = driver,
					ChangedBy = driver,
					ComplaintText = $"Заказ номер {orderId}\n" +
						$"По причине {reason}"
				};

				_uow.Save(complaint);
			}

			if(completeOrderInfo.BottlesReturnCount != vodovozOrder.BottlesReturn)
			{
				if(!string.IsNullOrWhiteSpace(completeOrderInfo.DriverComment))
				{
					vodovozOrder.DriverMobileAppComment = completeOrderInfo.DriverComment;
					vodovozOrder.DriverMobileAppCommentTime = actionTime;
				}

				vodovozOrder.DriverCallType = DriverCallType.CommentFromMobileApp;

				_uow.Save(vodovozOrder);
			}

			_uow.Save(routeListAddress);
			_uow.Save(routeList);

			_uow.Commit();
		}

		private void SaveScannedCodes(DateTime actionTime, IDriverCompleteOrderInfo completeOrderInfo)
		{
			if(completeOrderInfo.ScannedItems == null)
			{
				return;
			}

			var cashReceiptExists = _uow.Session.QueryOver<CashReceipt>()
				.Where(x => x.Order.Id == completeOrderInfo.OrderId).List().Any();

			if(cashReceiptExists)
			{
				_logger.LogInformation("Получен повторный запрос на сохранение кодов для заказа {0}", completeOrderInfo.OrderId);
				return;
			}

			var orderId = completeOrderInfo.OrderId;
			var unprocessedCodes = _orderRepository.GetIsAccountableInTrueMarkOrderItemsCount(_uow, orderId);
			if(unprocessedCodes > CashReceipt.MaxMarkCodesInReceipt)
			{
				CreateCashReceipts(completeOrderInfo, orderId, actionTime, unprocessedCodes);
			}
			else
			{
				CreateCashReceipt(completeOrderInfo, orderId, actionTime);
			}
		}

		private void CreateCashReceipts(
			ITrueMarkOrderScannedInfo completeOrderInfo, int orderId, DateTime actionTime, decimal unprocessedCodes)
		{
			var cashReceipts = new Queue<CashReceipt>();
			var countReceiptsNeeded = Math.Ceiling(unprocessedCodes / CashReceipt.MaxMarkCodesInReceipt);

			for(var i = 0; i < countReceiptsNeeded; i++)
			{
				var receipt = GetNewCashReceipt(completeOrderInfo, orderId, actionTime);
				receipt.InnerNumber = i + 1;
				cashReceipts.Enqueue(receipt);
			}

			var trueMarkCashReceiptOrder = cashReceipts.Peek();

			// Кидаю все отсканированные коды в один чек, скорее всего водитель не будет сканировать 128+ позиций,
			// если же он их отсканировал, обработчик отправит лишку в пул и они нормально распределятся
			FillCashReceiptByScannedCodes(completeOrderInfo.ScannedItems, trueMarkCashReceiptOrder);

			foreach(var cashReceipt in cashReceipts)
			{
				_uow.Save(cashReceipt);
			}
		}

		private void CreateCashReceipt(ITrueMarkOrderScannedInfo completeOrderInfo, int orderId, DateTime actionTime)
		{
			var trueMarkCashReceiptOrder = GetNewCashReceipt(completeOrderInfo, orderId, actionTime);

			FillCashReceiptByScannedCodes(completeOrderInfo.ScannedItems, trueMarkCashReceiptOrder);

			_uow.Save(trueMarkCashReceiptOrder);
		}

		private CashReceipt GetNewCashReceipt(ITrueMarkOrderScannedInfo completeOrderInfo, int orderId, DateTime actionTime)
		{
			return new CashReceipt
			{
				UnscannedCodesReason = completeOrderInfo.UnscannedCodesReason,
				Order = new Order { Id = orderId },
				CreateDate = actionTime,
				Status = CashReceiptStatus.New
			};
		}

		private CashReceiptProductCode CreateCashReceiptProductCode(string code, CashReceipt trueMarkCashReceiptOrder, OrderItem orderItem)
		{
			var orderProductCode = new CashReceiptProductCode
			{
				CashReceipt = trueMarkCashReceiptOrder,
				OrderItem = orderItem
			};

			TrueMarkWaterIdentificationCode codeEntity;

			var parsed = _trueMarkWaterCodeParser.TryParse(code, out TrueMarkWaterCode parsedCode);
			if(parsed)
			{
				codeEntity = LoadCode(parsedCode.SourceCode);
				if(codeEntity == null)
				{
					codeEntity = new TrueMarkWaterIdentificationCode
					{
						IsInvalid = false,
						RawCode = parsedCode.SourceCode,
						GTIN = parsedCode.GTIN,
						SerialNumber = parsedCode.SerialNumber,
						CheckCode = parsedCode.CheckCode
					};

					orderProductCode.SourceCode = codeEntity;
					_uow.Save(codeEntity);
				}
				else
				{
					//Не можем создать код идентификации честного знака, потому что такой уже существует.
					//Позже при обработке этого заказа будет подобран подходящий код
					orderProductCode.IsDuplicateSourceCode = true;
				}
			}
			else
			{
				codeEntity = LoadCode(code);
				if(codeEntity == null)
				{
					codeEntity = new TrueMarkWaterIdentificationCode
					{
						IsInvalid = true,
						RawCode = code
					};

					orderProductCode.SourceCode = codeEntity;
					_uow.Save(codeEntity);
				}
				else
				{
					//Не можем создать код идентификации честного знака, потому что такой уже существует.
					//Позже при обработке этого заказа будет подобран подходящий код
					orderProductCode.IsDuplicateSourceCode = true;
				}
			}

			return orderProductCode;
		}

		private void FillCashReceiptByScannedCodes(IEnumerable<ITrueMarkOrderItemScannedInfo> scannedItems, CashReceipt cashReceipt)
		{
			foreach(var scannedItem in scannedItems)
			{
				var orderItem = new OrderItem { Id = scannedItem.OrderSaleItemId };

				foreach(var defectiveCode in scannedItem.DefectiveBottleCodes)
				{
					var orderCode = CreateCashReceiptProductCode(defectiveCode, cashReceipt, orderItem);
					orderCode.IsDefectiveSourceCode = true;

					cashReceipt.ScannedCodes.Add(orderCode);
				}

				foreach(var code in scannedItem.BottleCodes)
				{
					var orderCode = CreateCashReceiptProductCode(code, cashReceipt, orderItem);

					cashReceipt.ScannedCodes.Add(orderCode);
				}
			}
		}

		private TrueMarkWaterIdentificationCode LoadCode(string code)
		{
			return _uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
				.Where(x => x.RawCode == code)
				.SingleOrDefault();
		}

		public void SendSmsPaymentRequest(
			int orderId,
			string phoneNumber,
			int driverId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null
			|| routeList is null
			|| routeListAddress is null)
			{
				throw new DataNotFoundException(nameof(orderId), "Не найден или не находится в МЛ");
			}

			if(routeList.Status != RouteListStatus.EnRoute
			|| routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				throw new InvalidOperationException("Нельзя отправлять СМС на оплату для адреса МЛ не в пути");
			}

			if(routeList.Driver.Id != driverId)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался запросить оплату по СМС для заказа {OrderId} водителя {DriverId}",
					driverId, orderId, routeList.Driver.Id);
				throw new InvalidOperationException("Нельзя запросить оплату по СМС для заказа другого водителя");
			}

			_smsPaymentServiceAPIHelper.SendPayment(orderId, phoneNumber).Wait();
		}

		public async Task<PayByQRResponseDTO> SendQRPaymentRequestAsync(int orderId, int driverId)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);
			var routeList = _routeListRepository.GetActualRouteListByOrder(_uow, vodovozOrder);
			var routeListAddress = routeList.Addresses.FirstOrDefault(x => x.Order.Id == orderId);

			if(vodovozOrder is null || routeList is null || routeListAddress is null)
			{
				throw new DataNotFoundException(nameof(orderId), "Не найден или не находится в МЛ");
			}

			if(routeList.Status != RouteListStatus.EnRoute || routeListAddress.Status != RouteListItemStatus.EnRoute)
			{
				throw new InvalidOperationException("Нельзя отправлять QR-код на оплату для адреса МЛ не в пути");
			}

			if(routeList.Driver.Id != driverId)
			{
				_logger.LogWarning("Сотрудник {EmployeeId} попытался запросить оплату по QR для заказа {OrderId} водителя {DriverId}",
					driverId, orderId, routeList.Driver.Id);
				throw new InvalidOperationException("Нельзя запросить оплату по QR для заказа другого водителя");
			}

			var qrResponseDto = await _fastPaymentsServiceApiHelper.SendPaymentAsync(orderId);
			var payByQRResponseDto = _qrPaymentConverter.ConvertToPayByQRResponseDto(qrResponseDto);

			if(payByQRResponseDto.QRPaymentStatus == QRPaymentDTOStatus.Paid)
			{
				payByQRResponseDto.AvailablePaymentTypes = Enumerable.Empty<PaymentDtoType>();
				payByQRResponseDto.CanReceiveQR = false;
			}
			else
			{
				payByQRResponseDto.AvailablePaymentTypes = GetAvailableToChangePaymentTypes(orderId);
				payByQRResponseDto.CanReceiveQR = true;
			}

			return payByQRResponseDto;
		}

		public void UpdateBottlesByStockActualCount(int orderId, int bottlesByStockActualCount)
		{
			var vodovozOrder = _orderRepository.GetOrder(_uow, orderId);

			if(vodovozOrder is null)
			{
				_logger.LogWarning("Заказ не найден: {OrderId}", orderId);
				throw new ArgumentOutOfRangeException(nameof(orderId), $"Заказ не найден: {orderId}");
			}

			if(!vodovozOrder.IsBottleStock
			   || vodovozOrder.BottlesByStockCount == bottlesByStockActualCount
			   || vodovozOrder.BottlesByStockActualCount == bottlesByStockActualCount)
			{
				return;
			}

			vodovozOrder.IsBottleStockDiscrepancy = true;
			vodovozOrder.BottlesByStockActualCount = bottlesByStockActualCount;
			vodovozOrder.CalculateBottlesStockDiscounts(_orderParametersProvider, true);
			_uow.Save(vodovozOrder);
			_uow.Commit();
		}
	}
}
