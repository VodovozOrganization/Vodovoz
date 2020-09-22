using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Order = Vodovoz.Domain.Orders.Order;

namespace SmsPaymentService
{
    public class SmsPaymentService : ISmsPaymentService
    {
        public SmsPaymentService(
            IPaymentWorker paymentWorker, 
            IDriverPaymentService androidDriverService, 
            ISmsPaymentServiceParametersProvider smsPaymentServiceParametersProvider,
            FileProvider fileProvider,
            UnsavedPaymentsWorker unsavedPaymentsWorker
            )
        {
            this.paymentWorker = paymentWorker ?? throw new ArgumentNullException(nameof(paymentWorker));
            this.androidDriverService = androidDriverService ?? throw new ArgumentNullException(nameof(androidDriverService));
            this.smsPaymentServiceParametersProvider = smsPaymentServiceParametersProvider ?? throw new ArgumentNullException(nameof(smsPaymentServiceParametersProvider));
            this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            this.unsavedPaymentsWorker = unsavedPaymentsWorker ?? throw new ArgumentNullException(nameof(unsavedPaymentsWorker));
        }
        
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IPaymentWorker paymentWorker;
		private readonly IDriverPaymentService androidDriverService;
        private readonly ISmsPaymentServiceParametersProvider smsPaymentServiceParametersProvider;
        private readonly FileProvider fileProvider;
        private readonly UnsavedPaymentsWorker unsavedPaymentsWorker;
        
		public StatusCode ReceivePayment(RequestBody body)
        {
            var externalId = body.ExternalId;
            var status = (SmsPaymentStatus)body.Status;
            var paidDate = DateTime.Parse(body.PaidDate);
			int orderId;
            
            logger.Info($"Поступил запрос на изменения статуса платежа с параметрами externalId: {externalId}, status: {status} и paidDate: {paidDate}");
            
            var acceptedStatuses = new[] { SmsPaymentStatus.Paid, SmsPaymentStatus.Cancelled };
            if (externalId == 0 || !acceptedStatuses.Contains(status)) { 
                logger.Error($"Запрос на изменение статуса пришёл с неверным статусом или внешним Id (status: {status}, externalId: {externalId})");
                return new StatusCode(HttpStatusCode.UnsupportedMediaType);
            }
            try {
                using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
                    var payment = uow.Session.QueryOver<SmsPayment>().Where(x => x.ExternalId == externalId).Take(1).SingleOrDefault();
                    if (payment == null) {
                        logger.Error($"Запрос на изменение статуса платежа указывает на несуществующий платеж (externalId: {externalId})"); 
                        return new StatusCode(HttpStatusCode.UnsupportedMediaType);
                    }
                    if(payment.SmsPaymentStatus == status) {
                        logger.Info($"Платеж с externalId: {externalId} уже имеет актуальный статус {status}"); 
                        return new StatusCode(HttpStatusCode.OK);
                    }
                    
                    var oldStatus = payment.SmsPaymentStatus;
                    var oldPaymentType = payment.Order.PaymentType;

                    switch (status) {
                        case SmsPaymentStatus.Paid:
                            payment.SetPaid(uow, DateTime.Now, uow.GetById<PaymentFrom>(smsPaymentServiceParametersProvider.GetSmsPaymentByCardFromId));
                            break;
                        case SmsPaymentStatus.Cancelled:
                            payment.SetCancelled();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    uow.Save(payment);
                    uow.Commit();
					orderId = payment.Order.Id;
                    
                    logger.Info($"Статус платежа с externalId: {payment.ExternalId} изменён c {oldStatus} на {status}");
                    if(oldPaymentType != payment.Order.PaymentType)
                        logger.Info($"Тип оплаты заказа № {payment.Order.Id} изменён c {oldPaymentType} на {payment.Order.PaymentType}");
				}
            }
            catch (Exception ex) {
                logger.Error(ex, $"Ошибка при обработке поступившего платежа (externalId: {externalId}, status: {status})");
                return new StatusCode(HttpStatusCode.InternalServerError);
            }

			try {
				androidDriverService.RefreshPaymentStatus(orderId);
			}
			catch(Exception ex) {
				logger.Error(ex, $"Не получилось уведомить службу водителей об обновлении статуса заказа");
			}

			return new StatusCode(HttpStatusCode.OK);
        }

        public PaymentResult SendPayment(int orderId, string phoneNumber)
        {
            logger.Info($"Поступил запрос на отправку платежа с данными orderId: {orderId}, phoneNumber: {phoneNumber}");
			PaymentResult resultMessage = new PaymentResult(SmsPaymentStatus.WaitingForPayment);
            if (orderId <= 0) {
                resultMessage.ErrorDescription = "Неверное значение номера заказа";
                logger.Error("Запрос на отправку платежа пришёл с неверным значением номера заказа");
                return resultMessage;
            }
            if (String.IsNullOrWhiteSpace(phoneNumber)) {
                resultMessage.ErrorDescription = "Неверное значение номера телефона";
                logger.Error("Запрос на отправку платежа пришёл с неверным значение номера телефона");
                return resultMessage;
            }
            phoneNumber = phoneNumber.TrimStart('+').TrimStart('7').TrimStart('8');
            if (String.IsNullOrWhiteSpace(phoneNumber)
                || phoneNumber.Length == 0
                || phoneNumber.First() != '9'
                || phoneNumber.Length != 10)
            {
                resultMessage.ErrorDescription = "Неверный формат номера телефона";
                logger.Error("Запрос на отправку платежа пришёл с неверным форматом номера телефона");
                return resultMessage;
            }
            phoneNumber = $"+7{phoneNumber}";
            try
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
                    var order = uow.GetById<Order>(orderId);
                    if (order == null) {
                        resultMessage.ErrorDescription = $"Заказ с номером {orderId} не существующет в базе";
                        logger.Error( $"Запрос на отправку платежа пришёл со значением номера заказа, не существующем в базе (Id: {orderId})");
                        return resultMessage;
                    }
                    var newPayment = new SmsPayment {
                        Amount = order.OrderTotalSum,
                        Order = order,
                        Recepient = order.Client,
                        CreationDate = DateTime.Now,
                        PhoneNumber = phoneNumber
                    };
                    newPayment.SetReadyToSend();
                    uow.Save(newPayment);
                    uow.Commit();
                    
                    var paymentDto = CreateSmsPaymentDTOFromSmsPayment(newPayment);
                    
                    var sendResponse = paymentWorker.SendPayment(paymentDto);

                    if (sendResponse.HttpStatusCode == HttpStatusCode.OK) {
                        newPayment.SetWaitingForPayment();
                        newPayment.ExternalId = sendResponse.ExternalId;
                        
                        try {
                            uow.Save(newPayment);
                            uow.Commit();
                            logger.Info($"Создан новый платеж с данными: ExternalId: {newPayment.ExternalId}, orderId: {newPayment.Order.Id}, phoneNumber: {newPayment.PhoneNumber}");
                        }
                        catch (Exception) {
                            logger.Warn("Не получилось сохранить платёж с ExternalId, записываю данные во временный файл...");
                            fileProvider.WriteExternalIdToFile(newPayment.Id, sendResponse.ExternalId);
                            unsavedPaymentsWorker.Trigger();
                        }
                    }
                    else if(sendResponse.Exception != null && !sendResponse.HttpStatusCode.HasValue) {
                        throw sendResponse.Exception;
                    }
                    else {
                        resultMessage.ErrorDescription = $"Не получилось отправить платеж. Http код: {sendResponse}";
                        logger.Error(resultMessage.ErrorDescription, $"Не получилось отправить платеж.  Http код: {sendResponse}." +
                                                                     $" (orderId: {orderId}, phoneNumber: {phoneNumber})");
                        return resultMessage;
                    }
                }
            }
            catch(Exception ex) {
                resultMessage.ErrorDescription = $"Ошибка при отправке платежа. {ex.Message}";
                logger.Error(ex, $"Ошибка при отправке платежа (orderId: {orderId}, phoneNumber: {phoneNumber})");
            }
            return resultMessage;
        }

        public PaymentResult RefreshPaymentStatus(int externalId)
        {
            logger.Info($"Поступил запрос на обновление статуса платежа с externalId: {externalId}");
            try {
                using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
                    var payment = uow.Session.QueryOver<SmsPayment>()
                       .Where(x => x.ExternalId == externalId)
                       .Take(1)
                       .SingleOrDefault();
                    
                    if (payment == null) {
                        logger.Error($"Платеж с externalId: {externalId} не найден в базе");
                        return new PaymentResult($"Платеж с externalId: {externalId} не найден в базе");
                    }
                    var status = paymentWorker.GetPaymentStatus(externalId);
                    if (status == null)
                        return new PaymentResult($"Ошибка при получении статуса платежа с externalId: {externalId}");
                    
                    if (payment.SmsPaymentStatus != status) {
                        var oldStatus = payment.SmsPaymentStatus;
                        
                        switch (status.Value) {
                            case SmsPaymentStatus.WaitingForPayment:
                                payment.SetWaitingForPayment();
                                break;
                            case SmsPaymentStatus.Paid:
                                payment.SetPaid(uow, DateTime.Now, uow.GetById<PaymentFrom>(smsPaymentServiceParametersProvider.GetSmsPaymentByCardFromId));
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
                        logger.Info($"Платеж с externalId: {externalId} сменил статус с {oldStatus} на {status}");
                    }
                    else {
                        logger.Info($"Платеж с externalId: {externalId} уже имеет актуальный статус {status}");
                    }
                    
                    return new PaymentResult(status.Value);
                }
            }
            catch (Exception ex) {
                logger.Error(ex, $"Ошибка при обновлении статуса платежа externalId: {externalId}");
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
            logger.Info($"Поступил запрос на актульный статус платежа для заказа с Id: {orderId}");

            try {
                using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
                    var payments = uow.Session.QueryOver<SmsPayment>().Where(x => x.Order.Id == orderId).List();
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
                logger.Error(ex, $"Ошибка при запросе актульного статуса платежа для заказа с Id: {orderId}");
                return new PaymentResult();
            }
        }

		public bool ServiceStatus()
        {
            try {
                using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
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
            try {
                logger.Info("Запущен процесс синхронизации статусов платежей");
                
                using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {

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
                    foreach (var payment in payments) {
                        var actualStatus = paymentWorker.GetPaymentStatus(payment.ExternalId);
                        if(actualStatus == null || actualStatus == payment.SmsPaymentStatus)
                            continue;

                        switch (actualStatus.Value) {
                            case SmsPaymentStatus.WaitingForPayment:
                                payment.SetWaitingForPayment();
                                break;
                            case SmsPaymentStatus.Paid:
                                payment.SetPaid(uow, DateTime.Now, uow.GetById<PaymentFrom>(smsPaymentServiceParametersProvider.GetSmsPaymentByCardFromId));
                                break;
                            case SmsPaymentStatus.Cancelled:
                                payment.SetCancelled();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        uow.Save(payment);
                        count++;
                    }

                    if(payments.Count == 0) {
                        logger.Info($"Не найдено ни одного платежа для синхронизации");
                    }
                    else {
                        if(count != 0)
                            uow.Commit();
                        logger.Info($"Синхронизировано {count} статусов платежей");
                    }
                }
            }
            catch (Exception ex) {
                logger.Error(ex,"При синхронизации произошла ошибка");
            }
        }

        private SmsPaymentDTO CreateSmsPaymentDTOFromSmsPayment(SmsPayment smsPayment)
        {
            return new SmsPaymentDTO {
                Recepient = smsPayment.Recepient.Name,
                RecepientId = smsPayment.Recepient.Id,
                PhoneNumber = smsPayment.PhoneNumber,
                PaymentStatus = SmsPaymentStatus.WaitingForPayment,
                OrderId = smsPayment.Order.Id,
                PaymentCreationDate = smsPayment.CreationDate,
                Amount = smsPayment.Amount,
                RecepientType = smsPayment.Recepient.PersonType
            };
        }
    }
}