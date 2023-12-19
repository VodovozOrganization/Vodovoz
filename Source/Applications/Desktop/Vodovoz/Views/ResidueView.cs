using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ResidueView : TabViewBase<ResidueViewModel>
	{
		public ResidueView(ResidueViewModel residueViewModel) : base(residueViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ypickerDocDate.Binding.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date).InitializeFromSource();

			entryClient.Binding.AddBinding(ViewModel.Entity, e => e.Customer, w => w.Subject).InitializeFromSource();
			entryClient.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelectorFactory);
			buttonOpenSlider.Clicked += OnButtonOpenSliderClicked;

			ySpecCmbDeliveryPoint.Binding.AddBinding(ViewModel.Entity, r => r.Customer, w => w.Sensitive, new NullToBooleanConverter()).InitializeFromSource();

			yenumcomboDebtPaymentType.ItemsEnum = typeof(PaymentType);
			yenumcomboDebtPaymentType.Binding.AddBinding(ViewModel.Entity, e => e.DebtPaymentType, w => w.SelectedItem).InitializeFromSource();
			yenumcomboDebtPaymentType.Sensitive = disablespinMoneyDebt.Active;

			disablespinBottlesResidue.Binding.AddBinding(ViewModel.Entity, e => e.BottlesResidue, w => w.ValueAsInt).InitializeFromSource();
			disablespinBottlesDeposit.Binding.AddBinding(ViewModel.Entity, e => e.BottlesDeposit, w => w.ValueAsDecimal).InitializeFromSource();
			disablespinMoneyDebt.Binding.AddBinding(ViewModel.Entity, e => e.DebtResidue, w => w.ValueAsDecimal).InitializeFromSource();
			disablespinMoneyDebt.ActiveChanged += DisablespinMoneyDebt_ActiveChanged;
			ytextviewComment.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			//bindings info panel
			labelCurrentBottleDebt.Binding.AddFuncBinding(ViewModel, vm => vm.CurrentBottlesDebt, w => w.LabelProp).InitializeFromSource();
			labelCurrentBottleDeposit.Binding.AddFuncBinding(ViewModel, vm => vm.CurrentBottlesDeposit, w => w.LabelProp).InitializeFromSource();
			labelCurrentMoneyDebt.Binding.AddFuncBinding(ViewModel, vm => vm.CurrentMoneyDebt, w => w.LabelProp).InitializeFromSource();
			labelCurrentEquipmentDeposit.Binding.AddFuncBinding(ViewModel, vm => vm.CurrentEquipmentDeposit, w => w.LabelProp).InitializeFromSource();

			ytreeviewEquipment.ColumnsConfig = FluentColumnsConfig<ResidueEquipmentDepositItem>.Create()
				.AddColumn("Номенклатура")
					.AddTextRenderer(x => x.Nomenclature.OfficialName)
				.AddColumn("Количество")
					.AddNumericRenderer(x => x.EquipmentCount)
					.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 0))
					.XAlign(0.5f)
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit)
				.AddColumn("Направление")
					.AddEnumRenderer<ResidueEquipmentDirection>(x => x.EquipmentDirection).Editing()
				.AddColumn("Количество залогов")
					.AddNumericRenderer(x => x.DepositCount)
					.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 0))
					.XAlign(0.5f)
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit)
				.AddColumn("Залог")
					.AddNumericRenderer(x => x.EquipmentDeposit)
					.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 0))
					.XAlign(0.5f)
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit)
				.AddColumn("Сумма залогов")
					.AddNumericRenderer(x => x.DepositCount * x.EquipmentDeposit)
					.XAlign(0.5f)
				.AddColumn("Форма оплаты")
					.AddComboRenderer(x => x.PaymentType)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((PaymentType[])Enum.GetValues(typeof(PaymentType))).ToList())
					.AddSetter((c, n) => c.Editable = ViewModel.CanEdit)
				.Finish();
			ytreeviewEquipment.ItemsDataSource = ViewModel.Entity.ObservableEquipmentDepositItems;
			ytreeviewEquipment.Selection.Changed += Selection_Changed;

			buttonAddEquipment.Clicked += ButtonAddEquipment_Clicked;
			buttonDeleteEquipment.Clicked += ButtonDeleteEquipment_Clicked;

			ViewModel.RemoveDepositEquipmentItemCommand.CanExecuteChanged += RemoveDepositEquipmentItemCommand_CanExecuteChanged;

			buttonSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
		}

		void RemoveDepositEquipmentItemCommand_CanExecuteChanged(object sender, EventArgs e)
		{
			UpdateButtonDeleteItemSensitive();
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			UpdateButtonDeleteItemSensitive();
		}

		private void UpdateButtonDeleteItemSensitive()
		{
			var selected = ytreeviewEquipment.GetSelectedObject() as ResidueEquipmentDepositItem;
			buttonDeleteEquipment.Sensitive = ViewModel.RemoveDepositEquipmentItemCommand.CanExecute(selected);
		}

		void ButtonAddEquipment_Clicked(object sender, EventArgs e)
		{
			ViewModel.AddDepositEquipmentItemCommand.Execute();
		}

		void ButtonDeleteEquipment_Clicked(object sender, EventArgs e)
		{
			var selected = ytreeviewEquipment.GetSelectedObject() as ResidueEquipmentDepositItem;
			ViewModel.RemoveDepositEquipmentItemCommand.Execute(selected);
		}

		protected void OnButtonOpenSliderClicked(object sender, EventArgs e)
		{
			tableInfo.Visible = !tableInfo.Visible;
			buttonOpenSlider.Label = !tableInfo.Visible ? "<" : ">";
		}

		void DisablespinMoneyDebt_ActiveChanged(object sender, EventArgs e)
		{
			yenumcomboDebtPaymentType.Sensitive = disablespinMoneyDebt.Active;
		}

		protected void OnEntryClientChanged(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Customer != null) {
				ySpecCmbDeliveryPoint.SetRenderTextFunc<DeliveryPoint>(d => string.Format("{0}: {1}", d.Id, d.ShortAddress));
				ySpecCmbDeliveryPoint.Binding.AddBinding(ViewModel.Entity.Customer, r => r.DeliveryPoints, w => w.ItemsList).InitializeFromSource();
				ySpecCmbDeliveryPoint.Binding.AddBinding(ViewModel.Entity, r => r.DeliveryPoint, w => w.SelectedItem).InitializeFromSource();
			}
		}
	}
}
