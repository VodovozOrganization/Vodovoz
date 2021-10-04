﻿using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.ViewModels.FuelDocuments;
using QS.Views.GtkUI;
using QS.Permissions;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Filters.ViewModels;
using QS.Project.Services;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.JournalViewModels;

namespace Vodovoz
{
	public partial class FuelDocumentView : TabViewBase<FuelDocumentViewModel>
	{
		public IUnitOfWork UoW { get; set; }
		private FuelRepository fuelRepository;

		public FuelDocumentView(FuelDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg ()
		{
			fuelRepository = new FuelRepository();

			yspeccomboboxSubdivision.Sensitive = ViewModel.FuelDocument.FuelExpenseOperation == null && ViewModel.IsNewEditable; 
			yspeccomboboxSubdivision.Binding.AddBinding(ViewModel, w => w.AvailableSubdivisionsForUser, e => e.ItemsList).InitializeFromSource();
			yspeccomboboxSubdivision.SetRenderTextFunc<Subdivision>(s => s.Name);
			yspeccomboboxSubdivision.Binding.AddBinding(ViewModel.FuelDocument, w => w.Subdivision, e => e.SelectedItem).InitializeFromSource();

			ydatepicker.Binding.AddBinding(ViewModel.FuelDocument, e => e.Date, w => w.Date).InitializeFromSource();

			yentrydriver.RepresentationModel = ViewModel.EmployeeJournal;
			yentrydriver.Binding.AddBinding(ViewModel.FuelDocument, e => e.Driver, w => w.Subject).InitializeFromSource();

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));
			entityviewmodelentryCar.Binding.AddBinding(ViewModel.FuelDocument, x => x.CarVersion, x => x.Subject).InitializeFromSource();

			yentryfuel.SubjectType = typeof(FuelType);
			yentryfuel.Binding.AddBinding(ViewModel.FuelDocument, e => e.Fuel, w => w.Subject).InitializeFromSource();

			yspinFuelTicketLiters.Binding.AddBinding (ViewModel.FuelDocument, e => e.FuelCoupons, w => w.ValueAsInt).InitializeFromSource ();

			disablespinMoney.Binding.AddBinding(ViewModel.FuelDocument, e => e.PayedForFuel, w => w.ValueAsDecimal).InitializeFromSource();

			labelResultInfo.Binding.AddBinding(ViewModel, e => e.ResultInfo, w => w.Text).InitializeFromSource();
			labelAvalilableFuel.Binding.AddBinding(ViewModel, e => e.BalanceState, w => w.Text).InitializeFromSource();
			labelExpenseInfo.Binding.AddBinding(ViewModel, e => e.CashExpenseInfo, w => w.Text).InitializeFromSource();

			disablespinMoney.Binding.AddBinding(ViewModel, e => e.FuelInMoney, w => w.Active).InitializeFromSource();

			ytextviewFuelInfo.Binding.AddBinding(ViewModel, e => e.FuelInfo, w => w.Buffer.Text).InitializeFromSource();

			yenumcomboboxPaymentType.ItemsEnum = typeof(FuelPaymentType);
			yenumcomboboxPaymentType.Binding.AddBinding(ViewModel.FuelDocument, e => e.FuelPaymentType, w => w.SelectedItem).InitializeFromSource();

			buttonSave.Binding.AddBinding(ViewModel, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
			buttonOpenExpense.Binding.AddBinding(ViewModel, e => e.CanOpenExpense, w => w.Sensitive).InitializeFromSource();
			spinFuelPrice.Binding.AddBinding(ViewModel, e => e.FuelInMoney, w => w.Sensitive).InitializeFromSource();
			ydatepicker.Binding.AddBinding(ViewModel, e => e.CanChangeDate, w => w.Sensitive).InitializeFromSource();
			yspinFuelTicketLiters.Binding.AddBinding(ViewModel, e => e.IsNewEditable, w => w.Sensitive).InitializeFromSource();
			yenumcomboboxPaymentType.Binding.AddBinding(ViewModel, e => e.IsNewEditable, w => w.Sensitive).InitializeFromSource();

			spinFuelPrice.Binding.AddBinding(ViewModel.FuelDocument, e => e.LiterCost, w => w.ValueAsDecimal).InitializeFromSource();

			if(ViewModel?.FuelDocument?.FuelOperation?.PayedLiters > 0m)
				disablespinMoney.Active = true; // Перенести в VM
			

			disablespinMoney.Binding.AddBinding(ViewModel, e => e.IsNewEditable, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnDisablespinMoneyValueChanged (object sender, EventArgs e)
		{
			ViewModel.UpdateInfo();
		}

		protected void OnButtonSetRemainClicked(object sender, EventArgs e)
		{
			ViewModel.SetRemainCommand.Execute();
		}

		protected void OnButtonOpenExpenseClicked(object sender, EventArgs e)
		{
			if(ViewModel.FuelDocument.FuelCashExpense?.Id > 0) 
				Tab.TabParent.AddSlaveTab(Tab, new CashExpenseDlg(ViewModel.FuelDocument.FuelCashExpense.Id, PermissionsSettings.PermissionService));
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e) => ViewModel.SaveCommand.Execute();

		protected void OnButtonCancelClicked(object sender, EventArgs e) => ViewModel.CancelCommand.Execute();
	}
}