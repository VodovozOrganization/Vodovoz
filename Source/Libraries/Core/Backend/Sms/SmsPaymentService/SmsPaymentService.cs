using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NLog;
using QS.DomainModel.UoW;
using SmsPaymentService.DTO;
using SmsPaymentService.PaymentControllers;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.NotificationRecievers;
using Vodovoz.Services;
using Order = Vodovoz.Domain.Orders.Order;

namespace SmsPaymentService
{
    public class SmsPaymentService : ISmsPaymentService
    {
	    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IPaymentController _paymentController;
	    private readonly ISmsPaymentStatusNotificationReciever _smsPaymentStatusNotificationReciever;
	    private readonly IOrderParametersProvider _orderParametersProvider;
	    private readonly SmsPaymentFileCache _smsPaymentFileCache;
	    private readonly ISmsPaymentDTOFactory _smsPaymentDTOFactory;
	    private readonly ISmsPaymentValidator _smsPaymentValidator;

	    public SmsPaymentService(
			IUnitOfWorkFactory uowFactory,
            IPaymentController paymentController, 
            ISmsPaymentStatusNotificationReciever smsPaymentStatusNotificationReciever,
            IOrderParametersProvider orderParametersProvider,
            SmsPaymentFileCache smsPaymentFileCache,
            ISmsPaymentDTOFactory smsPaymentDTOFactory,
            ISmsPaymentValidator smsPaymentValidator
        )
        {
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_paymentController = paymentController ?? throw new ArgumentNullException(nameof(paymentController));
            _smsPaymentStatusNotificationReciever = smsPaymentStatusNotificationReciever ?? throw new ArgumentNullException(nameof(smsPaymentStatusNotificationReciever));
            _orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
            _smsPaymentFileCache = smsPaymentFileCache ?? throw new ArgumentNullException(nameof(smsPaymentFileCache));
            _smsPaymentDTOFactory = smsPaymentDTOFactory ?? throw new ArgumentNullException(nameof(smsPaymentDTOFactory));
            _smsPaymentValidator = smsPaymentValidator ?? throw new ArgumentNullException(nameof(smsPaymentValidator));
        }
        
        public PaymentResult SendPayment(int orderId, string phoneNumber)
        {
            _logger.Info($"Поступил запрос на отправку платежа с данными orderId: {orderId}, phoneNumber: {phoneNumber}");
            if (orderId <= 0) {
                _logger.Error("Запрос на отправку платежа пришёл с неверным значением номера заказа");
                return new PaymentResult("Неверное значение номера заказа");
            }
            if (String.IsNullOrWhiteSpace(phoneNumber)) {
                _logger.Error("Запрос на отправку платежа пришёл с неверным значение номера телефона");
                return new PaymentResult("Неверное значение номера телефона");
            }
            phoneNumber = phoneNumber.TrimStart('+').TrimStart('7').TrimStart('8');
            if (String.IsNullOrWhiteSpace(phoneNumber)
                || phoneNumber.Length == 0
                || phoneNumber.First() != '9'
                || phoneNumber.Length != 10)
            {
                _logger.Error("Запрос на отправку платежа пришёл с неверным форматом номера телефона");
                return new PaymentResult("Неверный формат номера телефона");
            }
            phoneNumber = $"+7{phoneNumber}";
            
            try
            {
                using (var uow = _uowFactory.CreateWithoutRoot()) {
                    var order = uow.GetById<Order>(orderId);
                    if (order == null) {
                        _logger.Error( $"Запрос на отправку платежа пришёл со значением номера заказа, не существующем в базе (Id: {orderId})");
                        return new PaymentResult($"Заказ с номером {orderId} не существующет в базе");
                    }
                    var newPayment = new SmsPayment {
                        Amount = order.OrderSum,
                        Order = order,
                        Recepient = order.Client,
                        CreationDate = DateTime.Now,
                        PhoneNumber = phoneNumber
                    };
                    if(!order.DeliveryDate.HasValue)
                    {
	                    _logger.Error("Запрос на отправку платежа пришёл без даты доставки");
	                    return new PaymentResult("Нельзя отправить платеж на заказ, в котором не указана дата доставки");
                    }
                    if(order.OrderDepositItems.Any()) {
                        _logger.Error("Запрос на отправку платежа пришёл с возвратами залогов");
                        return new PaymentResult("Нельзя отправить платеж на заказ, в котором есть возврат залогов");
                    }
                    if(!order.OrderItems.Any()) {
                        _logger.Error("Запрос на отправку платежа пришёл без товаров на продажу");
                        return new PaymentResult("Нельзя отправить платеж на заказ, в котором нет товаров на продажу");
                    }
                    if(order.OrderItems.Any(x => x.Price < 0)) {
	                    _logger.Error("Запрос на отправку платежа пришёл с товаром с ценой меньше 0");
	                    return new PaymentResult("Нельзя отправить платеж на заказ, в котором есть товары с ценой меньше 0");
                    }
                    if(newPayment.Amount < 1) {
                        _logger.Error("Запрос на отправку платежа пришёл с суммой заказа меньше 1 рубля");
                        return new PaymentResult("Нельзя отправить платеж на заказ, сумма которого меньше 1 рубля");
                    }

                    newPayment.SetReadyToSend();
                    var paymentDto = _smsPaymentDTOFactory.CreateSmsPaymentDTO(uow, newPayment, order,
	                    uow.GetById<PaymentFrom>(_orderParametersProvider.PaymentByCardFromSmsId));

                    if(!_smsPaymentValidator.Validate(paymentDto, out var errorMessages))
                    {
	                    _logger.Error($"Оплата для заказа №{paymentDto.OrderId} не прошла валидацию:\n\t{string.Join("\n\t", errorMessages)}");
	                    return new PaymentResult(string.Join("\n", errorMessages));
                    }

                    uow.Save(newPayment);
                    uow.Commit();

                    var sendResponse = _paymentController.SendPayment(paymentDto);

                    if (sendResponse.HttpStatusCode == HttpStatusCode.OK && sendResponse.ExternalId.HasValue) {
                        newPayment.SetWaitingForPayment();
                        newPayment.ExternalId = sendResponse.ExternalId.Value;
                        
                        try {
                            uow.Save(newPayment);
                            uow.Commit();
                            _logger.Info($"Создан новый платеж с данными: ExternalId: {newPayment.ExternalId}, orderId: {newPayment.Order.Id}, phoneNumber: {newPayment.PhoneNumber}");
                        }
                        catch (Exception e) {
                            _logger.Error(e, "Не получилось сохранить платёж с ExternalId, записываю данные во временный файл...");
                            _smsPaymentFileCache.WritePaymentCache(newPayment.Id, sendResponse.ExternalId.Value);
                        }
                    }
                    else {
                        return new PaymentResult( "Сервис отправки платежей не смог отправить платеж."
                                                  + (sendResponse.HttpStatusCode.HasValue ? $" Http код: {sendResponse.HttpStatusCode}" : ""));
                    }
                }
            }
            catch(Exception ex) {
                _logger.Error(ex, $"Ошибка при отправке платежа (orderId: {orderId}, phoneNumber: {phoneNumber})");
                return new PaymentResult($"Ошибка при отправке платежа. {ex}");
            }
            return new PaymentResult(SmsPaymentStatus.WaitingForPayment);
        }

        public PaymentResult SendPaymentPost(SendPaymentRequest sendPaymentRequest)
        {
            return SendPayment(sendPaymentRequest.OrderId, sendPaymentRequest.PhoneNumber);
        }

        public StatusCode ReceivePayment(RequestBody body)
        {
            int orderId;
            int externalId = 0;
            SmsPaymentStatus status = SmsPaymentStatus.ReadyToSend;
            
            try
            {
	            orderId = body.OrderId;
	            externalId = body.ExternalId;
                status = (SmsPaymentStatus)body.Status;
                var paidDate = DateTime.Parse(body.PaidDate);
                
                _logger.Info($"Поступил запрос на изменения статуса платежа с параметрами externalId: {externalId}, orderId: {orderId}, status: {status} и paidDate: {paidDate}");

                var acceptedStatuses = new[] { SmsPaymentStatus.Paid, SmsPaymentStatus.Cancelled };
                if (externalId == 0 || !acceptedStatuses.Contains(status)) {
                    _logger.Error($"Запрос на изменение статуса пришёл с неверным статусом или внешним Id (status: {status}, externalId: {externalId})");
                    return new StatusCode(HttpStatusCode.UnsupportedMediaType);
                }

                using (var uow = _uowFactory.CreateWithoutRoot()) {

                    SmsPayment payment;
                    try {
                        payment = uow.Session.QueryOver<SmsPayment>().Where(x => x.ExternalId == externalId).Take(1).SingleOrDefault();
                    }
                    catch (Exception e) {
                        _logger.Error(e, "При загрузке платежа по externalId произошла ошибка, записываю данные файл...");
                        _smsPaymentFileCache.WritePaymentCache(null, externalId);
                        return new StatusCode(HttpStatusCode.OK);
                    }

                    if(payment == null)
                    {
	                    _logger.Warn($"Запрос на изменение статуса платежа указывает на несуществующий платеж (externalId: {externalId}).\n" +
		                    $"Применяю оплату для первого попавшегося неотправленного платёжа для заказа №{orderId}"
	                    );

	                    if(orderId <= 0)
	                    {
		                    _logger.Warn(
			                    $"Был передан невалидный номер заказа (orderId: {orderId}). Получить неотправленный платёж невозможно");
		                    return new StatusCode(HttpStatusCode.UnsupportedMediaType);
	                    }

	                    var unsendedPayment = uow.Session.QueryOver<SmsPayment>()
		                    .Where(sp => sp.Order.Id == orderId)
		                    .And(sp => sp.SmsPaymentStatus == SmsPaymentStatus.ReadyToSend)
		                    .Take(1)
		                    .SingleOrDefault();

	                    if(unsendedPayment != null)
	                    {
		                    _logger.Info($"Применяю оплату для платежа: {unsendedPayment.Id} {unsendedPayment.SmsPaymentStatus} {unsendedPayment.CreationDate}");
		                    unsendedPayment.ExternalId = externalId;
		                    payment = unsendedPayment;
	                    }
	                    else
	                    {
		                    _logger.Warn($"Для заказа №{orderId} не найдено неотправленных платежей");
		                    return new StatusCode(HttpStatusCode.UnsupportedMediaType);
	                    }
                    }
                    if(payment.SmsPaymentStatus == status) {
                        _logger.Info($"Платеж с externalId: {externalId} уже имеет актуальный статус {status}"); 
                        return new StatusCode(HttpStatusCode.OK);
                    }
                    
                    var oldStatus = payment.SmsPaymentStatus;
                    var oldPaymentType = payment.Order.PaymentType;

                    switch (status) {
                        case SmsPaymentStatus.Paid:
                            payment.SetPaid(uow, paidDate == default(DateTime) ? DateTime.Now : paidDate, uow.GetById<PaymentFrom>(_orderParametersProvider.PaymentByCardFromSmsId));
                            break;
                        case SmsPaymentStatus.Cancelled:
                            payment.SetCancelled();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    try {
                        uow.Save(payment);
                        uow.Commit();

                        orderId = payment.Order.Id;

                        _logger.Info($"Статус платежа с externalId: {payment.ExternalId} изменён c {oldStatus} на {status}");

                        if(oldPaymentType != payment.Order.PaymentType)
                        {
                            _logger.Info(
                                $"Тип оплаты заказа № {payment.Order.Id} изменён c {oldPaymentType} на {payment.Order.PaymentType}");
                        }
                    }
                    catch (Exception e) {
                        _logger.Error(e, "При сохранении платежа произошла ошибка, записываю в файл...");
                        _smsPaymentFileCache.WritePaymentCache(payment.Id, payment.ExternalId);
                        return new StatusCode(HttpStatusCode.OK);
                    }
                }
            }
            catch (Exception ex) {
                _logger.Error(ex, $"Ошибка при обработке поступившего платежа (externalId: {externalId}, status: {status})");
                return new StatusCode(HttpStatusCode.InternalServerError);
            }
            
			try
			{
				_smsPaymentStatusNotificationReciever.NotifyOfSmsPaymentStatusChanged(orderId).Wait();
			}
			catch(Exception ex)
			{
				_logger.Error(ex, $"Не получилось уведомить DriverAPI об обновлении статуса заказа");
			}

			return new StatusCode(HttpStatusCode.OK);
        }

        public PaymentResult RefreshPaymentStatus(int externalId)
        {
            _logger.Info($"Поступил запрос на обновление статуса платежа с externalId: {externalId}");
            try {
                using (var uow = _uowFactory.CreateWithoutRoot()) {
                    var payment = uow.Session.QueryOver<SmsPayment>()
                       .Where(x => x.ExternalId == externalId)
                       .Take(1)
                       .SingleOrDefault();
                    
                    if (payment == null) {
                        _logger.Error($"Платеж с externalId: {externalId} не найден в базе");
                        return new PaymentResult($"Платеж с externalId: {externalId} не найден в базе");
                    }
                    var status = _paymentController.GetPaymentStatus(externalId);
                    if(status == null) {
                        return new PaymentResult($"Ошибка при получении статуса платежа с externalId: {externalId}");
                    }

                    if (payment.SmsPaymentStatus != status) {
                        var oldStatus = payment.SmsPaymentStatus;
                        
                        switch (status.Value) {
                            case SmsPaymentStatus.WaitingForPayment:
                                payment.SetWaitingForPayment();
                                break;
                            case SmsPaymentStatus.Paid:
                                payment.SetPaid(uow, DateTime.Now, uow.GetById<PaymentFrom>(_orderParametersProvider.PaymentByCardFromSmsId));
                                break;
                            case SmsPaymentStatus.Cancelled:
                                payment.SetCancelled();
                                break;
                            case SmsPaymentStatus.ReadyToSend:
                                payment.SetReadyToSend();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        
                        uow.Save(payment);
                        uow.Commit();
                        _logger.Info($"Платеж с externalId: {externalId} сменил статус с {oldStatus} на {status}");
                    }
                    else {
                        _logger.Info($"Платеж с externalId: {externalId} уже имеет актуальный статус {status}");
                    }
                    
                    return new PaymentResult(status.Value);
                }
            }
            catch (Exception ex) {
                _logger.Error(ex, $"Ошибка при обновлении статуса платежа externalId: {externalId}");
				return new PaymentResult($"Ошибка при обновлении статуса платежа externalId: {externalId}");
            }
        }

        /// <summary>
        /// Если есть хотя бы один оплаченный платеж возвращает со статусом <see cref="SmsPaymentStatus.Paid"/>,
        /// если таких нет и есть хотя бы одна ошибка - возвращает с <see cref="PaymentResult.MessageStatus"/> = <see cref="PaymentResult.MessageStatus.Error"/>,
        /// если таких нет и есть хотя бы один в ожидании оплаты, возвращает <see cref="SmsPaymentStatus.WaitingForPayment"/>,
        /// иначе если есть хотя бы один платеж - <see cref="SmsPaymentStatus.Cancelled"/>
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
		public PaymentResult GetActualPaymentStatus(int orderId)
		{
            _logger.Info($"Поступил запрос на актульный статус платежа для заказа с Id: {orderId}");

            try {
                using (var uow = _uowFactory.CreateWithoutRoot()) {
                    var payments = uow.Session.QueryOver<SmsPayment>().Where(x => x.Order.Id == orderId && x.ExternalId != 0).List();
                    if (!payments.Any())
                        return new PaymentResult($"Для заказа с Id: {orderId} не создано ни одного платежа");

                    IList<PaymentResult> results = new List<PaymentResult>();
                    foreach (var payment in payments) {
                        results.Add(RefreshPaymentStatus(payment.ExternalId));
                    }
                    
                    if(results.Any(x => x.PaymentStatus == SmsPaymentStatus.Paid))
                        return new PaymentResult(SmsPaymentStatus.Paid);

                    var errorResult = results.FirstOrDefault(x => x.Status == PaymentResult.MessageStatus.Error);
                    if(errorResult != null)
                        return new PaymentResult(errorResult.ErrorDescription);
                    
                    if(results.Any(x => x.PaymentStatus == SmsPaymentStatus.WaitingForPayment))
                        return new PaymentResult(SmsPaymentStatus.WaitingForPayment);
                    
                    return new PaymentResult(SmsPaymentStatus.Cancelled);
                }
            }
            catch (Exception ex) {
                _logger.Error(ex, $"Ошибка при запросе актульного статуса платежа для заказа с Id: {orderId}");
                return new PaymentResult();
            }
        }

		public bool ServiceStatus()
        {
            try {
                using (var uow = _uowFactory.CreateWithoutRoot()) {
                    uow.GetById<Order>(123);
                }
            }
            catch {
                return false;
            }
        
            return true;
        }

        public void SynchronizePaymentStatuses()
        {
            try
			{
                _logger.Info("Запущен процесс синхронизации статусов платежей");
                
                using (var uow = _uowFactory.CreateWithoutRoot())
				{
					RouteListItem routeListItemAlias = null;
                    Order orderAlias = null;
                    SmsPayment smsPaymentAlias = null;
                    RouteList routeListAlias = null;
                    
                    var payments = uow.Session.QueryOver<SmsPayment>(() => smsPaymentAlias)
                        .Inner.JoinAlias(() => smsPaymentAlias.Order, () => orderAlias)
                        .JoinEntityAlias(() => routeListItemAlias, () => routeListItemAlias.Order.Id == orderAlias.Id, JoinType.LeftOuterJoin)
                        .Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
                        .Where(() => smsPaymentAlias.SmsPaymentStatus == SmsPaymentStatus.WaitingForPayment)
                        .And(Restrictions.Disjunction()
                            .Add(Restrictions.Eq(Projections.Property(() => routeListAlias.Id), null))
                            .Add(() => routeListAlias.Status != RouteListStatus.Closed))
                        .List<SmsPayment>();

                    int count = 0;
                    foreach (var payment in payments)
					{
                        var actualStatus = _paymentController.GetPaymentStatus(payment.ExternalId);
                        if(actualStatus == null || actualStatus == payment.SmsPaymentStatus)
						{
							continue;
						}

						switch (actualStatus.Value) {
                            case SmsPaymentStatus.WaitingForPayment:
                                payment.SetWaitingForPayment();
                                break;
                            case SmsPaymentStatus.Paid:
                                payment.SetPaid(uow, DateTime.Now, uow.GetById<PaymentFrom>(_orderParametersProvider.PaymentByCardFromSmsId));
                                break;
                            case SmsPaymentStatus.Cancelled:
                                payment.SetCancelled();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        uow.Save(payment);
						uow.Commit();
                        count++;
                    }

					_logger.Info(payments.Count == 0
						? $"Не найдено ни одного платежа для синхронизации"
						: $"Синхронизировано {count} статусов платежей");
				}
            }
            catch (Exception ex) {
                _logger.Error(ex,"При синхронизации произошла ошибка");
            }
        }
    }
}
