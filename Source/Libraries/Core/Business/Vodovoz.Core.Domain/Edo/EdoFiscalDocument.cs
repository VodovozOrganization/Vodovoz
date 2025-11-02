using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Фискальный документ
	/// Информация о чеке, которая была отправлена в фискальный регистратор
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "фискальный документ",
		NominativePlural = "фискальные документы"
	)]
	public class EdoFiscalDocument : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _creationTime;
		private DateTime _version;
		private ReceiptEdoTask _receiptEdoTask;
		private int _index;
		private FiscalDocumentStage _stage;
		private FiscalDocumentStatus _status;
		private DateTime? _statusChangeTime;
		private DateTime? _fiscalTime;
		private string _fiscalNumber;
		private string _fiscalMark;
		private string _fiscalKktNumber;
		private string _failureMessage;

		private Guid _documentGuid;
		private string _documentNumber;
		private FiscalDocumentType _documentType;
		private DateTime _checkoutTime;
		private string _contact;
		private string _clientInn;
		private string _cashierName;
		private bool _printReceipt;
		private IObservableList<FiscalInventPosition> _inventPositions = new ObservableList<FiscalInventPosition>();
		private IObservableList<FiscalMoneyPosition> _moneyPositions = new ObservableList<FiscalMoneyPosition>();


		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Время создания
		/// </summary>
		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		/// <summary>
		/// Версия (время последнего изменения)
		/// </summary>
		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		[Display(Name = "ЭДО задача")]
		public virtual ReceiptEdoTask ReceiptEdoTask
		{
			get => _receiptEdoTask;
			set => SetField(ref _receiptEdoTask, value);
		}

		[Display(Name = "Индекс")]
		public virtual int Index
		{
			get => _index;
			set => SetField(ref _index, value);
		}

		[Display(Name = "Стадия")]
		public virtual FiscalDocumentStage Stage
		{
			get => _stage;
			set => SetField(ref _stage, value);
		}


		/// <summary>
		/// Статус
		/// </summary>
		[Display(Name = "Статус")]
		public virtual FiscalDocumentStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		/// <summary>
		/// Время изменения статуса
		/// </summary>
		[Display(Name = "Время изменения статуса")]
		public virtual DateTime? StatusChangeTime
		{
			get => _statusChangeTime;
			set => SetField(ref _statusChangeTime, value);
		}

		/// <summary>
		/// Время фискализации
		/// </summary>
		[Display(Name = "Время фискализации")]
		public virtual DateTime? FiscalTime
		{
			get => _fiscalTime;
			set => SetField(ref _fiscalTime, value);
		}

		/// <summary>
		/// Фискальный номер документа
		/// </summary>
		[Display(Name = "Фискальный номер документа")]
		public virtual string FiscalNumber
		{
			get => _fiscalNumber;
			set => SetField(ref _fiscalNumber, value);
		}

		/// <summary>
		/// Фискальный признак документа
		/// </summary>
		[Display(Name = "Фискальный признак документа")]
		public virtual string FiscalMark
		{
			get => _fiscalMark;
			set => SetField(ref _fiscalMark, value);
		}

		/// <summary>
		/// Регистрационный номер ККТ
		/// </summary>
		[Display(Name = "Регистрационный номер ККТ")]
		public virtual string FiscalKktNumber
		{
			get => _fiscalKktNumber;
			set => SetField(ref _fiscalKktNumber, value);
		}

		/// <summary>
		/// Описание проблемы
		/// </summary>
		[Display(Name = "Описание проблемы")]
		public virtual string FailureMessage
		{
			get => _failureMessage;
			set => SetField(ref _failureMessage, value);
		}

		#region Поля фискального документа

		/// <summary>
		/// Уникальный код документа на стороне магазина
		/// </summary>
		[Display(Name = "Уникальный код документа")]
		public virtual Guid DocumentGuid
		{
			get => _documentGuid;
			set => SetField(ref _documentGuid, value);
		}

		/// <summary>
		/// Номер документа в формате магазина
		/// </summary>
		[Display(Name = "Номер документа")]
		public virtual string DocumentNumber
		{
			get => _documentNumber;
			set => SetField(ref _documentNumber, value);
		}

		/// <summary>
		/// Тип документа
		/// </summary>
		[Display(Name = "Тип документа")]
		public virtual FiscalDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		/// <summary>
		/// Время создания документа на оплату
		/// </summary>
		[Display(Name = "Время создания документа на оплату")]
		public virtual DateTime CheckoutTime
		{
			get => _checkoutTime;
			set => SetField(ref _checkoutTime, value);
		}

		/// <summary>
		/// Телефон или электронный адрес почты покупателя
		/// </summary>
		[Display(Name = "Контакт")]
		public virtual string Contact
		{
			get => _contact;
			set => SetField(ref _contact, value);
		}

		/// <summary>
		/// ИНН покупателя
		/// </summary>
		[Display(Name = "ИНН покупателя")]
		public virtual string ClientInn
		{
			get => _clientInn;
			set => SetField(ref _clientInn, value);
		}

		/// <summary>
		/// Имя кассира
		/// </summary>
		[Display(Name = "Имя кассира")]
		public virtual string CashierName
		{
			get => _cashierName;
			set => SetField(ref _cashierName, value);
		}

		/// <summary>
		/// Печатать чек
		/// </summary>
		[Display(Name = "Печатать чек")]
		public virtual bool PrintReceipt
		{
			get => _printReceipt;
			set => SetField(ref _printReceipt, value);
		}

		/// <summary>
		/// Товары
		/// </summary>
		[Display(Name = "Товары")]
		public virtual IObservableList<FiscalInventPosition> InventPositions
		{
			get => _inventPositions;
			set => SetField(ref _inventPositions, value);
		}

		/// <summary>
		/// Оплаты
		/// </summary>
		[Display(Name = "Оплаты")]
		public virtual IObservableList<FiscalMoneyPosition> MoneyPositions
		{
			get => _moneyPositions;
			set => SetField(ref _moneyPositions, value);
		}

		#endregion Поля фискального документа
	}
}
