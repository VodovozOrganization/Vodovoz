using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	public class InstanceShiftChangeWarehouseDocumentItem : PropertyChangedBase, IDomainObject
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;
		private string _comment;
		private bool _isMissing;
		private bool _canChangeIsMissing = true;
		private string _discrepancyDescription;

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

		[IgnoreHistoryTrace]
		[Display(Name = "Можно менять параметр отсутствует?")]
		public virtual bool CanChangeIsMissing
		{
			get => _canChangeIsMissing;
			set => SetField(ref _canChangeIsMissing, value);
		}
		
		[IgnoreHistoryTrace]
		[Display(Name = "Описание расхождения")]
		public virtual string DiscrepancyDescription
		{
			get => _discrepancyDescription;
			set => SetField(ref _discrepancyDescription, value);
		}
		
		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage =>
			InventoryNomenclatureInstance != null ? InventoryNomenclatureInstance.Nomenclature.SumOfDamage : 0;
	}
}
