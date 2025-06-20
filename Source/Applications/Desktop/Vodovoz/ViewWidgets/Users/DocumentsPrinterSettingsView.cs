using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core.Domain.PrintableDocuments;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.ViewModels.Widgets.Print;
namespace Vodovoz.ViewWidgets.Users
{
	[ToolboxItem(true)]
	public partial class DocumentsPrinterSettingsView : WidgetViewBase<DocumentsPrinterSettingsViewModel>
	{
		public DocumentsPrinterSettingsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			ConfigureTreeView();

			yenumcomboboxDocumentType.ItemsEnum = typeof(CustomPrintDocumentType);
			yenumcomboboxDocumentType.Binding
				.AddBinding(ViewModel, w => w.SelectedDocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			UpdateAvailableDocumentTypes();

			ybuttonAddToList.Binding
				.AddBinding(ViewModel, vm => vm.CanAddPrinterSetting, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonConfigurePrinter.Binding
				.AddBinding(ViewModel, vm => vm.CanConfigurePrinterSetting, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonRemoveFromList.Binding
				.AddBinding(ViewModel, vm => vm.CanRemovePrinterSetting, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonAddToList.BindCommand(ViewModel.AddPrinterSettingCommand);
			ybuttonConfigurePrinter.BindCommand(ViewModel.ConfigurePrinterSettingCommand);
			ybuttonRemoveFromList.BindCommand(ViewModel.RemovePrinterSettingCommand);

			ViewModel.ObservablePrinterSettings.CollectionChanged += OnPrinterSettingAdded;
		}

		private void ConfigureTreeView()
		{
			ytreeviewPrinterSettings.ColumnsConfig = FluentColumnsConfig<DocumentPrinterSetting>.Create()
				.AddColumn("Документ").HeaderAlignment(0.5f).AddEnumRenderer(x => x.DocumentType).XAlign(0f)
				.AddColumn("Копий").HeaderAlignment(0.5f).AddTextRenderer(x => x.NumberOfCopies.ToString()).XAlign(0.5f)
				.AddColumn("Принтер").HeaderAlignment(0.5f).AddTextRenderer(x => x.PrinterName).XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeviewPrinterSettings.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ObservablePrinterSettings, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedPrinterSetting, w => w.SelectedRow)
				.InitializeFromSource();
		}

		private void UpdateAvailableDocumentTypes()
		{
			var existingDocumentTypes = ViewModel.ObservablePrinterSettings
				.Select(s => s.DocumentType)
				.Cast<object>()
				.ToArray();

			yenumcomboboxDocumentType.ClearEnumHideList();
			yenumcomboboxDocumentType.AddEnumToHideList(existingDocumentTypes);
		}

		private void OnPrinterSettingAdded(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdateAvailableDocumentTypes();
		}

		public override void Destroy()
		{
			ViewModel.ObservablePrinterSettings.CollectionChanged -= OnPrinterSettingAdded;

			base.Destroy();
		}
	}
}
