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

		int floor;

		[Display (Name = "Этаж")]
		public virtual int Floor {
			get { return floor; }
			set { SetField (ref floor, value, () => Floor); }
		}

		string name;

		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
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

		string street;

		[Display (Name = "Улица")]
		[Required (ErrorMessage = "Улица должна быть заполнена.")]
		public virtual string Street {
			get { return street; }
			set { SetField (ref street, value, () => Street); }
		}

		string building;

		[Display (Name = "Номер дома")]
		[Required (ErrorMessage = "Номер дома должен быть заполнен.")]
		public virtual string Building {
			get { return building; }
			set { SetField (ref building, value, () => Building); }
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

		bool isActive;

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
			Name = String.Empty;
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

		public string Point { 
			get {
				return String.Format ("{0}г. {1}, ул. {2}, д.{3}, квартира/офис {4}", 
					(Name == String.Empty ? "" : "\"" + Name + "\": "), City, Street, Building, Room);
			} 
		}

		public static IUnitOfWorkGeneric<DeliveryPoint> Create (Counterparty counterparty)
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<DeliveryPoint> ();
			uow.Root.Counterparty = counterparty;
			return uow;
		}
	}
}

