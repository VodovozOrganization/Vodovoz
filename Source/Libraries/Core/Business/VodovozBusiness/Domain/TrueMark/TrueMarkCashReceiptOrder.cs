using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.TrueMark
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "заказы с честным знаком для отправки чеков",
			Nominative = "заказ с честным знаком для отправки чеков"
		)
	]
	public class TrueMarkCashReceiptOrder : PropertyChangedBase, IDomainObject
	{
		private Order _order;
		private DateTime _date;
		private TrueMarkCashReceiptOrderStatus _status;
		private string _unscannedCodesReason;
		private string _errorDescription;
		private CashReceipt _cashReceipt;
		private IList<TrueMarkCashReceiptProductCode> _scannedCodes = new List<TrueMarkCashReceiptProductCode>();
		private GenericObservableList<TrueMarkCashReceiptProductCode> _observableScannedCodes;

		public virtual int Id { get; set; }

		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		[Display(Name = "Статус")]
		public virtual TrueMarkCashReceiptOrderStatus Status
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

		[Display(Name = "Кассовый чек")]
		public virtual CashReceipt CashReceipt
		{
			get => _cashReceipt;
			set => SetField(ref _cashReceipt, value);
		}


		[Display(Name = "Отсканированные коды")]
		public virtual IList<TrueMarkCashReceiptProductCode> ScannedCodes
		{
			get => _scannedCodes;
			set => SetField(ref _scannedCodes, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<TrueMarkCashReceiptProductCode> ObservableScannedCodes
		{
			get
			{
				if(_observableScannedCodes == null)
				{
					_observableScannedCodes = new GenericObservableList<TrueMarkCashReceiptProductCode>(ScannedCodes);
				}

				return _observableScannedCodes;
			}
		}
	}
}
