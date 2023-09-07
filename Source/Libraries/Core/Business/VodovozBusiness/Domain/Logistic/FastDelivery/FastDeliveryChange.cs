using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic.FastDelivery
{
	public partial class FastDeliveryChange : PropertyChangedBase, IDomainObject
	{
		private RouteList _routeList;
		private Order _order;
		private ChangeTypeEnum _changeType;
		private DateTime _createdAt;

		public virtual int Id { get; set; }

		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}


		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Тип изменения")]
		public virtual ChangeTypeEnum ChangeType
		{
			get => _changeType;
			set => SetField(ref _changeType, value);
		}

		[Display(Name = "Создано")]
		public virtual DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
		}
	}
}
