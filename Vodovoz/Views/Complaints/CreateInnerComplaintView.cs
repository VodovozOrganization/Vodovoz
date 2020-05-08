using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;
namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CreateInnerComplaintView : TabViewBase<CreateInnerComplaintViewModel>
	{
		public CreateInnerComplaintView(CreateInnerComplaintViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytextviewComplaintText.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComplaintText.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			spLstComplaintKind.SetRenderTextFunc<ComplaintKind>(k => k.GetFullName);
			spLstComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKindSource, w => w.ItemsList).InitializeFromSource();
			spLstComplaintKind.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintKind, w => w.SelectedItem).InitializeFromSource();


			guiltyitemsview.ViewModel = ViewModel.GuiltyItemsViewModel;

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false, QS.Navigation.CloseSource.Cancel); };
		}
	}
}
