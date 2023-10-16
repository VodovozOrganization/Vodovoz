using Gamma.Utilities;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Osrm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "точки доставки",
		Nominative = "точка доставки",
		Accusative = "точки доставки")]
	[HistoryTrace]
	[EntityPermission]
	public class DeliveryPoint : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly IGlobalSettings _globalSettings = new GlobalSettings(new ParametersProvider());

		private TimeSpan? _lunchTimeFrom;
		private TimeSpan? _lunchTimeTo;
		private bool? _isBeforeIntervalDelivery;
		private Guid? _cityFiasGuid;
		private Guid? _streetFiasGuid;
		private string _streetTypeShort;
		private string _streetDistrict;
		private string _localityTypeShort;
		private string _localityType;
		private Guid? _buildingFiasGuid;
		private ReasonForLeaving _reasonForLeaving;
		private int _minutesToUnload;
		private string _letter;
		private string _placement;
		private string _floor;
		private EntranceType _entranceType;
		private string _entrance;
		private string _city;
		private string _cityDistrict;
		private string _street;
		private string _streetType;
		private string _building;
		private RoomType _roomType;
		private string _room;
		private string _comment;
		private decimal? _latitude;
		private decimal? _longitude;
		private bool _isActive = true;
		private IList<DeliveryPointResponsiblePerson> _responsiblePersons = new List<DeliveryPointResponsiblePerson>();
		private GenericObservableList<DeliveryPointResponsiblePerson> _observableResponsiblePersons;
		private District _district;
		private DeliverySchedule _deliverySchedule;
		private bool _foundOnOsm;
		private bool _manualCoordinates;
		private bool _isFixedInOsm;
		private Counterparty _counterparty;
		private string _kpp;
		private string _address1c;
		private string _code1c;
		private string _organization;
		private int _bottleReserv;
		private Nomenclature _defaultWaterNomenclature;
		private bool _alwaysFreeDelivery;
		private User _coordsLastChangeUser;
		private int? _distanceFromBaseMeters;
		private IList<Phone> _phones = new List<Phone>();
		private GenericObservableList<Phone> _observablePhones;
		private bool? _haveResidue;
		private IList<NomenclatureFixedPrice> _nomenclatureFixedPrices = new List<NomenclatureFixedPrice>();
		private GenericObservableList<NomenclatureFixedPrice> _observableNomenclatureFixedPrices;
		private int _minimalOrderSumLimit;
		private int _maximalOrderSumLimit;
		private IList<DeliveryPointEstimatedCoordinate> _deliveryPointEstimatedCoordinates = new List<DeliveryPointEstimatedCoordinate>();
		private GenericObservableList<DeliveryPointEstimatedCoordinate> _observableDeliveryPointEstimatedCoordinates;
		private LogisticsRequirements _logisticsRequirements;
		private decimal _fixPrice1;
		private decimal _fixPrice2;
		private decimal _fixPrice3;
		private decimal _fixPrice4;
		private decimal _fixPrice5;
		private bool _addCertificatesAlways;
		private DeliveryPointCategory _category;
		private string _onlineComment;
		private string _intercom;

		//FIXME вынести зависимость
		private readonly IDeliveryRepository _deliveryRepository = new DeliveryRepository();

		public DeliveryPoint()
		{
			CompiledAddress = string.Empty;
			City = "Санкт-Петербург";
			LocalityTypeShort = "г";
			Street = string.Empty;
			Building = string.Empty;
			Room = string.Empty;
			Comment = string.Empty;
		}

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Время разгрузки")]
		public virtual int MinutesToUnload
		{
			get => _minutesToUnload;
			set => SetField(ref _minutesToUnload, value);
		}

		[Display(Name = "Литера")]
		public virtual string Letter
		{
			get => _letter;
			set => SetField(ref _letter, value);
		}

		[Display(Name = "Помещение")]
		public virtual string Placement
		{
			get => _placement;
			set => SetField(ref _placement, value);
		}

		[Display(Name = "Этаж")]
		public virtual string Floor
		{
			get => _floor;
			set => SetField(ref _floor, value);
		}

		[Display(Name = "Тип входа")]
		public virtual EntranceType EntranceType
		{
			get => _entranceType;
			set => SetField(ref _entranceType, value);
		}

		[Display(Name = "Парадная")]
		public virtual string Entrance
		{
			get => _entrance;
			set => SetField(ref _entrance, value);
		}

		[Display(Name = "Полный адрес")]
		public virtual string CompiledAddress
		{
			get
			{
				string address = string.Empty;
				if(!string.IsNullOrWhiteSpace(LocalityTypeShort))
				{
					address += $"{LocalityTypeShort}. ";
				}

				if(!string.IsNullOrWhiteSpace(City))
				{
					address += $"{City}, ";
				}

				if(!string.IsNullOrWhiteSpace(StreetType))
				{
					address += $"{StreetType.ToLower()} ";
				}

				if(!string.IsNullOrWhiteSpace(Street))
				{
					address += $"{Street}, ";
				}

				if(!string.IsNullOrWhiteSpace(Building))
				{
					address += $"д.{Building}, ";
				}

				if(!string.IsNullOrWhiteSpace(Letter))
				{
					address += $"лит.{Letter}, ";
				}

				if(!string.IsNullOrWhiteSpace(Entrance))
				{
					address += $"{_entranceType.GetEnumShortTitle()} {Entrance}, ";
				}

				if(!string.IsNullOrWhiteSpace(Floor))
				{
					address += $"эт.{Floor}, ";
				}

				if(!string.IsNullOrWhiteSpace(Room))
				{
					address += $"{RoomType.GetEnumShortTitle()} {Room}, ";
				}

				if(!string.IsNullOrWhiteSpace(Comment))
				{
					address += $"{Comment}, ";
				}

				return address.TrimEnd(',', ' ');
			}
			set { }
		}

		[Display(Name = "Адрес без дополнения")]
		public virtual string CompiledAddressWOAddition
		{
			get
			{
				string address = string.Empty;
				if(!string.IsNullOrWhiteSpace(LocalityTypeShort))
				{
					address += $"{LocalityTypeShort}. ";
				}

				if(!string.IsNullOrWhiteSpace(City))
				{
					address += $"{City}, ";
				}

				if(!string.IsNullOrWhiteSpace(StreetTypeShort))
				{
					address += GetStreetTypeShort();
				}

				if(!string.IsNullOrWhiteSpace(Street))
				{
					address += $"{Street}, ";
				}

				if(!string.IsNullOrWhiteSpace(Building))
				{
					address += $"д.{Building}, ";
				}

				if(!string.IsNullOrWhiteSpace(Letter))
				{
					address += $"лит.{Letter}, ";
				}

				if(!string.IsNullOrWhiteSpace(Entrance))
				{
					address += $"{_entranceType.GetEnumShortTitle()} {Entrance}, ";
				}

				if(!string.IsNullOrWhiteSpace(Floor))
				{
					address += $"эт.{Floor}, ";
				}

				if(!string.IsNullOrWhiteSpace(Room))
				{
					address += $"{RoomType.GetEnumShortTitle()} {Room}, ";
				}

				return address.TrimEnd(',', ' ');
			}
		}

		[Display(Name = "Сокращенный адрес")]
		public virtual string ShortAddress
		{
			get
			{
				string address = string.Empty;
				if(!string.IsNullOrWhiteSpace(LocalityTypeShort) && City != "Санкт-Петербург")
				{
					address += $"{LocalityTypeShort}. ";
				}

				if(!string.IsNullOrWhiteSpace(City) && City != "Санкт-Петербург")
				{
					address += $"{City}, ";
				}

				if(!string.IsNullOrWhiteSpace(StreetTypeShort))
				{
					address += GetStreetTypeShort();
				}

				if(!string.IsNullOrWhiteSpace(Street))
				{
					address += $"{Street}, ";
				}

				if(!string.IsNullOrWhiteSpace(Building))
				{
					address += $"д.{Building}, ";
				}

				if(!string.IsNullOrWhiteSpace(Letter))
				{
					address += $"лит.{Letter}, ";
				}

				if(!string.IsNullOrWhiteSpace(Entrance))
				{
					address += $"{_entranceType.GetEnumShortTitle()} {Entrance}, ";
				}

				if(!string.IsNullOrWhiteSpace(Floor))
				{
					address += $"эт.{Floor}, ";
				}

				if(!string.IsNullOrWhiteSpace(Room))
				{
					address += $"{RoomType.GetEnumShortTitle()} {Room}, ";
				}

				return address.TrimEnd(',', ' ');
			}
			set { }
		}

		public virtual Guid? CityFiasGuid
		{
			get => _cityFiasGuid;
			set => SetField(ref _cityFiasGuid, value);
		}

		public virtual Guid? StreetFiasGuid
		{
			get => _streetFiasGuid;
			set => SetField(ref _streetFiasGuid, value);
		}

		public virtual Guid? BuildingFiasGuid
		{
			get => _buildingFiasGuid;
			set => SetField(ref _buildingFiasGuid, value);
		}

		[Display(Name = "Город")]
		public virtual string City
		{
			get => _city;
			set => SetField(ref _city, value);
		}

		[Display(Name = "Тип населенного пункта")]
		public virtual string LocalityType
		{
			get => _localityType;
			set => SetField(ref _localityType, value);
		}

		[Display(Name = "Тип населенного пункта (сокращ.)")]
		public virtual string LocalityTypeShort
		{
			get => _localityTypeShort;
			set => SetField(ref _localityTypeShort, value);
		}

		[Display(Name = "Район области")]
		public virtual string CityDistrict
		{
			get => _cityDistrict;
			set => SetField(ref _cityDistrict, value);
		}

		[Display(Name = "Улица")]
		public virtual string Street
		{
			get => _street;
			set => SetField(ref _street, value);
		}

		[Display(Name = "Тип улицы")]
		public virtual string StreetType
		{
			get => _streetType;
			set => SetField(ref _streetType, value);
		}

		[Display(Name = "Тип улицы (сокр.)")]
		public virtual string StreetTypeShort
		{
			get => _streetTypeShort;
			set => SetField(ref _streetTypeShort, value);
		}

		[Display(Name = "Район города")]
		public virtual string StreetDistrict
		{
			get => _streetDistrict;
			set => SetField(ref _streetDistrict, value);
		}

		[Display(Name = "Номер дома")]
		public virtual string Building
		{
			get => _building;
			set => SetField(ref _building, value);
		}

		[Display(Name = "Тип помещения")]
		public virtual RoomType RoomType
		{
			get => _roomType;
			set => SetField(ref _roomType, value);
		}

		[Display(Name = "Офис/Квартира")]
		public virtual string Room
		{
			get => _room;
			set => SetField(ref _room, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Широта. Для установки координат используйте метод SetСoordinates
		/// </summary>
		[Display(Name = "Широта")]
		[PropertyChangedAlso(nameof(CoordinatesText))]
		public virtual decimal? Latitude
		{
			get => _latitude;
			protected set => SetField(ref _latitude, value);
		}

		/// <summary>
		/// Долгота. Для установки координат используйте метод SetСoordinates
		/// </summary>
		[Display(Name = "Долгота")]
		[PropertyChangedAlso(nameof(CoordinatesText))]
		public virtual decimal? Longitude
		{
			get => _longitude;
			protected set => SetField(ref _longitude, value);
		}

		[Display(Name = "Активный")]
		public virtual bool IsActive
		{
			get => _isActive;
			set => SetField(ref _isActive, value);
		}

		[Display(Name = "Ответственные лица")]
		public virtual IList<DeliveryPointResponsiblePerson> ResponsiblePersons
		{
			get => _responsiblePersons;
			set => SetField(ref _responsiblePersons, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPointResponsiblePerson> ObservableResponsiblePersons
		{
			get
			{
				if(_observableResponsiblePersons == null)
				{
					_observableResponsiblePersons = new GenericObservableList<DeliveryPointResponsiblePerson>(ResponsiblePersons);
				}

				return _observableResponsiblePersons;
			}
		}

		[Display(Name = "Район доставки")]
		public virtual District District
		{
			get => _district;
			set => SetField(ref _district, value);
		}

		[Display(Name = "График доставки")]
		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule, value);
		}

		[Display(Name = "Адрес найден на карте OSM")]
		public virtual bool FoundOnOsm
		{
			get => _foundOnOsm;
			set => SetField(ref _foundOnOsm, value);
		}

		[Display(Name = "Ручные координаты")]
		public virtual bool ManualCoordinates
		{
			get => _manualCoordinates;
			set => SetField(ref _manualCoordinates, value);
		}

		[Display(Name = "Исправлен в OSM")]
		public virtual bool IsFixedInOsm
		{
			get => _isFixedInOsm;
			set => SetField(ref _isFixedInOsm, value);
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		[Display(Name = "КПП")]
		public virtual string KPP
		{
			get => _kpp;
			set => SetField(ref _kpp, value);
		}

		[Display(Name = "Адрес 1С")]
		public virtual string Address1c
		{
			get => _address1c;
			set => SetField(ref _address1c, value);
		}

		/// Код уникален только внутри контрагента
		[Display(Name = "Код в 1С")]
		public virtual string Code1c
		{
			get => _code1c;
			set => SetField(ref _code1c, value);
		}

		[Display(Name = "Организация")]
		public virtual string Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		[Display(Name = "Резерв бутылей")]
		public virtual int BottleReserv
		{
			get => _bottleReserv;
			set => SetField(ref _bottleReserv, value);
		}

		[Display(Name = "Вода по умолчанию")]
		public virtual Nomenclature DefaultWaterNomenclature
		{
			get => _defaultWaterNomenclature;
			set => SetField(ref _defaultWaterNomenclature, value);
		}

		[Display(Name = "Всегда бесплатная доставка")]
		public virtual bool AlwaysFreeDelivery
		{
			get => _alwaysFreeDelivery;
			set => SetField(ref _alwaysFreeDelivery, value);
		}

		[Display(Name = "Последнее изменение пользователем")]
		public virtual User СoordsLastChangeUser
		{
			get => _coordsLastChangeUser;
			set => SetField(ref _coordsLastChangeUser, value);
		}

		[Display(Name = "Расстояние от базы в метрах")]
		public virtual int? DistanceFromBaseMeters
		{
			get => _distanceFromBaseMeters;
			set => SetField(ref _distanceFromBaseMeters, value);
		}

		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones
		{
			get => _phones;
			set => SetField(ref _phones, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Phone> ObservablePhones
		{
			get
			{
				if(_observablePhones == null)
				{
					_observablePhones = new GenericObservableList<Phone>(Phones);
				}

				return _observablePhones;
			}
		}

		[Display(Name = "Посчитан ввод остатков")]
		public virtual bool? HaveResidue
		{
			get => _haveResidue;
			set => SetField(ref _haveResidue, value);
		}

		[Display(Name = "Фиксированные цены")]
		public virtual IList<NomenclatureFixedPrice> NomenclatureFixedPrices
		{
			get => _nomenclatureFixedPrices;
			set => SetField(ref _nomenclatureFixedPrices, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclatureFixedPrice> ObservableNomenclatureFixedPrices
		{
			get
			{
				if(_observableNomenclatureFixedPrices == null)
				{
					_observableNomenclatureFixedPrices = new GenericObservableList<NomenclatureFixedPrice>(NomenclatureFixedPrices);
				}

				return _observableNomenclatureFixedPrices;
			}
		}

		/// <summary>
		/// Минимальный порог суммы заказа
		/// </summary>
		public virtual int MinimalOrderSumLimit
		{
			get => _minimalOrderSumLimit;
			set => SetField(ref _minimalOrderSumLimit, value);
		}

		/// <summary>
		/// Максимальный порог суммы заказа
		/// </summary>
		public virtual int MaximalOrderSumLimit
		{
			get => _maximalOrderSumLimit;
			set => SetField(ref _maximalOrderSumLimit, value);
		}

		[Display(Name = "Предполагаемые координаты доставки")]
		public virtual IList<DeliveryPointEstimatedCoordinate> DeliveryPointEstimatedCoordinates
		{
			get => _deliveryPointEstimatedCoordinates;
			set => SetField(ref _deliveryPointEstimatedCoordinates, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPointEstimatedCoordinate> ObservableDeliveryPointEstimatedCoordinates => _observableDeliveryPointEstimatedCoordinates
					?? (_observableDeliveryPointEstimatedCoordinates = new GenericObservableList<DeliveryPointEstimatedCoordinate>(DeliveryPointEstimatedCoordinates));

		[Display(Name = "Требования к логистике")]
		public virtual LogisticsRequirements LogisticsRequirements
		{
			get => _logisticsRequirements;
			set => SetField(ref _logisticsRequirements, value);
		}

		#region Временные поля для хранения фиксированных цен из 1с

		/// <summary>
		/// Фикса Семиозерье из 1с
		/// </summary>
		[Display(Name = "Фикса Семиозерье из 1с")]
		public virtual decimal FixPrice1
		{
			get => _fixPrice1;
			set => SetField(ref _fixPrice1, value);
		}

		/// <summary>
		/// Фикса Кислородная из 1с
		/// </summary>
		[Display(Name = "Фикса Кислородная из 1с")]
		public virtual decimal FixPrice2
		{
			get => _fixPrice2;
			set => SetField(ref _fixPrice2, value);
		}

		/// <summary>
		/// Фикса Снятогорская из 1с
		/// </summary>
		[Display(Name = "Фикса Снятогорская из 1с")]
		public virtual decimal FixPrice3
		{
			get => _fixPrice3;
			set => SetField(ref _fixPrice3, value);
		}

		/// <summary>
		/// Фикса Стройка из 1с
		/// </summary>
		[Display(Name = "Фикса Стройка из 1с")]
		public virtual decimal FixPrice4
		{
			get => _fixPrice4;
			set => SetField(ref _fixPrice4, value);
		}

		/// <summary>
		/// Фикса С Ручками из 1с
		/// </summary>
		[Display(Name = "Фикса С Ручками из 1с")]
		public virtual decimal FixPrice5
		{
			get => _fixPrice5;
			set => SetField(ref _fixPrice5, value);
		}

		[Display(Name = "Всегда добавлять сертификаты")]
		public virtual bool AddCertificatesAlways
		{
			get => _addCertificatesAlways;
			set => SetField(ref _addCertificatesAlways, value);
		}

		[Display(Name = "Тип объекта")]
		public virtual DeliveryPointCategory Category
		{
			get => _category;
			set
			{
				if(value != null && value.IsArchive)
				{
					value = null;
				}

				SetField(ref _category, value);
			}
		}

		[Display(Name = "Время начала обеда")]
		public virtual TimeSpan? LunchTimeFrom
		{
			get => _lunchTimeFrom;
			set => SetField(ref _lunchTimeFrom, value);
		}

		[Display(Name = "Время окончания обеда")]
		public virtual TimeSpan? LunchTimeTo
		{
			get => _lunchTimeTo;
			set => SetField(ref _lunchTimeTo, value);
		}

		#endregion Временные поля для хранения фиксированных цен из 1с

		#region Свойства для интеграции

		[Display(Name = "Комментарий к ТД из ИПЗ")]
		public virtual string OnlineComment
		{
			get => _onlineComment;
			set => SetField(ref _onlineComment, value);
		}
		
		[Display(Name = "Домофон")]
		public virtual string Intercom
		{
			get => _intercom;
			set => SetField(ref _intercom, value);
		}

		#endregion

		#endregion Свойства

		#region Расчетные

		public virtual string Title => string.IsNullOrWhiteSpace(CompiledAddress) ? "АДРЕС ПУСТОЙ" : CompiledAddress;

		public virtual string CoordinatesText => Latitude == null || Longitude == null ? string.Empty : $"(ш. {Latitude:F5}, д. {Longitude:F5})";

		public virtual bool CoordinatesExist => Latitude.HasValue && Longitude.HasValue;

		public virtual Point NetTopologyPoint => CoordinatesExist ? new Point((double)Latitude, (double)Longitude) : null;

		public virtual PointOnEarth PointOnEarth => new PointOnEarth(Latitude.Value, Longitude.Value);

		public virtual GMap.NET.PointLatLng GmapPoint => new GMap.NET.PointLatLng((double)Latitude, (double)Longitude);

		public virtual long СoordinatesHash => CachedDistance.GetHash(this);

		public virtual bool HasFixedPrices => NomenclatureFixedPrices.Any();

		#endregion Расчетные

		/// <summary>
		/// Возврат районов доставки, в которые попадает точка доставки
		/// </summary>
		/// <param name="uow">UnitOfWork через который будет получены все районы доставки,
		/// среди которых будет производится поиск подходящего района</param>
		public virtual IEnumerable<District> CalculateDistricts(IUnitOfWork uow)
		{
			return !CoordinatesExist ? new List<District>() : _deliveryRepository.GetDistricts(uow, Latitude.Value, Longitude.Value);
		}

		/// <summary>
		/// Поиск района города, в котором находится текущая точка доставки
		/// </summary>
		/// <returns><c>true</c>, если район города найден</returns>
		/// <param name="uow">UnitOfWork через который будет производится поиск подходящего района города</param>
		/// <param name="districtsSet">Версия районов, из которой будет ассоциироваться район. Если равно null, то будет браться активная версия</param>
		public virtual bool FindAndAssociateDistrict(IUnitOfWork uow, DistrictsSet districtsSet = null)
		{
			if(!CoordinatesExist)
			{
				return false;
			}

			District foundDistrict = _deliveryRepository.GetDistrict(uow, Latitude.Value, Longitude.Value, districtsSet);

			if(foundDistrict == null)
			{
				return false;
			}

			District = foundDistrict;

			return true;
		}

		/// <summary>
		/// Устанавливает правильно координты точки.
		/// </summary>
		/// <returns><c>true</c>, если координаты установлены</returns>
		/// <param name="latitude">Широта</param>
		/// <param name="longitude">Долгота</param>
		/// <param name="uow">UnitOfWork через который будет производится поиск подходящего района города
		/// для определения расстояния до базы</param>
		public virtual bool SetСoordinates(decimal? latitude, decimal? longitude, IUnitOfWork uow = null)
		{
			Latitude = latitude;
			Longitude = longitude;

			OnPropertyChanged(nameof(CoordinatesExist));

			if(Longitude == null || Latitude == null || !FindAndAssociateDistrict(uow))
			{
				return true;
			}

			GeoGroupVersion geoGroupVersion = District.GeographicGroup.GetActualVersionOrNull();

			if(geoGroupVersion == null)
			{
				throw new InvalidOperationException($"Не установлена активная версия данных в части города {District.GeographicGroup.Name}");
			}

			List<PointOnEarth> route = new List<PointOnEarth>(2) {
				new PointOnEarth(geoGroupVersion.BaseLatitude.Value, geoGroupVersion.BaseLongitude.Value),
				new PointOnEarth(Latitude.Value, Longitude.Value)
			};

			RouteResponse result = OsrmClientFactory.Instance.GetRoute(route, false, GeometryOverview.False, _globalSettings.ExcludeToll);

			if(result == null)
			{
				_logger.Error("Сервер расчета расстояний не вернул ответа.");
				return false;
			}

			if(result.Code != "Ok")
			{
				_logger.Error("Сервер расчета расстояний вернул следующее сообщение:\n" + result.StatusMessageRus);
				return false;
			}

			DistanceFromBaseMeters = result.Routes[0].TotalDistance;

			return true;
		}

		private string GetStreetTypeShort()
		{
			return string.Equals(_streetType, StreetTypeShort, StringComparison.CurrentCultureIgnoreCase)
				? $"{StreetTypeShort} "
				: $"{StreetTypeShort}. ";
		}

		#region Фабричные методы

		public static IUnitOfWorkGeneric<DeliveryPoint> CreateUowForNew(Counterparty counterparty)
		{
			IUnitOfWorkGeneric<DeliveryPoint> uow = UnitOfWorkFactory.CreateWithNewRoot<DeliveryPoint>();

			uow.Root.Counterparty = counterparty;

			return uow;
		}

		public static DeliveryPoint Create(Counterparty counterparty)
		{
			DeliveryPoint point = new DeliveryPoint
			{
				Counterparty = counterparty
			};

			counterparty.DeliveryPoints.Add(point);

			return point;
		}

		#endregion Фабричные методы

		#region IValidatableObject Implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Category == null)
			{
				yield return new ValidationResult(
					"Необходимо выбрать тип точки доставки",
					new[] { this.GetPropertyName(o => o.Category) });
			}

			if(Counterparty == null)
			{
				yield return new ValidationResult(
					"Необходимо выбрать клиента",
					new[] { this.GetPropertyName(o => o.Counterparty) });
			}

			if(Building?.Length == 0)
			{
				yield return new ValidationResult(
					"Заполните поле \"Дом\"",
					new[] { this.GetPropertyName(o => o.Building) });
			}

			if(Building?.Length > 20)
			{
				yield return new ValidationResult(
					"Длина строки \"Дом\" не должна превышать 20 символов",
					new[] { this.GetPropertyName(o => o.Building) });
			}

			if(City?.Length == 0)
			{
				yield return new ValidationResult(
					"Заполните поле \"Город\"",
					new[] { this.GetPropertyName(o => o.City) });
			}

			if(City?.Length > 45)
			{
				yield return new ValidationResult(
					"Длина строки \"Город\" не должна превышать 45 символов",
					new[] { this.GetPropertyName(o => o.City) });
			}

			if(Street?.Length == 0)
			{
				yield return new ValidationResult(
					"Заполните поле \"Улица\"",
					new[] { this.GetPropertyName(o => o.Street) });
			}

			if(Street?.Length > 50)
			{
				yield return new ValidationResult(
					"Длина строки \"Улица\" не должна превышать 50 символов",
					new[] { this.GetPropertyName(o => o.Street) });
			}

			if(Room?.Length > 20)
			{
				yield return new ValidationResult(
					"Длина строки \"Офис/Квартира\" не должна превышать 20 символов",
					new[] { this.GetPropertyName(o => o.Room) });
			}

			if(Entrance?.Length > 50)
			{
				yield return new ValidationResult(
					"Длина строки \"Парадная\" не должна превышать 50 символов",
					new[] { this.GetPropertyName(o => o.Entrance) });
			}

			if(Floor?.Length > 20)
			{
				yield return new ValidationResult(
					"Длина строки \"Этаж\" не должна превышать 20 символов",
					new[] { this.GetPropertyName(o => o.Floor) });
			}

			if(Code1c?.Length > 10)
			{
				yield return new ValidationResult(
					"Длина строки \"Код 1С\" не должна превышать 10 символов",
					new[] { this.GetPropertyName(o => o.Code1c) });
			}

			if(KPP?.Length > 45)
			{
				yield return new ValidationResult(
					"Длина строки \"КПП\" не должна превышать 45 символов",
					new[] { this.GetPropertyName(o => o.KPP) });
			}

			RoomType[] notNeedOrganizationRoomTypes = new RoomType[] { RoomType.Apartment, RoomType.Chamber };

			if(Counterparty.PersonType == PersonType.natural && !notNeedOrganizationRoomTypes.Contains(RoomType))
			{
				if(string.IsNullOrWhiteSpace(Organization))
				{
					yield return new ValidationResult(
						"Необходимо заполнить поле \"Организация\"",
						new[] { this.GetPropertyName(o => o.Organization) });
				}

				if(Organization?.Length > 45)
				{
					yield return new ValidationResult(
						"Длина строки \"Организация\" не должна превышать 45 символов",
						new[] { this.GetPropertyName(o => o.Organization) });
				}
			}

			var everyAddedMinCountValueCount = NomenclatureFixedPrices
				.GroupBy(p => new { p.Nomenclature, p.MinCount })
				.Select(p => new { NomenclatureName = p.Key.Nomenclature?.Name, MinCountValue = p.Key.MinCount, Count = p.Count() });

			foreach(var p in everyAddedMinCountValueCount)
			{
				if(p.Count > 1)
				{
					yield return new ValidationResult(
							$"\"{p.NomenclatureName}\": фиксированная цена для количества \"{p.MinCountValue}\" указана {p.Count} раз(а)",
							new[] { this.GetPropertyName(o => o.NomenclatureFixedPrices) });
				}
			}

			foreach(NomenclatureFixedPrice fixedPrice in NomenclatureFixedPrices)
			{
				IEnumerable<ValidationResult> fixedPriceValidationResults = fixedPrice.Validate(validationContext);

				foreach(ValidationResult fixedPriceValidationResult in fixedPriceValidationResults)
				{
					yield return fixedPriceValidationResult;
				}
			}

			if(LunchTimeFrom == null && LunchTimeTo != null)
			{
				yield return new ValidationResult("При заполненной дате окончания обеда должна быть указана и дата начала обеда.",
					new[] { nameof(LunchTimeTo) });
			}

			if(LunchTimeTo == null && LunchTimeFrom != null)
			{
				yield return new ValidationResult("При заполненной дате начала обеда должна быть указана и дата окончания обеда.",
					new[] { nameof(LunchTimeTo) });
			}

			StringBuilder phonesValidationStringBuilder = new StringBuilder();

			foreach(Phone phone in Phones)
			{
				if(phone.RoboAtsCounterpartyName == null)
				{
					phonesValidationStringBuilder.AppendLine($"Для телефона {phone.Number} не указано имя контрагента.");
				}

				if(phone.RoboAtsCounterpartyPatronymic == null)
				{
					phonesValidationStringBuilder.AppendLine($"Для телефона {phone.Number} не указано отчество контрагента.");
				}

				if(!phone.IsValidPhoneNumber)
				{
					phonesValidationStringBuilder.AppendLine($"Номер {phone.Number} имеет неправильный формат.");
				}
			}

			string phonesValidationMessage = phonesValidationStringBuilder.ToString();

			if(!string.IsNullOrEmpty(phonesValidationMessage))
			{
				yield return new ValidationResult(phonesValidationMessage);
			}

			if(ResponsiblePersons.Any(x => x.DeliveryPointResponsiblePersonType == null || x.Employee == null || string.IsNullOrWhiteSpace(x.Phone)))
			{
				yield return new ValidationResult("Для ответственных лиц должны быть заполнены Тип, Сотрудник и Телефон", new[] { nameof(ResponsiblePersons) });
			}
		}

		#endregion IValidatableObject Implementation
	}
}
