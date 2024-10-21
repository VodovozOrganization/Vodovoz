using Gamma.ColumnConfig;
using QS.Navigation;
using QS.Views.GtkUI;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class CarEventView : TabViewBase<CarEventViewModel>
	{

		public CarEventView(CarEventViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ylabelCreateDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.CreateDate.ToString("g"), w => w.LabelProp).InitializeFromSource();

			ylabelOriginalCarEvent.Binding.AddBinding(ViewModel, vm => vm.CompensationFromInsuranceByCourt, w => w.Visible).InitializeFromSource();

			ylabelAuthor.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Author != null ? e.Author.GetPersonNameWithInitials() : "", w => w.LabelProp)
				.InitializeFromSource();

			entityviewmodelentryCarEventType.SetEntityAutocompleteSelectorFactory(ViewModel.CarEventTypeSelectorFactory);
			entityviewmodelentryCarEventType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CarEventType, e => e.Subject)
				.AddBinding(vm => vm.CanChangeCarEventType, w => w.Sensitive)
				.InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;

			entityentryCar.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibrationCarEventType, w => w.Sensitive)
				.InitializeFromSource();

			entryOriginalCarEvent.ViewModel = ViewModel.OriginalCarEventViewModel;

			entryOriginalCarEvent.Binding
				.AddBinding(ViewModel, vm => vm.CompensationFromInsuranceByCourt, w => w.Visible)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			evmeDriver.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibrationCarEventType, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject)
				.InitializeFromSource();

			ydatepickerStartEventDate.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibrationCarEventType, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.StartDate, w => w.Date)
				.InitializeFromSource();
			
			ydatepickerEndEventDate.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibrationCarEventType, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.EndDate, w => w.Date)
				.InitializeFromSource();

			ylabelCarTechnicalCheckupEndDate.Binding
				.AddBinding(ViewModel, vm => vm.IsCarTechnicalCheckupEventType, w => w.Visible)
				.InitializeFromSource();

			datepickerCarTechnicalCheckupEndDate.Binding
				.AddBinding(ViewModel, vm => vm.IsCarTechnicalCheckupEventType, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanChangeCarTechnicalCheckupEndDate, w => w.IsEditable)
				.AddBinding(ViewModel.Entity, e => e.CarTechnicalCheckupEndingDate, w => w.DateOrNull)
				.InitializeFromSource();

			yspinRepairCost.Binding
				.AddBinding(ViewModel, vm => vm.RepairCost, w => w.ValueAsDecimal)
				.InitializeFromSource();

			yspinRepairPartsCost.Sensitive = false;
			yspinRepairPartsCost.Binding
				.AddBinding(ViewModel.Entity, e => e.RepairPartsCost, w => w.ValueAsDecimal)
				.InitializeFromSource();

			yspinRepairSummaryCost.Sensitive = false;
			yspinRepairSummaryCost.Binding
				.AddBinding(ViewModel.Entity, e => e.RepairAndPartsSummaryCost, w => w.ValueAsDecimal)
				.InitializeFromSource();

			entityentryWriteOffDocument.ViewModel = ViewModel.WriteOffDocumentEntryViewModel;
			entityentryWriteOffDocument.Binding
				.AddBinding(ViewModel, vm => vm.CanAttachWriteOffDocument, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			ycheckbuttonIsWriteOffDocumentNotRequired.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeWriteOffDocumentNotRequired, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.IsWriteOffDocumentNotRequired, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonIsWriteOffDocumentNotRequired.Clicked += (s, e) => ViewModel.WriteOffDocumentNotRequiredChangedCommand.Execute();

			checkbuttonDoNotShowInOperation.Binding
				.AddBinding(ViewModel, vw => vw.DoNotShowInOperation, w => w.Active)
				.InitializeFromSource();

			ytextviewFoundation.Binding.AddBinding(ViewModel.Entity, e => e.Foundation, w => w.Buffer.Text).InitializeFromSource();

			ytextviewCommnet.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			ytreeviewFines.ColumnsConfig = FluentColumnsConfig<FineItem>.Create()
				.AddColumn("№").AddTextRenderer(x => x.Fine.Id.ToString())
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Сумма штрафа").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.Money))
				.Finish();
			ytreeviewFines.Binding.AddBinding(ViewModel, vm => vm.FineItems, w => w.ItemsDataSource).InitializeFromSource();

			yspinBtnOdometerReading.Binding
				.AddBinding(ViewModel.Entity, e => e.Odometer, w => w.ValueAsInt)
				.AddBinding(ViewModel, vm => vm.IsTechInspectCarEventType, w => w.Visible)
				.InitializeFromSource();

			ylblOdometerReading.Binding
				.AddBinding(ViewModel, vm => vm.IsTechInspectCarEventType, w => w.Visible)
				.InitializeFromSource();

			ylabelActualFuelBalance.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibrationCarEventType, w => w.Sensitive)
				.InitializeFromSource();

			yhboxFuel.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibrationCarEventType, w => w.Sensitive)
				.InitializeFromSource();

			buttonAddFine.Clicked += (sender, e) => { ViewModel.AddFineCommand.Execute(); };
			buttonAddFine.Binding.AddBinding(ViewModel, vm => vm.CanAddFine, w => w.Sensitive).InitializeFromSource();

			buttonAttachFine.Clicked += (sender, e) => { ViewModel.AttachFineCommand.Execute(); };
			buttonAttachFine.Binding.AddBinding(ViewModel, vm => vm.CanAttachFine, w => w.Sensitive).InitializeFromSource();

			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(true, CloseSource.Cancel);

			UpdateSensitivity();
		}

		private void UpdateSensitivity()
		{
			if(!ViewModel.CanEdit)
			{
				ylabelCreateDate.Sensitive =
				ylabelAuthor.Sensitive =
				entityviewmodelentryCarEventType.Sensitive =
				entityentryCar.Sensitive =
				evmeDriver.Sensitive =
				ydatepickerStartEventDate.Sensitive =
				ydatepickerEndEventDate.Sensitive =
				yspinRepairCost.Sensitive =
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
