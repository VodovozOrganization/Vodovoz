using Autofac;
using Gamma.Utilities;
using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Goods
{
	public class Nomenclature : NomenclatureEntity, IArchivable, IValidatableObject
	{
		private IList<NomenclaturePurchasePrice> _purchasePrices = new List<NomenclaturePurchasePrice>();
		private IList<NomenclatureCostPrice> _costPrices = new List<NomenclatureCostPrice>();
		private IList<NomenclatureInnerDeliveryPrice> _innerDeliveryPrices = new List<NomenclatureInnerDeliveryPrice>();
		private IList<AlternativeNomenclaturePrice> _alternativeNomenclaturePrices = new List<AlternativeNomenclaturePrice>();
		private GenericObservableList<NomenclaturePurchasePrice> _observablePurchasePrices;
		private GenericObservableList<NomenclatureCostPrice> _observableCostPrices;
		private GenericObservableList<NomenclatureInnerDeliveryPrice> _observableInnerDeliveryPrices;
		private GenericObservableList<NomenclaturePrice> _observableNomenclaturePrices;
		private GenericObservableList<AlternativeNomenclaturePrice> _observableAlternativeNomenclaturePrices;		
		private bool _usingInGroupPriceSet;
		private bool _hasInventoryAccounting;
		private bool _hasConditionAccounting;
		private GlassHolderType? _glassHolderType;
		private MobileAppNomenclatureOnlineCatalog _mobileAppNomenclatureOnlineCatalog;
		private VodovozWebSiteNomenclatureOnlineCatalog _vodovozWebSiteNomenclatureOnlineCatalog;
		private KulerSaleWebSiteNomenclatureOnlineCatalog _kulerSaleWebSiteNomenclatureOnlineCatalog;
		private NomenclatureOnlineGroup _nomenclatureOnlineGroup;
		private NomenclatureOnlineCategory _nomenclatureOnlineCategory;
		private string _onlineName;
		private EquipmentInstallationType? _equipmentInstallationType;
		private EquipmentWorkloadType? _equipmentWorkloadType;
		private PumpType? _pumpType;
		private CupHolderBracingType? _cupHolderBracingType;
		private bool? _hasHeating;
		private int? _newHeatingPower;
		private decimal? _heatingProductivity;
		private ProtectionOnHotWaterTap? _protectionOnHotWaterTap;
		private bool? _hasCooling;
		private int? _newCoolingPower;
		private decimal? _coolingProductivity;
		private CoolingType? _newCoolingType;
		private LockerRefrigeratorType? _lockerRefrigeratorType;
		private int? _lockerRefrigeratorVolume;
		private TapType? _tapType;
		private bool _isSparklingWater;

		private decimal _length;
		private decimal _width;
		private decimal _height;
		private IList<NomenclatureOnlineParameters> _nomenclatureOnlineParameters = new List<NomenclatureOnlineParameters>();

		private DateTime? _createDate;
		private User _createdBy;
		private string _officialName;
		private bool _isArchive;
		private bool _isDiler;
		private bool _canPrintPrice;
		private string _code1c;
		private Folder1c _folder1;
		private string _model;
		private decimal _weight;
		private VAT _vAT = VAT.Vat18;
		private bool _doNotReserve;
		private bool _rentPriority;
		private bool _isDuty;
		private MobileCatalog _mobileCatalog;
		private bool _isSerial;
		private MeasurementUnits _unit;
		private decimal _minStockCount;
		private bool _isDisposableTare;
		private TareVolume? _tareVolume;
		private NomenclatureCategory _category;
		private SaleCategory? _saleCategory;
		private TypeOfDepositCategory? _typeOfDepositCategory;
		private EquipmentColors _equipmentColor;
		private EquipmentKind _kind;
		private Manufacturer _manufacturer;
		private RouteColumn _routeListColumn;
		private decimal _sumOfDamage;
		private IList<NomenclaturePrice> _nomenclaturePrice = new List<NomenclaturePrice>();
		private string _shortName;
		private bool _hide;
		private bool _isNewBottle;
		private bool _isDefectiveBottle;
		private bool _isShabbyBottle;
		private FuelType _fuelType;
		private bool _noDelivery;
		private Nomenclature _dependsOnNomenclature;
		private double _percentForMaster;
		private Guid? _onlineStoreGuid;
		private string _description;
		private string _bottleCapColor;
		private OnlineStore _onlineStore;
		private ProductGroup _productGroup;

		private string _onlineStoreExternalId;
		private Counterparty _shipperCounterparty;
		private string _storageCell;
		private string _color;
		private string _material;
		private string _liters;
		private string _size;
		private string _package;
		private string _degreeOfRoast;
		private string _smell;
		private string _taste;
		private string _refrigeratorCapacity;
		private string _coolingType;
		private string _heatingPower;
		private string _coolingPower;
		private string _heatingPerformance;
		private string _coolingPerformance;
		private string _numberOfCartridges;
		private string _characteristicsOfCartridges;
		private string _countryOfOrigin;
		private int? _planMonth;
		private string _amountInAPackage;
		private int? _planDay;
		private int? _heatingTemperatureFromOnline;
		private int? _heatingTemperatureToOnline;
		private int? _coolingTemperatureFromOnline;
		private int? _coolingTemperatureToOnline;
		private int? _lengthOnline;
		private int? _widthOnline;
		private int? _heightOnline;
		private decimal? _weightOnline;
		private PowerUnits? _heatingPowerUnits;
		private PowerUnits? _coolingPowerUnits;
		private ProductivityUnits? _heatingProductivityUnits;
		private ProductivityUnits? _coolingProductivityUnits;
		private ProductivityComparisionSign? _heatingProductivityComparisionSign;
		private ProductivityComparisionSign? _coolingProductivityComparisionSign;
		private IObservableList<NomenclatureMinimumBalanceByWarehouse> _nomenclatureMinimumBalancesByWarehouse = new ObservableList<NomenclatureMinimumBalanceByWarehouse>();

		#region Свойства

		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate
		{
			get => _createDate;
			set => SetField(ref _createDate, value);
		}

		/// <summary>
		/// Кем создана(пользователь)
		/// </summary>
		[Display(Name = "Кем создана")]
		public virtual User CreatedBy
		{
			get => _createdBy;
			set => SetField(ref _createdBy, value);
		}

		/// <summary>
		/// Официальное название
		/// </summary>
		[Display(Name = "Официальное название")]
		public virtual string OfficialName
		{
			get => _officialName;
			set => SetField(ref _officialName, value);
		}

		/// <summary>
		/// Архив
		/// </summary>
		[Display(Name = "Архивная")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		/// <summary>
		/// Дилер
		/// </summary>
		[Display(Name = "Дилер")]
		public virtual bool IsDiler
		{
			get => _isDiler;
			set => SetField(ref _isDiler, value);
		}

		/// <summary>
		/// Печатается прайс в документах
		/// </summary>
		[Display(Name = "Печатается прайс в документах")]
		public virtual bool CanPrintPrice
		{
			get => _canPrintPrice;
			set => SetField(ref _canPrintPrice, value);
		}

		/// <summary>
		/// Код 1с
		/// </summary>
		[Display(Name = "Код 1с")]
		[StringLength(11)]
		public virtual string Code1c
		{
			get => _code1c;
			set => SetField(ref _code1c, value);
		}

		/// <summary>
		/// Папка в 1с
		/// </summary>
		[Display(Name = "Папка в 1с")]
		public virtual Folder1c Folder1C
		{
			get => _folder1;
			set => SetField(ref _folder1, value);
		}

		/// <summary>
		/// Модель оборудования
		/// </summary>
		[Display(Name = "Модель оборудования")]
		public virtual string Model
		{
			get => _model;
			set => SetField(ref _model, value);
		}

		/// <summary>
		/// Вес
		/// </summary>
		[Display(Name = "Вес")]
		public virtual decimal Weight
		{
			get => _weight;
			set => SetField(ref _weight, value);
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

		/// <summary>
		/// НДС
		/// </summary>
		[Display(Name = "НДС")]
		public virtual VAT VAT
		{
			get => _vAT;
			set => SetField(ref _vAT, value);
		}

		/// <summary>
		/// Не резервировать
		/// </summary>
		[Display(Name = "Не резервировать")]
		public virtual bool DoNotReserve
		{
			get => _doNotReserve;
			set => SetField(ref _doNotReserve, value);
		}

		/// <summary>
		/// Приоритет аренды
		/// </summary>
		[Display(Name = "Приоритет аренды")]
		public virtual bool RentPriority
		{
			get => _rentPriority;
			set => SetField(ref _rentPriority, value);
		}

		/// <summary>
		/// Дежурное оборудование
		/// </summary>
		[Display(Name = "Дежурное оборудование")]
		public virtual bool IsDuty
		{
			get => _isDuty;
			set => SetField(ref _isDuty, value);
		}

		/// <summary>
		/// Серийный номер
		/// </summary>
		[Display(Name = "Серийный номер")]
		public virtual bool IsSerial
		{
			get => _isSerial;
			set => SetField(ref _isSerial, value);
		}

		/// <summary>
		/// Единица измерения
		/// </summary>
		[Display(Name = "Единица измерения")]
		public virtual MeasurementUnits Unit
		{
			get => _unit;
			set => SetField(ref _unit, value);
		}

		/// <summary>
		/// Минимальное количество на складе
		/// </summary>
		[Display(Name = "Минимальное количество на складе")]
		public virtual decimal MinStockCount
		{
			get => _minStockCount;
			set => SetField(ref _minStockCount, value);
		}

		/// <summary>
		/// Одноразовая тара
		/// </summary>
		[Display(Name = "Одноразовая тара для воды")]
		public virtual bool IsDisposableTare
		{
			get => _isDisposableTare;
			set => SetField(ref _isDisposableTare, value);
		}

		/// <summary>
		/// Объем тары
		/// </summary>
		[Display(Name = "Объем тары для воды")]
		public virtual TareVolume? TareVolume
		{
			get => _tareVolume;
			set => SetField(ref _tareVolume, value);
		}

		/// <summary>
		/// Категория
		/// </summary>
		[Display(Name = "Категория")]
		public virtual new NomenclatureCategory Category
		{
			get => _category;
			set
			{
				if(SetField(ref _category, value))
				{
					if(!CategoriesWithSerial.Contains(Category))
					{
						IsSerial = false;
					}

					if(Category != NomenclatureCategory.water)
					{
						TareVolume = null;
					}

					if(!GetCategoriesWithSaleCategory().Contains(value))
					{
						SaleCategory = null;
					}
					if(value != NomenclatureCategory.master)
					{
						MasterServiceType = null;
					}
				}
			}
		}

		/// <summary>
		/// Доступность для продаж
		/// </summary>
		[Display(Name = "Доступность для продаж")]
		public virtual SaleCategory? SaleCategory
		{
			get => _saleCategory;
			set => SetField(ref _saleCategory, value);
		}

		/// <summary>
		/// Подкатегория залогов
		/// </summary>
		[Display(Name = "Подкатегория залогов")]
		public virtual TypeOfDepositCategory? TypeOfDepositCategory
		{
			get => _typeOfDepositCategory;
			set => SetField(ref _typeOfDepositCategory, value);
		}

		/// <summary>
		/// Тип выезда мастера
		/// </summary>
		[Display(Name = "Тип выезда мастера")]
		public virtual MasterServiceType? MasterServiceType
		{
			get => _masterServiceType;
			set => SetField(ref _masterServiceType, value);
		}

		/// <summary>
		/// Цвет оборудования
		/// </summary>
		[Display(Name = "Цвет оборудования")]
		public virtual EquipmentColors EquipmentColor
		{
			get => _equipmentColor;
			set => SetField(ref _equipmentColor, value);
		}

		/// <summary>
		/// Вид оборудования
		/// </summary>
		[Display(Name = "Вид оборудования")]
		public virtual EquipmentKind Kind
		{
			get => _kind;
			set => SetField(ref _kind, value);
		}

		/// <summary>
		/// Производитель
		/// </summary>
		[Display(Name = "Производитель")]
		public virtual Manufacturer Manufacturer
		{
			get => _manufacturer;
			set => SetField(ref _manufacturer, value);
		}

		/// <summary>
		/// Колонка МЛ
		/// </summary>
		[Display(Name = "Колонка МЛ")]
		public virtual RouteColumn RouteListColumn
		{
			get => _routeListColumn;
			set => SetField(ref _routeListColumn, value);
		}

		/// <summary>
		/// Сумма ущерба
		/// </summary>
		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage
		{
			get => _sumOfDamage;
			set => SetField(ref _sumOfDamage, value);
		}

		/// <summary>
		/// Цены
		/// </summary>
		[Display(Name = "Цены")]
		public virtual IList<NomenclaturePrice> NomenclaturePrice
		{
			get => _nomenclaturePrice;
			set => SetField(ref _nomenclaturePrice, value);
		}

		/// <summary>
		/// Альтернативные цены
		/// </summary>
		[Display(Name = "Альтернативные цены")]
		public virtual IList<AlternativeNomenclaturePrice> AlternativeNomenclaturePrices
		{
			get => _alternativeNomenclaturePrices;
			set => SetField(ref _alternativeNomenclaturePrices, value);
		}

		/// <summary>
		/// Сокращенное название
		/// </summary>
		[Display(Name = "Сокращенное название")]
		public virtual string ShortName
		{
			get => _shortName;
			set => SetField(ref _shortName, value);
		}

		/// <summary>
		/// Скрыть из МЛ
		/// </summary>
		[Display(Name = "Скрыть из МЛ")]
		public virtual bool Hide
		{
			get => _hide;
			set => SetField(ref _hide, value);
		}

		/// <summary>
		/// Доставка не требуется
		/// </summary>
		[Display(Name = "Доставка не требуется")]
		public virtual bool NoDelivery
		{
			get => _noDelivery;
			set => SetField(ref _noDelivery, value);
		}

		/// <summary>
		/// Это новая бутыль
		/// </summary>
		[Display(Name = "Это новая бутыль")]
		public virtual bool IsNewBottle
		{
			get => _isNewBottle;
			set
			{
				if(SetField(ref _isNewBottle, value) && _isNewBottle)
				{
					IsDefectiveBottle = false;
					IsShabbyBottle = false;
				}
			}
		}

		/// <summary>
		/// Это бракованая бутыль
		/// </summary>
		[Display(Name = "Это бракованая бутыль")]
		public virtual bool IsDefectiveBottle
		{
			get => _isDefectiveBottle;
			set
			{
				if(SetField(ref _isDefectiveBottle, value) && _isDefectiveBottle)
				{
					IsNewBottle = false;
					IsShabbyBottle = false;
				}
			}
		}

		/// <summary>
		/// Стройка
		/// </summary>
		[Display(Name = "Стройка")]
		public virtual bool IsShabbyBottle
		{
			get => _isShabbyBottle;
			set
			{
				if(SetField(ref _isShabbyBottle, value) && _isShabbyBottle)
				{
					IsNewBottle = false;
					IsDefectiveBottle = false;
				}
			}
		}

		/// <summary>
		/// Тип топлива
		/// </summary>
		[Display(Name = "Тип топлива")]
		public virtual FuelType FuelType
		{
			get => _fuelType;
			set => SetField(ref _fuelType, value);
		}

		/// <summary>
		/// Влияющая номенклатура
		/// </summary>
		[Display(Name = "Влияющая номенклатура")]
		public virtual Nomenclature DependsOnNomenclature
		{
			get => _dependsOnNomenclature;
			set => SetField(ref _dependsOnNomenclature, value);
		}

		/// <summary>
		/// Процент зарплаты мастера
		/// </summary>
		[Display(Name = "Процент зарплаты мастера")]
		public virtual double PercentForMaster
		{
			get => _percentForMaster;
			set => SetField(ref _percentForMaster, value);
		}

		/// <summary>
		/// Guid интернет магазина
		/// </summary>
		[Display(Name = "Guid интернет магазина")]
		public virtual Guid? OnlineStoreGuid
		{
			get => _onlineStoreGuid;
			set => SetField(ref _onlineStoreGuid, value);
		}

		/// <summary>
		/// Группа товаров
		/// </summary>
		[Display(Name = "Группа товаров")]
		public virtual ProductGroup ProductGroup
		{
			get => _productGroup;
			set => SetField(ref _productGroup, value);
		}

		/// <summary>
		/// Каталог в мобильном приложении
		/// </summary>
		[Display(Name = "Каталог в мобильном приложении")]
		public virtual MobileCatalog MobileCatalog
		{
			get => _mobileCatalog;
			set => SetField(ref _mobileCatalog, value);
		}

		/// <summary>
		/// Описание товара
		/// </summary>
		[Display(Name = "Описание товара")]
		public virtual string Description
		{
			get => _description;
			set => SetField(ref _description, value);
		}

		/// <summary>
		/// Цвет пробки 19л бутыли
		/// </summary>
		[Display(Name = "Цвет пробки 19л бутыли")]
		public virtual string BottleCapColor
		{
			get => _bottleCapColor;
			set => SetField(ref _bottleCapColor, value);
		}

		/// <summary>
		/// Интернет-магазин
		/// </summary>
		[Display(Name = "Интернет-магазин")]
		public virtual OnlineStore OnlineStore
		{
			get => _onlineStore;
			set => SetField(ref _onlineStore, value);
		}

		/// <summary>
		/// Участвует в групповом заполнении себестоимости
		/// </summary>
		[Display(Name = "Участвует в групповом заполнении себестоимости")]
		public virtual bool UsingInGroupPriceSet
		{
			get => _usingInGroupPriceSet;
			set => SetField(ref _usingInGroupPriceSet, value);
		}

		/// <summary>
		/// Параметры номенклатуры для ИПЗ
		/// </summary>
		public virtual IList<NomenclatureOnlineParameters> NomenclatureOnlineParameters
		{
			get => _nomenclatureOnlineParameters;
			set => SetField(ref _nomenclatureOnlineParameters, value);
		}

		/// <summary>
		/// Цены закупки ТМЦ
		/// </summary>
		[Display(Name = "Цены закупки ТМЦ")]
		public virtual IList<NomenclaturePurchasePrice> PurchasePrices
		{
			get => _purchasePrices;
			set => SetField(ref _purchasePrices, value);
		}

		public virtual GenericObservableList<NomenclaturePurchasePrice> ObservablePurchasePrices =>
			_observablePurchasePrices ?? (_observablePurchasePrices = new GenericObservableList<NomenclaturePurchasePrice>(PurchasePrices));

		/// <summary>
		/// Себестоимость ТМЦ
		/// </summary>
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

		/// <summary>
		/// Стоимости доставки ТМЦ на склад
		/// </summary>
		[Display(Name = "Стоимости доставки ТМЦ на склад")]
		public virtual IList<NomenclatureInnerDeliveryPrice> InnerDeliveryPrices
		{
			get => _innerDeliveryPrices;
			set => SetField(ref _innerDeliveryPrices, value);
		}

		/// <summary>
		/// Минимальный остаток на складе
		/// </summary>
		[Display(Name = "Минимальный остаток на складе")]
		public virtual IObservableList<NomenclatureMinimumBalanceByWarehouse> NomenclatureMinimumBalancesByWarehouse
		{
			get => _nomenclatureMinimumBalancesByWarehouse;
			set => SetField(ref _nomenclatureMinimumBalancesByWarehouse, value);
		}

		public virtual GenericObservableList<NomenclatureInnerDeliveryPrice> ObservableInnerDeliveryPrices =>
			_observableInnerDeliveryPrices ?? (_observableInnerDeliveryPrices = new GenericObservableList<NomenclatureInnerDeliveryPrice>(InnerDeliveryPrices));

		/// <summary>
		/// Инвентарный учет
		/// </summary>
		[Display(Name = "Инвентарный учет")]
		public virtual bool HasInventoryAccounting
		{
			get => _hasInventoryAccounting;
			set => SetField(ref _hasInventoryAccounting, value);
		}
		
		/// <summary>
		/// Учет состояния ТМЦ(б/у | Нов)
		/// </summary>
		[Display(Name = "Учет состояния ТМЦ(б/у | Нов)")]
		public virtual bool HasConditionAccounting
		{
			get => _hasConditionAccounting;
			set => SetField(ref _hasConditionAccounting, value);
		}

		/// <summary>
		/// Тип стаканодержателя
		/// </summary>
		[Display(Name = "Тип стаканодержателя")]
		public virtual GlassHolderType? GlassHolderType
		{
			get => _glassHolderType;
			set => SetField(ref _glassHolderType, value);
		}

		#endregion Свойства

		#region Свойства товаров для магазина

		/// <summary>
		/// Id в интернет магазине
		/// </summary>
		[Display(Name = "Id в интернет магазине")]
		public virtual string OnlineStoreExternalId
		{
			get => _onlineStoreExternalId;
			set => SetField(ref _onlineStoreExternalId, value);
		}

		/// <summary>
		/// Поставщик
		/// </summary>
		[Display(Name = "Поставщик")]
		public virtual Counterparty ShipperCounterparty
		{
			get => _shipperCounterparty;
			set => SetField(ref _shipperCounterparty, value);
		}

		/// <summary>
		/// Ячейка хранения
		/// </summary>
		[Display(Name = "Ячейка хранения")]
		public virtual string StorageCell
		{
			get => _storageCell;
			set => SetField(ref _storageCell, value);
		}

		/// <summary>
		/// Цвет
		/// </summary>
		[Display(Name = "Цвет")]
		public virtual string Color
		{
			get => _color;
			set => SetField(ref _color, value);
		}

		/// <summary>
		/// Материал
		/// </summary>
		[Display(Name = "Материал")]
		public virtual string Material
		{
			get => _material;
			set => SetField(ref _material, value);
		}

		/// <summary>
		/// Объем
		/// </summary>
		[Display(Name = "Объем")]
		public virtual string Liters
		{
			get => _liters;
			set => SetField(ref _liters, value);
		}

		/// <summary>
		/// Размеры
		/// </summary>
		[Display(Name = "Размеры")]
		public virtual string Size
		{
			get => _size;
			set => SetField(ref _size, value);
		}

		/// <summary>
		/// Тип упаковки
		/// </summary>
		[Display(Name = "Тип упаковки")]
		public virtual string Package
		{
			get => _package;
			set => SetField(ref _package, value);
		}

		/// <summary>
		/// Степень обжарки
		/// </summary>
		[Display(Name = "Степень обжарки")]
		public virtual string DegreeOfRoast
		{
			get => _degreeOfRoast;
			set => SetField(ref _degreeOfRoast, value);
		}

		/// <summary>
		/// Запах
		/// </summary>
		[Display(Name = "Запах")]
		public virtual string Smell
		{
			get => _smell;
			set => SetField(ref _smell, value);
		}

		/// <summary>
		/// Вкус
		/// </summary>
		[Display(Name = "Вкус")]
		public virtual string Taste
		{
			get => _taste;
			set => SetField(ref _taste, value);
		}

		/// <summary>
		/// Объем шкафчика/холодильника
		/// </summary>
		[Display(Name = "Объем шкафчика/холодильника")]
		public virtual string RefrigeratorCapacity
		{
			get => _refrigeratorCapacity;
			set => SetField(ref _refrigeratorCapacity, value);
		}

		/// <summary>
		/// Тип охлаждения
		/// </summary>
		[Display(Name = "Тип охлаждения")]
		public virtual string CoolingType
		{
			get => _coolingType;
			set => SetField(ref _coolingType, value);
		}

		/// <summary>
		/// Мощность нагрева
		/// </summary>
		[Display(Name = "Мощность нагрева")]
		public virtual string HeatingPower
		{
			get => _heatingPower;
			set => SetField(ref _heatingPower, value);
		}

		/// <summary>
		/// Мощность охлаждения
		/// </summary>
		[Display(Name = "Мощность охлаждения")]
		public virtual string CoolingPower
		{
			get => _coolingPower;
			set => SetField(ref _coolingPower, value);
		}

		/// <summary>
		/// Производительность нагрева
		/// </summary>
		[Display(Name = "Производительность нагрева")]
		public virtual string HeatingPerformance
		{
			get => _heatingPerformance;
			set => SetField(ref _heatingPerformance, value);
		}

		/// <summary>
		/// Производительность охлаждения
		/// </summary>
		[Display(Name = "Производительность охлаждения")]
		public virtual string CoolingPerformance
		{
			get => _coolingPerformance;
			set => SetField(ref _coolingPerformance, value);
		}

		/// <summary>
		/// Количество картриджей
		/// </summary>
		[Display(Name = "Количество картриджей")]
		public virtual string NumberOfCartridges
		{
			get => _numberOfCartridges;
			set => SetField(ref _numberOfCartridges, value);
		}

		/// <summary>
		/// Характеристика картриджей
		/// </summary>
		[Display(Name = "Характеристика картриджей")]
		public virtual string CharacteristicsOfCartridges
		{
			get => _characteristicsOfCartridges;
			set => SetField(ref _characteristicsOfCartridges, value);
		}

		/// <summary>
		/// Страна происхождения
		/// </summary>
		[Display(Name = "Страна происхождения")]
		public virtual string CountryOfOrigin
		{
			get => _countryOfOrigin;
			set => SetField(ref _countryOfOrigin, value);
		}

		/// <summary>
		/// Количество  в упаковке
		/// </summary>
		[Display(Name = "Количество  в упаковке")]
		public virtual string AmountInAPackage
		{
			get => _amountInAPackage;
			set => SetField(ref _amountInAPackage, value);
		}

		/// <summary>
		/// План день
		/// </summary>
		[Display(Name = "План день")]
		public virtual int? PlanDay
		{
			get => _planDay;
			set => SetField(ref _planDay, value);
		}

		/// <summary>
		/// План месяц
		/// </summary>
		[Display(Name = "План месяц")]
		public virtual int? PlanMonth
		{
			get => _planMonth;
			set => SetField(ref _planMonth, value);
		}

		#endregion Свойства товаров для магазина

		#region Онлайн характеристики для ИПЗ

		/// <summary>
		/// Онлайн каталог в мобильном приложении
		/// </summary>
		[Display(Name = "Онлайн каталог в мобильном приложении")]
		public virtual MobileAppNomenclatureOnlineCatalog MobileAppNomenclatureOnlineCatalog
		{
			get => _mobileAppNomenclatureOnlineCatalog;
			set => SetField(ref _mobileAppNomenclatureOnlineCatalog, value);
		}

		/// <summary>
		/// Онлайн каталог на сайте ВВ
		/// </summary>
		[Display(Name = "Онлайн каталог на сайте ВВ")]
		public virtual VodovozWebSiteNomenclatureOnlineCatalog VodovozWebSiteNomenclatureOnlineCatalog
		{
			get => _vodovozWebSiteNomenclatureOnlineCatalog;
			set => SetField(ref _vodovozWebSiteNomenclatureOnlineCatalog, value);
		}

		/// <summary>
		/// Онлайн каталог на сайте Кулер Сэйл
		/// </summary>
		[Display(Name = "Онлайн каталог на сайте Кулер Сэйл")]
		public virtual KulerSaleWebSiteNomenclatureOnlineCatalog KulerSaleWebSiteNomenclatureOnlineCatalog
		{
			get => _kulerSaleWebSiteNomenclatureOnlineCatalog;
			set => SetField(ref _kulerSaleWebSiteNomenclatureOnlineCatalog, value);
		}

		/// <summary>
		/// Онлайн вид товара
		/// </summary>
		[Display(Name = "Онлайн вид товара")]
		public virtual NomenclatureOnlineGroup NomenclatureOnlineGroup
		{
			get => _nomenclatureOnlineGroup;
			set => SetField(ref _nomenclatureOnlineGroup, value);
		}

		/// <summary>
		/// Онлайн тип товара
		/// </summary>
		[Display(Name = "Онлайн тип товара")]
		public virtual NomenclatureOnlineCategory NomenclatureOnlineCategory
		{
			get => _nomenclatureOnlineCategory;
			set => SetField(ref _nomenclatureOnlineCategory, value);
		}

		/// <summary>
		/// Название в ИПЗ
		/// </summary>
		[Display(Name = "Название в ИПЗ")]
		public virtual string OnlineName
		{
			get => _onlineName;
			set => SetField(ref _onlineName, value);
		}

		/// <summary>
		/// Тип установки
		/// </summary>
		[Display(Name = "Тип установки")]
		public virtual EquipmentInstallationType? EquipmentInstallationType
		{
			get => _equipmentInstallationType;
			set => SetField(ref _equipmentInstallationType, value);
		}

		/// <summary>
		/// Тип загрузки
		/// </summary>
		[Display(Name = "Тип загрузки")]
		public virtual EquipmentWorkloadType? EquipmentWorkloadType
		{
			get => _equipmentWorkloadType;
			set => SetField(ref _equipmentWorkloadType, value);
		}

		/// <summary>
		/// Тип помпы
		/// </summary>
		[Display(Name = "Тип помпы")]
		public virtual PumpType? PumpType
		{
			get => _pumpType;
			set => SetField(ref _pumpType, value);
		}

		/// <summary>
		/// Тип крепления(стаканодержатель)
		/// </summary>
		[Display(Name = "Тип крепления(стаканодержатель)")]
		public virtual CupHolderBracingType? CupHolderBracingType
		{
			get => _cupHolderBracingType;
			set => SetField(ref _cupHolderBracingType, value);
		}

		/// <summary>
		/// Нагрев
		/// </summary>
		[Display(Name = "Нагрев")]
		public virtual bool? HasHeating
		{
			get => _hasHeating;
			set => SetField(ref _hasHeating, value);
		}

		/// <summary>
		/// Мощность нагрева
		/// </summary>
		[Display(Name = "Мощность нагрева")]
		public virtual int? NewHeatingPower
		{
			get => _newHeatingPower;
			set => SetField(ref _newHeatingPower, value);
		}

		/// <summary>
		/// Производительность нагрева
		/// </summary>
		[Display(Name = "Производительность нагрева")]
		public virtual decimal? HeatingProductivity
		{
			get => _heatingProductivity;
			set => SetField(ref _heatingProductivity, value);
		}

		/// <summary>
		/// Защита на кране горячей воды
		/// </summary>
		[Display(Name = "Защита на кране горячей воды")]
		public virtual ProtectionOnHotWaterTap? ProtectionOnHotWaterTap
		{
			get => _protectionOnHotWaterTap;
			set => SetField(ref _protectionOnHotWaterTap, value);
		}

		/// <summary>
		/// Охлаждение
		/// </summary>
		[Display(Name = "Охлаждение")]
		public virtual bool? HasCooling
		{
			get => _hasCooling;
			set => SetField(ref _hasCooling, value);
		}

		/// <summary>
		/// Мощность охлаждения
		/// </summary>
		[Display(Name = "Мощность охлаждения")]
		public virtual int? NewCoolingPower
		{
			get => _newCoolingPower;
			set => SetField(ref _newCoolingPower, value);
		}

		/// <summary>
		/// Производительность охлаждения
		/// </summary>
		[Display(Name = "Производительность охлаждения")]
		public virtual decimal? CoolingProductivity
		{
			get => _coolingProductivity;
			set => SetField(ref _coolingProductivity, value);
		}

		/// <summary>
		/// Тип охлаждения
		/// </summary>
		[Display(Name = "Тип охлаждения")]
		public virtual CoolingType? NewCoolingType
		{
			get => _newCoolingType;
			set => SetField(ref _newCoolingType, value);
		}

		/// <summary>
		/// Шкафчик/холодильник
		/// </summary>
		[Display(Name = "Шкафчик/холодильник")]
		public virtual LockerRefrigeratorType? LockerRefrigeratorType
		{
			get => _lockerRefrigeratorType;
			set => SetField(ref _lockerRefrigeratorType, value);
		}

		/// <summary>
		/// Объем шкафчика/холодильника
		/// </summary>
		[Display(Name = "Объем шкафчика/холодильника")]
		public virtual int? LockerRefrigeratorVolume
		{
			get => _lockerRefrigeratorVolume;
			set => SetField(ref _lockerRefrigeratorVolume, value);
		}

		/// <summary>
		/// Тип кранов
		/// </summary>
		[Display(Name = "Тип кранов")]
		public virtual TapType? TapType
		{
			get => _tapType;
			set => SetField(ref _tapType, value);
		}

		/// <summary>
		/// Газированная вода
		/// </summary>
		[Display(Name = "Газированная вода")]
		public virtual bool IsSparklingWater
		{
			get => _isSparklingWater;
			set => SetField(ref _isSparklingWater, value);
		}

		/// <summary>
		/// Температура нагрева от
		/// </summary>
		[Display(Name = "Температура нагрева от")]
		public virtual int? HeatingTemperatureFromOnline
		{
			get => _heatingTemperatureFromOnline;
			set => SetField(ref _heatingTemperatureFromOnline, value);
		}

		/// <summary>
		/// Температура нагрева до
		/// </summary>
		[Display(Name = "Температура нагрева до")]
		public virtual int? HeatingTemperatureToOnline
		{
			get => _heatingTemperatureToOnline;
			set => SetField(ref _heatingTemperatureToOnline, value);
		}

		/// <summary>
		/// Температура охлаждения от
		/// </summary>
		[Display(Name = "Температура охлаждения от")]
		public virtual int? CoolingTemperatureFromOnline
		{
			get => _coolingTemperatureFromOnline;
			set => SetField(ref _coolingTemperatureFromOnline, value);
		}

		/// <summary>
		/// Температура охлаждения до
		/// </summary>
		[Display(Name = "Температура охлаждения до")]
		public virtual int? CoolingTemperatureToOnline
		{
			get => _coolingTemperatureToOnline;
			set => SetField(ref _coolingTemperatureToOnline, value);
		}

		/// <summary>
		/// Длина для ИПЗ
		/// </summary>
		[Display(Name = "Длина для ИПЗ")]
		public virtual int? LengthOnline
		{
			get => _lengthOnline;
			set => SetField(ref _lengthOnline, value);
		}
		
		/// <summary>
		/// Ширина для ИПЗ
		/// </summary>
		[Display(Name = "Ширина для ИПЗ")]
		public virtual int? WidthOnline
		{
			get => _widthOnline;
			set => SetField(ref _widthOnline, value);
		}
		
		/// <summary>
		/// Высота для ИПЗ
		/// </summary>
		[Display(Name = "Высота для ИПЗ")]
		public virtual int? HeightOnline
		{
			get => _heightOnline;
			set => SetField(ref _heightOnline, value);
		}
		
		/// <summary>
		/// Вес для ИПЗ
		/// </summary>
		[Display(Name = "Вес для ИПЗ")]
		public virtual decimal? WeightOnline
		{
			get => _weightOnline;
			set => SetField(ref _weightOnline, value);
		}

		/// <summary>
		/// Единицы измерения мощности нагрева
		/// </summary>
		[Display(Name = "Единицы измерения мощности нагрева")]
		public virtual PowerUnits? HeatingPowerUnits
		{
			get => _heatingPowerUnits;
			set => SetField(ref _heatingPowerUnits, value);
		}
		
		/// <summary>
		/// Единицы измерения мощности охлаждения
		/// </summary>
		[Display(Name = "Единицы измерения мощности охлаждения")]
		public virtual PowerUnits? CoolingPowerUnits
		{
			get => _coolingPowerUnits;
			set => SetField(ref _coolingPowerUnits, value);
		}
		
		/// <summary>
		/// Единицы измерения производительности нагрева
		/// </summary>
		[Display(Name = "Единицы измерения производительности нагрева")]
		public virtual ProductivityUnits? HeatingProductivityUnits
		{
			get => _heatingProductivityUnits;
			set => SetField(ref _heatingProductivityUnits, value);
		}
		
		/// <summary>
		/// Единицы измерения производительности охлаждения
		/// </summary>
		[Display(Name = "Единицы измерения производительности охлаждения")]
		public virtual ProductivityUnits? CoolingProductivityUnits
		{
			get => _coolingProductivityUnits;
			set => SetField(ref _coolingProductivityUnits, value);
		}
		
		/// <summary>
		/// Показатель производительности нагрева
		/// </summary>
		[Display(Name = "Показатель производительности нагрева")]
		public virtual ProductivityComparisionSign? HeatingProductivityComparisionSign
		{
			get => _heatingProductivityComparisionSign;
			set => SetField(ref _heatingProductivityComparisionSign, value);
		}
		
		/// <summary>
		/// Показатель производительности охлаждения
		/// </summary>
		[Display(Name = "Показатель производительности охлаждения")]
		public virtual ProductivityComparisionSign? CoolingProductivityComparisionSign
		{
			get => _coolingProductivityComparisionSign;
			set => SetField(ref _coolingProductivityComparisionSign, value);
		}

		#endregion Онлайн характеристики для ИПЗ

		#region Рассчетные

		public virtual string CategoryString => Category.GetEnumTitle();

		public virtual string ShortOrFullName => string.IsNullOrWhiteSpace(ShortName) ? Name : ShortName;

		public virtual bool IsWater19L =>
			Category == NomenclatureCategory.water
			&& TareVolume.HasValue
			&& TareVolume.Value == Core.Domain.Goods.TareVolume.Vol19L;

		public override string ToString() => $"id = {Id} Name = {Name}";

		#endregion Рассчетные

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
			{
				itemsCount = 1;
			}

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
				{
					return true;
				}

				parent = parent.Parent;
			}
			return false;
		}

		public virtual decimal GetPurchasePriceOnDate(DateTime date)
		{
			var purchasePrice =
				PurchasePrices
				.Where(p => p.StartDate <= date && (p.EndDate == null || p.EndDate >= date))
				.Select(p => p.PurchasePrice)
				.FirstOrDefault();

			return purchasePrice;
		}

		#endregion Методы

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.GetService(
				typeof(INomenclatureRepository)) is INomenclatureRepository nomenclatureRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(nomenclatureRepository)}");
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult(
					"Название номенклатуры должно быть заполнено.", new[] { nameof(Name) });
			}
			else if(Name.Length > 220)
			{
				yield return new ValidationResult(
					"Превышено максимальное количество символов в названии (220).", new[] { nameof(Name) });
			}

			if(string.IsNullOrWhiteSpace(OfficialName))
			{
				yield return new ValidationResult(
					"Официальное название номенклатуры должно быть заполнено.", new[] { nameof(OfficialName) });
			}
			else if(Name.Length > 220)
			{
				yield return new ValidationResult(
					"Превышено максимальное количество символов в официальном названии (220).", new[] { nameof(OfficialName) });
			}

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
			{
				yield return new ValidationResult(
					"Папка 1С обязательна для заполнения", new[] { nameof(Folder1C) });
			}

			if(string.IsNullOrWhiteSpace(Code1c))
			{
				yield return new ValidationResult(
					"Код 1С обязателен для заполнения", new[] { nameof(Code1c) });
			}

			if(Category == NomenclatureCategory.equipment && Kind == null)
			{
				yield return new ValidationResult(
					"Не указан вид оборудования.",
					new[] { nameof(Kind) });
			}

			if(GetCategoriesWithSaleCategory().Contains(_category) && SaleCategory == null)
			{
				yield return new ValidationResult(
					"Не указана \"Доступность для продажи\"",
					new[] { nameof(SaleCategory) }
				);
			}

			if(Category == NomenclatureCategory.deposit && TypeOfDepositCategory == null)
			{
				yield return new ValidationResult(
					"Не указан тип залога.",
					new[] { nameof(TypeOfDepositCategory) });
			}

			if(Category == NomenclatureCategory.water && !TareVolume.HasValue)
			{
				yield return new ValidationResult(
					"Не выбран объем тары",
					new[] { nameof(TareVolume) }
				);
			}

			if(Category == NomenclatureCategory.fuel && FuelType == null)
			{
				yield return new ValidationResult("Не выбран тип топлива");
			}

			if(Unit == null)
			{
				yield return new ValidationResult(
					"Не указаны единицы измерения",
					new[] { nameof(Unit) });
			}

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

					yield return new ValidationResult(dependedNomenclaturesText, new[] { nameof(DependsOnNomenclature) });
				}

				if(DependsOnNomenclature.DependsOnNomenclature != null)
				{
					yield return new ValidationResult(
						$"Номенклатура '{DependsOnNomenclature.ShortOrFullName}' указанная в качеcтве основной для цен этой номеклатуры, сама зависит от '{DependsOnNomenclature.DependsOnNomenclature.ShortOrFullName}'",
						new[] { nameof(DependsOnNomenclature) });
				}
			}

			if(Code1c != null && Code1c.StartsWith(PrefixOfCode1c))
			{
				if(Code1c.Length != LengthOfCode1c)
				{
					yield return new ValidationResult(
						$"Код 1с с префиксом автоформирования '{PrefixOfCode1c}', должен содержать {LengthOfCode1c}-символов.",
						new[] { nameof(Code1c) });
				}

				var next = nomenclatureRepository.GetNextCode1c(UoW);
				if(string.Compare(Code1c, next) > 0)
				{
					yield return new ValidationResult(
						$"Код 1с использует префикс автоматического формирования кодов '{PrefixOfCode1c}'. При этом пропускает некоторое количество значений. Используйте в качестве следующего кода {next} или оставьте это поле пустым для автозаполенения.",
						new[] { nameof(Code1c) });
				}
			}

			if(DateTime.Now >= new DateTime(2019, 01, 01) && VAT == VAT.Vat18)
			{
				yield return new ValidationResult(
					"С 01.01.2019 ставка НДС 20%",
					new[] { nameof(VAT) }
				);
			}

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

			if((_lengthOnline >= 0 && (!_widthOnline.HasValue || _widthOnline == 0 || !_heightOnline.HasValue || _heightOnline == 0))
				|| (_widthOnline >= 0 && (!_lengthOnline.HasValue || _lengthOnline == 0 || !_heightOnline.HasValue || _heightOnline == 0))
				|| (_heightOnline >= 0 && (!_lengthOnline.HasValue || _lengthOnline == 0 || !_widthOnline.HasValue || _widthOnline == 0)))
			{
				yield return new ValidationResult(
					"Габариты на вкладке Сайты и приложения должны быть либо пустыми, либо заполнены и больше 0",
					new[] { nameof(LengthOnline), nameof(WidthOnline), nameof(HeightOnline) });
			}
			
			if(_weightOnline == 0)
			{
				yield return new ValidationResult(
					"Вес на вкладке Сайты и приложения должен быть больше 0",
					new[] { nameof(WeightOnline) });
			}
			
			if(_coolingTemperatureFromOnline > _coolingTemperatureToOnline)
			{
				yield return new ValidationResult("Начальное значение температуры охлаждения не может быть больше конечного",
					new[] { nameof(CoolingTemperatureFromOnline), nameof(CoolingTemperatureToOnline) });
			}
			
			if(_heatingTemperatureFromOnline > _heatingTemperatureToOnline)
			{
				yield return new ValidationResult("Начальное значение температуры нагрева не может быть больше конечного",
					new[] { nameof(HeatingTemperatureFromOnline), nameof(HeatingTemperatureToOnline) });
			}
		}

		#endregion IValidatableObject implementation

		#region Statics

		public static string PrefixOfCode1c = "ДВ";
		public static int LengthOfCode1c = 10;
		private MasterServiceType? _masterServiceType;

		/// <summary>
		/// Категории товаров к которым применима категория продажи
		/// (доступность для продаж) "<see cref="SaleCategory"/>"
		/// </summary>
		/// <returns>Массив <see cref="NomenclatureCategory"/> к которым может применяться <see cref="SaleCategory"/></returns>
		public static NomenclatureCategory[] GetCategoriesWithSaleCategory()
		{
			return new[]
			{
				NomenclatureCategory.equipment,
				NomenclatureCategory.material,
				NomenclatureCategory.bottle,
				NomenclatureCategory.spare_parts
			};
		}

		public static NomenclatureCategory[] GetCategoriesForShipment()
		{
			return new[]
			{
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
			return new[]
			{
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
			return new[]
			{
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
			return new[]
			{
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
			return GetCategoriesForSale()
				.Concat(new []
				{
					NomenclatureCategory.master,
					NomenclatureCategory.spare_parts
				}).ToArray();
		}

		/// <summary>
		/// Категории товаров. Товары могут хранится на складе.
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesForGoods()
		{
			return new[]
			{
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
				NomenclatureCategory.Tools,
				NomenclatureCategory.CarParts
			};
		}

		/// <summary>
		/// Категории товаров. Товары могут хранится на складе без учёта 19л воды.
		/// </summary>
		public static NomenclatureCategory[] GetCategoriesForGoodsWithoutEmptyBottles()
		{
			return new[]
			{
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
			return new[]
			{
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
			return new[]
			{
				NomenclatureCategory.water
			};
		}

		public static NomenclatureCategory[] GetCategoriesNotNeededToLoad()
		{
			return new[]
			{
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

		#endregion Statics

		public virtual void ResetNotWaterOnlineParameters()
		{
			EquipmentInstallationType = null;
			EquipmentWorkloadType = null;
			PumpType = null;
			CupHolderBracingType = null;
			HasHeating = null;
			HasCooling = null;
			ResetHeatingParameters();
			ResetCoolingParameters();
			LockerRefrigeratorType = null;
			LockerRefrigeratorVolume = null;
			TapType = null;
		}

		public virtual void ResetNotKulerOnlineParameters()
		{
			IsSparklingWater = false;
			PumpType = null;
			CupHolderBracingType = null;
		}

		public virtual void ResetNotPurifierOnlineParameters()
		{
			ResetNotKulerOnlineParameters();
			EquipmentWorkloadType = null;
			LockerRefrigeratorType = null;
			LockerRefrigeratorVolume = null;
			TapType = null;
		}

		public virtual void ResetNotWaterPumpOnlineParameters()
		{
			IsSparklingWater = false;
			EquipmentInstallationType = null;
			EquipmentWorkloadType = null;
			CupHolderBracingType = null;
			HasHeating = null;
			HasCooling = null;
			ResetHeatingParameters();
			ResetCoolingParameters();
			LockerRefrigeratorType = null;
			LockerRefrigeratorVolume = null;
			TapType = null;
		}

		public virtual void ResetNotCupHolderOnlineParameters()
		{
			IsSparklingWater = false;
			EquipmentInstallationType = null;
			EquipmentWorkloadType = null;
			PumpType = null;
			HasHeating = null;
			HasCooling = null;
			ResetHeatingParameters();
			ResetCoolingParameters();
			LockerRefrigeratorType = null;
			LockerRefrigeratorVolume = null;
			TapType = null;
		}

		public virtual void ResetCoolingParameters()
		{
			NewCoolingPower = null;
			CoolingPowerUnits = null;
			CoolingProductivity = null;
			CoolingProductivityComparisionSign = null;
			CoolingProductivityUnits = null;
			NewCoolingType = null;
			CoolingTemperatureFromOnline = null;
			CoolingTemperatureToOnline = null;
		}

		public virtual void ResetHeatingParameters()
		{
			NewHeatingPower = null;
			HeatingPowerUnits = null;
			HeatingProductivity = null;
			HeatingProductivityComparisionSign = null;
			HeatingProductivityUnits = null;
			ProtectionOnHotWaterTap = null;
			HeatingTemperatureFromOnline = null;
			HeatingTemperatureToOnline = null;
		}

		public virtual void ResetLockerRefrigeratorVolume()
		{
			LockerRefrigeratorVolume = null;
		}
	}
}
