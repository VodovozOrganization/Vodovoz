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
	public partial class CommonCashTransferDlg : TdiTabBase, IViewModelBasedDialog<CommonCashTransferDocumentViewModel, CommonCashTransferDocument>
	{
		private bool tabClosed = false;

		public CommonCashTransferDocumentViewModel ViewModel { get; set; }

		public CommonCashTransferDlg(CommonCashTransferDocumentViewModel viewModel)
		{
			this.Build();
			this.ViewModel = viewModel;
			viewModel.EntitySaved += ViewModel_EntitySaved;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ConfigureBindings();
			InitSensitivity();
		}

		private void ConfigureBindings()
		{
			ylabelCreationDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.CreationDate.ToString("g"), w => w.LabelProp).InitializeFromSource();
			ylabelAuthor.Binding.AddFuncBinding(ViewModel.Entity, e => e.Author != null ? e.Author.GetPersonNameWithInitials() : "", w => w.LabelProp).InitializeFromSource();
			ylabelStatus.Binding.AddFuncBinding(ViewModel.Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			yspinMoney.Binding.AddBinding(ViewModel.Entity, e => e.TransferedSum, w => w.ValueAsDecimal).InitializeFromSource();
			yspinMoney.Binding.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();

			var filterDriver = new EmployeeFilter(ViewModel.UoW);
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
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

			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ViewModel.SendCommand.CanExecuteChanged += (sender, e) => {
				buttonSend.Sensitive = ViewModel.SendCommand.CanExecute();
			};

			ViewModel.ReceiveCommand.CanExecuteChanged += (sender, e) => {
				buttonReceive.Sensitive = ViewModel.ReceiveCommand.CanExecute();
			};

			buttonPrint.Sensitive = ViewModel.PrintCommand.CanExecute();
		}

		private void InitSensitivity()
		{
			buttonSend.Sensitive = ViewModel.SendCommand.CanExecute();
			buttonReceive.Sensitive = ViewModel.ReceiveCommand.CanExecute();
			comboboxCashSubdivisionFrom.Sensitive = ViewModel.CanEdit;
			comboboxCashSubdivisionTo.Sensitive = ViewModel.CanEdit;
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

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintCommand.Execute();
		}
	}
}
