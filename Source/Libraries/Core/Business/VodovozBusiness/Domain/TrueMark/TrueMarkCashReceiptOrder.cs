using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
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

		public int Id { get; set; }

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
	}
}
