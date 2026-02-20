using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Services;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Domain.FastPayments
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "быстрые платежи",
		Nominative = "быстрый платеж")]
	[HistoryTrace]
	public class FastPayment : PropertyChangedBase, IDomainObject
	{
		private string _ticket;
		private string _qrPngBase64;
		private string _phoneNumber;
		private string _callbackUrlForMobileApp;
		private string _callbackUrlForAiBot;
		private Order _order;
		private Organization _organization;
		private PaymentFrom _paymentByCardFrom;
		private PaymentType _paymentType;
		private DateTime _creationDate;
		private DateTime? _paidDate;
		private FastPaymentStatus _fastPaymentStatus;
		private decimal _amount;
		private int _externalId;
		private int? _onlineOrderId;
		private Guid _fastPaymnetGuid;
		private FastPaymentPayType _fastPaymentPayType;

		public virtual int Id { get; set; }
		
		[Display(Name = "Статус оплаты")]
		public virtual FastPaymentStatus FastPaymentStatus
		{
			get => _fastPaymentStatus;
			protected set => SetField(ref _fastPaymentStatus, value);
		}

		[Display(Name = "Сумма платежа")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Организация")]
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		[Display(Name = "Тип оплаты по карте")]
		public virtual PaymentType PaymentType
		{
			get => _paymentType;
			set => SetField(ref _paymentType, value);
		}

		[Display(Name = "Источник оплаты по карте")]
		public virtual PaymentFrom PaymentByCardFrom
		{
			get => _paymentByCardFrom;
			set => SetField(ref _paymentByCardFrom, value);
		}

		[Display(Name = "Сессия оплаты")]
		public virtual string Ticket
		{
			get => _ticket;
			set => SetField(ref _ticket, value);
		}
		
		[IgnoreHistoryTrace]
		[Display(Name = "Qr-код")]
		public virtual string QRPngBase64
		{
			get => _qrPngBase64;
			set => SetField(ref _qrPngBase64, value);
		}
		
		[Display(Name = "Дата создания")]
		public virtual DateTime CreationDate
		{
			get => _creationDate;
			set => SetField(ref _creationDate, value);
		}
		
		[Display(Name = "Дата оплаты")]
		public virtual DateTime? PaidDate
		{
			get => _paidDate;
			set => SetField(ref _paidDate, value);
		}
		
		[Display(Name = "Внешний ID")]
		public virtual int ExternalId {
			get => _externalId;
			set => SetField(ref _externalId, value);
		}
		
		[Display(Name = "Онлайн-заказ")]
		public virtual int? OnlineOrderId {
			get => _onlineOrderId;
			set => SetField(ref _onlineOrderId, value);
		}
		
		[Display(Name = "Номер телефона")]
		public virtual string PhoneNumber
		{
			get => _phoneNumber;
			set => SetField(ref _phoneNumber, value);
		}

		[IgnoreHistoryTrace]
		public virtual Guid FastPaymentGuid
		{
			get => _fastPaymnetGuid;
			set => SetField(ref _fastPaymnetGuid, value);
		}

		public virtual FastPaymentPayType FastPaymentPayType
		{
			get => _fastPaymentPayType;
			set => SetField(ref _fastPaymentPayType, value);
		}

		[IgnoreHistoryTrace]
		[Display(Name = "Адрес коллбэка для мобильного приложения")]
		public virtual string CallbackUrlForMobileApp
		{
			get => _callbackUrlForMobileApp;
			set => SetField(ref _callbackUrlForMobileApp, value);
		}
		
		/// <summary>
		/// Адрес коллбэка для ИИ Бота
		/// </summary>
		[IgnoreHistoryTrace]
		[Display(Name = "Адрес коллбэка для ИИ Бота")]
		public virtual string CallbackUrlForAiBot
		{
			get => _callbackUrlForAiBot;
			set => SetField(ref _callbackUrlForAiBot, value);
		}

		public virtual void SetProcessingStatus()
		{
			FastPaymentStatus = FastPaymentStatus.Processing;
		}
		
		public virtual void SetPerformedStatusForOrder(
			IUnitOfWork uow,
			DateTime paidDate,
			INomenclatureSettings nomenclatureSettings,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository,
			IOrderContractUpdater contractUpdater)
		{
			FastPaymentStatus = FastPaymentStatus.Performed;

			var selfDeliveryOrderPaymentTypes = new PaymentType[] { PaymentType.Cash, PaymentType.SmsQR };

			if(selfDeliveryOrderPaymentTypes.Contains(Order.PaymentType)
				&& Order.SelfDelivery
				&& Order.OrderStatus == OrderStatus.WaitForPayment
				&& Order.PayAfterShipment)
			{
				Order.TryCloseSelfDeliveryPayAfterShipmentOrder(
					uow,
					nomenclatureSettings,
					routeListItemRepository,
					selfDeliveryRepository,
					cashRepository);
				Order.IsSelfDeliveryPaid = true;
			}

			if(selfDeliveryOrderPaymentTypes.Contains(Order.PaymentType)
				&& Order.SelfDelivery
				&& Order.OrderStatus == OrderStatus.WaitForPayment
				&& !Order.PayAfterShipment)
			{
				Order.ChangeStatus(OrderStatus.OnLoading);
				Order.IsSelfDeliveryPaid = true;
			}
			
			PaidDate = paidDate;
			Order.OnlinePaymentNumber = ExternalId;
			Order.UpdatePaymentType(PaymentType, contractUpdater, false);
			Order.UpdatePaymentByCardFrom(PaymentByCardFrom, contractUpdater, false);
			contractUpdater.ForceUpdateContract(uow, Order, Organization);

			foreach(var routeListItem in routeListItemRepository.GetRouteListItemsForOrder(uow, Order.Id))
			{
				routeListItem.RecalculateTotalCash();
				uow.Save(routeListItem);
			}
		}
		
		public virtual void SetPerformedStatusForOnlineOrder(DateTime paidDate)
		{
			FastPaymentStatus = FastPaymentStatus.Performed;
			PaidDate = paidDate;
		}
		
		public virtual void SetRejectedStatus()
		{
			FastPaymentStatus = FastPaymentStatus.Rejected;
		}
	}

	public class FastPaymentStatusStringType : EnumStringType<FastPaymentStatus> { }
	public class FastPaymentPayTypeStringType : EnumStringType<FastPaymentPayType> { }
}
