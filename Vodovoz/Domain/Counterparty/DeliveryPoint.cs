using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "точки доставки",
		Nominative = "точка доставки",
		Accusative = "точки доставки"
	)]
	public class DeliveryPoint : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int minutesToUnload;

		[Display (Name = "Время разгрузки")]
		public virtual int MinutesToUnload {
			get { return minutesToUnload; }
			set { SetField (ref minutesToUnload, value, () => MinutesToUnload); }
		}

		string housing;

		[Display (Name = "Корпус")]
		public virtual string Housing {
			get { return housing; }
			set { SetField (ref housing, value, () => Housing); }
		}

		string letter;

		[Display (Name = "Литера")]
		public virtual string Letter {
			get { return letter; }
			set { SetField (ref letter, value, () => Letter); }
		}

		string structure;

		[Display (Name = "Строение")]
		public virtual string Structure {
			get { return structure; }
			set { SetField (ref structure, value, () => Structure); }
		}

		string placement;

		[Display (Name = "Помещение")]
		public virtual string Placement {
			get { return placement; }
			set { SetField (ref placement, value, () => Placement); }
		}

		int floor;

		[Display (Name = "Этаж")]
		public virtual int Floor {
			get { return floor; }
			set { SetField (ref floor, value, () => Floor); }
		}

		string compiledAddress;

		[Display (Name = "Полный адрес")]
		public virtual string CompiledAddress {
			get {
				string address = String.Empty;
				if (!String.IsNullOrWhiteSpace (City))
					address += String.Format ("г.{0}, ", City);
				if (!String.IsNullOrWhiteSpace (Street))
					address += String.Format ("{0}, ", Street);
				if (!String.IsNullOrWhiteSpace (Building))
					address += String.Format ("д.{0}, ", Building);
				if (!String.IsNullOrWhiteSpace (Housing))
					address += String.Format ("корп.{0}, ", Housing);
				if (!String.IsNullOrWhiteSpace (Structure))
					address += String.Format ("стр.{0}, ", Structure);
				if (!String.IsNullOrWhiteSpace (Letter))
					address += String.Format ("лит.{0}, ", Letter);
				if (default(int) != Floor)
					address += String.Format ("эт.{0}, ", Floor);
				if (!String.IsNullOrWhiteSpace (Room))
					address += String.Format ("{0} {1}, ", GetShortNameOfRoomType (RoomType), Room);

				return address.TrimEnd (',', ' ');
			}
			set { SetField (ref compiledAddress, value, () => CompiledAddress); }
		}

		string region;

		[Display (Name = "Регион")]
		public virtual string Region {
			get { return region; }
			set { SetField (ref region, value, () => Region); }
		}

		string city;

		[Display (Name = "Город")]
		[Required (ErrorMessage = "Город должен быть заполнен.")]
		public virtual string City {
			get { return city; }
			set { SetField (ref city, value, () => City); }
		}

		string cityDistrict;

		[Display (Name = "Район области")]
		public virtual string CityDistrict {
			get { return cityDistrict; }
			set { SetField (ref cityDistrict, value, () => CityDistrict); }
		}

		string street;

		[Display (Name = "Улица")]
		[Required (ErrorMessage = "Улица должна быть заполнена.")]
		public virtual string Street {
			get { return street; }
			set { SetField (ref street, value, () => Street); }
		}

		string streetDistrict;

		[Display (Name = "Район города")]
		public virtual string StreetDistrict {
			get { return streetDistrict; }
			set { SetField (ref streetDistrict, value, () => StreetDistrict); }
		}


		string building;

		[Display (Name = "Номер дома")]
		[Required (ErrorMessage = "Номер дома должен быть заполнен.")]
		public virtual string Building {
			get { return building; }
			set { SetField (ref building, value, () => Building); }
		}

		RoomType roomType;

		[Display (Name = "Тип помещения")]
		public virtual RoomType RoomType {
			get { return roomType; }
			set { SetField (ref roomType, value, () => RoomType); }
		}

		string room;

		[Display (Name = "Офис/Квартира")]
		public virtual string Room {
			get { return room; }
			set { SetField (ref room, value, () => Room); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		string latitude;

		[Display (Name = "Широта")]
		public virtual string Latitude {
			get { return latitude; }
			set { SetField (ref latitude, value, () => Latitude); }
		}

		string longitude;

		[Display (Name = "Долгота")]
		public virtual string Longitude {
			get { return longitude; }
			set { SetField (ref longitude, value, () => Longitude); }
		}

		bool isActive = true;

		[Display (Name = "Активный")]
		public virtual bool IsActive {
			get { return isActive; }
			set { SetField (ref isActive, value, () => IsActive); }
		}

		Contact contact;

		[Display (Name = "Контактное лицо")]
		public virtual Contact Contact {
			get { return contact; }
			set { SetField (ref contact, value, () => Contact); }
		}

		string phone;

		[Display (Name = "Телефон точки")]
		public virtual string Phone {
			get { return phone; }
			set { SetField (ref phone, value, () => Phone); }
		}

		LogisticsArea logisticsArea;

		[Display (Name = "Логистический район")]
		public virtual LogisticsArea LogisticsArea {
			get { return logisticsArea; }
			set { SetField (ref logisticsArea, value, () => LogisticsArea); }
		}

		DeliverySchedule deliverySchedule;

		[Display (Name = "График доставки")]
		public virtual DeliverySchedule DeliverySchedule {
			get { return deliverySchedule; }
			set { SetField (ref deliverySchedule, value, () => DeliverySchedule); }
		}

		bool isNew;

		public virtual bool IsNew {
			get { return isNew; }
			set { SetField (ref isNew, value, () => IsNew); }
		}

		Counterparty counterparty;

		[Required]
		[Display (Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get { return counterparty; }
			protected set { SetField (ref counterparty, value, () => Counterparty); }
		}

		//Масштаб карты
		//Файл схемы проезда

		#endregion

		public DeliveryPoint ()
		{
			CompiledAddress = String.Empty;
			Region = String.Empty;
			City = String.Empty;
			Street = String.Empty;
			Building = String.Empty;
			Room = String.Empty;
			Comment = String.Empty;
			Latitude = String.Empty;
			Longitude = String.Empty;
			Phone = String.Empty;
		}

		public static IUnitOfWorkGeneric<DeliveryPoint> Create (Counterparty counterparty)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<DeliveryPoint> ();
			uow.Root.Counterparty = counterparty;
			return uow;
		}

		public static string GetShortNameOfRoomType(RoomType type)
		{
			switch(type)
			{
			case RoomType.Apartment :
				return "кв.";
			case RoomType.Office: 
				return "оф.";
			case RoomType.Room:
				return "пом.";
			default:
				throw new NotSupportedException ();
			}
		}
	}

	public enum RoomType
	{
		[Display (Name = "Квартира")]
		Apartment,
		[Display (Name = "Офис")]
		Office,
		[Display (Name = "Помещение")]
		Room
	}

	public class RoomTypeStringType : NHibernate.Type.EnumStringType
	{
		public RoomTypeStringType () : base (typeof(RoomType))
		{
		}
	}
}

