using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.DomainModel.UoW;
using QS.Views.GtkUI;
using QSProjectsLib;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Cash.Transfer;

namespace Vodovoz.Cash.Transfer
{
	[ToolboxItem(true)]
	public partial class IncomeCashTransferView : TabViewBase<IncomeCashTransferDocumentViewModel>
	{
		public IncomeCashTransferView(IncomeCashTransferDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ConfigureBindings();
			ConfigureTreeViews();
			ConfigureUpdates();
			InitSensitivity();
		}

		private void ConfigureBindings()
		{
			ytreeviewIncomes.Selection.Changed += ytreeviewIncomesSelection_Changed;
			ytreeviewExpenses.Selection.Changed += ytreeviewExpensesSelection_Changed;

			ylabelCreationDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.CreationDate.ToString("g"), w => w.LabelProp).InitializeFromSource();
			ylabelAuthor.Binding.AddFuncBinding(ViewModel.Entity, e => e.Author != null ? e.Author.GetPersonNameWithInitials() : "", w => w.LabelProp).InitializeFromSource();
			ylabelStatus.Binding.AddFuncBinding(ViewModel.Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelSum.Binding.AddFuncBinding(ViewModel.Entity, e => e.TransferedSum.ToShortCurrencyString(), w => w.LabelProp).InitializeFromSource();

			evmeDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject).InitializeFromSource();
			evmeDriver.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(vm => vm.EmployeeAutocompleteSelectorFactory, w => w.EntitySelectorAutocompleteFactory)
				.InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;

			comboboxCashSubdivisionFrom.SetRenderTextFunc<Subdivision>(s => s.Name);
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel, vm => vm.SubdivisionsFrom, w => w.ItemsList).InitializeFromSource();
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel.Entity, e => e.CashSubdivisionFrom, w => w.SelectedItem).InitializeFromSource();

			comboboxCashSubdivisionTo.SetRenderTextFunc<Subdivision>(s => s.Name);
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel, vm => vm.SubdivisionsTo, w => w.ItemsList).InitializeFromSource();
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel.Entity, e => e.CashSubdivisionTo, w => w.SelectedItem).InitializeFromSource();

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			entryIncomeFinancialCategory.ViewModel = ViewModel.FinancialIncomeCategoryViewModel;

			ylabelCashierSender.Binding.AddFuncBinding(ViewModel.Entity, e => e.CashierSender != null ? e.CashierSender.GetPersonNameWithInitials() : "", w => w.LabelProp).InitializeFromSource();
			ylabelCashierReceiver.Binding.AddFuncBinding(ViewModel.Entity, e => e.CashierReceiver != null ? e.CashierReceiver.GetPersonNameWithInitials() : "", w => w.LabelProp).InitializeFromSource();
			ylabelSendTime.Binding.AddFuncBinding(ViewModel.Entity, e => e.SendTime.HasValue ? e.SendTime.Value.ToString("g") : "", w => w.LabelProp).InitializeFromSource();
			ylabelReceiveTime.Binding.AddFuncBinding(ViewModel.Entity, e => e.ReceiveTime.HasValue ? e.ReceiveTime.Value.ToString("g") : "", w => w.LabelProp).InitializeFromSource();

			ylabelIncomesSummary.Binding.AddFuncBinding(ViewModel.Entity, e => e.IncomesSummary.ToShortCurrencyString(), w => w.LabelProp).InitializeFromSource();
			ylabelExpensesSummary.Binding.AddFuncBinding(ViewModel.Entity, e => e.ExpensesSummary.ToShortCurrencyString(), w => w.LabelProp).InitializeFromSource();

			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ViewModel.DeleteIncomesCommand.CanExecuteChanged += (sender, e) => {
				buttonDeleteSelectedIncomes.Sensitive = GetButtonDeleteIncomesSensitivity();
			};

			ViewModel.DeleteExpensesCommand.CanExecuteChanged += (sender, e) => {
				buttonDeleteSelectedExpenses.Sensitive = GetButtonDeleteExpensesSensitivity();
			};

			ViewModel.SendCommand.CanExecuteChanged += (sender, e) => {
				buttonSend.Sensitive = ViewModel.SendCommand.CanExecute();
			};

			ViewModel.ReceiveCommand.CanExecuteChanged += (sender, e) => {
				buttonReceive.Sensitive = ViewModel.ReceiveCommand.CanExecute();
			};

			ViewModel.AddIncomesCommand.CanExecuteChanged += (sender, e) => {
				buttonAddIncomes.Sensitive = ViewModel.AddIncomesCommand.CanExecute();
			};

			ViewModel.AddExpensesCommand.CanExecuteChanged += (sender, e) => {
				buttonAddExpenses.Sensitive = ViewModel.AddExpensesCommand.CanExecute();
			};

			buttonPrint.Sensitive = ViewModel.PrintCommand.CanExecute();

			buttonSave.Clicked += (s, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (s, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}

		private void ConfigureTreeViews()
		{
			ytreeviewIncomes.ColumnsConfig = FluentColumnsConfig<IncomeCashTransferedItem>.Create()
				.AddColumn("Ордер").AddTextRenderer(x => x.Income.Title)
				.AddColumn("Сумма").AddTextRenderer(x => x.IncomeMoney.ToShortCurrencyString())
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment).AddSetter((cell, node) => cell.Editable = ViewModel.CanEdit)
				.RowCells().AddSetter<CellRenderer>((cell, node) => {
					if(node.Income?.RouteListClosing != null && node.Income.RouteListClosing.Status == RouteListStatus.Closed) {
						cell.CellBackgroundGdk = GdkColors.SuccessText;
					} else {
						cell.CellBackgroundGdk = GdkColors.PrimaryBase;
					}
				})
				.Finish();
			ytreeviewIncomes.ItemsDataSource = ViewModel.Entity.ObservableCashTransferDocumentIncomeItems;
			ytreeviewIncomes.RowActivated += YtreeviewIncomes_RowActivated;

			ytreeviewExpenses.ColumnsConfig = FluentColumnsConfig<ExpenseCashTransferedItem>.Create()
				.AddColumn("Ордер").AddTextRenderer(x => x.Expense.Title)
				.AddColumn("Сумма").AddTextRenderer(x => x.ExpenseMoney.ToShortCurrencyString())
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment).AddSetter((cell, node) => cell.Editable = ViewModel.CanEdit)
				.Finish();
			ytreeviewExpenses.ItemsDataSource = ViewModel.Entity.ObservableCashTransferDocumentExpenseItems;
		}

		private void ConfigureUpdates()
		{
			ytreeviewIncomes.Selection.Changed += (sender, e) => {
				buttonDeleteSelectedIncomes.Sensitive = GetButtonDeleteIncomesSensitivity();
			};

			ytreeviewExpenses.Selection.Changed += (sender, e) => {
				buttonDeleteSelectedExpenses.Sensitive = GetButtonDeleteExpensesSensitivity();
			};
		}

		private bool GetButtonDeleteIncomesSensitivity()
		{
			return ViewModel.DeleteIncomesCommand.CanExecute(ytreeviewIncomes.GetSelectedObjects<IncomeCashTransferedItem>());
		}

		private bool GetButtonDeleteExpensesSensitivity()
		{
			return ViewModel.DeleteExpensesCommand.CanExecute(ytreeviewExpenses.GetSelectedObjects<ExpenseCashTransferedItem>());
		}

		private void InitSensitivity()
		{
			buttonDeleteSelectedIncomes.Sensitive = GetButtonDeleteIncomesSensitivity();
			buttonDeleteSelectedExpenses.Sensitive = GetButtonDeleteExpensesSensitivity();
			buttonSend.Sensitive = ViewModel.SendCommand.CanExecute();
			buttonReceive.Sensitive = ViewModel.ReceiveCommand.CanExecute();
			buttonAddIncomes.Sensitive = ViewModel.AddIncomesCommand.CanExecute();
			buttonAddExpenses.Sensitive = ViewModel.AddExpensesCommand.CanExecute();
			comboboxCashSubdivisionFrom.Sensitive = ViewModel.CanEdit;
			comboboxCashSubdivisionTo.Sensitive = ViewModel.CanEdit;
		}

		void YtreeviewIncomes_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			var selectedIncomeItem = ytreeviewIncomes.GetSelectedObject<IncomeCashTransferedItem>();
			ViewModel.OpenRouteListCommand.Execute(selectedIncomeItem.Income);
		}

		void ytreeviewExpensesSelection_Changed(object sender, EventArgs e)
		{
			ViewModel.ExpensesSelected = ytreeviewExpenses.GetSelectedObjects().Any();
		}

		void ytreeviewIncomesSelection_Changed(object sender, EventArgs e)
		{
			ViewModel.IncomesSelected = ytreeviewIncomes.GetSelectedObjects().Any();
		}

		public bool HasChanges => ViewModel.HasChanges;

		public IUnitOfWork UoW => ViewModel.UoW;

		protected void OnButtonSendClicked(object sender, EventArgs e)
		{
			ViewModel.SendCommand.Execute();
		}

		protected void OnButtonReceiveClicked(object sender, EventArgs e)
		{
			ViewModel.ReceiveCommand.Execute();
		}

		protected void OnButtonAddIncomesClicked(object sender, EventArgs e)
		{
			ViewModel.AddIncomesCommand.Execute();
		}

		protected void OnButtonAddExpensesClicked(object sender, EventArgs e)
		{
			ViewModel.AddExpensesCommand.Execute();
		}

		protected void OnButtonDeleteSelectedIncomesClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewIncomes.GetSelectedObjects<IncomeCashTransferedItem>();
			ViewModel.DeleteIncomesCommand.Execute(selected);
		}

		protected void OnButtonDeleteSelectedExpensesClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewExpenses.GetSelectedObjects<ExpenseCashTransferedItem>();
			ViewModel.DeleteExpensesCommand.Execute(selected);
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintCommand.Execute();
		}
	}
}
