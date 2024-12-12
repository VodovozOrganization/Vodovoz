using Gamma.ColumnConfig;
using QS.Navigation;
using QS.Views.GtkUI;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
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
			var primaryTextColor = GdkColors.PrimaryText.ToHtmlColor();
			var dangerTextColor = GdkColors.DangerText.ToHtmlColor();

			ylabelCreateDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.CreateDate.ToString("g"), w => w.LabelProp).InitializeFromSource();

			ylabelOriginalCarEvent.Binding.AddBinding(ViewModel, vm => vm.CompensationFromInsuranceByCourt, w => w.Visible).InitializeFromSource();

			ylabelAuthor.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Author != null ? e.Author.GetPersonNameWithInitials() : "", w => w.LabelProp)
				.InitializeFromSource();

			entityEntryCarEventType.ViewModel = ViewModel.CarEventTypeEntryViewModel;

			entityEntryCarEventType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeCarEventType, w => w.Sensitive)
				.InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;

			entityentryCar.Binding
				.AddBinding(ViewModel, vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
				.InitializeFromSource();

			entryOriginalCarEvent.ViewModel = ViewModel.OriginalCarEventViewModel;

			entryOriginalCarEvent.Binding
				.AddBinding(ViewModel, vm => vm.CompensationFromInsuranceByCourt, w => w.Visible)
				.InitializeFromSource();

			evmeDriver.SetEntityAutocompleteSelectorFactory(ViewModel.EmployeeSelectorFactory);
			evmeDriver.Binding
				.AddBinding(ViewModel, vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject)
				.InitializeFromSource();

			ydatepickerStartEventDate.Binding
				.AddBinding(ViewModel, vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.StartDate, w => w.Date)
				.InitializeFromSource();

			ydatepickerEndEventDate.Binding
				.AddBinding(ViewModel, vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
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

			ylabelRepairCost.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
				.InitializeFromSource();

			yspinRepairCost.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RepairCost, w => w.ValueAsDecimal)
				.AddFuncBinding(vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
				.AddBinding(vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
				.InitializeFromSource();

			ylabelRepairPartsCost.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
				.InitializeFromSource();

			yspinRepairPartsCost.Sensitive = false;
			yspinRepairPartsCost.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.RepairPartsCost, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
				.InitializeFromSource();

			ylabelRepairSummaryCost.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
				.InitializeFromSource();

			yspinRepairSummaryCost.Sensitive = false;
			yspinRepairSummaryCost.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.RepairAndPartsSummaryCost, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
				.InitializeFromSource();

			ylabelWriteOffDocument.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
				.InitializeFromSource();

			entityentryWriteOffDocument.ViewModel = ViewModel.WriteOffDocumentEntryViewModel;
			entityentryWriteOffDocument.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
				.AddBinding(vm => vm.CanAttachWriteOffDocument, w => w.ViewModel.IsEditable)
				.InitializeFromSource();

			ycheckbuttonIsWriteOffDocumentNotRequired.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsFuelBalanceCalibration, w => w.Visible)
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
				.AddBinding(ViewModel, vm => vm.IsFuelBalanceCalibration, w => w.Visible)
				.InitializeFromSource();

			yhboxFuel.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsFuelBalanceCalibration, w => w.Visible)
				.AddBinding(vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
				.InitializeFromSource();

			ylabelFuelCost.Binding
				.AddBinding(ViewModel, vm => vm.IsFuelBalanceCalibration, w => w.Visible)
				.InitializeFromSource();

			yentryFuelCost.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsFuelBalanceCalibration, w => w.Visible)
				.AddBinding(vm => vm.CanEditFuelBalanceCalibration, w => w.Sensitive)
				.InitializeFromSource();

			yspinActualFuelBalance.Binding
				.AddBinding(ViewModel.Entity, e => e.ActualFuelBalance, w => w.ValueAsDecimal)
				.InitializeFromSource();

			yentryCurrentFuelBalance.Binding
				.AddBinding(ViewModel.Entity, e => e.CurrentFuelBalance, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();

			yentryFuelCost.Binding.AddSource(ViewModel.Entity)
			.AddFuncBinding(e => e.FuelCost < 0 ? dangerTextColor : primaryTextColor, w => w.TextColor)
			.AddBinding(e => e.FuelCost, w => w.Text, new NullableDecimalToStringConverter())
			.InitializeFromSource();

			yentrySubstractionFuelBalance.Binding
				.AddBinding(ViewModel.Entity, e => e.SubstractionFuelBalance, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();

			buttonAddFine.Clicked += (sender, e) => { ViewModel.AddFineCommand.Execute(); };
			buttonAddFine.Binding.AddBinding(ViewModel, vm => vm.CanAddFine, w => w.Sensitive).InitializeFromSource();

			buttonAttachFine.Clicked += (sender, e) => { ViewModel.AttachFineCommand.Execute(); };
			buttonAttachFine.Binding.AddBinding(ViewModel, vm => vm.CanAttachFine, w => w.Sensitive).InitializeFromSource();

			buttonInfo.BindCommand(ViewModel.InfoCommand);

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
				entityEntryCarEventType.Sensitive =
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
				yspinBtnOdometerReading.Sensitive =
				yspinRepairSummaryCost.Sensitive =
				yspinRepairPartsCost.Sensitive =
				buttonSave.Sensitive = false;
			}
		}
	}
}
