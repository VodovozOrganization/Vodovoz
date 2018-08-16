using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Store;
using Vodovoz.Repository;

namespace Vodovoz.Domain.Goods
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры",
		Nominative = "номенклатура")]
	public class Nomenclature : BusinessObjectBase<Nomenclature>, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		DateTime? createDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate {
			get { return createDate; }
			set { SetField(ref createDate, value, () => CreateDate); }
		}

		User createdBy;
		[Display(Name = "Кем создана")]
		public virtual User CreatedBy {
			get { return createdBy; }
			set { SetField(ref createdBy, value, () => CreatedBy); }
		}

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
		[Required(ErrorMessage = "Код 1с должен быть заполнен.")]
		[StringLength(11)]
		public virtual string Code1c {
			get { return code1c; }
			set { SetField(ref code1c, value, () => Code1c); }
		}

		private Folder1c folder1;

		[Display(Name = "Папка в 1с")]
		[Required(ErrorMessage = "Папка 1с обязательна для заполнения.")]
		public virtual Folder1c Folder1C {
			get { return folder1; }
			set { SetField(ref folder1, value, () => Folder1C); }
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

		SubtypeOfEquipmentCategory? subTypeOfEquipmentCategory;

		[Display(Name = "Подкатегория товаров")]
		public virtual SubtypeOfEquipmentCategory? SubTypeOfEquipmentCategory {
			get { return subTypeOfEquipmentCategory; }
			set { SetField(ref subTypeOfEquipmentCategory, value, () => SubTypeOfEquipmentCategory);}
		}

		TypeOfDepositCategory? typeOfDepositCategory;

		[Display(Name = "Подкатегория залогов")]
		public virtual TypeOfDepositCategory? TypeOfDepositCategory {
			get { return typeOfDepositCategory; }
			set { SetField(ref typeOfDepositCategory, value, () => TypeOfDepositCategory); }
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
		
		private double percentForMaster;

		[Display(Name = "Процент зарплаты мастера")]
		public virtual double PercentForMaster {
			get { return percentForMaster; }
			set { SetField(ref percentForMaster, value, () => PercentForMaster); }
		}

		private Guid? onlineStoreGuid;

		[Display(Name = "Guid интернет магазина")]
		public virtual Guid? OnlineStoreGuid {
			get { return onlineStoreGuid; }
			set { SetField(ref onlineStoreGuid, value, () => OnlineStoreGuid); }
		}

		private ProductGroup productGroup;

		[Display(Name = "Группа товаров")]
		public virtual ProductGroup ProductGroup {
			get { return productGroup; }
			set { SetField(ref productGroup, value, () => ProductGroup); }
		}

		#endregion

		public virtual void SetNomenclatureCreationInfo()
		{
			if(Id == 0 && !CreateDate.HasValue) {
				CreateDate = DateTime.Now;
				CreatedBy = UserRepository.GetCurrentUser(UoW);
			}
		}

		#region Рассчетные

		public virtual string CategoryString => Category.GetEnumTitle();

		public virtual string ShortOrFullName => String.IsNullOrWhiteSpace(ShortName) ? Name : ShortName;

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
			if(Category == NomenclatureCategory.equipment && Type == null)
				yield return new ValidationResult(
					String.Format("Не указан тип оборудования."),
					new[] { this.GetPropertyName(o => o.Type) });

			if(Category == NomenclatureCategory.equipment && SubTypeOfEquipmentCategory == null)
				yield return new ValidationResult(
					String.Format("Не указан подтип оборудования (для продажи или нет)."),
					new[] { this.GetPropertyName(o => o.SubTypeOfEquipmentCategory) });

			if(Category == NomenclatureCategory.deposit && TypeOfDepositCategory == null)
				yield return new ValidationResult(
					String.Format("Не указан тип залога."),
					new[] { this.GetPropertyName(o => o.TypeOfDepositCategory) });

			//Проверка зависимостей номенклатур #1: если есть зависимые
			if(DependsOnNomenclature != null) {
				IList<Nomenclature> dependedNomenclatures = Repository.NomenclatureRepository.GetDependedNomenclatures(UoW, this);
				if(dependedNomenclatures.Any()) {
					string dependedNomenclaturesText = "Цена данной номенклатуры не может зависеть от другой номенклатуры, т.к. от данной номенклатуры зависят цены следующих номенклатур:\n";
					foreach(Nomenclature n in dependedNomenclatures)
						dependedNomenclaturesText += String.Format("{0}: {1} ({2})\n", n.Id, n.OfficialName, n.CategoryString);
					yield return new ValidationResult(dependedNomenclaturesText, new[] { this.GetPropertyName(o => o.DependsOnNomenclature) });
				}
				if(DependsOnNomenclature.DependsOnNomenclature != null)
					yield return new ValidationResult(
						String.Format("Номенклатура '{0}' указанная в качеcтве основной для цен этой номеклатуры, сама зависить от '{1}'",
						              DependsOnNomenclature.ShortOrFullName, DependsOnNomenclature.DependsOnNomenclature.ShortOrFullName),
						new[] { this.GetPropertyName(o => o.DependsOnNomenclature) });
			}

			if(Code1c != null && Code1c.StartsWith(PrefixOfCode1c))
			{
				if(Code1c.Length != LengthOfCode1c)
					yield return new ValidationResult(
						String.Format("Код 1с с префиксом автоформирования '{0}', должен содержать {1}-символов.",
						              PrefixOfCode1c, LengthOfCode1c),
						new[] { this.GetPropertyName(o => o.Code1c) });

				var next = NomenclatureRepository.GetNextCode1c(UoW);
				if(String.Compare(Code1c, next) > 0)
					yield return new ValidationResult(
						String.Format("Код 1с использует префикс автоматического формирования кодов '{0}'. При этом пропускает некоторое количество значений. Используйте в качестве следующего кода {1} или оставьте это поле пустым для автозаполенения.",
						              PrefixOfCode1c, next),
						new[] { this.GetPropertyName(o => o.Code1c) });

			}
		}

		#endregion

		#region statics

		public static string PrefixOfCode1c = "ДВ";
		public static int LengthOfCode1c = 10;

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

		public static NomenclatureCategory[] GetCategoriesForSaleToOrder()
		{
			List<NomenclatureCategory> forSale = new List<NomenclatureCategory>(
				new[] {
					NomenclatureCategory.additional,
					NomenclatureCategory.equipment,
					NomenclatureCategory.water,
					NomenclatureCategory.disposableBottleWater,
					NomenclatureCategory.rent,
					NomenclatureCategory.deposit,
					NomenclatureCategory.service
				}
			);

			if(QSMain.User.Permissions["can_add_spares_to_order"])
				forSale.Add(NomenclatureCategory.spare_parts);
			if(QSMain.User.Permissions["can_add_bottles_to_order"])
				forSale.Add(NomenclatureCategory.bottle);
			if(QSMain.User.Permissions["can_add_materials_to_order"])
				forSale.Add(NomenclatureCategory.material);

			return forSale.ToArray();
		}

		/// <summary>
		/// Список номенклатур доступных для добавления в товары 
		/// из диалога изменения заказа в закрытии МЛ
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesForEditOrderFromRL()
		{
			return new[] {
				NomenclatureCategory.additional,
				NomenclatureCategory.water,
				NomenclatureCategory.disposableBottleWater,
				NomenclatureCategory.bottle,
				NomenclatureCategory.deposit,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.service,
				NomenclatureCategory.master
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

		/// <summary>
		/// Категории товаров. Товары могут хранится на складе без учёта 19л воды.
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesForGoodsWithoutEmptyBottles()
		{
			return new[] {
				NomenclatureCategory.water,
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.material,
				NomenclatureCategory.spare_parts,
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

		/// <summary>
		/// Определяет категории для которых необходимо создавать доп соглашение по продаже воды
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesRequirementForWaterAgreement()
		{
			return new[] {
				NomenclatureCategory.water,
				NomenclatureCategory.disposableBottleWater
			};
		}
		#endregion
	}

	public enum NomenclatureCategory
	{
		[Display(Name = "Аренда кулеров")]
		rent,
		[Display(Name = "Вода в многооборотной таре")]
		water,
		[Display(Name = "Залог")]
		deposit,
		[Display(Name = "Запчасти")]
		spare_parts,
		[Display(Name = "Оборудование")]
		equipment,
		[Display(Name = "Товары")]
		additional,
		[Display(Name = "Услуга")]
		service,
		[Display(Name = "Тара")]
		bottle,
		[Display(Name = "Сырьё")]
		material,
		[Display(Name = "Вода в одноразовой таре")]
		disposableBottleWater,
		[Display(Name = "Выезд мастера")]
		master
	}

	/// <summary>
	/// Подтип категории "Товары"
	/// </summary>
	public enum SubtypeOfEquipmentCategory
	{
		[Display(Name = "На продажу")]
		forSale,
		[Display(Name = "Не для продажи")]
		notForSale
	}

	/// <summary>
	/// Подтип категории "Залог"
	/// </summary>
	public enum TypeOfDepositCategory
	{
		[Display(Name = "Залог за бутыли")]
		BottleDeposit,
		[Display(Name = "Залог за оборудование")]
		EquipmentDeposit
	}

	public class NomenclatureCategoryStringType : NHibernate.Type.EnumStringType
	{
		public NomenclatureCategoryStringType() : base(typeof(NomenclatureCategory))
		{
		}
	}

	public class SubtypeOfEquipmentCategoryStringType : NHibernate.Type.EnumStringType
	{
		public SubtypeOfEquipmentCategoryStringType() : base(typeof(SubtypeOfEquipmentCategory))
		{
		}
	}

	public class TypeOfDepositCategoryStringType : NHibernate.Type.EnumStringType
	{
		public TypeOfDepositCategoryStringType() : base(typeof(TypeOfDepositCategory))
		{
		}
	}
}

