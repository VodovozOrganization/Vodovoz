using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Linq;
using QSOrmProject;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры",
		Nominative = "номенклатура")]
	public class Nomenclature: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Название номенклатуры должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string model;

		[Display (Name = "Модель оборудования")]
		public virtual string Model {
			get { return model; }
			set { SetField (ref model, value, () => Model); }
		}

		double weight;

		[Display (Name = "Вес")]
		public virtual double Weight {
			get { return weight; }
			set { SetField (ref weight, value, () => Weight); }
		}

		VAT vAT;

		[Display (Name = "НДС")]
		public virtual VAT VAT {
			get { return vAT; }
			set { SetField (ref vAT, value, () => VAT); }
		}

		bool doNotReserve;

		[Display (Name = "Не резервировать")]
		public virtual bool DoNotReserve {
			get { return doNotReserve; }
			set { SetField (ref doNotReserve, value, () => DoNotReserve); }
		}

		bool serial;

		[Display (Name = "Серийный номер")]
		public virtual bool Serial {
			get { return serial; }
			set { SetField (ref serial, value, () => Serial); }
		}

		MeasurementUnits unit;

		[Display (Name = "Единица измерения")]
		public virtual MeasurementUnits Unit {
			get { return unit; }
			set { SetField (ref unit, value, () => Unit); }
		}

		NomenclatureCategory category;

		[Display (Name = "Категория")]
		public virtual NomenclatureCategory Category {
			get { return category; }
			set { SetField (ref category, value, () => Category); }
		}

		EquipmentColors color;

		[Display (Name = "Цвет оборудования")]
		public virtual EquipmentColors Color {
			get { return color; }
			set { SetField (ref color, value, () => Color); }
		}

		EquipmentType type;

		[Display (Name = "Тип оборудования")]
		public virtual EquipmentType Type {
			get { return type; }
			set { SetField (ref type, value, () => Type); }
		}

		Manufacturer manufacturer;

		[Display (Name = "Производитель")]
		public virtual Manufacturer Manufacturer {
			get { return manufacturer; }
			set { SetField (ref manufacturer, value, () => Manufacturer); }
		}

		Logistic.RouteColumn routeListColumn;

		[Display (Name = "Производитель")]
		public virtual Logistic.RouteColumn RouteListColumn {
			get { return routeListColumn; }
			set { SetField (ref routeListColumn, value, () => RouteListColumn); }
		}

		IList<NomenclaturePrice> nomenclaturePrice;

		[Display (Name = "Цены")]
		public virtual IList<NomenclaturePrice> NomenclaturePrice {
			get { return nomenclaturePrice; }
			set { SetField (ref nomenclaturePrice, value, () => NomenclaturePrice); }
		}

		#endregion

		public Nomenclature ()
		{
			Name = String.Empty;
			Model = String.Empty;
			Category = NomenclatureCategory.water;
		}

		public virtual string CategoryString { get { return Category.GetEnumTitle (); } }

		public decimal GetPrice (int itemsCount)
		{
			var price = NomenclaturePrice
				.OrderByDescending (p => p.MinCount).FirstOrDefault (p => (p.MinCount <= itemsCount));

			return price != null ? price.Price : 0m;
		}

		#region statics

		public static NomenclatureCategory[] GetCategoriesForProductMaterial ()
		{
			return new [] { NomenclatureCategory.material, NomenclatureCategory.bottle };
		}

		public static NomenclatureCategory[] GetCategoriesForSale ()
		{
			return new [] { NomenclatureCategory.additional, NomenclatureCategory.equipment, NomenclatureCategory.water };
		}

		public static NomenclatureCategory[] GetCategoriesForGoods ()
		{
			return new [] { NomenclatureCategory.bottle, NomenclatureCategory.additional, 
				NomenclatureCategory.equipment, NomenclatureCategory.material, 
				NomenclatureCategory.spare_parts, NomenclatureCategory.water
			};
		}

		#endregion
	}

	public enum NomenclatureCategory
	{
		[ItemTitleAttribute ("Аренда")] rent,
		[ItemTitleAttribute ("Вода в многооборотной таре")] water,
		[ItemTitleAttribute ("Залог")] deposit,
		[ItemTitleAttribute ("Запчасти")] spare_parts,
		[ItemTitleAttribute ("Оборудование")] equipment,
		[ItemTitleAttribute ("Товары")] additional,
		[ItemTitleAttribute ("Услуга")] service,
		[ItemTitleAttribute ("Тара")] bottle,
		[ItemTitleAttribute ("Сырьё")] material
	}

	public class NomenclatureCategoryStringType : NHibernate.Type.EnumStringType
	{
		public NomenclatureCategoryStringType () : base (typeof(NomenclatureCategory))
		{
		}
	}
}

