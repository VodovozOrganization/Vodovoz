using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Clients.DeliveryPoints
{
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
		
		[Display(Name = "Всегда бесплатная доставка")]
		public virtual bool AlwaysFreeDelivery
		{
			get => _alwaysFreeDelivery;
			set => SetField(ref _alwaysFreeDelivery, value);
		}
		
		[Display(Name = "Расстояние от базы в метрах")]
		public virtual int? DistanceFromBaseMeters
		{
			get => _distanceFromBaseMeters;
			set => SetField(ref _distanceFromBaseMeters, value);
		}
		
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
		
		[Display(Name = "Всегда добавлять сертификаты")]
		public virtual bool AddCertificatesAlways
		{
			get => _addCertificatesAlways;
			set => SetField(ref _addCertificatesAlways, value);
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
		
		public virtual string CoordinatesText => Latitude == null || Longitude == null ? string.Empty : $"(ш. {Latitude:F5}, д. {Longitude:F5})";
		
		private string GetStreetTypeShort()
		{
			return string.Equals(StreetType, StreetTypeShort, StringComparison.CurrentCultureIgnoreCase)
				? $"{StreetTypeShort} "
				: $"{StreetTypeShort}. ";
		}
	}
}
