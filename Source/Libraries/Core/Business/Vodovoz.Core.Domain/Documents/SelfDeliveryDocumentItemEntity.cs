using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Строка документа самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentItemEntity : PropertyChangedBase, IDomainObject
	{
		private decimal _amount;
		private IObservableList<SelfDeliveryDocumentItemTrueMarkProductCode> _trueMarkProductCodes = new ObservableList<SelfDeliveryDocumentItemTrueMarkProductCode>();
		private SelfDeliveryDocumentEntity _document;
		private NomenclatureEntity _nomenclature;
		private decimal _amountInStock;
		private decimal _amountUnloaded;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		/// <summary>
		/// Документ самовывоза, к которому относится строка
		/// </summary>
		[Display(Name = "Документ самовывоза")]
		public virtual SelfDeliveryDocumentEntity Document
		{
			get => _document;
			set => SetField(ref _document, value);
		}

		/// <summary>
		/// Коды ЧЗ товаров, которые были отсканированы в строке документа самовывоза
		/// </summary>
		[Display(Name = "Коды ЧЗ товаров")]
		public virtual IObservableList<SelfDeliveryDocumentItemTrueMarkProductCode> TrueMarkProductCodes
		{
			get => _trueMarkProductCodes;
			set => SetField(ref _trueMarkProductCodes, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			//Нельзя устанавливать, см. логику в SelfDeliveryDocumentItem.cs
			protected set => SetField(ref _nomenclature, value);
		}

		#region Не сохраняемые

		/// <summary>
		/// Количество на складе
		/// </summary>
		[Display(Name = "Количество на складе")]
		public virtual decimal AmountInStock
		{
			get => _amountInStock;
			set => SetField(ref _amountInStock, value);
		}

		/// <summary>
		/// Уже отгружено
		/// </summary>
		[Display(Name = "Уже отгружено")]
		public virtual decimal AmountUnloaded
		{
			get => _amountUnloaded;
			set => SetField(ref _amountUnloaded, value);
		}

		#endregion
	}
}
