using Gamma.Binding.Converters;
using Gamma.Widgets.Additions;
using QS.Navigation;
using QS.Views.GtkUI;
using System;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	public partial class ProductGroupView : TabViewBase<ProductGroupViewModel>
	{
		public ProductGroupView(ProductGroupViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yentryOnlineStoreGuid.Binding.AddBinding(
				ViewModel.Entity, e => e.OnlineStoreGuid, w => w.Text, new GuidToStringConverter()).InitializeFromSource();

			ycheckExportToOnlineStore.Binding.AddBinding(ViewModel.Entity, e => e.ExportToOnlineStore, w => w.Active).InitializeFromSource();

			ycheckArchived.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			ycheckArchived.Toggled += (sender, args) => ViewModel.SetArchiveCommand.Execute();

			entityParent.SetEntityAutocompleteSelectorFactory(ViewModel.ProductGroupSelectorFactory);
			entityParent.Binding.AddBinding(ViewModel.Entity, e => e.Parent, w => w.Subject).InitializeFromSource();

			checklistCharacteristics.EnumType = typeof(NomenclatureProperties);
			checklistCharacteristics.Binding.AddBinding(
				ViewModel.Entity, e => e.Characteristics, w => w.SelectedValuesList, new EnumsListConverter<NomenclatureProperties>()).InitializeFromSource();

			ylblOnlineStore.Text = ViewModel.Entity.OnlineStore?.Name;
			ylblOnlineStore.Visible = !String.IsNullOrWhiteSpace(ViewModel.Entity.OnlineStore?.Name);
			ylblOnlineStoreStr.Visible = !String.IsNullOrWhiteSpace(ViewModel.Entity.OnlineStore?.Name);

			vbox2.Sensitive = ViewModel.CanEditOnlineStore;

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}
	}
}
