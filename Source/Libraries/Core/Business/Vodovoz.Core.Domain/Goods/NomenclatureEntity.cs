using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Номенклатура
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Accusative = "номенклатуру",
		AccusativePlural = "номенклатуры",
		Genitive = "номенклатуры",
		GenitivePlural = "номенклатур",
		Nominative = "номенклатура",
		NominativePlural = "номенклатуры",
		Prepositional = "номенклатуре",
		PrepositionalPlural = "номенклатурах")]
	[EntityPermission]
	[HistoryTrace]
	public class NomenclatureEntity : PropertyChangedBase, INamedDomainObject, IBusinessObject, IHasAttachedFilesInformations<NomenclatureFileInformation>
	{
		private int _id;
		private string _name;
		private NomenclatureCategory _category;
		private bool _isAccountableInTrueMark;
		private string _gtin;

		private bool _usingInGroupPriceSet;
		private bool _hasInventoryAccounting;
		private bool _hasConditionAccounting;
		private GlassHolderType? _glassHolderType;
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

		private DateTime? _createDate;
		private string _officialName;
		private bool _isArchive;
		private bool _isDiler;
		private bool _canPrintPrice;
		private string _code1c;
		private string _model;
		private decimal _weight;
		private bool _doNotReserve;
		private bool _rentPriority;
		private bool _isDuty;
		private MobileCatalog _mobileCatalog;
		private bool _isSerial;
		private decimal _minStockCount;
		private bool _isDisposableTare;
		private TareVolume? _tareVolume;
		private SaleCategory? _saleCategory;
		private TypeOfDepositCategory? _typeOfDepositCategory;
		private decimal _sumOfDamage;
		private string _shortName;
		private bool _hide;

		private bool _noDelivery;
		private double _percentForMaster;
		private Guid? _onlineStoreGuid;
		private string _description;
		private string _bottleCapColor;
		private string _onlineStoreExternalId;
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
		private bool _isNeedSanitisation;

		private MeasurementUnits _unit;
		private NomenclatureEntity _dependsOnNomenclature;
		private IObservableList<NomenclatureFileInformation> _attachedFileInformations = new ObservableList<NomenclatureFileInformation>();
		private IObservableList<NomenclaturePriceEntity> _nomenclaturePrice = new ObservableList<NomenclaturePriceEntity>();
		private IObservableList<AlternativeNomenclaturePriceEntity> _alternativeNomenclaturePrices = new ObservableList<AlternativeNomenclaturePriceEntity>();
		private IObservableList<GtinEntity> _gtins = new ObservableList<GtinEntity>();
		private IObservableList<GroupGtinEntity> _groupGtins = new ObservableList<GroupGtinEntity>();
		private IObservableList<NomenclaturePurchasePrice> _purchasePrices = new ObservableList<NomenclaturePurchasePrice>();
		private IObservableList<VatRateVersion> _vatRateVersions = new ObservableList<VatRateVersion>();

		public NomenclatureEntity() 
		{
			Category = NomenclatureCategory.water;
		}

		public virtual IUnitOfWork UoW { set; get; }

		/// <summary>
		/// Идентификатор
		/// Код номенклатуры
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set
			{
				if(SetField(ref _id, value))
				{
					UpdateFileInformations();
				}
			}
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Категория
		/// </summary>
		[Display(Name = "Категория")]
		public virtual NomenclatureCategory Category
		{
			get => _category;
			//Нельзя устанавливать, см. логику в Nomenclature.cs
			protected set => SetField(ref _category, value);
		}

		/// <summary>
		/// Подлежит учету в Честном Знаке
		/// </summary>
		[Display(Name = "Подлежит учету в Честном Знаке")]
		public virtual bool IsAccountableInTrueMark
		{
			get => _isAccountableInTrueMark;
			set => SetField(ref _isAccountableInTrueMark, value);
		}

		/// <summary>
		/// Номер товарной продукции GTIN
		/// </summary>
		[Display(Name = "Номер товарной продукции GTIN")]
		public virtual string Gtin
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}

		/// <summary>
		/// Инфоррмация о прикрепленных файлах
		/// </summary>
		[Display(Name = "Информация о прикрепленных файлах")]
		public virtual IObservableList<NomenclatureFileInformation> AttachedFileInformations
		{
			get => _attachedFileInformations;
			set => SetField(ref _attachedFileInformations, value);
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
		/// Версии ставок НДС
		/// </summary>
		[Display(Name = "Версии ставок НДС")]
		public virtual IObservableList<VatRateVersion> VatRateVersions
		{
			get => _vatRateVersions;
			set => SetField(ref _vatRateVersions, value);
		}

		/// <summary>
		/// Цены
		/// </summary>
		[Display(Name = "Цены")]
		public virtual IObservableList<NomenclaturePriceEntity> NomenclaturePrice
		{
			get => _nomenclaturePrice;
			set => SetField(ref _nomenclaturePrice, value);
		}

		/// <summary>
		/// Альтернативные цены
		/// </summary>
		[Display(Name = "Альтернативные цены")]
		public virtual IObservableList<AlternativeNomenclaturePriceEntity> AlternativeNomenclaturePrices
		{
			get => _alternativeNomenclaturePrices;
			set => SetField(ref _alternativeNomenclaturePrices, value);
		}

		/// <summary>
		/// Влияющая номенклатура
		/// </summary>
		[Display(Name = "Влияющая номенклатура")]
		public virtual NomenclatureEntity DependsOnNomenclature
		{
			get => _dependsOnNomenclature;
			set => SetField(ref _dependsOnNomenclature, value);
		}

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
		/// Сумма ущерба
		/// </summary>
		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage
		{
			get => _sumOfDamage;
			set => SetField(ref _sumOfDamage, value);
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
		/// Участвует в групповом заполнении себестоимости
		/// </summary>
		[Display(Name = "Участвует в групповом заполнении себестоимости")]
		public virtual bool UsingInGroupPriceSet
		{
			get => _usingInGroupPriceSet;
			set => SetField(ref _usingInGroupPriceSet, value);
		}

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

		/// <summary>
		/// Gtin
		/// </summary>
		[Display(Name = "Gtin")]
		public virtual IObservableList<GtinEntity> Gtins
		{
			get => _gtins;
			set => SetField(ref _gtins, value);
		}

		/// <summary>
		/// Gtin группы
		/// </summary>
		[Display(Name = "Gtin группы")]
		public virtual IObservableList<GroupGtinEntity> GroupGtins
		{
			get => _groupGtins;
			set => SetField(ref _groupGtins, value);
		}

		/// <summary>
		/// Цены закупки ТМЦ
		/// </summary>
		[Display(Name = "Цены закупки ТМЦ")]
		public virtual IObservableList<NomenclaturePurchasePrice> PurchasePrices
		{
			get => _purchasePrices;
			set => SetField(ref _purchasePrices, value);
		}
		

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
		
		/// <summary>
		/// Необходима ли сан обработка для номенклатуры
		/// </summary>
		[Display(Name = "Санитарная обработка")]
		public virtual bool IsNeedSanitisation
		{
			get => _isNeedSanitisation;
			set => SetField(ref _isNeedSanitisation, value);
		}

		#endregion Онлайн характеристики для ИПЗ
		
		/// <summary>
		/// Добавление информации о файле
		/// </summary>
		/// <param name="filename">Имя файла</param>
		public virtual void AddFileInformation(string fileName)
		{
			if(AttachedFileInformations.Any(a => a.FileName == fileName))
			{
				return;
			}

			AttachedFileInformations.Add(new NomenclatureFileInformation
			{
				NomenclatureId = Id,
				FileName = fileName
			});
		}

		/// <summary>
		/// Удаление информации о файле
		/// </summary>
		/// <param name="filename">Имя файла</param>
		public virtual void RemoveFileInformation(string filename)
		{
			if(!AttachedFileInformations.Any(fi => fi.FileName == filename))
			{
				return;
			}

			AttachedFileInformations.Remove(AttachedFileInformations.First(x => x.FileName == filename));
		}

		/// <summary>
		/// Обновление информации о файлах
		/// </summary>
		private void UpdateFileInformations()
		{
			foreach(var fileInformation in AttachedFileInformations)
			{
				fileInformation.NomenclatureId = Id;
			}
		}

		/// <summary>
		/// Получение цены номенклатуры
		/// </summary>
		/// <param name="itemsCount">Количество единиц товара</param>
		/// <param name="useAlternativePrice">Использовать ли альтернативную цену</param>
		/// <returns></returns>
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
						? AlternativeNomenclaturePrices.Cast<NomenclaturePriceGeneralBase>()
						: NomenclaturePrice.Cast<NomenclaturePriceGeneralBase>())
					.OrderByDescending(p => p.MinCount)
					.FirstOrDefault(p => p.MinCount <= itemsCount);
				price = nomPrice?.Price ?? 0;
			}
			return price;
		}

		/// <summary>
		/// Получение цены закупки на дату
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public virtual decimal GetPurchasePriceOnDate(DateTime date)
		{
			var purchasePrice =
				PurchasePrices
				.Where(p => p.StartDate <= date && (p.EndDate == null || p.EndDate >= date))
				.Select(p => p.PurchasePrice)
				.FirstOrDefault();

			return purchasePrice;
		}
		
		public override string ToString() => $"id = {Id} Name = {Name}";
	}
}
