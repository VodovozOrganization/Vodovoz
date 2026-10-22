using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Logistics
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "адреса маршрутного листа",
		Nominative = "адрес маршрутного листа")]
	[HistoryTrace]
	public class RouteListItemEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _version;
		private RouteListEntity _routeList;
		private string _unscannedCodesReason;
		private OrderEntity _order;

		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}


		/// <summary>
		/// Версия
		/// </summary>
		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		[Display(Name = "Маршрутный лист")]
		public virtual RouteListEntity RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		/// <summary>
		/// Причина не отсканированных кодов
		/// </summary>
		[Display(Name = "Причина не отсканированных кодов")]
		public virtual string UnscannedCodesReason
		{
			get => _unscannedCodesReason;
			set => SetField(ref _unscannedCodesReason, value);
		}

		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
	}
}
