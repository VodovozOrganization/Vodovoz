using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;

namespace Vodovoz.Domain.Goods
{
	[HistoryTrace]
	public class InventoryNomenclatureInstance : NomenclatureInstance
	{
		private string _inventoryNumber;
		
		[Display(Name = "Инвентарный номер")]
		public virtual string InventoryNumber
		{
			get => _inventoryNumber;
			set => SetField(ref _inventoryNumber, value);
		}

		public override NomenclatureInstanceType Type => NomenclatureInstanceType.InventoryNomenclatureInstance;

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : string.Empty;
		
		public override string ToString()
		{
			if(Id == 0 && Nomenclature == null)
			{
				return "Новый экземпляр";
			}
			
			return $"Экземпляр №{Id} {Nomenclature?.Name} инв. номер: {InventoryNumber}";
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach(var validationResult in base.Validate(validationContext))
			{
				yield return validationResult;
			}

			if(string.IsNullOrWhiteSpace(InventoryNumber))
			{
				yield return new ValidationResult("Инвентарный номер не заполнен");
			}
		}
	}
}
