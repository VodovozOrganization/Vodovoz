using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	[ToolboxItem(true)]
	public partial class ProfitCategoryView : ViewBase<ProfitCategoryViewModel>
	{
		public ProfitCategoryView(ProfitCategoryViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
			
			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();
			
			yChkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
