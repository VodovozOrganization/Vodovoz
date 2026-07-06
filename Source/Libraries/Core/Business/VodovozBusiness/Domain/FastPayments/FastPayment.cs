using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.FastPayments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

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

		/// <summary>
		/// Установить статус "Исполнен" с датой оплаты
		/// </summary>
		/// <param name="paidDate"></param>
		public virtual void SetPerformedStatusForOnlineOrder(DateTime paidDate)
		{
			FastPaymentStatus = FastPaymentStatus.Performed;
			PaidDate = paidDate;
		}

		/// <summary>
		/// Установить статус "Отбракован"
		/// </summary>
		public virtual void SetRejectedStatus()
		{
			FastPaymentStatus = FastPaymentStatus.Rejected;
		}

		/// <summary>
		/// Установить статус "Возврат"
		/// </summary>
		public virtual void SetRefundStatus()
		{
			FastPaymentStatus = FastPaymentStatus.Refund;
		}
	}

	public class FastPaymentStatusStringType : EnumStringType<FastPaymentStatus> { }
	public class FastPaymentPayTypeStringType : EnumStringType<FastPaymentPayType> { }
}
