using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.TrueMark
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды честного знака для отправки чеков",
			Nominative = "код честного знака для отправки чеков"
		)
	]
	public class TrueMarkCashReceiptProductCode : PropertyChangedBase, IDomainObject
	{
		private TrueMarkCashReceiptOrder _trueMarkCashReceiptOrder;
		private OrderItem _orderItem;
		private bool _isDefectiveSourceCode;
		private string _codeSource;
		private string _codeResult;

		public virtual int Id { get; set; }

		[Display(Name = "Заказ с честным знаком для отправки чека")]
		public virtual TrueMarkCashReceiptOrder TrueMarkCashReceiptOrder
		{
			get => _trueMarkCashReceiptOrder;
			set => SetField(ref _trueMarkCashReceiptOrder, value);
		}

		[Display(Name = "Строка заказа")]
		public virtual OrderItem OrderItem
		{
			get => _orderItem;
			set => SetField(ref _orderItem, value);
		}

		[Display(Name = "Код источник бракованный")]
		public virtual bool IsDefectiveSourceCode
		{
			get => _isDefectiveSourceCode;
			set => SetField(ref _isDefectiveSourceCode, value);
		}

		[Display(Name = "Код источник")]
		public virtual string CodeSource
		{
			get => _codeSource;
			set => SetField(ref _codeSource, value);
		}

		[Display(Name = "Код результат")]
		public virtual string CodeResult
		{
			get => _codeResult;
			set => SetField(ref _codeResult, value);
		}
	}
}
