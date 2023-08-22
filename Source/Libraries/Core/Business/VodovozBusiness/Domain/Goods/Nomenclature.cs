using Gamma.Utilities;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "номенклатуры",
		Nominative = "номенклатура")]
	[EntityPermission]
	[HistoryTrace]
	public class Nomenclature : BusinessObjectBase<Nomenclature>, INamedDomainObject, INamed, IArchivable, IValidatableObject
	{
		private IList<NomenclaturePurchasePrice> _purchasePrices = new List<NomenclaturePurchasePrice>();
		private IList<NomenclatureCostPrice> _costPrices = new List<NomenclatureCostPrice>();
		private IObservableList<NomenclatureInnerDeliveryPrice> _innerDeliveryPrices = new ObservableList<NomenclatureInnerDeliveryPrice>();
		private IList<AlternativeNomenclaturePrice> _alternativeNomenclaturePrices = new List<AlternativeNomenclaturePrice>();
		private GenericObservableList<NomenclaturePurchasePrice> _observablePurchasePrices;
		private GenericObservableList<NomenclatureCostPrice> _observableCostPrices;
		private GenericObservableList<NomenclatureInnerDeliveryPrice> _observableInnerDeliveryPrices;
		private GenericObservableList<NomenclaturePrice> _observableNomenclaturePrices;
		private GenericObservableList<AlternativeNomenclaturePrice> _observableAlternativeNomenclaturePrices;
		private bool _usingInGroupPriceSet;
		private bool _hasInventoryAccounting;
		
		private int _id;

		private decimal _length;
		private decimal _width;
		private decimal _height;

		private bool _isAccountableInTrueMark;
		private string _gtin;

		public Nomenclature()
		{
			Category = NomenclatureCategory.water;
		}

		#region Свойства

		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		private DateTime? createDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate
		{
			get => createDate;
			set => SetField(ref createDate, value, () => CreateDate);
		}

		private User createdBy;
		[Display(Name = "Кем создана")]
		public virtual User CreatedBy
		{
			get => createdBy;
			set => SetField(ref createdBy, value, () => CreatedBy);
		}

		private string name;

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		private string officialName;

		[Display(Name = "Официальное название")]
		public virtual string OfficialName
		{
			get => officialName;
			set => SetField(ref officialName, value, () => OfficialName);
		}

		private bool isArchive;

		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => isArchive;
			set => SetField(ref isArchive, value, () => IsArchive);
		}

		private bool isDiler;

		[Display(Name = "Дилер")]
		public virtual bool IsDiler
		{
			get => isDiler;
			set => SetField(ref isDiler, value, () => IsDiler);
		}

		private bool canPrintPrice;

		[Display(Name = "Печатается прайс в документах")]
		public virtual bool CanPrintPrice
		{
			get => canPrintPrice;
			set => SetField(ref canPrintPrice, value, () => CanPrintPrice);
		}

		private string code1c;
		[Display(Name = "Код 1с")]
		[StringLength(11)]
		public virtual string Code1c
		{
			get => code1c;
			set => SetField(ref code1c, value, () => Code1c);
		}

		private Folder1c folder1;

		[Display(Name = "Папка в 1с")]
		public virtual Folder1c Folder1C
		{
			get => folder1;
			set => SetField(ref folder1, value, () => Folder1C);
		}

		private string model;

		[Display(Name = "Модель оборудования")]
		public virtual string Model
		{
			get => model;
			set => SetField(ref model, value, () => Model);
		}

		private decimal weight;

		[Display(Name = "Вес")]
		public virtual decimal Weight
		{
			get => weight;
			set => SetField(ref weight, value, () => Weight);
		}

		/// <summary>
		/// Объем номенклатуры, измеряемый в квадратных метрах
		/// </summary>
		[Display(Name = "Объём")]
		public virtual decimal Volume => Length * Width * Height / 1000000;    // 1 000 000

		/// <summary>
		/// Длина номенклатуры, измеряемая в сантиметрах
		/// </summary>
		[Display(Name = "Длина")]
		public virtual decimal Length
		{
			get => _length;
			set
			{
				if(SetField(ref _length, value))
				{
					OnPropertyChanged(nameof(Volume));
				}
			}
		}

		/// <summary>
		/// Ширина номенклатуры, измеряемая в сантиметрах
		/// </summary>
		[Display(Name = "Ширина")]
		public virtual decimal Width
		{
			get => _width;
			set
			{
				if(SetField(ref _width, value))
				{
					OnPropertyChanged(nameof(Volume));
				}
			}
		}

		/// <summary>
		/// Высота номенклатуры, измеряемая в сантиметрах
		/// </summary>
		[Display(Name = "Высота")]
		public virtual decimal Height
		{
			get => _height;
			set
			{
				if(SetField(ref _height, value))
				{
					OnPropertyChanged(nameof(Volume));
				}
			}
		}

		private VAT vAT = VAT.Vat18;

		[Display(Name = "НДС")]
		public virtual VAT VAT
		{
			get => vAT;
			set => SetField(ref vAT, value, () => VAT);
		}

		private bool doNotReserve;

		[Display(Name = "Не резервировать")]
		public virtual bool DoNotReserve
		{
			get => doNotReserve;
			set => SetField(ref doNotReserve, value, () => DoNotReserve);
		}

		private bool rentPriority;

		[Display(Name = "Приоритет аренды")]
		public virtual bool RentPriority
		{
			get => rentPriority;
			set => SetField(ref rentPriority, value, () => RentPriority);
		}

		private bool isDuty;
		/// <summary>
		/// Дежурное оборудование
		/// </summary>
		[Display(Name = "Дежурное оборудование")]
		public virtual bool IsDuty
		{
			get => isDuty;
			set => SetField(ref isDuty, value, () => IsDuty);
		}

		private bool isSerial;

		[Display(Name = "Серийный номер")]
		public virtual bool IsSerial
		{
			get => isSerial;
			set => SetField(ref isSerial, value, () => IsSerial);
		}

		private MeasurementUnits unit;

		[Display(Name = "Единица измерения")]
		public virtual MeasurementUnits Unit
		{
			get => unit;
			set => SetField(ref unit, value, () => Unit);
		}

		private decimal minStockCount;

		[Display(Name = "Минимальное количество на складе")]
		public virtual decimal MinStockCount
		{
			get => minStockCount;
			set => SetField(ref minStockCount, value, () => MinStockCount);
		}

		private bool isDisposableTare;

		[Display(Name = "Одноразовая тара для воды")]
		public virtual bool IsDisposableTare
		{
			get => isDisposableTare;
			set => SetField(ref isDisposableTare, value, () => IsDisposableTare);
		}

		private TareVolume? tareVolume;

		[Display(Name = "Объем тары для воды")]
		public virtual TareVolume? TareVolume
		{
			get => tareVolume;
			set => SetField(ref tareVolume, value, () => TareVolume);
		}

		private NomenclatureCategory category;

		[Display(Name = "Категория")]
		public virtual NomenclatureCategory Category
		{
			get => category;
			set
			{
				if(SetField(ref category, value))
				{
					if(!CategoriesWithSerial.Contains(Category))
					{
						IsSerial = false;
					}

					if(Category != NomenclatureCategory.water)
						TareVolume = null;

					if(!GetCategoriesWithSaleCategory().Contains(value))
						SaleCategory = null;
				}
			}
		}

		private SaleCategory? saleCategory;

		[Display(Name = "Доступность для продаж")]
		public virtual SaleCategory? SaleCategory
		{
			get => saleCategory;
			set => SetField(ref saleCategory, value, () => SaleCategory);
		}

		private TypeOfDepositCategory? typeOfDepositCategory;

		[Display(Name = "Подкатегория залогов")]
		public virtual TypeOfDepositCategory? TypeOfDepositCategory
		{
			get => typeOfDepositCategory;
			set => SetField(ref typeOfDepositCategory, value, () => TypeOfDepositCategory);
		}

		private EquipmentColors equipmentColor;

		[Display(Name = "Цвет оборудования")]
		public virtual EquipmentColors EquipmentColor
		{
			get => equipmentColor;
			set => SetField(ref equipmentColor, value, () => EquipmentColor);
		}

		private EquipmentKind _kind;

		[Display(Name = "Вид оборудования")]
		public virtual EquipmentKind Kind
		{
			get => _kind;
			set => SetField(ref _kind, value, () => Kind);
		}

		private Manufacturer manufacturer;

		[Display(Name = "Производитель")]
		public virtual Manufacturer Manufacturer
		{
			get => manufacturer;
			set => SetField(ref manufacturer, value, () => Manufacturer);
		}

		private RouteColumn routeListColumn;

		[Display(Name = "Производитель")]
		public virtual RouteColumn RouteListColumn
		{
			get => routeListColumn;
			set => SetField(ref routeListColumn, value, () => RouteListColumn);
		}

		private decimal sumOfDamage;

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage
		{
			get => sumOfDamage;
			set => SetField(ref sumOfDamage, value, () => SumOfDamage);
		}

		private IList<NomenclaturePrice> nomenclaturePrice = new List<NomenclaturePrice>();

		[Display(Name = "Цены")]
		public virtual IList<NomenclaturePrice> NomenclaturePrice
		{
			get => nomenclaturePrice;
			set => SetField(ref nomenclaturePrice, value, () => NomenclaturePrice);
		}

		[Display(Name = "Альтернативные цены")]
		public virtual IList<AlternativeNomenclaturePrice> AlternativeNomenclaturePrices
		{
			get => _alternativeNomenclaturePrices;
			set => SetField(ref _alternativeNomenclaturePrices, value);
		}

		private string shortName;

		[Display(Name = "Сокращенное название")]
		public virtual string ShortName
		{
			get => shortName;
			set => SetField(ref shortName, value, () => ShortName);
		}

		private bool hide;

		[Display(Name = "Скрыть из МЛ")]
		public virtual bool Hide
		{
			get => hide;
			set => SetField(ref hide, value, () => Hide);
		}

		private bool _noDelivery;

		[Display(Name = "Доставка не требуется")]
		public virtual bool NoDelivery
		{
			get => _noDelivery;
			set => SetField(ref _noDelivery, value, () => NoDelivery);
		}

		private bool isNewBottle;
		[Display(Name = "Это новая бутыль")]
		public virtual bool IsNewBottle
		{
			get => isNewBottle;
			set
			{
				if(SetField(ref isNewBottle, value) && isNewBottle)
				{
					IsDefectiveBottle = false;
					IsShabbyBottle = false;
				}
			}
		}

		private bool isDefectiveBottle;
		[Display(Name = "Это бракованая бутыль")]
		public virtual bool IsDefectiveBottle
		{
			get => isDefectiveBottle;
			set
			{
				if(SetField(ref isDefectiveBottle, value) && isDefectiveBottle)
				{
					IsNewBottle = false;
					IsShabbyBottle = false;
				}
			}
		}

		private bool isShabbyBottle;
		[Display(Name = "Стройка")]
		public virtual bool IsShabbyBottle
		{
			get => isShabbyBottle;
			set
			{
				if(SetField(ref isShabbyBottle, value) && isShabbyBottle)
				{
					IsNewBottle = false;
					IsDefectiveBottle = false;
				}
			}
		}

		private FuelType fuelType;
		[Display(Name = "Тип топлива")]
		public virtual FuelType FuelType
		{
			get => fuelType;
			set => SetField(ref fuelType, value, () => FuelType);
		}

		private Nomenclature dependsOnNomenclature;

		[Display(Name = "Влияющая номенклатура")]
		public virtual Nomenclature DependsOnNomenclature
		{
			get => dependsOnNomenclature;
			set => SetField(ref dependsOnNomenclature, value, () => DependsOnNomenclature);
		}

		private double percentForMaster;

		[Display(Name = "Процент зарплаты мастера")]
		public virtual double PercentForMaster
		{
			get => percentForMaster;
			set => SetField(ref percentForMaster, value, () => PercentForMaster);
		}

		private Guid? onlineStoreGuid;

		[Display(Name = "Guid интернет магазина")]
		public virtual Guid? OnlineStoreGuid
		{
			get => onlineStoreGuid;
			set => SetField(ref onlineStoreGuid, value, () => OnlineStoreGuid);
		}

		private ProductGroup productGroup;

		[Display(Name = "Группа товаров")]
		public virtual ProductGroup ProductGroup
		{
			get => productGroup;
			set => SetField(ref productGroup, value, () => ProductGroup);
		}

		private IObservableList<NomenclatureImage> images = new ObservableList<NomenclatureImage>();
		[Display(Name = "Изображения")]
		public virtual IObservableList<NomenclatureImage> Images
		{
			get => images;
			set => SetField(ref images, value, () => Images);
		}

		private MobileCatalog mobileCatalog;

		[Display(Name = "Каталог в мобильном приложении")]
		public virtual MobileCatalog MobileCatalog
		{
			get => mobileCatalog;
			set => SetField(ref mobileCatalog, value, () => MobileCatalog);
		}

		private string description;

		[Display(Name = "Описание товара")]
		public virtual string Description
		{
			get => description;
			set => SetField(ref description, value);
		}

		private string bottleCapColor;
		[Display(Name = "Цвет пробки 19л бутыли")]
		public virtual string BottleCapColor
		{
			get => bottleCapColor;
			set => SetField(ref bottleCapColor, value);
		}

		private OnlineStore onlineStore;
		[Display(Name = "Интернет-магазин")]
		public virtual OnlineStore OnlineStore
		{
			get => onlineStore;
			set => SetField(ref onlineStore, value);
		}

		[Display(Name = "Участвует в групповом заполнении себестоимости")]
		public virtual bool UsingInGroupPriceSet
		{
			get => _usingInGroupPriceSet;
			set => SetField(ref _usingInGroupPriceSet, value);
		}

		[Display(Name = "Цены закупки ТМЦ")]
		public virtual IList<NomenclaturePurchasePrice> PurchasePrices
		{
			get => _purchasePrices;
			set => SetField(ref _purchasePrices, value);
		}

		public virtual GenericObservableList<NomenclaturePurchasePrice> ObservablePurchasePrices =>
			_observablePurchasePrices ?? (_observablePurchasePrices = new GenericObservableList<NomenclaturePurchasePrice>(PurchasePrices));

		[Display(Name = "Себестоимость ТМЦ")]
		public virtual IList<NomenclatureCostPrice> CostPrices
		{
			get => _costPrices;
			set => SetField(ref _costPrices, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclatureCostPrice> ObservableCostPrices =>
			_observableCostPrices ?? (_observableCostPrices = new GenericObservableList<NomenclatureCostPrice>(CostPrices));

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclaturePrice> ObservableNomenclaturePrices
		{
			get => _observableNomenclaturePrices ?? (_observableNomenclaturePrices = new GenericObservableList<NomenclaturePrice>(NomenclaturePrice));
			set => _observableNomenclaturePrices = value;
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AlternativeNomenclaturePrice> ObservableAlternativeNomenclaturePrices
		{
			get => _observableAlternativeNomenclaturePrices ?? (_observableAlternativeNomenclaturePrices = new GenericObservableList<AlternativeNomenclaturePrice>(AlternativeNomenclaturePrices));
			set => _observableAlternativeNomenclaturePrices = value;
		}


		[Display(Name = "Стоимости доставки ТМЦ на склад")]
		public virtual IObservableList<NomenclatureInnerDeliveryPrice> InnerDeliveryPrices
		{
			get => _innerDeliveryPrices;
			set => SetField(ref _innerDeliveryPrices, value);
		}

		[Display(Name = "Подлежит учету в Честном Знаке")]
		public virtual bool IsAccountableInTrueMark
		{
			get => _isAccountableInTrueMark;
			set => SetField(ref _isAccountableInTrueMark, value);
		}

		[Display(Name = "Номер товарной продукции GTIN")]
		public virtual string Gtin
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}
		
		[Display(Name = "Инвентарный учет")]
		public virtual bool HasInventoryAccounting
		{
			get => _hasInventoryAccounting;
			set => SetField(ref _hasInventoryAccounting, value);
		}
		#endregion

		#region Свойства товаров для магазина

		private string onlineStoreExternalId;
		[Display(Name = "Id в интернет магазине")]
		public virtual string OnlineStoreExternalId
		{
			get => onlineStoreExternalId;
			set => SetField(ref onlineStoreExternalId, value);
		}

		private Counterparty shipperCounterparty;
		[Display(Name = "Поставщик")]
		public virtual Counterparty ShipperCounterparty
		{
			get => shipperCounterparty;
			set => SetField(ref shipperCounterparty, value);
		}

		private string storageCell;
		[Display(Name = "Ячейка хранения")]
		public virtual string StorageCell
		{
			get => storageCell;
			set => SetField(ref storageCell, value);
		}

		private string color;

		[Display(Name = "Цвет")]
		public virtual string Color
		{
			get => color;
			set => SetField(ref color, value);
		}

		private string material;

		[Display(Name = "Материал")]
		public virtual string Material
		{
			get => material;
			set => SetField(ref material, value);
		}

		private string liters;

		[Display(Name = "Объем")]
		public virtual string Liters
		{
			get => liters;
			set => SetField(ref liters, value);
		}

		private string size;

		[Display(Name = "Размеры")]
		public virtual string Size
		{
			get => size;
			set => SetField(ref size, value);
		}

		private string package;

		[Display(Name = "Тип упаковки")]
		public virtual string Package
		{
			get => package;
			set => SetField(ref package, value);
		}

		private string degreeOfRoast;

		[Display(Name = "Степень обжарки")]
		public virtual string DegreeOfRoast
		{
			get => degreeOfRoast;
			set => SetField(ref degreeOfRoast, value);
		}

		private string smell;

		[Display(Name = "Запах")]
		public virtual string Smell
		{
			get => smell;
			set => SetField(ref smell, value);
		}

		private string taste;

		[Display(Name = "Вкус")]
		public virtual string Taste
		{
			get => taste;
			set => SetField(ref taste, value);
		}

		private string refrigeratorCapacity;

		[Display(Name = "Объем шкафчика/холодильника")]
		public virtual string RefrigeratorCapacity
		{
			get => refrigeratorCapacity;
			set => SetField(ref refrigeratorCapacity, value);
		}

		private string coolingType;

		[Display(Name = "Тип охлаждения")]
		public virtual string CoolingType
		{
			get => coolingType;
			set => SetField(ref coolingType, value);
		}

		private string heatingPower;

		[Display(Name = "Мощность нагрева")]
		public virtual string HeatingPower
		{
			get => heatingPower;
			set => SetField(ref heatingPower, value);
		}

		private string coolingPower;

		[Display(Name = "Мощность охлаждения")]
		public virtual string CoolingPower
		{
			get => coolingPower;
			set => SetField(ref coolingPower, value);
		}

		private string heatingPerformance;

		[Display(Name = "Производительность нагрева")]
		public virtual string HeatingPerformance
		{
			get => heatingPerformance;
			set => SetField(ref heatingPerformance, value);
		}

		private string coolingPerformance;

		[Display(Name = "Производительность охлаждения")]
		public virtual string CoolingPerformance
		{
			get => coolingPerformance;
			set => SetField(ref coolingPerformance, value);
		}

		private string numberOfCartridges;

		[Display(Name = "Количество картриджей")]
		public virtual string NumberOfCartridges
		{
			get => numberOfCartridges;
			set => SetField(ref numberOfCartridges, value);
		}

		private string characteristicsOfCartridges;

		[Display(Name = "Характеристика картриджей")]
		public virtual string CharacteristicsOfCartridges
		{
			get => characteristicsOfCartridges;
			set => SetField(ref characteristicsOfCartridges, value);
		}

		private string countryOfOrigin;

		[Display(Name = "Страна происхождения")]
		public virtual string CountryOfOrigin
		{
			get => countryOfOrigin;
			set => SetField(ref countryOfOrigin, value);
		}

		private string amountInAPackage;

		[Display(Name = "Количество  в упаковке")]
		public virtual string AmountInAPackage
		{
			get => amountInAPackage;
			set => SetField(ref amountInAPackage, value);
		}


		private int? planDay;

		[Display(Name = "План день")]
		public virtual int? PlanDay
		{
			get => planDay;
			set => SetField(ref planDay, value);
		}

		private int? planMonth;

		[Display(Name = "План месяц")]
		public virtual int? PlanMonth
		{
			get => planMonth;
			set => SetField(ref planMonth, value);
		}

		#endregion

		#region Рассчетные

		public virtual string CategoryString => Category.GetEnumTitle();

		public virtual string ShortOrFullName => string.IsNullOrWhiteSpace(ShortName) ? Name : ShortName;

		public virtual bool IsWater19L =>
			Category == NomenclatureCategory.water
			&& TareVolume.HasValue
			&& TareVolume.Value == Goods.TareVolume.Vol19L;

		public override string ToString() => $"id ={Id} Name = {Name}";

		#endregion

		#region Методы

		public virtual void SetNomenclatureCreationInfo(IUserRepository userRepository)
		{
			if(Id == 0 && !CreateDate.HasValue)
			{
				CreateDate = DateTime.Now;
				CreatedBy = userRepository.GetCurrentUser(UoW);
			}
		}

		public virtual decimal GetPrice(decimal? itemsCount, bool useAlternativePrice = false)
		{
			if(itemsCount < 1)
				itemsCount = 1;
			decimal price = 0m;
			if(DependsOnNomenclature != null)
			{
				price = DependsOnNomenclature.GetPrice(itemsCount, useAlternativePrice);
			}
			else
			{
				var nomPrice = (useAlternativePrice
						? AlternativeNomenclaturePrices.Cast<NomenclaturePriceBase>()
						: NomenclaturePrice.Cast<NomenclaturePriceBase>())
					.OrderByDescending(p => p.MinCount)
					.FirstOrDefault(p => p.MinCount <= itemsCount);
				price = nomPrice?.Price ?? 0;
			}
			return price;
		}

		/// <summary>
		/// Cоздает новый Guid. Uow необходим для сохранения созданного Guid в базу.
		/// </summary>
		public virtual void CreateGuidIfNotExist(IUnitOfWork uow)
		{
			if(OnlineStoreGuid == null)
			{
				OnlineStoreGuid = Guid.NewGuid();
				uow.Save(this);
			}
		}

		public virtual bool IsFromOnlineShopGroup(int idOfOnlineShopGroup)
		{
			ProductGroup parent = ProductGroup;
			while(parent != null)
			{
				if(parent.Id == idOfOnlineShopGroup)
					return true;
				parent = parent.Parent;
			}
			return false;
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.ServiceContainer.GetService(
				typeof(INomenclatureRepository)) is INomenclatureRepository nomenclatureRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(nomenclatureRepository) }");
			}

			if(String.IsNullOrWhiteSpace(Name))
				yield return new ValidationResult(
					"Название номенклатуры должно быть заполнено.", new[] { this.GetPropertyName(o => o.Name) });
			else if(Name.Length > 220)
				yield return new ValidationResult(
					"Превышено максимальное количество символов в названии (220).", new[] { this.GetPropertyName(o => o.Name) });

			if(String.IsNullOrWhiteSpace(OfficialName))
				yield return new ValidationResult(
					"Официальное название номенклатуры должно быть заполнено.", new[] { this.GetPropertyName(o => o.OfficialName) });
			else if(Name.Length > 220)
				yield return new ValidationResult(
					"Превышено максимальное количество символов в официальном названии (220).", new[] { this.GetPropertyName(o => o.OfficialName) });

			if(CategoriesWithWeightAndVolume.Contains(Category) && (Length == 0 || Width == 0 || Height == 0 || Weight == 0))
			{
				yield return new ValidationResult("Длина, ширина, высота и вес номенклатуры обязательны для заполнения",
					new[] { nameof(Length), nameof(Width), nameof(Height), nameof(Weight) });
			}

			if(Length < 0 || Width < 0 || Height < 0 || Weight < 0)
			{
				yield return new ValidationResult("Длина, ширина, высота и вес номенклатуры должны быть положительными",
					new[] { nameof(Length), nameof(Width), nameof(Height), nameof(Weight) });
			}

			if(Folder1C == null)
				yield return new ValidationResult(
					"Папка 1С обязательна для заполнения", new[] { this.GetPropertyName(o => o.Folder1C) });

			if(String.IsNullOrWhiteSpace(Code1c))
				yield return new ValidationResult(
					"Код 1С обязателен для заполнения", new[] { this.GetPropertyName(o => o.Code1c) });

			if(Category == NomenclatureCategory.equipment && Kind == null)
				yield return new ValidationResult(
					"Не указан вид оборудования.",
					new[] { this.GetPropertyName(o => o.Kind) });

			if(GetCategoriesWithSaleCategory().Contains(category) && SaleCategory == null)
				yield return new ValidationResult(
					"Не указана \"Доступность для продажи\"",
					new[] { this.GetPropertyName(o => o.SaleCategory) }
				);

			if(Category == NomenclatureCategory.deposit && TypeOfDepositCategory == null)
				yield return new ValidationResult(
					"Не указан тип залога.",
					new[] { this.GetPropertyName(o => o.TypeOfDepositCategory) });

			if(Category == NomenclatureCategory.water && !TareVolume.HasValue)
				yield return new ValidationResult(
					"Не выбран объем тары",
					new[] { this.GetPropertyName(o => o.TareVolume) }
				);

			if(Category == NomenclatureCategory.fuel && FuelType == null)
			{
				yield return new ValidationResult("Не выбран тип топлива");
			}

			if(Unit == null)
				yield return new ValidationResult(
					"Не указаны единицы измерения",
					new[] { this.GetPropertyName(o => o.Unit) });

			//Проверка зависимостей номенклатур #1: если есть зависимые
			if(DependsOnNomenclature != null)
			{
				IList<Nomenclature> dependedNomenclatures = nomenclatureRepository.GetDependedNomenclatures(UoW, this);

				if(dependedNomenclatures.Any())
				{
					string dependedNomenclaturesText = "Цена данной номенклатуры не может зависеть от другой номенклатуры, т.к. от данной номенклатуры зависят цены следующих номенклатур:\n";

					foreach(Nomenclature n in dependedNomenclatures)
					{
						dependedNomenclaturesText += $"{n.Id}: {n.OfficialName} ({n.CategoryString})\n";
					}

					yield return new ValidationResult(dependedNomenclaturesText, new[] { this.GetPropertyName(o => o.DependsOnNomenclature) });
				}

				if(DependsOnNomenclature.DependsOnNomenclature != null)
					yield return new ValidationResult(
						$"Номенклатура '{DependsOnNomenclature.ShortOrFullName}' указанная в качеcтве основной для цен этой номеклатуры, сама зависит от '{DependsOnNomenclature.DependsOnNomenclature.ShortOrFullName}'",
						new[] { this.GetPropertyName(o => o.DependsOnNomenclature) });
			}

			if(Code1c != null && Code1c.StartsWith(PrefixOfCode1c))
			{
				if(Code1c.Length != LengthOfCode1c)
					yield return new ValidationResult(
						$"Код 1с с префиксом автоформирования '{PrefixOfCode1c}', должен содержать {LengthOfCode1c}-символов.",
						new[] { this.GetPropertyName(o => o.Code1c) });

				var next = nomenclatureRepository.GetNextCode1c(UoW);
				if(string.Compare(Code1c, next) > 0)
				{
					yield return new ValidationResult(
						$"Код 1с использует префикс автоматического формирования кодов '{PrefixOfCode1c}'. При этом пропускает некоторое количество значений. Используйте в качестве следующего кода {next} или оставьте это поле пустым для автозаполенения.",
						new[] { this.GetPropertyName(o => o.Code1c) });
				}
			}

			if(DateTime.Now >= new DateTime(2019, 01, 01) && VAT == VAT.Vat18)
				yield return new ValidationResult(
					"С 01.01.2019 ставка НДС 20%",
					new[] { this.GetPropertyName(o => o.VAT) }
				);

			foreach(var purchasePrice in PurchasePrices)
			{
				foreach(var validationResult in purchasePrice.Validate(validationContext))
				{
					yield return validationResult;
				}
			}

			if(IsAccountableInTrueMark && string.IsNullOrWhiteSpace(Gtin))
			{
				yield return new ValidationResult("Должен быть заполнен GTIN для ТМЦ, подлежащих учёту в Честном знаке.",
					new[] { nameof(Gtin) });
			}

			if(Gtin?.Length < 8 || Gtin?.Length > 14)
			{
				yield return new ValidationResult("Длина GTIN должна быть от 8 до 14 символов",
					new[] { nameof(Gtin) });
			}

			if(ProductGroup == null)
			{
				yield return new ValidationResult("Должна быть выбрана принадлежность номенклатуры к группе товаров",
					new[] { nameof(ProductGroup) });
			}
		}

		#endregion

		#region Statics

		public static string PrefixOfCode1c = "ДВ";
		public static int LengthOfCode1c = 10;

		/// <summary>
		/// Категории товаров к которым применима категория продажи
		/// (доступность для продаж) "<see cref="SaleCategory"/>"
		/// </summary>
		/// <returns>Массив <see cref="NomenclatureCategory"/> к которым может применяться <see cref="SaleCategory"/></returns>
		public static NomenclatureCategory[] GetCategoriesWithSaleCategory()
		{
			return new[] {
				NomenclatureCategory.equipment,
				NomenclatureCategory.material,
				NomenclatureCategory.bottle,
				NomenclatureCategory.spare_parts
			};
		}

		public static NomenclatureCategory[] GetCategoriesForShipment()
		{
			return new[] {
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.water,
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
				NomenclatureCategory.bottle,
				NomenclatureCategory.deposit,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.service,
				NomenclatureCategory.material
			};
		}

		public static NomenclatureCategory[] GetCategoriesForSaleToOrder()
		{
			return new[] {
				NomenclatureCategory.additional,
				NomenclatureCategory.equipment,
				NomenclatureCategory.water,
				NomenclatureCategory.deposit,
				NomenclatureCategory.service,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.bottle,
				NomenclatureCategory.material
			};
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
				NomenclatureCategory.bottle,
				NomenclatureCategory.deposit,
				NomenclatureCategory.spare_parts,
				NomenclatureCategory.service,
				NomenclatureCategory.master
			};
		}

		public static NomenclatureCategory[] GetCategoriesForMaster()
		{
			List<NomenclatureCategory> list = new List<NomenclatureCategory>(GetCategoriesForSale()) {
				NomenclatureCategory.master,
				NomenclatureCategory.spare_parts
			};
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
				NomenclatureCategory.CashEquipment,
				NomenclatureCategory.Stationery,
				NomenclatureCategory.OfficeEquipment,
				NomenclatureCategory.PromotionalProducts,
				NomenclatureCategory.Overalls,
				NomenclatureCategory.HouseholdInventory,
				NomenclatureCategory.Tools
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
				NomenclatureCategory.PromotionalProducts
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
				NomenclatureCategory.service,
				NomenclatureCategory.deposit,
				NomenclatureCategory.master
			};
		}

		public static NomenclatureCategory[] GetAllCategories()
		{
			return Enum.GetValues(typeof(NomenclatureCategory)).Cast<NomenclatureCategory>().ToArray();
		}

		/// <summary>
		/// Определяет категории для которых необходимо создавать доп соглашение по продаже воды
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesRequirementForWaterAgreement()
		{
			return new[] {
				NomenclatureCategory.water
			};
		}

		public static NomenclatureCategory[] GetCategoriesNotNeededToLoad()
		{
			return new[] {
				NomenclatureCategory.service,
				NomenclatureCategory.deposit,
				NomenclatureCategory.master
			};
		}

		/// <summary>
		/// Категории, для которых обазательно должны быть заполнены вес и объём
		/// </summary>
		public static readonly NomenclatureCategory[] CategoriesWithWeightAndVolume =
		{
			NomenclatureCategory.water,
			NomenclatureCategory.equipment,
			NomenclatureCategory.additional,
			NomenclatureCategory.bottle
		};

		/// <summary>
		/// Категории для номенклатур с серийным номером
		/// </summary>
		public static readonly NomenclatureCategory[] CategoriesWithSerial =
		{
			NomenclatureCategory.equipment,
			NomenclatureCategory.Stationery,
			NomenclatureCategory.EquipmentForIndoorUse,
			NomenclatureCategory.OfficeEquipment,
			NomenclatureCategory.ProductionEquipment,
			NomenclatureCategory.Vehicle
		};

		#endregion
	}

	public enum TareVolume
	{
		[Display(Name = "19 л.")]
		Vol19L = 19000,
		[Display(Name = "6 л.")]
		Vol6L = 6000,
		[Display(Name = "1,5 л.")]
		Vol1500ml = 1500,
		[Display(Name = "0,6 л.")]
		Vol600ml = 600,
		[Display(Name = "0,5 л.")]
		Vol500ml = 500
	}

	/// <summary>
	/// Подтип категории "Товары"
	/// </summary>
	public enum SaleCategory
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
}

