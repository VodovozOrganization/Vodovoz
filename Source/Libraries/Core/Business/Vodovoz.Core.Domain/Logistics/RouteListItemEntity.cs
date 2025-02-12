using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Logistics
{
	/// <summary>
	/// Адрес маршрутного листа
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "адреса маршрутного листа",
		Nominative = "адрес маршрутного листа")]
	[HistoryTrace]
	public class RouteListItemEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _version;
		private OrderEntity _order;
		private int _bottlesReturned;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
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

		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Возвращено бутылей
		/// </summary>
		[Display(Name = "Возвращено бутылей")]
		public virtual int BottlesReturned
		{
			get => _bottlesReturned;
			set => SetField(ref _bottlesReturned, value);
		}
	}
}
