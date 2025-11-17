using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	public class SelectedNomenclaturePlan : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }
	}

	public class SelectedNomenclature : SelectedNomenclaturePlan
	{
		public virtual Nomenclature Nomenclature { get; set; }
	}

	public class SelectedEquipmentKind : SelectedNomenclaturePlan
	{
		public virtual EquipmentKind EquipmentKind { get; set; }
	}

	public class SelectedEquipmentType : SelectedNomenclaturePlan
	{
		public virtual EquipmentType EquipmentType { get; set; }
	}

	public class SelectedProceeds : SelectedNomenclaturePlan
	{
		public virtual bool InludeProceeds { get; set; }
	}


}
