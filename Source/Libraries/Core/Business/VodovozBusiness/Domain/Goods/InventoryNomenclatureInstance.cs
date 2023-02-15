﻿using System.ComponentModel.DataAnnotations;
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

		public override string ToString()
		{
			if(Id == 0 && Nomenclature == null)
			{
				return "Новый экземпляр";
			}
			
			return $"{Nomenclature.Name} инв. номер: {InventoryNumber}";
		}
	}
}
