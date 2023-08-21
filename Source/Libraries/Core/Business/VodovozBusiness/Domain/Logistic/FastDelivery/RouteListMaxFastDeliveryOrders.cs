using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Максимальные кол-ва заказов ДЗЧ",
		Nominative = "Максимальное кол-во заказов ДЗЧ")]
	public class RouteListMaxFastDeliveryOrders : PropertyChangedBase, IDomainObject
	{
		private DateTime _startDate;
		private DateTime? _endDate;
		private RouteList _routeList;
		private int _maxOrders;

		public virtual int Id { get; set; }

		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[Display(Name = "Максимально кол-во заказов")]
		public virtual int MaxOrders
		{
			get => _maxOrders;
			set => SetField(ref _maxOrders, value);
		}
	}
}
