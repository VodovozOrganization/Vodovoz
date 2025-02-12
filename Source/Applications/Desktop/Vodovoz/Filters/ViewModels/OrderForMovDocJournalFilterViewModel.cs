using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Project.Filter;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Store;

namespace Vodovoz.Filters.ViewModels
{
	public class OrderForMovDocJournalFilterViewModel : FilterViewModelBase<OrderForMovDocJournalFilterViewModel>
	{
		private bool isOnlineStoreOrders;
		[Display(Name = "Заказы ИМ")]
		public bool IsOnlineStoreOrders {
			get => isOnlineStoreOrders;
			set { UpdateFilterField(ref isOnlineStoreOrders, value, () => IsOnlineStoreOrders); }
		}

		private IEnumerable<OrderStatus> orderStatuses;
		[Display(Name = "Статусы заказов")]
		public IEnumerable<OrderStatus> OrderStatuses {
			get => orderStatuses;
			set { SetField(ref orderStatuses, value, () => OrderStatuses); }
		}

		private DateTime? startDate;
		public virtual DateTime? StartDate {
			get => startDate;
			set { UpdateFilterField(ref startDate, value, () => StartDate); }
		}

		private DateTime? endDate;
		public virtual DateTime? EndDate {
			get => endDate;
			set { UpdateFilterField(ref endDate, value, () => EndDate);}
		}
	}
}
