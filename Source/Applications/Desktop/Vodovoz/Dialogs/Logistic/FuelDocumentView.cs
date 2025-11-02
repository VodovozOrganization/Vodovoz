using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.FuelDocuments;

namespace Vodovoz
{
	public partial class FuelDocumentView : TabViewBase<FuelDocumentViewModel>
	{
		public FuelDocumentView(FuelDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yspeccomboboxSubdivision.Sensitive = ViewModel.FuelDocument.FuelExpenseOperation == null && ViewModel.IsNewEditable;
			yspeccomboboxSubdivision.SetRenderTextFunc<Subdivision>(s => s.Name);

			yspeccomboboxSubdivision.Binding
				.AddBinding(ViewModel, vm => vm.AvailableSubdivisionsForUser, e => e.ItemsList)
				.AddBinding(ViewModel.FuelDocument, w => w.Subdivision, e => e.SelectedItem)
				.InitializeFromSource();

			ydatepicker.Binding
				.AddBinding(ViewModel.FuelDocument, e => e.Date, w => w.Date)
				.AddBinding(ViewModel, vm => vm.CanChangeDate, w => w.Sensitive)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeAutocompleteSelector);
			evmeDriver.Binding.AddBinding(ViewModel.FuelDocument, e => e.Driver, w => w.Subject).InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;

			entityentryFuelType.ViewModel = ViewModel.FuelTypeEntryViewModel;

			yspinFuelLimitsLiters.Binding
				.AddBinding(ViewModel.FuelDocument, e => e.FuelLimitLitersAmount, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ycheckbuttonOnlyDocumentsCreation.Binding
				.AddBinding(ViewModel, vm => vm.IsOnlyDocumentsCreation, w => w.Active)
				.InitializeFromSource();

			SetTransactionsCountRange();
			yspinTransactionsCount.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.FuelLimitTransactionsCount, w => w.ValueAsInt)
				.AddFuncBinding(vm => vm.IsFuelLimitsCanBeEdited, w => w.Sensitive)
				.InitializeFromSource();

			yhboxGivedLimits.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsFuelLimitsCanBeEdited, w => w.Sensitive)
				.InitializeFromSource();

			labelResultInfo.Binding.AddBinding(ViewModel, vm => vm.ResultInfo, w => w.Text).InitializeFromSource();

			labelExpenseInfo.Binding.AddBinding(ViewModel, vm => vm.CashExpenseInfo, w => w.Text).InitializeFromSource();

			ytextviewFuelInfo.Binding.AddBinding(ViewModel, vm => vm.FuelInfo, w => w.Buffer.Text).InitializeFromSource();

			yenumcomboboxPaymentType.ItemsEnum = typeof(FuelPaymentType);
			yenumcomboboxPaymentType.Binding.AddBinding(ViewModel.FuelDocument, e => e.FuelPaymentType, w => w.SelectedItem).InitializeFromSource();

			buttonSave.Binding.AddBinding(ViewModel, vm => vm.IsDocumentCanBeSaved, w => w.Sensitive).InitializeFromSource();
			buttonOpenExpense.Binding.AddBinding(ViewModel, vm => vm.CanOpenExpense, w => w.Sensitive).InitializeFromSource();

			yenumcomboboxPaymentType.Binding.AddBinding(ViewModel, vm => vm.IsNewEditable, w => w.Sensitive).InitializeFromSource();

			spinFuelPrice.Binding
				.AddBinding(ViewModel, vm => vm.IsGiveFuelInMoneySelected, w => w.Sensitive)
				.AddBinding(ViewModel.FuelDocument, e => e.LiterCost, w => w.ValueAsDecimal)
				.InitializeFromSource();

			disablespinMoney.Binding.AddBinding(ViewModel.FuelDocument, e => e.PayedForFuel, w => w.ValueAsDecimal).InitializeFromSource();
			disablespinMoney.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsGiveFuelInMoneySelected, w => w.Active)
				.AddFuncBinding(vm => vm.IsFuelInMoneyCanBeEdited, w => w.Sensitive)
				.InitializeFromSource();

			disablespinMoney.ActiveChanged += (s, e) => ViewModel.SetFuelDocumentTodayDateIfNeedCommand.Execute();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.FuelLimitTransactionsCountMaxValue))
			{
				SetTransactionsCountRange();
			}
		}

		private void SetTransactionsCountRange()
		{
			yspinTransactionsCount.SetRange(1, ViewModel.FuelLimitTransactionsCountMaxValue);
		}

		protected void OnDisablespinMoneyValueChanged(object sender, EventArgs e)
		{
			ViewModel.UpdateInfo();
		}

		protected void OnButtonSetRemainClicked(object sender, EventArgs e)
		{
			ViewModel.SetRemainCommand.Execute();
		}

		protected void OnButtonOpenExpenseClicked(object sender, EventArgs e)
		{
			ViewModel.OpenExpenseCommand.Execute();
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e) => ViewModel.SaveCommand.Execute();

		protected void OnButtonCancelClicked(object sender, EventArgs e) => ViewModel.CancelCommand.Execute();
	}
}
