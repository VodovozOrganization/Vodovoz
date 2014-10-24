using System;
using System.Data.Bindings;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubjectAttibutes("Номенклатуры")]
	[Magic]
	public class Nomenclature
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Model { get; set; }
		public virtual double Weight { get; set; }
		public virtual double Price { get; set; }
		public virtual VAT VAT { get; set; }
		public virtual bool DoNotReserve { get; set; }
		public virtual bool Serial { get; set; }
		public virtual MeasurementUnits Unit { get; set; }
		public virtual NomenclatureCategory Category { get; set; }
		public virtual EquipmentColors Color { get; set; }
		public virtual EquipmentType Type { get; set; }
		public virtual Manufacturer Manufacturer { get; set; }
		#endregion

		public Nomenclature()
		{
			Name = String.Empty;
			Model = String.Empty;
			Category = NomenclatureCategory.water;
		}
	}

	public enum VAT{
		[ItemTitleAttribute("Без НДС")] excluded,
		[ItemTitleAttribute("НДС 18%")] included
	}

	public class VATStringType : NHibernate.Type.EnumStringType
	{
		public VATStringType() : base(typeof(VAT))
		{}
	}

	public enum NomenclatureCategory{
		[ItemTitleAttribute("Вода")] water,
		[ItemTitleAttribute("Оборудование")] equipment,
		[ItemTitleAttribute("Услуга")] service,
		[ItemTitleAttribute("Аренда")] rent,
		[ItemTitleAttribute("Запчасти")] spare_parts,
		[ItemTitleAttribute("Дополнительно")] additional
	}

	public class NomenclatureCategoryStringType : NHibernate.Type.EnumStringType 
	{
		public NomenclatureCategoryStringType() : base(typeof(NomenclatureCategory))
		{}
	}
}

