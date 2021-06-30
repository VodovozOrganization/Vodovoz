using Vodovoz.Domain.Fuel;
using Gtk;
using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Dialogs.Fuel;

namespace Vodovoz.Dialogs.Fuel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FuelIncomeInvoiceView : TabViewBase<FuelIncomeInvoiceViewModel>
	{
		public FuelIncomeInvoiceView(FuelIncomeInvoiceViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureTreeView()
		{
			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<FuelIncomeInvoiceItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.OfficialName)
				.AddColumn("Тип топлива").AddTextRenderer(x => x.Nomenclature.FuelType.Name)
				.AddColumn("Цена в номенклатуре").AddNumericRenderer(x => x.Nomenclature.FuelType.Cost)
				.AddColumn("Цена закупки").AddNumericRenderer(x => x.Price)
					.Adjustment(new Adjustment(0, 0, 100000000, 1, 10, 10))
					.AddSetter((cell, node) => cell.Editable = ViewModel.CanEdit)
				.AddColumn("Количество").AddNumericRenderer(x => x.Liters).Editing()
					.AddSetter((cell, node) => cell.Editable = ViewModel.CanEdit)
					.AddSetter((cell, node) => cell.Adjustment = new Adjustment(0, (double)ViewModel.GetMinimumAvailableLiters(node), 10000000, 1, 100, 0))
				.AddColumn("Сумма").AddNumericRenderer(x => x.TotalSum)
				.Finish();
			ytreeviewItems.ItemsDataSource = ViewModel.Entity.ObservableFuelIncomeInvoiceItems;
			ytreeviewItems.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
		}

		private void ConfigureDlg()
		{
			fuelbalanceview.ViewModel = ViewModel.FuelBalanceViewModel;

			ylabelCreationDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.СreationTime.ToShortDateString(), w => w.LabelProp).InitializeFromSource();
			ylabelAuthor.Binding.AddFuncBinding(ViewModel.Entity, e => e.Author.GetPersonNameWithInitials(), w => w.LabelProp).InitializeFromSource();

			yentryInvoiceDoc.Binding.AddBinding(ViewModel.Entity, e => e.InvoiceDoc, w => w.Text).InitializeFromSource();
			yentryInvoiceDoc.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			yentryInvoiceBillDoc.Binding.AddBinding(ViewModel.Entity, e => e.InvoiceBillDoc, w => w.Text).InitializeFromSource();
			yentryInvoiceBillDoc.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.Counterparty, w => w.Subject).InitializeFromSource();
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ycomboboxCashSubdivision.SetRenderTextFunc<Subdivision>(s => s.Name);
			ycomboboxCashSubdivision.Binding.AddBinding(ViewModel, vm => vm.AvailableSubdivisions, w => w.ItemsList).InitializeFromSource();
			ycomboboxCashSubdivision.Binding.AddBinding(ViewModel.Entity, e => e.Subdivision, w => w.SelectedItem).InitializeFromSource();
			ycomboboxCashSubdivision.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAdd.Binding.AddBinding(ViewModel, vm => vm.CanAddItems, w => w.Sensitive).InitializeFromSource();
			ybuttonAdd.Clicked += (sender, e) => ViewModel.AddItemCommand.Execute();

			ybuttonDelete.Binding.AddBinding(ViewModel, vm => vm.CanDeleteItems, w => w.Sensitive).InitializeFromSource();
			ybuttonDelete.Clicked += (sender, e) => ViewModel.DeleteItemCommand.Execute();

			ytreeviewItems.Selection.Changed += (sender, e) => { ViewModel.SelectedItem = ytreeviewItems.GetSelectedObject() as FuelIncomeInvoiceItem; };

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			ConfigureTreeView();
		}
	}
}
