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
		private bool _isUnscannedSourceCode;
		private bool _isDuplicateSourceCode;
		private bool _isDefectiveSourceCode;
		private TrueMarkWaterIdentificationCode _sourceCode;
		private TrueMarkWaterIdentificationCode _resultCode;

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

		/// <summary>
		/// Заполняется автоматически, если водитель не отсканировал все коды.
		/// Код же подбирается из пула
		/// </summary>
		[Display(Name = "Не отсканированный код источника")]
		public virtual bool IsUnscannedSourceCode
		{
			get => _isUnscannedSourceCode;
			set => SetField(ref _isUnscannedSourceCode, value);
		}

		[Display(Name = "Код источник бракованный")]
		public virtual bool IsDefectiveSourceCode
		{
			get => _isDefectiveSourceCode;
			set => SetField(ref _isDefectiveSourceCode, value);
		}

		[Display(Name = "Дубликат кода источника")]
		public virtual bool IsDuplicateSourceCode
		{
			get => _isDuplicateSourceCode;
			set => SetField(ref _isDuplicateSourceCode, value);
		}

		[Display(Name = "Код источник")]
		public virtual TrueMarkWaterIdentificationCode SourceCode
		{
			get => _sourceCode;
			set => SetField(ref _sourceCode, value);
		}

		[Display(Name = "Код результат")]
		public virtual TrueMarkWaterIdentificationCode ResultCode
		{
			get => _resultCode;
			set => SetField(ref _resultCode, value);
		}
	}
}
