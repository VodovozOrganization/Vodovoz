using Gamma.ColumnConfig;
using QS.Navigation;
using QS.Views.GtkUI;
using QSProjectsLib;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class CarEventView : TabViewBase<CarEventViewModel>
	{
		private string _carEventTypeCompensation = "Компенсация от страховой, по суду";

		public CarEventView(CarEventViewModel viewModel) :
			base(viewModel)
		{
			this.Build();
			Configure();
			CheckPeriod();
		}

		private void Configure()
		{
			ylabelCreateDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.CreateDate.ToString("g"), w => w.LabelProp).InitializeFromSource();

			originalCarEventId.Visible = labelOriginalCarEvent.Visible = false;

			ylabelAuthor.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Author != null ? e.Author.GetPersonNameWithInitials() : "", w => w.LabelProp)
				.InitializeFromSource();

			entityviewmodelentryCarEventType.SetEntityAutocompleteSelectorFactory(ViewModel.CarEventTypeSelectorFactory);
			entityviewmodelentryCarEventType.Binding.AddBinding(ViewModel.Entity, e => e.CarEventType, e => e.Subject).InitializeFromSource();
			entityviewmodelentryCarEventType.ChangedByUser += (sender, e) => ViewModel.ChangeEventTypeCommand.Execute();
			entityviewmodelentryCarEventType.ChangedByUser += (sender, e) => UpdateVisibleOriginalCarEvent();

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(ViewModel.CarSelectorFactory);
			entityviewmodelentryCar.Binding.AddBinding(ViewModel.Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.ChangedByUser += (sender, e) => ViewModel.ChangeDriverCommand.Execute();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			evmeDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			ydatepickerStartEventDate.Binding.AddBinding(ViewModel.Entity, e => e.StartDate, w => w.Date).InitializeFromSource();

			ydatepickerEndEventDate.Binding.AddBinding(ViewModel.Entity, e => e.EndDate, w => w.Date).InitializeFromSource();

			yspinPaymentTotalCarEvent.Binding
				.AddBinding(ViewModel.Entity, e => e.RepairCost, w => w.ValueAsDecimal)
				.InitializeFromSource();

			checkbuttonDoNotShowInOperation.Binding
				.AddBinding(ViewModel.Entity, e => e.DoNotShowInOperation, w => w.Active)
				.InitializeFromSource();

			ytextviewFoundation.Binding.AddBinding(ViewModel.Entity, e => e.Foundation, w => w.Buffer.Text).InitializeFromSource();

			ytextviewCommnet.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeviewFines.ColumnsConfig = FluentColumnsConfig<FineItem>.Create()
				.AddColumn("№").AddTextRenderer(x => x.Fine.Id.ToString())
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Сумма штрафа").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.Money))
				.Finish();
			ytreeviewFines.Binding.AddBinding(ViewModel, vm => vm.FineItems, w => w.ItemsDataSource).InitializeFromSource();

			originalCarEventId.Binding.AddBinding(ViewModel, vm => vm.OriginalCarEventId, w => w.ValueAsInt).InitializeFromSource();

			buttonAddFine.Clicked += (sender, e) => { ViewModel.AddFineCommand.Execute(); };
			buttonAddFine.Binding.AddBinding(ViewModel, vm => vm.CanAddFine, w => w.Sensitive).InitializeFromSource();

			buttonAttachFine.Clicked += (sender, e) => { ViewModel.AttachFineCommand.Execute(); };
			buttonAttachFine.Binding.AddBinding(ViewModel, vm => vm.CanAttachFine, w => w.Sensitive).InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);
		}

		private void UpdateVisibleOriginalCarEvent()
		{
			if(ViewModel.Entity.CarEventType?.Name == _carEventTypeCompensation)
			{
				originalCarEventId.Visible = labelOriginalCarEvent.Visible = true;
			}
		}

		private void CheckPeriod()
		{
			if (!ViewModel.CanEdit)
			{
				ylabelCreateDate.Sensitive =
				ylabelAuthor.Sensitive =
				entityviewmodelentryCarEventType.Sensitive =
				entityviewmodelentryCar.Sensitive =
				evmeDriver.Sensitive =
				ydatepickerStartEventDate.Sensitive =
				ydatepickerEndEventDate.Sensitive =
				yspinPaymentTotalCarEvent.Sensitive =
				checkbuttonDoNotShowInOperation.Sensitive =
				ytextviewFoundation.Sensitive =
				ytextviewCommnet.Sensitive =
				ytreeviewFines.Sensitive =
				buttonAddFine.Sensitive =
				buttonAttachFine.Sensitive =
				buttonSave.Sensitive = false;
			}
		}
	}
}
