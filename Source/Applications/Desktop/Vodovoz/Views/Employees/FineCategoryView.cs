using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Employees;
namespace Vodovoz.Views.Employees
{
	[ToolboxItem(true)]
	public partial class FineCategoryView : TabViewBase<FineCategoryViewModel>
	{
		public FineCategoryView(FineCategoryViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryFineCategoryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonIsArchieve.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				//.AddBinding(ViewModel, vm => !vm.IsNew, w => w.Visible)
				.InitializeFromSource();

			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();

			buttonCancel.Clicked += (sender, e) => ViewModel.Close(ViewModel.AskSaveOnClose, QS.Navigation.CloseSource.Cancel);

			btnCopyEntityId.Binding
				//.AddBinding(ViewModel, vm => !vm.IsNew, w => w.Visible)
				.InitializeFromSource();

			btnCopyEntityId.Clicked += (sender, e) => OnBtnCopyEntityIdClicked(sender, e);
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = ViewModel.Entity.Id.ToString();
			}
		}
	}
}
