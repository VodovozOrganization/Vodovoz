using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class NomenclatureOnlineGroupView : TabViewBase<NomenclatureOnlineGroupViewModel>
	{
		public NomenclatureOnlineGroupView(NomenclatureOnlineGroupViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.Clicked += OnSaveClicked;
			btnCancel.Clicked += OnCancelClicked;

			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();
			lblId.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.IdString, w => w.LabelProp)
				.InitializeFromSource();

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		private void OnCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(false, QS.Navigation.CloseSource.Cancel);
		}
	}
}
