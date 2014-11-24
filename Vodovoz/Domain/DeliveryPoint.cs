using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using QSContacts;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubjectAttributes("Точки доставки")]
	public class DeliveryPoint : IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual int MinutesToUnload { get; set; }
		public virtual int Floor { get; set; }
		[Required(ErrorMessage="Название точки доставки должно быть заполнено.")]
		public virtual string Name { get; set; }
		public virtual string Region { get; set; }
		[Required(ErrorMessage="Город должен быть заполнен.")]
		public virtual string City { get; set; }
		[Required(ErrorMessage="Улица должна быть заполнена.")]
		public virtual string Street { get; set; }
		[Required(ErrorMessage="Номер дома должен быть заполнен.")]
		public virtual string Building { get; set; }
		public virtual string Room { get; set; }
		public virtual string Comment { get; set; }
		public virtual string Latitude { get; set; }
		public virtual string Longitude { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual Contact Contact { get; set; }
		public virtual Phone Phone { get; set; }
		//График доставки по умолчанию(справочник)
		//Масштаб карты
		//Файл схемы проезда
		//Логистический район
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
		}
	}
	public interface IDeliveryPointOwner
	{
		IList<DeliveryPoint> DeliveryPoints { get; set;}
	}
}

