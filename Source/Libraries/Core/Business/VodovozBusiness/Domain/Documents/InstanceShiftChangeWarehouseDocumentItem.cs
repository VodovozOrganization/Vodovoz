using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки акта передачи склада(экземплярный учет)",
		Nominative = "строка акта передачи склада(экземплярный учет)")]
	[HistoryTrace]
	public class InstanceShiftChangeWarehouseDocumentItem : PropertyChangedBase, IDomainObject
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;
		private string _comment;
		private bool _isMissing;
		private decimal _amountInDb;

		public virtual int Id { get; set; }

		public virtual ShiftChangeWarehouseDocument Document { get; set; }
		
		[Display(Name = "Экземпляр")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set => SetField(ref _inventoryNomenclatureInstance, value);
		}
		
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}
		
		[Display(Name = "Отсутствует")]
		public virtual bool IsMissing
		{
			get => _isMissing;
			set => SetField(ref _isMissing, value);
		}
		
		[Display (Name = "Количество по базе")]
		public virtual decimal AmountInDB
		{
			get => _amountInDb;
			set => SetField(ref _amountInDb, value);
		}

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage =>
			Difference > 0
				? 0
				: InventoryNomenclatureInstance.Nomenclature.SumOfDamage * Math.Abs(Difference);

		public virtual bool CanChangeIsMissing => AmountInDB != 0;

		public virtual decimal Difference => AmountInFact - AmountInDB;

		private decimal AmountInFact => IsMissing ? 0 : 1;
	}
}
