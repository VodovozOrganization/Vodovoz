using System;
using Gamma.Binding.Converters;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class NomenclatureOnlineCatalogView : TabViewBase<NomenclatureOnlineCatalogViewModel>
	{
		public NomenclatureOnlineCatalogView(NomenclatureOnlineCatalogViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			const int entryWidth = 350;
			
			btnSave.Clicked += OnSaveClicked;
			btnCancel.Clicked += OnCancelClicked;

			lblIdTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();
			lblId.Binding
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.IdString, w => w.LabelProp)
				.InitializeFromSource();

			entryName.WidthRequest = entryWidth;
			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();

			entryExternalId.WidthRequest = entryWidth;
			entryExternalId.Binding
				.AddBinding(ViewModel.Entity, vm => vm.ExternalId, w => w.Text, new GuidToStringConverter())
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
