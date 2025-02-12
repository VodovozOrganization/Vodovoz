using System;
using Gamma.Widgets.Additions;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderForMovDocFilterView : FilterViewBase<OrderForMovDocJournalFilterViewModel>
	{
		public OrderForMovDocFilterView(OrderForMovDocJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ycheckIsOnlineStore.Active = true;
			ycheckIsOnlineStore.Binding.AddBinding(ViewModel, vm => vm.IsOnlineStoreOrders, w => w.Active);

			daterangepickerOrderCreateDate.Binding.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull).InitializeFromSource();
			daterangepickerOrderCreateDate.Binding.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull).InitializeFromSource();

			enumchecklistStatuses.EnumType = typeof(OrderStatus);
			enumchecklistStatuses.Binding.AddBinding(
				ViewModel, vm => vm.OrderStatuses, w => w.SelectedValuesList, new EnumsListConverter<OrderStatus>()).InitializeFromSource();
		}
	}
}
