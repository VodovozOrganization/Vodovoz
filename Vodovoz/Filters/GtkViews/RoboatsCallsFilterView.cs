using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Roboats;
using Vodovoz.ViewModels.Journals.FilterViewModels.Roboats;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RoboatsCallsFilterView : FilterViewBase<RoboatsCallsFilterViewModel>
	{
		public RoboatsCallsFilterView(RoboatsCallsFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			enumcomboStatus.ShowSpecialStateAll = true;
			enumcomboStatus.ItemsEnum = typeof(RoboatsCallStatus);
			enumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeStatus, w => w.Sensitive)
				.AddBinding(vm => vm.Status, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			dateperiodOrders.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.AddFuncBinding(vm => vm.CanChangeStartDate && vm.CanChangeEndDate, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
