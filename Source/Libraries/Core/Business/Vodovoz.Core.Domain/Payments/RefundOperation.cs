using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Операция возврата средств
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Nominative = "операция возврата",
		NominativePlural = "операции возврата",
		Accusative = "операции возврата",
		AccusativePlural = "операций возврата",
		Genitive = "операции возврата",
		GenitivePlural = "операций возврата",
		Prepositional = "операции возврата",
		PrepositionalPlural = "операциях возврата"
	)]
	[HistoryTrace]
	public class RefundOperation : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _idempotenceKey;
		private int _onlineOrderId;
		private string _transactionId;
		private string _refundId;
		private OnlinePaymentSource? _paymentSource;
		private DateTime _createdAt;
		private bool _isSuccess;
		private string _errorMessage;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификтор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Ключ идемпотентности
		/// </summary>
		[Display(Name = "Ключ идемпотентности")]
		public virtual string IdempotenceKey
		{
			get => _idempotenceKey;
			set => SetField(ref _idempotenceKey, value);
		}

		/// <summary>
		/// ID онлайн заказа
		/// </summary>
		[Display(Name = "ID онлайн заказа")]
		public virtual int OnlineOrderId
		{
			get => _onlineOrderId;
			set => SetField(ref _onlineOrderId, value);
		}

		/// <summary>
		/// ID транзакции в платёжной системе
		/// </summary>
		[Display(Name = "ID транзакции")]
		public virtual string TransactionId
		{
			get => _transactionId;
			set => SetField(ref _transactionId, value);
		}

		/// <summary>
		/// ID возврата в платёжной системе
		/// </summary>
		[Display(Name = "ID возврата")]
		public virtual string RefundId
		{
			get => _refundId;
			set => SetField(ref _refundId, value);
		}

		/// <summary>
		/// Источник оплаты
		/// </summary>
		[Display(Name = "Источник оплаты")]
		public virtual OnlinePaymentSource? PaymentSource
		{
			get => _paymentSource;
			set => SetField(ref _paymentSource, value);
		}

		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
		}

		/// <summary>
		/// Успешен ли возврат
		/// </summary>
		[Display(Name = "Успешно")]
		public virtual bool IsSuccess
		{
			get => _isSuccess;
			set => SetField(ref _isSuccess, value);
		}

		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		[Display(Name = "Ошибка")]
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}

		/// <summary>
		/// Создать новую операцию возврата
		/// </summary>
		public static RefundOperation Create(
			string key,
			int onlineOrderId,
			string transactionId,
			OnlinePaymentSource? paymentSource)
		{
			var now = DateTime.UtcNow;

			return new RefundOperation
			{
				IdempotenceKey = key,
				OnlineOrderId = onlineOrderId,
				TransactionId = transactionId,
				PaymentSource = paymentSource,
				CreatedAt = now
			};
		}

		/// <summary>
		/// Отметить возврат как успешный
		/// </summary>
		public virtual void MarkAsSucceeded(string refundId)
		{
			IsSuccess = true;
			RefundId = refundId;
			ErrorMessage = null;
		}

		/// <summary>
		/// Отметить возврат как ошибочный
		/// </summary>
		public virtual void MarkAsFailed(string errorMessage)
		{
			IsSuccess = false;
			ErrorMessage = errorMessage;
		}
	}
}
