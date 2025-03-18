using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.TrueMark
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "кассовый чек",
			Nominative = "кассовый чек"
		)
	]
	public class CashReceipt : PropertyChangedBase, IDomainObject
	{
		private Order _order;
		private DateTime _createDate;
		private DateTime _updateDate;
		private CashReceiptStatus _status;
		private string _unscannedCodesReason;
		private string _errorDescription;
		private FiscalDocumentStatus _fiscalDocumentStatus;
		private long? _fiscalDocumentNumber;
		private DateTime? _fiscalDocumentDate;
		private DateTime? _fiscalDocumentStatusChangeTime;
		private decimal? _sum;
		private bool _manualSent;
		private string _contact;
		private bool _withoutMarks;
		private int? _innerNumber;
		private int? _сashboxId;
		private int? _edoTaskId;
		private IList<CashReceiptProductCode> _scannedCodes = new List<CashReceiptProductCode>();
		private GenericObservableList<CashReceiptProductCode> _observableScannedCodes;

		public virtual int Id { get; set; }

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Дата создания")]
		public virtual DateTime CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		[Display(Name = "Дата изменения")]
		public virtual DateTime UpdateDate
		{
			get => _updateDate;
			set => SetField(ref _updateDate, value);
		}

		[Display(Name = "Статус")]
		public virtual CashReceiptStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Причина не отсканированных кодов")]
		public virtual string UnscannedCodesReason
		{
			get => _unscannedCodesReason;
			set => SetField(ref _unscannedCodesReason, value);
		}

		[Display(Name = "Описание ошибки")]
		public virtual string ErrorDescription
		{
			get => _errorDescription;
			set => SetField(ref _errorDescription, value);
		}

		[Display(Name = "Статус фискального документа")]
		public virtual FiscalDocumentStatus FiscalDocumentStatus
		{
			get => _fiscalDocumentStatus;
			set => SetField(ref _fiscalDocumentStatus, value);
		}

		[Display(Name = "Номер фискального документа")]
		public virtual long? FiscalDocumentNumber
		{
			get => _fiscalDocumentNumber;
			set => SetField(ref _fiscalDocumentNumber, value);
		}

		[Display(Name = "Дата фискального документа")]
		public virtual DateTime? FiscalDocumentDate
		{
			get => _fiscalDocumentDate;
			set => SetField(ref _fiscalDocumentDate, value);
		}

		[Display(Name = "Время смены статуса фискального документа")]
		public virtual DateTime? FiscalDocumentStatusChangeTime
		{
			get => _fiscalDocumentStatusChangeTime;
			set => SetField(ref _fiscalDocumentStatusChangeTime, value);
		}

		[Display(Name = "Сумма чека")]
		public virtual decimal? Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}

		[Display(Name = "Отправлен вручную")]
		public virtual bool ManualSent
		{
			get => _manualSent;
			set => SetField(ref _manualSent, value);
		}

		[Display(Name = "Контакт для чека")]
		public virtual string Contact
		{
			get => _contact;
			set => SetField(ref _contact, value);
		}

		[Display(Name = "Без маркировки (архив)")]
		public virtual bool WithoutMarks
		{
			get => _withoutMarks;
			set => SetField(ref _withoutMarks, value);
		}
		
		[Display(Name = "Порядковый номер чека")]
		public virtual int? InnerNumber
		{
			get => _innerNumber;
			set => SetField(ref _innerNumber, value);
		}

		[Display(Name = "Касса куда был отправлен чек")]
		public virtual int? CashboxId
		{
			get => _сashboxId;
			set => SetField(ref _сashboxId, value);
		}

		[Display(Name = "Отсканированные коды")]
		public virtual IList<CashReceiptProductCode> ScannedCodes
		{
			get => _scannedCodes;
			set => SetField(ref _scannedCodes, value);
		}
		
		[Display(Name = "Id задачи на создание чека")]
		public virtual int? EdoTaskId
		{
			get => _edoTaskId;
			set => SetField(ref _edoTaskId, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CashReceiptProductCode> ObservableScannedCodes
		{
			get
			{
				if(_observableScannedCodes == null)
				{
					_observableScannedCodes = new GenericObservableList<CashReceiptProductCode>(ScannedCodes);
				}

				return _observableScannedCodes;
			}
		}

		public static string GetDocumentId(int orderId, int? innerNumber)
		{
			return innerNumber is null ? $"vod_{orderId}" : $"vod_{orderId}_{innerNumber}";
		}

		public virtual string DocumentId => GetDocumentId(Order.Id, InnerNumber);

		public static int MaxMarkCodesInReceipt => 100;
	}
}
