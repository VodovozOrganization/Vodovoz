using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryScheduleFilterView : FilterViewBase<DeliveryScheduleFilterViewModel>
	{
		public DeliveryScheduleFilterView(DeliveryScheduleFilterViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ycheckbuttonIsNotArchive.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsNotArchive, w => w.Active)
				.AddBinding(vm => vm.CanChangeIsNotArchive, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
