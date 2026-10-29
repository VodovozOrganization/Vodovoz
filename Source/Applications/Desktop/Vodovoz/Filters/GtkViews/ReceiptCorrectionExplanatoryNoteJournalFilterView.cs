using Gtk;
using QS.Views.GtkUI;
using QSWidgetLib;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Receipts;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ReceiptCorrectionExplanatoryNoteJournalFilterView : FilterViewBase<ReceiptCorrectionExplanatoryNoteJournalFilterViewModel>
	{
		public ReceiptCorrectionExplanatoryNoteJournalFilterView(ReceiptCorrectionExplanatoryNoteJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryOrder.KeyReleaseEvent += OnKeyReleased;
			yentryOrder.ValidationMode = ValidationType.numeric;
			yentryOrder.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			speciallistCmbOrganisations.ShowSpecialStateAll = true;
			speciallistCmbOrganisations.ItemsList = ViewModel.Organizations;
			speciallistCmbOrganisations.Binding
				.AddBinding(ViewModel, vm => vm.Organization, w => w.SelectedItem)
				.InitializeFromSource();

			dateRangeFilter.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DateFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.DateTo, w => w.EndDateOrNull)
				.InitializeFromSource();
		}

		private void OnKeyReleased(object sender, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}

		public override void Dispose()
		{
			yentryOrder.KeyReleaseEvent -= OnKeyReleased;
			base.Dispose();
		}
	}
}
