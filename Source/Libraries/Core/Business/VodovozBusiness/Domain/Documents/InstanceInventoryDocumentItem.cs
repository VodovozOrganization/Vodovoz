using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	public class InstanceInventoryDocumentItem : PropertyChangedBase, IDomainObject
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;
		private string _comment;
		private bool _isMissing;
		private Fine _fine;

		public virtual int Id { get; set; }

		public virtual InventoryDocument Document { get; set; }
		
		[Display(Name = "Экземпляр")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set => SetField(ref _inventoryNomenclatureInstance, value);
		}
		
		[Display (Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}
		
		[Display (Name = "Отсутствует")]
		public virtual bool IsMissing
		{
			get => _isMissing;
			set => SetField(ref _isMissing, value);
		}

		[Display (Name = "Штраф")]
		public virtual Fine Fine
		{
			get => _fine;
			set => SetField(ref _fine, value);
		}
	}
}
