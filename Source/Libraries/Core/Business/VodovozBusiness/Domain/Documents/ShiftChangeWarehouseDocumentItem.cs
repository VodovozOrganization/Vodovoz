using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки акта передачи склада(объемный учет)",
		Nominative = "строка акта передачи склада(объемный учет)")]
	[HistoryTrace]
	public class ShiftChangeWarehouseDocumentItem : PropertyChangedBase, IDomainObject
	{
		private Nomenclature _nomenclature;
		private decimal _amountInDb;
		private decimal _amountInFact;
		private string _comment;

		public virtual int Id { get; set; }

		public virtual ShiftChangeWarehouseDocument Document { get; set; }

		[Required(ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Количество по базе")]
		public virtual decimal AmountInDB
		{
			get => _amountInDb;
			set => SetField(ref _amountInDb, value);
		}

		[Display(Name = "Количество по базе")]
		[PropertyChangedAlso("SumOfDamage")]
		public virtual decimal AmountInFact
		{
			get => _amountInFact;
			set => SetField(ref _amountInFact, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage => Difference > 0 ? 0 : Nomenclature.SumOfDamage * Math.Abs(Difference);

		#region Расчетные

		public virtual string Title => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(AmountInFact)}";

		public virtual decimal Difference => AmountInFact - AmountInDB;

		#endregion
	}
}
