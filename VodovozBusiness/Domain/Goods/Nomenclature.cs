using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QSBusinessCommon.Domain;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Goods
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры",
		Nominative = "номенклатура")]
	public class Nomenclature : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display(Name = "Название")]
		[StringLength(220)]
		[Required(ErrorMessage = "Название номенклатуры должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		string officialName;

		[Display(Name = "Официальное название")]
		[StringLength(220)]
		[Required(ErrorMessage = "Официальное название номенклатуры должно быть заполнено.")]
		public virtual string OfficialName {
			get { return officialName; }
			set { SetField(ref officialName, value, () => OfficialName); }
		}

		bool isArchive;

		[Display(Name = "Архивная")]
		public virtual bool IsArchive {
			get { return isArchive; }
			set { SetField(ref isArchive, value, () => IsArchive); }
		}

		bool isDiler;

		[Display(Name = "Дилер")]
		public virtual bool IsDiler {
			get { return isDiler; }
			set { SetField(ref isDiler, value, () => IsDiler); }
		}

		bool canPrintPrice;

		[Display(Name = "Печатается прайс в документах")]
		public virtual bool CanPrintPrice {
			get { return canPrintPrice; }
			set { SetField(ref canPrintPrice, value, () => CanPrintPrice); }
		}

		string code1c;
		[Display(Name = "Код 1с")]
		public virtual string Code1c {
			get { return code1c; }
			set { SetField(ref code1c, value, () => Code1c); }
		}

		string model;

		[Display(Name = "Модель оборудования")]
		public virtual string Model {
			get { return model; }
			set { SetField(ref model, value, () => Model); }
		}

		double weight;

		[Display(Name = "Вес")]
		public virtual double Weight {
			get { return weight; }
			set { SetField(ref weight, value, () => Weight); }
		}

		double volume;

		/// <summary>
		/// Объем измеряемый в квадратных метрах
		/// </summary>
		[Display(Name = "Объём")]
		public virtual double Volume {
			get { return volume; }
			set { SetField(ref volume, value, () => Volume); }
		}

		VAT vAT = VAT.Vat18;

		[Display(Name = "НДС")]
		public virtual VAT VAT {
			get { return vAT; }
			set { SetField(ref vAT, value, () => VAT); }
		}

		bool doNotReserve;

		[Display(Name = "Не резервировать")]
		public virtual bool DoNotReserve {
			get { return doNotReserve; }
			set { SetField(ref doNotReserve, value, () => DoNotReserve); }
		}

		bool rentPriority;

		[Display(Name = "Приоритет аренды")]
		public virtual bool RentPriority {
			get { return rentPriority; }
			set { SetField(ref rentPriority, value, () => RentPriority); }
		}

		bool isDuty;
		/// <summary>
		/// Дежурное оборудование
		/// </summary>
		[Display(Name = "Дежурное оборудование")]
		public virtual bool IsDuty {
			get { return isDuty; }
			set { SetField(ref isDuty, value, () => IsDuty); }
		}

		bool isSerial;

		[Display(Name = "Серийный номер")]
		public virtual bool IsSerial {
			get { return isSerial; }
			set { SetField(ref isSerial, value, () => IsSerial); }
		}

		MeasurementUnits unit;

		[Display(Name = "Единица измерения")]
		public virtual MeasurementUnits Unit {
			get { return unit; }
			set { SetField(ref unit, value, () => Unit); }
		}

		NomenclatureCategory category;

		[Display(Name = "Категория")]
		public virtual NomenclatureCategory Category {
			get { return category; }
			set {
				if(SetField(ref category, value, () => Category)) {
					if(Category != NomenclatureCategory.equipment)
						IsSerial = false;
				}

			}
		}

		EquipmentColors color;

		[Display(Name = "Цвет оборудования")]
		public virtual EquipmentColors Color {
			get { return color; }
			set { SetField(ref color, value, () => Color); }
		}

		EquipmentType type;

		[Display(Name = "Тип оборудования")]
		public virtual EquipmentType Type {
			get { return type; }
			set { SetField(ref type, value, () => Type); }
		}

		Manufacturer manufacturer;

		[Display(Name = "Производитель")]
		public virtual Manufacturer Manufacturer {
			get { return manufacturer; }
			set { SetField(ref manufacturer, value, () => Manufacturer); }
		}

		Logistic.RouteColumn routeListColumn;

		[Display(Name = "Производитель")]
		public virtual Logistic.RouteColumn RouteListColumn {
			get { return routeListColumn; }
			set { SetField(ref routeListColumn, value, () => RouteListColumn); }
		}

		Warehouse warehouse;

		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		decimal sumOfDamage;

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage {
			get { return sumOfDamage; }
			set {
				SetField(ref sumOfDamage, value, () => SumOfDamage);
			}
		}

		IList<NomenclaturePrice> nomenclaturePrice = new List<NomenclaturePrice>();

		[Display(Name = "Цены")]
		public virtual IList<NomenclaturePrice> NomenclaturePrice {
			get { return nomenclaturePrice; }
			set { SetField(ref nomenclaturePrice, value, () => NomenclaturePrice); }
		}

		private string shortName;

		[Display(Name = "Сокращенное название")]
		public virtual string ShortName {
			get { return shortName; }
			set { SetField(ref shortName, value, () => ShortName); }
		}

		bool hide;

		[Display(Name = "Скрыть из МЛ")]
		public virtual bool Hide {
			get { return hide; }
			set { SetField(ref hide, value, () => Hide); }
		}

		bool noDelivey;

		[Display(Name = "Доставка не требуется")]
		public virtual bool NoDelivey {
			get { return noDelivey; }
			set { SetField(ref noDelivey, value, () => NoDelivey); }
		}

		private bool isNewBottle;

		[Display(Name = "Это новая бутыль")]
		public virtual bool IsNewBottle {
			get { return isNewBottle; }
			set {
				SetField(ref isNewBottle, value, () => IsNewBottle);
				if(isNewBottle)
					IsDefectiveBottle = false;
			}
		}

		private bool isDefectiveBottle;

		[Display(Name = "Это бракованая бутыль")]
		public virtual bool IsDefectiveBottle {
			get { return isDefectiveBottle; }
			set {
				SetField(ref isDefectiveBottle, value, () => IsDefectiveBottle);
				if(isDefectiveBottle)
					IsNewBottle = false;
			}
		}

		private Nomenclature dependsOnNomenclature;

		[Display(Name = "Влияющая номенклатура")]
		public virtual Nomenclature DependsOnNomenclature {
			get { return dependsOnNomenclature; }
			set { SetField(ref dependsOnNomenclature, value, () => DependsOnNomenclature); }
		}

		#endregion

		#region Рассчетные

		public virtual string CategoryString { get { return Category.GetEnumTitle(); } }

		public virtual string ShortOrFullName {
			get {
				return String.IsNullOrWhiteSpace(ShortName) ? Name : ShortName;
			}
		}

		#endregion

		public override string ToString()
		{
			return String.Format("id ={0} Name = {1}", Id, Name);

		}

		public Nomenclature()
		{
			Category = NomenclatureCategory.water;
		}


		public virtual decimal GetPrice(int? itemsCount)
		{
			if(itemsCount < 1)
				itemsCount = 1;
			decimal price = 0m;
			if(DependsOnNomenclature != null) {
				price = DependsOnNomenclature.GetPrice(itemsCount);
			} else {
				var query = NomenclaturePrice
					.OrderByDescending(p => p.MinCount)
					.FirstOrDefault(p => (p.MinCount <= itemsCount));
				price = query == null ? 0 : query.Price;
			}
			return price;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
#if !SHORT
			if(GetCategoriesForShipment ().Contains (Category) && Warehouse == null)
				yield return new ValidationResult (
					String.Format ("Для номенклатур вида «{0}», необходимо указывать склад отгрузки.", Category.GetEnumTitle ()),
					new[] { this.GetPropertyName (o => o.Warehouse) });
#endif
			if(Category == NomenclatureCategory.equipment && Type == null)
				yield return new ValidationResult(
					String.Format("Не указан тип оборудования."),
					new[] { this.GetPropertyName(o => o.Type) });

			//Проверка зависимостей номенклатур #1: если есть зависимые
			IList<Nomenclature> dependedNomenclatures = Repository.NomenclatureRepository.GetDependedNomenclatures(UnitOfWorkFactory.CreateWithoutRoot(), this);
			if(dependedNomenclatures.Any()) {
				string dependedNomenclaturesText = String.Format("Цена данной номенклатуры не может зависеть от цены другой номенклатуры, т.к. от \"{0}\" зависят цены следующих номенклатур:\n", DependsOnNomenclature.ShortOrFullName);
				foreach(Nomenclature n in dependedNomenclatures)
					dependedNomenclaturesText += String.Format("{0}: {1} ({2})\n", n.Id, n.OfficialName, n.CategoryString);
				yield return new ValidationResult(dependedNomenclaturesText);
			}
		}

		#endregion

		#region statics

		public static NomenclatureCategory[] GetCategoriesForShipment()
		{
			return new[] {
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.water,
				NomenclatureCategory.disposableBottleWater,
				NomenclatureCategory.bottle,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.material
			};
		}

		public static NomenclatureCategory[] GetCategoriesForProductMaterial()
		{
			return new[] { NomenclatureCategory.material, NomenclatureCategory.bottle };
		}

		public static NomenclatureCategory[] GetCategoriesForSale()
		{
			return new[] {
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.water,
				NomenclatureCategory.disposableBottleWater,
				NomenclatureCategory.bottle,
				NomenclatureCategory.deposit,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.rent,
				NomenclatureCategory.service
			};
		}

		public static NomenclatureCategory[] GetCategoriesForMaster()
		{
			List<NomenclatureCategory> list = new List<NomenclatureCategory>(GetCategoriesForSale());
			list.Add(NomenclatureCategory.master);
			list.Add(NomenclatureCategory.spare_parts);
			return list.ToArray();
		}

		/// <summary>
		/// Категории товаров. Товары могут хранится на складе.
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesForGoods()
		{
			return new[] {
				NomenclatureCategory.bottle, 
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment, 
				NomenclatureCategory.material,
				NomenclatureCategory.spare_parts, 
				NomenclatureCategory.water, 
				NomenclatureCategory.disposableBottleWater
			};
		}

		public static NomenclatureCategory[] GetCategoriesWithEditablePrice()
		{
			return new[] {
				NomenclatureCategory.bottle,
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.material,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.water,
				NomenclatureCategory.disposableBottleWater,
				NomenclatureCategory.service,
				NomenclatureCategory.deposit,
				NomenclatureCategory.master
			};
		}
		#endregion
	}

	public enum NomenclatureCategory
	{
		[Display(Name = "Аренда кулеров")]
		[Code1c("00001301")]
		rent,
		[Display(Name = "Вода в многооборотной таре")]
		[Code1c("790070")]
		water,
		[Display(Name = "Залог")]
		[Code1c("00000312")]
		deposit,
		[Display(Name = "Запчасти")]
		[Code1c("00000939")]
		spare_parts,
		[Display(Name = "Оборудование")]
		[Code1c("00000959")]
		equipment,
		[Display(Name = "Товары")]
		[Code1c("00000310")]
		additional,
		[Display(Name = "Услуга")]
		[Code1c("00000311")]
		service,
		[Display(Name = "Тара")]
		[Code1c("00000000010")]
		bottle,
		[Display(Name = "Сырьё")]
		[Code1c("002077")]
		material,
		[Display(Name = "Вода в одноразовой таре")]
		[Code1c("0790070")]
		disposableBottleWater,
		[Display(Name = "Выезд мастера")]
		[Code1c("790930")] //Придуман в 1с такого кода не было
		master
	}

	public class NomenclatureCategoryStringType : NHibernate.Type.EnumStringType
	{
		public NomenclatureCategoryStringType() : base(typeof(NomenclatureCategory))
		{
		}
	}
}

