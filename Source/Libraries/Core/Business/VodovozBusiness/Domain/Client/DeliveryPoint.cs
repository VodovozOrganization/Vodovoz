using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using NetTopologySuite.Geometries;
using NHibernate.Impl;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Osrm;
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
		Accusative = "точки доставки"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class DeliveryPoint : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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

		#region Свойства

		public virtual int Id { get; set; }

		int minutesToUnload;

		[Display(Name = "Время разгрузки")]
		public virtual int MinutesToUnload {
			get => minutesToUnload;
			set => SetField(ref minutesToUnload, value, () => MinutesToUnload);
		}

		string letter;

		[Display(Name = "Литера")]
		public virtual string Letter {
			get => letter;
			set => SetField(ref letter, value, () => Letter);
		}
		
		string placement;

		[Display(Name = "Помещение")]
		public virtual string Placement {
			get => placement;
			set => SetField(ref placement, value, () => Placement);
		}

		string floor;

		[Display(Name = "Этаж")]
		public virtual string Floor {
			get => floor;
			set => SetField(ref floor, value, () => Floor);
		}

		EntranceType entranceType;

		[Display(Name = "Тип входа")]
		public virtual EntranceType EntranceType {
			get => entranceType;
			set => SetField(ref entranceType, value, () => EntranceType);
		}

		string entrance;

		[Display(Name = "Парадная")]
		public virtual string Entrance {
			get => entrance;
			set => SetField(ref entrance, value, () => Entrance);
		}

		public virtual string Title => string.IsNullOrWhiteSpace(CompiledAddress) ? "АДРЕС ПУСТОЙ" : CompiledAddress;

		[Display(Name = "Полный адрес")]
		public virtual string CompiledAddress {
			get {
				string address = string.Empty;
				if(!string.IsNullOrWhiteSpace(LocalityTypeShort))
					address += $"{LocalityTypeShort}. ";
				if(!string.IsNullOrWhiteSpace(City))
					address += $"{City}, ";
				if(!string.IsNullOrWhiteSpace(StreetType))
					address += $"{StreetType.ToLower()} ";
				if(!string.IsNullOrWhiteSpace(Street))
					address += $"{Street}, ";
				if(!string.IsNullOrWhiteSpace(Building))
					address += $"д.{Building}, ";
				if(!string.IsNullOrWhiteSpace(Letter))
					address += $"лит.{Letter}, ";
				if(!string.IsNullOrWhiteSpace(Entrance))
					address += $"{entranceType.GetEnumShortTitle()} {Entrance}, ";
				if(!string.IsNullOrWhiteSpace(Floor))
					address += $"эт.{Floor}, ";
				if(!string.IsNullOrWhiteSpace(Room))
					address += $"{RoomType.GetEnumShortTitle()} {Room}, ";
				if(!string.IsNullOrWhiteSpace(Comment))
					address += $"{Comment}, ";

				return address.TrimEnd(',', ' ');
			}
			set { }
		}

		[Display(Name = "Адрес без дополнения")]
		public virtual string CompiledAddressWOAddition {
			get {
				string address = string.Empty;
				if(!string.IsNullOrWhiteSpace(LocalityTypeShort))
					address += $"{LocalityTypeShort}. ";
				if(!string.IsNullOrWhiteSpace(City))
					address += $"{City}, ";
				if(!string.IsNullOrWhiteSpace(StreetTypeShort))
					address += GetStreetTypeShort();
				if(!string.IsNullOrWhiteSpace(Street))
					address += $"{Street}, ";
				if(!string.IsNullOrWhiteSpace(Building))
					address += $"д.{Building}, ";
				if(!string.IsNullOrWhiteSpace(Letter))
					address += $"лит.{Letter}, ";
				if(!string.IsNullOrWhiteSpace(Entrance))
					address += $"{entranceType.GetEnumShortTitle()} {Entrance}, ";
				if(!string.IsNullOrWhiteSpace(Floor))
					address += $"эт.{Floor}, ";
				if(!string.IsNullOrWhiteSpace(Room))
					address += $"{RoomType.GetEnumShortTitle()} {Room}, ";

				return address.TrimEnd(',', ' ');
			}
		}

		private string shortAddress;
		[Display(Name = "Сокращенный адрес")]
		public virtual string ShortAddress {
			get {
				string address = string.Empty;
				if(!string.IsNullOrWhiteSpace(LocalityTypeShort) && City != "Санкт-Петербург")
					address += $"{LocalityTypeShort}. ";
				if(!string.IsNullOrWhiteSpace(City) && City != "Санкт-Петербург")
					address += $"{City}, ";
				if(!string.IsNullOrWhiteSpace(StreetTypeShort))
					address += GetStreetTypeShort();
				if(!string.IsNullOrWhiteSpace(Street))
					address += $"{Street}, ";
				if(!string.IsNullOrWhiteSpace(Building))
					address += $"д.{Building}, ";
				if(!string.IsNullOrWhiteSpace(Letter))
					address += $"лит.{Letter}, ";
				if(!string.IsNullOrWhiteSpace(Entrance))
					address += $"{entranceType.GetEnumShortTitle()} {Entrance}, ";
				if(!string.IsNullOrWhiteSpace(Floor))
					address += $"эт.{Floor}, ";
				if(!string.IsNullOrWhiteSpace(Room))
					address += $"{RoomType.GetEnumShortTitle()} {Room}, ";

				return address.TrimEnd(',', ' ');
			}
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
		
		string city;

		[Display(Name = "Город")]
		public virtual string City
		{
			get => city;
			set => SetField(ref city, value);
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

		string cityDistrict;

		[Display(Name = "Район области")]
		public virtual string CityDistrict {
			get => cityDistrict;
			set => SetField(ref cityDistrict, value);
		}

		string street;

		[Display(Name = "Улица")]
		public virtual string Street {
			get => street;
			set => SetField(ref street, value);
		}

		string streetType;

		[Display(Name = "Тип улицы")]
		public virtual string StreetType
		{
			get => streetType;
			set => SetField(ref streetType, value);
		}

		[Display(Name = "Тип улицы (сокр.)")]
		public virtual string StreetTypeShort
		{
			get => _streetTypeShort;
			set => SetField(ref _streetTypeShort, value);
		}

		[Display(Name = "Район города")]
		public virtual string StreetDistrict {
			get => _streetDistrict;
			set => SetField(ref _streetDistrict, value);
		}


		string building;

		[Display(Name = "Номер дома")]
		public virtual string Building {
			get => building;
			set => SetField(ref building, value, () => Building);
		}

		RoomType roomType;

		[Display(Name = "Тип помещения")]
		public virtual RoomType RoomType {
			get => roomType;
			set => SetField(ref roomType, value, () => RoomType);
		}

		string room;

		[Display(Name = "Офис/Квартира")]
		public virtual string Room {
			get => room;
			set => SetField(ref room, value, () => Room);
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		decimal? latitude;

		/// <summary>
		/// Широта. Для установки координат используйте метод SetСoordinates
		/// </summary>
		[Display(Name = "Широта")]
		[PropertyChangedAlso("СoordinatesText")]
		public virtual decimal? Latitude {
			get => latitude;
			protected set => SetField(ref latitude, value, () => Latitude);
		}

		decimal? longitude;

		/// <summary>
		/// Долгота. Для установки координат используйте метод SetСoordinates
		/// </summary>
		[Display(Name = "Долгота")]
		[PropertyChangedAlso("СoordinatesText")]
		public virtual decimal? Longitude {
			get => longitude;
			protected set => SetField(ref longitude, value, () => Longitude);
		}

		bool isActive = true;

		[Display(Name = "Активный")]
		public virtual bool IsActive {
			get => isActive;
			set => SetField(ref isActive, value, () => IsActive);
		}

		private IList<DeliveryPointResponsiblePerson> responsiblePersons = new List<DeliveryPointResponsiblePerson>();

		[Display(Name = "Ответственные лица")]
		public virtual IList<DeliveryPointResponsiblePerson> ResponsiblePersons {
			get => responsiblePersons;
			set => SetField(ref responsiblePersons, value, () => ResponsiblePersons);
		}

		GenericObservableList<DeliveryPointResponsiblePerson> observableResponsiblePersons;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPointResponsiblePerson> ObservableResponsiblePersons {
			get {
				if(observableResponsiblePersons == null)
					observableResponsiblePersons = new GenericObservableList<DeliveryPointResponsiblePerson>(ResponsiblePersons);
				return observableResponsiblePersons;
			}
		}

		District district;
		[Display(Name = "Район доставки")]
		public virtual District District {
			get => district;
			set => SetField(ref district, value, () => District);
		}

		DeliverySchedule deliverySchedule;

		[Display(Name = "График доставки")]
		public virtual DeliverySchedule DeliverySchedule {
			get => deliverySchedule;
			set => SetField(ref deliverySchedule, value, () => DeliverySchedule);
		}

		bool foundOnOsm;

		[Display(Name = "Адрес найден на карте OSM")]
		public virtual bool FoundOnOsm {
			get => foundOnOsm;
			set => SetField(ref foundOnOsm, value, () => FoundOnOsm);
		}

		bool manualCoordinates;

		[Display(Name = "Ручные координаты")]
		public virtual bool ManualCoordinates {
			get => manualCoordinates;
			set => SetField(ref manualCoordinates, value, () => ManualCoordinates);
		}

		bool isFixedInOsm;

		[Display(Name = "Исправлен в OSM")]
		public virtual bool IsFixedInOsm {
			get => isFixedInOsm;
			set => SetField(ref isFixedInOsm, value, () => IsFixedInOsm);
		}

		Counterparty counterparty;

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value, () => Counterparty);
		}

		private string kpp;
		[Display(Name = "КПП")]
		public virtual string KPP {
			get => kpp;
			set => SetField(ref kpp, value);
		}

		private string address1c;

		[Display(Name = "Адрес 1С")]
		public virtual string Address1c {
			get => address1c;
			set => SetField(ref address1c, value, () => Address1c);
		}

		string code1c;

		[Display(Name = "Код в 1С")]
		/// Код уникален только внутри контрагента
		public virtual string Code1c {
			get => code1c;
			set => SetField(ref code1c, value, () => Code1c);
		}

		string organization;
		[Display(Name = "Организация")]
		public virtual string Organization {
			get => organization;
			set => SetField(ref organization, value, () => Organization);
		}

		int bottleReserv;

		[Display(Name = "Резерв бутылей")]
		public virtual int BottleReserv {
			get => bottleReserv;
			set => SetField(ref bottleReserv, value, () => BottleReserv);
		}

		Nomenclature defaultWaterNomenclature;

		[Display(Name = "Вода по умолчанию")]
		public virtual Nomenclature DefaultWaterNomenclature {
			get => defaultWaterNomenclature;
			set => SetField(ref defaultWaterNomenclature, value, () => DefaultWaterNomenclature);
		}

		bool alwaysFreeDelivery;

		[Display(Name = "Всегда бесплатная доставка")]
		public virtual bool AlwaysFreeDelivery {
			get => alwaysFreeDelivery;
			set => SetField(ref alwaysFreeDelivery, value, () => AlwaysFreeDelivery);
		}

		User coordsLastChangeUser;

		[Display(Name = "Последнее изменение пользователем")]
		public virtual User СoordsLastChangeUser {
			get => coordsLastChangeUser;
			set => SetField(ref coordsLastChangeUser, value, () => СoordsLastChangeUser);
		}

		private int? distanceFromBaseMeters;

		[Display(Name = "Расстояние от базы в метрах")]
		public virtual int? DistanceFromBaseMeters {
			get => distanceFromBaseMeters;
			set => SetField(ref distanceFromBaseMeters, value, () => DistanceFromBaseMeters);
		}

		IList<Phone> phones = new List<Phone>();

		[Display(Name = "Телефоны")]
		public virtual IList<Phone> Phones {
			get => phones;
			set => SetField(ref phones, value, () => Phones);
		}

		GenericObservableList<Phone> observablePhones;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Phone> ObservablePhones {
			get {
				if(observablePhones == null)
					observablePhones = new GenericObservableList<Phone>(Phones);
				return observablePhones;
			}
		}

		private bool? haveResidue;

		[Display(Name = "Посчитан ввод остатков")]
		public virtual bool? HaveResidue {
			get => haveResidue;
			set => SetField(ref haveResidue, value, () => HaveResidue);
		}
		
		private IList<NomenclatureFixedPrice> nomenclatureFixedPrices = new List<NomenclatureFixedPrice>();
		[Display(Name = "Фиксированные цены")]
		public virtual IList<NomenclatureFixedPrice> NomenclatureFixedPrices {
			get => nomenclatureFixedPrices;
			set => SetField(ref nomenclatureFixedPrices, value);
		}

		private GenericObservableList<NomenclatureFixedPrice> observableNomenclatureFixedPrices;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<NomenclatureFixedPrice> ObservableNomenclatureFixedPrices {
			get {
				if (observableNomenclatureFixedPrices == null)
					observableNomenclatureFixedPrices = new GenericObservableList<NomenclatureFixedPrice>(NomenclatureFixedPrices);
				return observableNomenclatureFixedPrices;
			}
		}

		private int minimalOrderSumLimit;
		/// <summary>
		/// Минимальный порог суммы заказа
		/// </summary>
		public virtual int MinimalOrderSumLimit {
			get => minimalOrderSumLimit;
			set {
				SetField(ref minimalOrderSumLimit, value);
			}
		}

		private int maximalOrderSumLimit;
		/// <summary>
		/// Максимальный порог суммы заказа
		/// </summary>
		public virtual int MaximalOrderSumLimit {
			get => maximalOrderSumLimit;
			set {
				SetField(ref maximalOrderSumLimit, value);
			}
		}

		private IList<DeliveryPointEstimatedCoordinate> deliveryPointEstimatedCoordinates = new List<DeliveryPointEstimatedCoordinate>();
		[Display(Name = "Предполагаемые координаты доставки")]
		public virtual IList<DeliveryPointEstimatedCoordinate> DeliveryPointEstimatedCoordinates
		{
			get => deliveryPointEstimatedCoordinates;
			set => SetField(ref deliveryPointEstimatedCoordinates, value);
		}

		GenericObservableList<DeliveryPointEstimatedCoordinate> observableDeliveryPointEstimatedCoordinates;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryPointEstimatedCoordinate> ObservableDeliveryPointEstimatedCoordinates
		{
			get => observableDeliveryPointEstimatedCoordinates
					?? (observableDeliveryPointEstimatedCoordinates = new GenericObservableList<DeliveryPointEstimatedCoordinate>(DeliveryPointEstimatedCoordinates));
		}

		private LogisticsRequirements _logisticsRequirements;
		[Display(Name = "Требования к логистике")]
		public virtual LogisticsRequirements LogisticsRequirements
		{
			get => _logisticsRequirements;
			set => SetField(ref _logisticsRequirements, value);
		}

		#region Временные поля для хранения фиксированных цен из 1с

		private decimal fixPrice1;
		/// <summary>
		/// Фикса Семиозерье из 1с
		/// </summary>
		[Display(Name = "Фикса Семиозерье из 1с")]
		public virtual decimal FixPrice1 {
			get => fixPrice1;
			set => SetField(ref fixPrice1, value, () => FixPrice1);
		}

		private decimal fixPrice2;
		/// <summary>
		/// Фикса Кислородная из 1с
		/// </summary>
		[Display(Name = "Фикса Кислородная из 1с")]
		public virtual decimal FixPrice2 {
			get => fixPrice2;
			set => SetField(ref fixPrice2, value, () => FixPrice2);
		}

		private decimal fixPrice3;
		/// <summary>
		/// Фикса Снятогорская из 1с
		/// </summary>
		[Display(Name = "Фикса Снятогорская из 1с")]
		public virtual decimal FixPrice3 {
			get => fixPrice3;
			set => SetField(ref fixPrice3, value, () => FixPrice3);
		}

		private decimal fixPrice4;
		/// <summary>
		/// Фикса Стройка из 1с
		/// </summary>
		[Display(Name = "Фикса Стройка из 1с")]
		public virtual decimal FixPrice4 {
			get => fixPrice4;
			set => SetField(ref fixPrice4, value, () => FixPrice4);
		}

		private decimal fixPrice5;
		/// <summary>
		/// Фикса С Ручками из 1с
		/// </summary>
		[Display(Name = "Фикса С Ручками из 1с")]
		public virtual decimal FixPrice5 {
			get => fixPrice5;
			set => SetField(ref fixPrice5, value, () => FixPrice5);
		}

		bool addCertificatesAlways;
		[Display(Name = "Всегда добавлять сертификаты")]
		public virtual bool AddCertificatesAlways {
			get => addCertificatesAlways;
			set => SetField(ref addCertificatesAlways, value, () => AddCertificatesAlways);
		}

		DeliveryPointCategory category;
		[Display(Name = "Тип объекта")]
		public virtual DeliveryPointCategory Category {
			get => category;
			set {
				if(value != null && value.IsArchive)
					value = null;
				SetField(ref category, value, () => Category);
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

		#endregion


		#endregion

		#region Расчетные

		public virtual string CoordinatesText {
			get {
				if(Latitude == null || Longitude == null)
					return string.Empty;
				return string.Format("(ш. {0:F5}, д. {1:F5})", Latitude, Longitude);
			}
		}

		public virtual bool CoordinatesExist => Latitude.HasValue && Longitude.HasValue;

		public virtual Point NetTopologyPoint => CoordinatesExist ? new Point((double)Latitude, (double)Longitude) : null;

		public virtual PointOnEarth PointOnEarth => new PointOnEarth(Latitude.Value, Longitude.Value);

		public virtual GMap.NET.PointLatLng GmapPoint => new GMap.NET.PointLatLng((double)Latitude, (double)Longitude);

		public virtual long СoordinatesHash => CachedDistance.GetHash(this);

		#endregion

		//FIXME вынести зависимость
		IDeliveryRepository deliveryRepository = new DeliveryRepository();

		/// <summary>
		/// Возврат районов доставки, в которые попадает точка доставки
		/// </summary>
		/// <param name="uow">UnitOfWork через который будет получены все районы доставки,
		/// среди которых будет производится поиск подходящего района</param>
		public virtual IEnumerable<District> CalculateDistricts(IUnitOfWork uow)
		{
			if(!CoordinatesExist) {
				return new List<District>();
			}
			return deliveryRepository.GetDistricts(uow, Latitude.Value, Longitude.Value);
		}

		/// <summary>
		/// Поиск района города, в котором находится текущая точка доставки
		/// </summary>
		/// <returns><c>true</c>, если район города найден</returns>
		/// <param name="uow">UnitOfWork через который будет производится поиск подходящего района города</param>
		/// <param name="districtsSet">Версия районов, из которой будет ассоциироваться район. Если равно null, то будет браться активная версия</param>
		public bool FindAndAssociateDistrict(IUnitOfWork uow, DistrictsSet districtsSet = null)
		{
			if(!CoordinatesExist) {
				return false;
			}

			District foundDistrict = deliveryRepository.GetDistrict(uow, Latitude.Value, Longitude.Value, districtsSet);
			if(foundDistrict == null) {
				return false;
			}
			District = foundDistrict;
			return true;
		}

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

			var geoGroupVersion = District.GeographicGroup.GetActualVersionOrNull();
			if(geoGroupVersion == null)
			{
				throw new InvalidOperationException($"Не установлена активная версия данных в части города {District.GeographicGroup.Name}");
			}

			var route = new List<PointOnEarth>(2) {
				new PointOnEarth(geoGroupVersion.BaseLatitude.Value, geoGroupVersion.BaseLongitude.Value),
				new PointOnEarth(Latitude.Value, Longitude.Value)
			};
			
			var result = OsrmClientFactory.Instance.GetRoute(route, false, GeometryOverview.False, _globalSettings.ExcludeToll);
			if(result == null) {
				logger.Error("Сервер расчета расстояний не вернул ответа.");
				return false;
			}
			if(result.Code != "Ok") {
				logger.Error("Сервер расчета расстояний вернул следующее сообщение:\n" + result.StatusMessageRus);
				return false;
			}
			DistanceFromBaseMeters = result.Routes[0].TotalDistance;
			return true;
		}

		[Obsolete]
		public static IUnitOfWorkGeneric<DeliveryPoint> CreateUowForNew(Counterparty counterparty)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<DeliveryPoint>();
			uow.Root.Counterparty = counterparty;
			return uow;
		}

		public static DeliveryPoint Create(Counterparty counterparty)
		{
			var point = new DeliveryPoint {
				Counterparty = counterparty
			};
			counterparty.DeliveryPoints.Add(point);
			return point;
		}

		#region IValidatableObject Implementation

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Category == null)
				yield return new ValidationResult(
					string.Format("Необходимо выбрать тип точки доставки"),
					new[] { this.GetPropertyName(o => o.Category) });

			if(Counterparty == null)
				yield return new ValidationResult(
					string.Format("Необходимо выбрать клиента"),
					new[] { this.GetPropertyName(o => o.Counterparty) });

			if(Building?.Length == 0)
				yield return new ValidationResult(
					string.Format("Заполните поле \"Дом\""),
					new[] { this.GetPropertyName(o => o.Building) });

			if(Building?.Length > 20)
				yield return new ValidationResult(
					string.Format("Длина строки \"Дом\" не должна превышать 20 символов"),
					new[] { this.GetPropertyName(o => o.Building) });

			if(City?.Length == 0)
				yield return new ValidationResult(
					string.Format("Заполните поле \"Город\""),
					new[] { this.GetPropertyName(o => o.City) });

			if(City?.Length > 45)
				yield return new ValidationResult(
					string.Format("Длина строки \"Город\" не должна превышать 45 символов"),
					new[] { this.GetPropertyName(o => o.City) });

			if(Street?.Length == 0)
				yield return new ValidationResult(
					string.Format("Заполните поле \"Улица\""),
					new[] { this.GetPropertyName(o => o.Street) });

			if(Street?.Length > 50)
				yield return new ValidationResult(
					string.Format("Длина строки \"Улица\" не должна превышать 50 символов"),
					new[] { this.GetPropertyName(o => o.Street) });

			if(Room?.Length > 20)
				yield return new ValidationResult(
					string.Format("Длина строки \"Офис/Квартира\" не должна превышать 20 символов"),
					new[] { this.GetPropertyName(o => o.Room) });

			if(Entrance?.Length > 50)
				yield return new ValidationResult(
					string.Format("Длина строки \"Парадная\" не должна превышать 50 символов"),
					new[] { this.GetPropertyName(o => o.Entrance) });

			if(Floor?.Length > 20)
				yield return new ValidationResult(
					string.Format("Длина строки \"Этаж\" не должна превышать 20 символов"),
					new[] { this.GetPropertyName(o => o.Floor) });

			if(Code1c?.Length > 10)
				yield return new ValidationResult(
					string.Format("Длина строки \"Код 1С\" не должна превышать 10 символов"),
					new[] { this.GetPropertyName(o => o.Code1c) });

			if(KPP?.Length > 45)
				yield return new ValidationResult(
					string.Format("Длина строки \"КПП\" не должна превышать 45 символов"),
					new[] { this.GetPropertyName(o => o.KPP) });
					
			var notNeedOrganizationRoomTypes = new RoomType[] { RoomType.Apartment, RoomType.Chamber };
			if(Counterparty.PersonType == PersonType.natural && !notNeedOrganizationRoomTypes.Contains(RoomType)) {
				if(String.IsNullOrWhiteSpace(Organization))
					yield return new ValidationResult(
						string.Format("Необходимо заполнить поле \"Организация\""),
						new[] { this.GetPropertyName(o => o.Organization) });

				if(Organization?.Length > 45)
					yield return new ValidationResult(
						string.Format("Длина строки \"Организация\" не должна превышать 45 символов"),
						new[] { this.GetPropertyName(o => o.Organization) });
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

			foreach (var fixedPrice in NomenclatureFixedPrices) {
				var fixedPriceValidationResults = fixedPrice.Validate(validationContext);
				foreach (var fixedPriceValidationResult in fixedPriceValidationResults) {
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

			foreach(var phone in Phones)
			{
				if(phone.RoboAtsCounterpartyName == null)
				{
					phonesValidationStringBuilder.AppendLine($"Для телефона { phone.Number } не указано имя контрагента.");
				}

				if(phone.RoboAtsCounterpartyPatronymic == null)
				{
					phonesValidationStringBuilder.AppendLine($"Для телефона { phone.Number } не указано отчество контрагента.");
				}

				if(!phone.IsValidPhoneNumber)
				{
					phonesValidationStringBuilder.AppendLine($"Номер {phone.Number} имеет неправильный формат.");
				}
			}

			var phonesValidationMessage = phonesValidationStringBuilder.ToString();

			if(!string.IsNullOrEmpty(phonesValidationMessage))
			{
				yield return new ValidationResult(phonesValidationMessage);
			}

			if(ResponsiblePersons.Any(x => x.DeliveryPointResponsiblePersonType == null || x.Employee == null || string.IsNullOrWhiteSpace(x.Phone)))
			{
				yield return new ValidationResult("Для ответственных лиц должны быть заполнены Тип, Сотрудник и Телефон", new[] { nameof(ResponsiblePersons) });
			}
		}

		#endregion

		private string GetStreetTypeShort()
		{
			return string.Equals(streetType, StreetTypeShort, StringComparison.CurrentCultureIgnoreCase)
				? $"{StreetTypeShort} "
				: $"{StreetTypeShort}. ";
		}
	}

	public enum EntranceType
	{
		[Display(Name = "Парадная", ShortName = "пар.")]
		Entrance,
		[Display(Name = "Торговый центр", ShortName = "ТЦ")]
		TradeCenter,
		[Display(Name = "Торговый комплекс", ShortName = "ТК")]
		TradeComplex,
		[Display(Name = "Бизнес-центр", ShortName = "БЦ")]
		BusinessCenter,
		[Display(Name = "Школа", ShortName = "шк.")]
		School,
		[Display(Name = "Общежитие", ShortName = "общ.")]
		Hostel
	}

	public class EntranceTypeStringType : NHibernate.Type.EnumStringType
	{
		public EntranceTypeStringType() : base(typeof(EntranceType)) { }
	}
}

