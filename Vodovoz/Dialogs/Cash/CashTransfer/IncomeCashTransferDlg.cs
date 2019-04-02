using System;
using QS.Dialog.Gtk;
using Vodovoz.Domain.Cash.CashTransfer;
using QS.Tdi;
using Gamma.Utilities;
using QSProjectsLib;
using System.Linq;
using Gamma.ColumnConfig;
using Vodovoz.ViewModelBased;
using Vodovoz.Domain.Cash;
using Gtk;
using Vodovoz.Domain.Logistic;
using QS.RepresentationModel.GtkUI;
using Vodovoz.ViewModel;
using Vodovoz.Domain.Employees;
using NHibernate.Criterion;

namespace Vodovoz.Dialogs.Cash.CashTransfer
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeCashTransferDlg : TdiTabBase, IViewModelBasedDialog<IncomeCashTransferDocumentViewModel, IncomeCashTransferDocument>
	{
		private bool tabClosed = false;

		public IncomeCashTransferDocumentViewModel ViewModel { get; set; }

		public IncomeCashTransferDlg(IncomeCashTransferDocumentViewModel viewModel)
		{
			this.Build();
			this.ViewModel = viewModel;
			viewModel.EntitySaved += ViewModel_EntitySaved;
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

			var filterDriver = new EmployeeFilter(ViewModel.UoW);
			filterDriver.SetAndRefilterAtOnce(
				x => x.ShowFired = false
			);
			entryDriver.RepresentationModel = new EmployeesVM(filterDriver);
			entryDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject).InitializeFromSource();
			entryDriver.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			var carVM = new EntityCommonRepresentationModelConstructor<Car>(ViewModel.UoW)
				.AddColumn("Название", x => x.Title).AddSearch(x => x.Title)
				.AddColumn("Номер", x => x.RegistrationNumber).AddSearch(x => x.RegistrationNumber)
				.SetFixedRestriction(Restrictions.Where<Car>(x => !x.IsArchive))
				.Finish();
			entryCar.RepresentationModel = carVM;
			entryCar.Binding.AddBinding(ViewModel.Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entryCar.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			comboboxCashSubdivisionFrom.SetRenderTextFunc<Subdivision>(s => s.Name);
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel, vm => vm.SubdivisionsFrom, w => w.ItemsList).InitializeFromSource();
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel.Entity, e => e.CashSubdivisionFrom, w => w.SelectedItem).InitializeFromSource();

			comboboxCashSubdivisionTo.SetRenderTextFunc<Subdivision>(s => s.Name);
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel, vm => vm.SubdivisionsTo, w => w.ItemsList).InitializeFromSource();
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel.Entity, e => e.CashSubdivisionTo, w => w.SelectedItem).InitializeFromSource();

			comboExpenseCategory.SetRenderTextFunc<ExpenseCategory>(s => s.Name);
			comboExpenseCategory.Binding.AddBinding(ViewModel, vm => vm.ExpenseCategories, w => w.ItemsList).InitializeFromSource();
			comboExpenseCategory.Binding.AddBinding(ViewModel.Entity, e => e.ExpenseCategory, w => w.SelectedItem).InitializeFromSource();
			comboExpenseCategory.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			comboIncomeCategory.SetRenderTextFunc<IncomeCategory>(s => s.Name);
			comboIncomeCategory.Binding.AddBinding(ViewModel, vm => vm.IncomeCategories, w => w.ItemsList).InitializeFromSource();
			comboIncomeCategory.Binding.AddBinding(ViewModel.Entity, e => e.IncomeCategory, w => w.SelectedItem).InitializeFromSource();
			comboIncomeCategory.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

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
		}

		private void ConfigureTreeViews()
		{
			ytreeviewIncomes.ColumnsConfig = FluentColumnsConfig<IncomeCashTransferedItem>.Create()
				.AddColumn("Ордер").AddTextRenderer(x => x.Income.Title)
				.AddColumn("Сумма").AddTextRenderer(x => x.IncomeMoney.ToShortCurrencyString())
				.AddColumn("Комментарий").AddTextRenderer(x => x.Comment).AddSetter((cell, node) => cell.Editable = ViewModel.CanEdit)
				.RowCells().AddSetter<CellRenderer>((cell, node) => {
					if(node.Income != null && node.Income.RouteListClosing.Status == RouteListStatus.Closed) {
						cell.CellBackgroundGdk = new Gdk.Color(84, 158, 91);
					} else {
						cell.CellBackgroundGdk = new Gdk.Color(255, 255, 255);
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

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		void ViewModel_EntitySaved(object sender, EventArgs e)
		{
			EntitySaved?.Invoke(this, new EntitySavedEventArgs(ViewModel.RootEntity, tabClosed));
		}

		public bool Save()
		{
			return ViewModel.Save();
		}

		public override void Destroy()
		{
			ViewModel.Dispose();
			base.Destroy();
		}

		public void SaveAndClose()
		{
			tabClosed = true;
			if(Save()) {
				OnCloseTab(false);
			}
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			SaveAndClose();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(true);
		}

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
