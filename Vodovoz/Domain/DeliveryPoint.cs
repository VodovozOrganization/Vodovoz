using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using QSContacts;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject ("Точки доставки")]
	public class DeliveryPoint : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int minutesToUnload;

		public virtual int MinutesToUnload {
			get { return minutesToUnload; }
			set { SetField (ref minutesToUnload, value, () => MinutesToUnload); }
		}

		int floor;

		public virtual int Floor {
			get { return floor; }
			set { SetField (ref floor, value, () => Floor); }
		}

		string name;

		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		string region;

		public virtual string Region {
			get { return region; }
			set { SetField (ref region, value, () => Region); }
		}

		string city;

		[Required (ErrorMessage = "Город должен быть заполнен.")]
		public virtual string City {
			get { return city; }
			set { SetField (ref city, value, () => City); }
		}

		string street;

		[Required (ErrorMessage = "Улица должна быть заполнена.")]
		public virtual string Street {
			get { return street; }
			set { SetField (ref street, value, () => Street); }
		}

		string building;

		[Required (ErrorMessage = "Номер дома должен быть заполнен.")]
		public virtual string Building {
			get { return building; }
			set { SetField (ref building, value, () => Building); }
		}

		string room;

		public virtual string Room {
			get { return room; }
			set { SetField (ref room, value, () => Room); }
		}

		string comment;

		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		string latitude;

		public virtual string Latitude {
			get { return latitude; }
			set { SetField (ref latitude, value, () => Latitude); }
		}

		string longitude;

		public virtual string Longitude {
			get { return longitude; }
			set { SetField (ref longitude, value, () => Longitude); }
		}

		bool isActive;

		public virtual bool IsActive {
			get { return isActive; }
			set { SetField (ref isActive, value, () => IsActive); }
		}

		Contact contact;

		public virtual Contact Contact {
			get { return contact; }
			set { SetField (ref contact, value, () => Contact); }
		}

		string phone;

		public virtual string Phone {
			get { return phone; }
			set { SetField (ref phone, value, () => Phone); }
		}

		LogisticsArea logisticsArea;

		public virtual LogisticsArea LogisticsArea {
			get { return logisticsArea; }
			set { SetField (ref logisticsArea, value, () => LogisticsArea); }
		}

		DeliverySchedule deliverySchedule;

		public virtual DeliverySchedule DeliverySchedule {
			get { return deliverySchedule; }
			set { SetField (ref deliverySchedule, value, () => DeliverySchedule); }
		}

		bool isNew;

		public virtual bool IsNew {
			get { return isNew; }
			set { SetField (ref isNew, value, () => IsNew); }
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
			get { return String.Format ("{0}г. {1}, ул. {2}, д.{3}, квартира/офис {4}", 
				(Name == String.Empty ? "" : "\"" + Name + "\": "), City, Street, Building, Room); } 
		}
	}

	public interface IDeliveryPointOwner
	{
		IList<DeliveryPoint> DeliveryPoints { get; set; }
	}
}

