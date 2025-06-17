using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Contacts;

namespace Vodovoz.Core.Domain.Clients.DeliveryPoints
{
	/// <summary>
	/// Точка доставки
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "точки доставки",
		Nominative = "точка доставки",
		Accusative = "точки доставки")]
	[HistoryTrace]
	[EntityPermission]
	public class DeliveryPointEntity : PropertyChangedBase, IDomainObject
	{
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
		private bool _foundOnOsm;
		private bool _manualCoordinates;
		private bool _isFixedInOsm;
		private string _kpp;
		private string _address1c;
		private string _code1c;
		private string _organization;
		private int _bottleReserv;
		private bool _alwaysFreeDelivery;
		private int? _distanceFromBaseMeters;
		private bool? _haveResidue;
		private int _minimalOrderSumLimit;
		private int _maximalOrderSumLimit;
		private decimal _fixPrice1;
		private decimal _fixPrice2;
		private decimal _fixPrice3;
		private decimal _fixPrice4;
		private decimal _fixPrice5;
		private bool _addCertificatesAlways;
		private string _onlineComment;
		private string _intercom;
		private string _buildingFromOnline;

		private IObservableList<PhoneEntity> _phones = new ObservableList<PhoneEntity>();
		private IObservableList<EmailEntity> _emails;

		/// <summary>
		/// Конструктор
		/// </summary>
		public DeliveryPointEntity()
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
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Время разгрузки
		/// </summary>
		[Display(Name = "Время разгрузки")]
		public virtual int MinutesToUnload
		{
			get => _minutesToUnload;
			set => SetField(ref _minutesToUnload, value);
		}

		/// <summary>
		/// Литера
		/// </summary>
		[Display(Name = "Литера")]
		public virtual string Letter
		{
			get => _letter;
			set => SetField(ref _letter, value);
		}

		/// <summary>
		/// Помещение
		/// </summary>
		[Display(Name = "Помещение")]
		public virtual string Placement
		{
			get => _placement;
			set => SetField(ref _placement, value);
		}

		/// <summary>
		/// Этаж
		/// </summary>
		[Display(Name = "Этаж")]
		public virtual string Floor
		{
			get => _floor;
			set => SetField(ref _floor, value);
		}

		/// <summary>
		/// Тип входа
		/// </summary>
		[Display(Name = "Тип входа")]
		public virtual EntranceType EntranceType
		{
			get => _entranceType;
			set => SetField(ref _entranceType, value);
		}

		/// <summary>
		/// Парадная
		/// </summary>
		[Display(Name = "Парадная")]
		public virtual string Entrance
		{
			get => _entrance;
			set => SetField(ref _entrance, value);
		}
		
		/// <summary>
		/// ФИАС идентификатор города
		/// </summary>
		public virtual Guid? CityFiasGuid
		{
			get => _cityFiasGuid;
			set => SetField(ref _cityFiasGuid, value);
		}

		/// <summary>
		/// ФИАС идентификатор улицы
		/// </summary>
		public virtual Guid? StreetFiasGuid
		{
			get => _streetFiasGuid;
			set => SetField(ref _streetFiasGuid, value);
		}

		/// <summary>
		/// ФИАС идентификатор здания
		/// </summary>
		public virtual Guid? BuildingFiasGuid
		{
			get => _buildingFiasGuid;
			set => SetField(ref _buildingFiasGuid, value);
		}

		/// <summary>
		/// Город
		/// </summary>
		[Display(Name = "Город")]
		public virtual string City
		{
			get => _city;
			set => SetField(ref _city, value);
		}

		/// <summary>
		/// Тип населенного пункта
		/// </summary>
		[Display(Name = "Тип населенного пункта")]
		public virtual string LocalityType
		{
			get => _localityType;
			set => SetField(ref _localityType, value);
		}

		/// <summary>
		/// Тип населенного пункта (сокращ.)
		/// </summary>
		[Display(Name = "Тип населенного пункта (сокращ.)")]
		public virtual string LocalityTypeShort
		{
			get => _localityTypeShort;
			set => SetField(ref _localityTypeShort, value);
		}

		/// <summary>
		/// Район области
		/// </summary>
		[Display(Name = "Район области")]
		public virtual string CityDistrict
		{
			get => _cityDistrict;
			set => SetField(ref _cityDistrict, value);
		}

		/// <summary>
		/// Улица
		/// </summary>
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

		/// <summary>
		/// Тип улицы (сокр.)
		/// </summary>
		[Display(Name = "Тип улицы (сокр.)")]
		public virtual string StreetTypeShort
		{
			get => _streetTypeShort;
			set => SetField(ref _streetTypeShort, value);
		}

		/// <summary>
		/// Район города
		/// </summary>
		[Display(Name = "Район города")]
		public virtual string StreetDistrict
		{
			get => _streetDistrict;
			set => SetField(ref _streetDistrict, value);
		}

		/// <summary>
		/// Номер дома
		/// </summary>
		[Display(Name = "Номер дома")]
		public virtual string Building
		{
			get => _building;
			set => SetField(ref _building, value);
		}

		/// <summary>
		/// Тип помещения
		/// </summary>
		[Display(Name = "Тип помещения")]
		public virtual RoomType RoomType
		{
			get => _roomType;
			set => SetField(ref _roomType, value);
		}

		/// <summary>
		/// Офис/Квартира
		/// </summary>
		[Display(Name = "Офис/Квартира")]
		public virtual string Room
		{
			get => _room;
			set => SetField(ref _room, value);
		}

		/// <summary>
		/// Комментарий
		/// </summary>
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

		/// <summary>
		/// Активный
		/// </summary>
		[Display(Name = "Активный")]
		public virtual bool IsActive
		{
			get => _isActive;
			set => SetField(ref _isActive, value);
		}

		/// <summary>
		/// Адрес найден на карте OSM
		/// </summary>
		[Display(Name = "Адрес найден на карте OSM")]
		public virtual bool FoundOnOsm
		{
			get => _foundOnOsm;
			set => SetField(ref _foundOnOsm, value);
		}

		/// <summary>
		/// Ручные координаты
		/// </summary>
		[Display(Name = "Ручные координаты")]
		public virtual bool ManualCoordinates
		{
			get => _manualCoordinates;
			set => SetField(ref _manualCoordinates, value);
		}

		/// <summary>
		/// Исправлен в OSM
		/// </summary>
		[Display(Name = "Исправлен в OSM")]
		public virtual bool IsFixedInOsm
		{
			get => _isFixedInOsm;
			set => SetField(ref _isFixedInOsm, value);
		}
		
		/// <summary>
		/// КПП
		/// </summary>
		[Display(Name = "КПП")]
		public virtual string KPP
		{
			get => _kpp;
			set => SetField(ref _kpp, value);
		}

		/// <summary>
		/// Адрес 1С
		/// </summary>
		[Display(Name = "Адрес 1С")]
		public virtual string Address1c
		{
			get => _address1c;
			set => SetField(ref _address1c, value);
		}

		/// <summary>
		/// Код в 1С
		/// Код уникален только внутри контрагента
		/// </summary>
		[Display(Name = "Код в 1С")]
		public virtual string Code1c
		{
			get => _code1c;
			set => SetField(ref _code1c, value);
		}

		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual string Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}

		/// <summary>
		/// Резерв бутылей
		/// </summary>
		[Display(Name = "Резерв бутылей")]
		public virtual int BottleReserv
		{
			get => _bottleReserv;
			set => SetField(ref _bottleReserv, value);
		}

		/// <summary>
		/// Всегда бесплатная доставка
		/// </summary>
		[Display(Name = "Всегда бесплатная доставка")]
		public virtual bool AlwaysFreeDelivery
		{
			get => _alwaysFreeDelivery;
			set => SetField(ref _alwaysFreeDelivery, value);
		}

		/// <summary>
		/// Расстояние от базы в метрах
		/// </summary>
		[Display(Name = "Расстояние от базы в метрах")]
		public virtual int? DistanceFromBaseMeters
		{
			get => _distanceFromBaseMeters;
			set => SetField(ref _distanceFromBaseMeters, value);
		}

		/// <summary>
		/// Посчитан ввод остатков
		/// </summary>
		[Display(Name = "Посчитан ввод остатков")]
		public virtual bool? HaveResidue
		{
			get => _haveResidue;
			set => SetField(ref _haveResidue, value);
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

		#endregion Временные поля для хранения фиксированных цен из 1с

		/// <summary>
		/// Всегда добавлять сертификаты
		/// </summary>
		[Display(Name = "Всегда добавлять сертификаты")]
		public virtual bool AddCertificatesAlways
		{
			get => _addCertificatesAlways;
			set => SetField(ref _addCertificatesAlways, value);
		}

		/// <summary>
		/// Время начала обеда
		/// </summary>
		[Display(Name = "Время начала обеда")]
		public virtual TimeSpan? LunchTimeFrom
		{
			get => _lunchTimeFrom;
			set => SetField(ref _lunchTimeFrom, value);
		}

		/// <summary>
		/// Время окончания обеда
		/// </summary>
		[Display(Name = "Время окончания обеда")]
		public virtual TimeSpan? LunchTimeTo
		{
			get => _lunchTimeTo;
			set => SetField(ref _lunchTimeTo, value);
		}

		/// <summary>
		/// Телефоны
		/// </summary>
		[Display(Name = "Телефоны")]
		public virtual IObservableList<PhoneEntity> Phones
		{
			get => _phones;
			set => SetField(ref _phones, value);
		}

		#region Свойства для интеграции

		/// <summary>
		/// Комментарий к ТД из ИПЗ
		/// </summary>
		[Display(Name = "Комментарий к ТД из ИПЗ")]
		public virtual string OnlineComment
		{
			get => _onlineComment;
			set => SetField(ref _onlineComment, value);
		}

		/// <summary>
		/// Домофон
		/// </summary>
		[Display(Name = "Домофон")]
		public virtual string Intercom
		{
			get => _intercom;
			set => SetField(ref _intercom, value);
		}
		
		/// <summary>
		/// Номер дома из ИПЗ, как есть(без форматирования)
		/// </summary>
		[Display(Name = "Номер дома из ИПЗ, как есть(без форматирования)")]
		[IgnoreHistoryTrace]
		public virtual string BuildingFromOnline
		{
			get => _buildingFromOnline;
			set => SetField(ref _buildingFromOnline, value);
		}

		#endregion

		/// <summary>
		/// Полный адрес
		/// </summary>
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
					address += $"{EntranceType.GetEnumShortTitle()} {Entrance}, ";
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

		/// <summary>
		/// Адрес без дополнения
		/// </summary>
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
					address += $"{EntranceType.GetEnumShortTitle()} {Entrance}, ";
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

		/// <summary>
		/// Сокращенный адрес
		/// </summary>
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
					address += $"{EntranceType.GetEnumShortTitle()} {Entrance}, ";
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

		/// <summary>
		/// Координаты в виде текста
		/// </summary>
		public virtual string CoordinatesText => Latitude == null || Longitude == null ? string.Empty : $"(ш. {Latitude:F5}, д. {Longitude:F5})";

		/// <summary>
		/// Сокращенное название типа улицы
		/// </summary>
		/// <returns></returns>
		private string GetStreetTypeShort()
		{
			return string.Equals(StreetType, StreetTypeShort, StringComparison.CurrentCultureIgnoreCase)
				? $"{StreetTypeShort} "
				: $"{StreetTypeShort}. ";
		}
	}
}
