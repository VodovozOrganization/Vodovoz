using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Dialogs.Fuel;

namespace Vodovoz.Dialogs.Fuel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FuelTransferDocumentView : TabViewBase<FuelTransferDocumentViewModel>
	{
		public FuelTransferDocumentView(FuelTransferDocumentViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			fuelbalanceview.ViewModel = ViewModel.FuelBalanceViewModel;

			ylabelCreationDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.CreationTime.ToShortDateString(), w => w.LabelProp).InitializeFromSource();
			ylabelAuthor.Binding.AddFuncBinding(ViewModel.Entity, e => e.Author.GetPersonNameWithInitials(), w => w.LabelProp).InitializeFromSource();
			ylabelStatus.Binding.AddFuncBinding(ViewModel.Entity, e => e.Status.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelCashierSender.Binding.AddFuncBinding(ViewModel.Entity, e => e.CashierSender != null ? e.CashierSender.GetPersonNameWithInitials() : "", w => w.LabelProp).InitializeFromSource();
			ylabelCashierReceiver.Binding.AddFuncBinding(ViewModel.Entity, e => e.CashierReceiver != null ? e.CashierReceiver.GetPersonNameWithInitials() : "", w => w.LabelProp).InitializeFromSource();
			ylabelSendTime.Binding.AddFuncBinding(ViewModel.Entity, e => e.SendTime.HasValue ? e.SendTime.Value.ToShortDateString() : "", w => w.LabelProp).InitializeFromSource();
			ylabelReceiveTime.Binding.AddFuncBinding(ViewModel.Entity, e => e.ReceiveTime.HasValue ? e.ReceiveTime.Value.ToShortDateString() : "", w => w.LabelProp).InitializeFromSource();
			ylabelLitersAvailableValue.Binding.AddFuncBinding(ViewModel, vm => GetAvailableFuelLabel(vm.FuelBalanceCache), w => w.LabelProp).InitializeFromSource();
			ylabelLitersAvailableValue.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Visible).InitializeFromSource();
			yspinLiters.Binding.AddBinding(ViewModel.Entity, e => e.TransferedLiters, w => w.ValueAsDecimal).InitializeFromSource();
			yspinLiters.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverSelectorFactory);
			entryDriver.Binding.AddBinding(ViewModel.Entity, e => e.Driver, w => w.Subject).InitializeFromSource();
			entryDriver.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entityentryCar.ViewModel = ViewModel.CarEntryViewModel;
			entityentryCar.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ycomboFuelType.SetRenderTextFunc<FuelType>(x => x.Name);
			ycomboFuelType.Binding.AddBinding(ViewModel, vm => vm.FuelTypes, w => w.ItemsList).InitializeFromSource();
			ycomboFuelType.Binding.AddBinding(ViewModel.Entity, e => e.FuelType, w => w.SelectedItem).InitializeFromSource();
			ycomboFuelType.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			comboboxCashSubdivisionFrom.SetRenderTextFunc<Subdivision>(s => s.Name);
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel, vm => vm.SubdivisionsFrom, w => w.ItemsList).InitializeFromSource();
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			comboboxCashSubdivisionFrom.Binding.AddBinding(ViewModel, e => e.CashSubdivisionFrom, w => w.SelectedItem).InitializeFromSource();

			comboboxCashSubdivisionTo.SetRenderTextFunc<Subdivision>(s => s.Name);
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel, vm => vm.SubdivisionsTo, w => w.ItemsList).InitializeFromSource();
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			comboboxCashSubdivisionTo.Binding.AddBinding(ViewModel, e => e.CashSubdivisionTo, w => w.SelectedItem).InitializeFromSource();

			buttonSend.Clicked += (sender, e) => { ViewModel.SendCommand.Execute(); };
			buttonSend.Binding.AddBinding(ViewModel, vm => vm.CanSend, w => w.Sensitive).InitializeFromSource();

			buttonReceive.Clicked += (sender, e) => { ViewModel.ReceiveCommand.Execute(); };
			buttonReceive.Binding.AddBinding(ViewModel, vm => vm.CanReceive, w => w.Sensitive).InitializeFromSource();

			buttonSave.Binding.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive).InitializeFromSource();
			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();

			buttonPrint.Binding.AddBinding(ViewModel, vm => vm.CanPrint, w => w.Sensitive).InitializeFromSource();
			buttonPrint.Clicked += (sender, e) => ViewModel.PrintCommand.Execute();

			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);

			ViewModel.PropertyChanged += (sender, e) => {
				if(e.PropertyName == nameof(ViewModel.FuelBalanceCache)) {
					SetLitersAdjustment();
				}
			};

			//Необходимо из-за того что gtk не всегда отрисовывает изменения за событием изменения свойства
			ViewModel.Entity.PropertyChanged += (sender, e) => {
				if(e.PropertyName == nameof(ViewModel.Entity.FuelType)) {
					yspinLiters.Update();
				}
			};

			SetLitersAdjustment();
		}

		private void SetLitersAdjustment()
		{
			double upperValue = 100000000;
			if(ViewModel.CanEdit) {
				upperValue = (double)ViewModel.FuelBalanceCache;
			}
			yspinLiters.Adjustment = new Adjustment(yspinLiters.Value, 0, upperValue, 1, 10, 0);
		}

		private string GetAvailableFuelLabel(decimal value)
		{
			if(value <= 0) {
				return "Нет доступного топлива для перемещения";
			}
			return $"Доступно к отправке: {value.ToString("#")}";
		}
	}
}
