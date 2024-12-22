using Gamma.Binding.Converters;
using Gamma.Widgets.Additions;
using QS.Navigation;
using QS.Views.GtkUI;
using System;
using Vodovoz.Core.Domain.Goods;
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
			hbox1.Sensitive = ViewModel.CanEdit;
			buttonSave.Sensitive = ViewModel.CanEdit;
			vbox1.Sensitive = ViewModel.CanEditOnlineStoreParametersInProductGroup;

			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();

			yentryOnlineStoreGuid.Binding
				.AddBinding(ViewModel.Entity, e => e.OnlineStoreGuid, w => w.Text, new GuidToStringConverter())
				.AddBinding(ViewModel, vm => vm.CanEditOnlineStoreParametersInProductGroup, w => w.Sensitive)
				.InitializeFromSource();

			ycheckExportToOnlineStore.Binding
				.AddBinding(ViewModel.Entity, e => e.ExportToOnlineStore, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditOnlineStoreParametersInProductGroup, w => w.Sensitive)
				.InitializeFromSource();

			ycheckArchived.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			ycheckArchived.Toggled += OnCheckArchivedToggled;

			ycheckbuttonIsHighlightInCarLoadDocument.Binding
				.AddBinding(ViewModel.Entity, e => e.IsHighlightInCarLoadDocument, w => w.Active)
				.InitializeFromSource();
			ycheckbuttonIsHighlightInCarLoadDocument.Toggled += OnCheckbuttonIsHighlightInCarLoadDocumentToggled;

			ycheckbuttonIsNeedAdditionalControl.Binding
				.AddBinding(ViewModel.Entity, e => e.IsNeedAdditionalControl, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditAdditionalControlSettingsInProductGroup, w => w.Sensitive)
				.InitializeFromSource();
			ycheckbuttonIsNeedAdditionalControl.Toggled += OnCheckbuttonIsNeedAdditionalControlToggled;

			checklistCharacteristics.EnumType = typeof(NomenclatureProperties);
			checklistCharacteristics.Binding.AddBinding(
				ViewModel.Entity, e => e.Characteristics, w => w.SelectedValuesList, new EnumsListConverter<NomenclatureProperties>()).InitializeFromSource();

			ylblOnlineStore.Text = ViewModel.Entity.OnlineStore?.Name;
			ylblOnlineStore.Visible = !string.IsNullOrWhiteSpace(ViewModel.Entity.OnlineStore?.Name);
			ylblOnlineStoreStr.Visible = !string.IsNullOrWhiteSpace(ViewModel.Entity.OnlineStore?.Name);

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);

			entityentryParentGroup.ViewModel = ViewModel.ProductGroupEntityEntryViewModel;
		}

		private void OnCheckArchivedToggled(object sender, EventArgs e)
		{
			ViewModel.SetArchiveCommand.Execute();
		}

		private void OnCheckbuttonIsHighlightInCarLoadDocumentToggled(object sender, EventArgs e)
		{
			ViewModel.SetIsHighlightInCarLoadDocumentCommand.Execute();
		}

		private void OnCheckbuttonIsNeedAdditionalControlToggled(object sender, EventArgs e)
		{
			ViewModel.SetIsNeedAdditionalControlCommand.Execute();
		}

		public override void Destroy()
		{
			ycheckArchived.Toggled -= OnCheckArchivedToggled;
			ycheckbuttonIsHighlightInCarLoadDocument.Toggled -= OnCheckbuttonIsHighlightInCarLoadDocumentToggled;
			ycheckbuttonIsNeedAdditionalControl.Toggled -= OnCheckbuttonIsNeedAdditionalControlToggled;

			base.Destroy();
		}
	}
}
