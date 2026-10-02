using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Receipts
{
	/// <summary>
	/// Строка таблицы позиций объяснительной записки.
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "позиция объяснительной записки",
		NominativePlural = "позиции объяснительной записки")]
	public class ReceiptCorrectionExplanatoryNoteItem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private ReceiptCorrectionExplanatoryNote _explanatoryNote;
		private int _lineNumber;
		private int? _nomenclatureId;
		private string _name;
		private decimal _quantity;
		private decimal _price;
		private decimal _discountSum;
		private decimal _sum;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Объяснительная записка")]
		public virtual ReceiptCorrectionExplanatoryNote ExplanatoryNote
		{
			get => _explanatoryNote;
			set => SetField(ref _explanatoryNote, value);
		}

		[Display(Name = "Номер строки")]
		public virtual int LineNumber
		{
			get => _lineNumber;
			set => SetField(ref _lineNumber, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual int? NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}

		[Display(Name = "Наименование")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Количество")]
		public virtual decimal Quantity
		{
			get => _quantity;
			set => SetField(ref _quantity, value);
		}

		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		[Display(Name = "Скидка")]
		public virtual decimal DiscountSum
		{
			get => _discountSum;
			set => SetField(ref _discountSum, value);
		}

		[Display(Name = "Сумма")]
		public virtual decimal Sum
		{
			get => _sum;
			set => SetField(ref _sum, value);
		}
	}
}
