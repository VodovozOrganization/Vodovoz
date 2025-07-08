using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Domain
{
	[Appellative(
            Gender = GrammaticalGender.Masculine,
            NominativePlural = "платежи по Sms",
            Nominative = "платёж по Sms")]
    [HistoryTrace]
    public class SmsPayment : PropertyChangedBase, IDomainObject
    {
        #region Свойства

        public virtual int Id { get; set; }
        
        private SmsPaymentStatus smsPaymentStatus;
        [Display(Name = "Статус оплаты")]
        public virtual SmsPaymentStatus SmsPaymentStatus {
            get => smsPaymentStatus;
            protected set => SetField(ref smsPaymentStatus, value, () => SmsPaymentStatus);
        }
        
        private int externalId;
        [Display(Name = "Внешний ID")]
        public virtual int ExternalId {
            get => externalId;
            set => SetField(ref externalId, value, () => ExternalId);
        }
        
        private decimal amount;
        [Display(Name = "Сумма платежа")]
        public virtual decimal Amount {
            get => amount;
            set => SetField(ref amount, value, () => Amount);
        }

        private Order order;
        [Display(Name = "Заказ")]
        public virtual Order Order {
            get => order;
            set => SetField(ref order, value, () => Order);
        }
        
        private Counterparty recepient;
        [Display(Name = "Получатель")]
        public virtual Counterparty Recepient {
            get => recepient;
            set => SetField(ref recepient, value, () => Recepient);
        }
        
        private string phoneNumber;
        [Display(Name = "Номер телефона")]
        public virtual string PhoneNumber {
            get => phoneNumber;
            set => SetField(ref phoneNumber, value, () => PhoneNumber);
        }
        
        private DateTime creationDate;
        [Display(Name = "Дата создания")]
        public virtual DateTime CreationDate {
            get => creationDate;
            set => SetField(ref creationDate, value, () => CreationDate);
        }
        
        private DateTime? paidDate;
        [Display(Name = "Дата оплаты")]
        public virtual DateTime? PaidDate {
            get => paidDate;
            set => SetField(ref paidDate, value, () => PaidDate);
        }

        #endregion

        #region Функции

        public virtual SmsPayment SetPaid(
			IUnitOfWork uow,
			DateTime datePaid,
			PaymentFrom paymentFrom,
			INomenclatureSettings nomenclatureSettings,
			IOrderContractUpdater contractUpdater)
        {
            SmsPaymentStatus = SmsPaymentStatus.Paid;
            
            if (Order.PaymentType == PaymentType.Cash
                && Order.SelfDelivery
                && Order.OrderStatus == OrderStatus.WaitForPayment
                && Order.PayAfterShipment)
            {
                Order.TryCloseSelfDeliveryPayAfterShipmentOrder(
                    uow,
					nomenclatureSettings,
                    ScopeProvider.Scope.Resolve<IRouteListItemRepository>(),
                    ScopeProvider.Scope.Resolve<ISelfDeliveryRepository>(),
					ScopeProvider.Scope.Resolve<ICashRepository>());
                Order.IsSelfDeliveryPaid = true;
            }

            if (Order.PaymentType == PaymentType.Cash
                && Order.SelfDelivery
                && Order.OrderStatus == OrderStatus.WaitForPayment
                && !Order.PayAfterShipment)
            {
                Order.ChangeStatus(OrderStatus.OnLoading);
                Order.IsSelfDeliveryPaid = true;
            }
            
            PaidDate = datePaid;
            Order.OnlinePaymentNumber = ExternalId;
            Order.UpdatePaymentType(PaymentType.PaidOnline, contractUpdater);    
            Order.UpdatePaymentByCardFrom(paymentFrom, contractUpdater);

            foreach (var routeListItem in uow.Session.QueryOver<RouteListItem>().Where(x => x.Order.Id == Order.Id).List<RouteListItem>()) {
                routeListItem.RecalculateTotalCash();
                uow.Save(routeListItem);
            }
            
            return this;
        }
        
        public virtual SmsPayment SetCancelled()
        {
            SmsPaymentStatus = SmsPaymentStatus.Cancelled;
            return this;
        }
        
        public virtual SmsPayment SetWaitingForPayment()
        {
            SmsPaymentStatus = SmsPaymentStatus.WaitingForPayment;
            return this;
        }
        
        public virtual SmsPayment SetReadyToSend()
        {
            SmsPaymentStatus = SmsPaymentStatus.ReadyToSend;
            return this;
        }

        #endregion
        
    }
    
    public enum SmsPaymentStatus
    {
        [Display(Name = "Ожидание оплаты")]
        WaitingForPayment = 0,
        [Display(Name = "Оплачен")]
        Paid = 1,
        [Display(Name = "Оплата отменена")]
        Cancelled = 2,
        [Display(Name = "Готов к отправке")]
        ReadyToSend = 3,
    }
}
