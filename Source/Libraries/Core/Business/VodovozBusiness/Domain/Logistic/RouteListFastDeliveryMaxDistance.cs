using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public class RouteListFastDeliveryMaxDistance : PropertyChangedBase, IDomainObject
	{
		private DateTime _startDate;
		private DateTime? _endDate;
		private RouteList _routeList;
		private decimal _distance;

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

		[Display(Name = "Значение в километрах")]
		public virtual decimal Distance
		{
			get => _distance;
			set => SetField(ref _distance, value);
		}
	}
}
