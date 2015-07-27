using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки маршрутного листа",
		Nominative = "строка маршрутного листа")]
	public class RouteListItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Orders.Order order;

		[Display (Name = "Заказ")]
		public virtual Orders.Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		public RouteListItem ()
		{
		}
	}
}

