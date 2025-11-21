using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Views.WageCalculation
{
	public partial class SalesPlanView : TabViewBase<SalesPlanViewModel>
	{
		public SalesPlanView(SalesPlanViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureWidget();
		}

		protected void ConfigureWidget()
		{
			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
			
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			chkIsArchive.Binding.AddBinding(ViewModel.Entity, s => s.IsArchive, w => w.Active).InitializeFromSource();
			entryFullBottlesToSell.Binding.AddBinding(ViewModel.Entity, e => e.FullBottleToSell, w => w.ValueAsInt).InitializeFromSource();
			entryEmptyBottlesToTake.Binding.AddBinding(ViewModel.Entity, e => e.EmptyBottlesToTake, w => w.ValueAsInt).InitializeFromSource();
			entryProceedsDay.Binding.AddBinding(ViewModel.Entity, e => e.ProceedsDay, w => w.ValueAsDecimal).InitializeFromSource();
			entryProceedsMonth.Binding.AddBinding(ViewModel.Entity, e => e.ProceedsMonth, w => w.ValueAsDecimal).InitializeFromSource();

			ytreeviewNomenclatureSalesPlan.ColumnsConfig = FluentColumnsConfig<NomenclatureSalesPlanItem>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature != null ? x.Nomenclature.Name : "")
				.AddColumn("План день").AddNumericRenderer(x => x.PlanDay).WidthChars(10).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.AddColumn("План месяц").AddNumericRenderer(x => x.PlanMonth).WidthChars(10).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.Finish();
			ytreeviewNomenclatureSalesPlan.ItemsDataSource = ViewModel.Entity.ObservableNomenclatureItemSalesPlans;

			ybuttonAddNomenclature.Clicked += (sender, e) => ViewModel.AddNomenclatureItemCommand.Execute();
			ybuttonDeleteNomenclature.Clicked += (sender, e) =>
				ViewModel.RemoveNomenclatureItemCommand.Execute(ytreeviewNomenclatureSalesPlan.GetSelectedObject<NomenclatureSalesPlanItem>());

			ytreeviewEquipmentKindSalesPlan.ColumnsConfig = FluentColumnsConfig<EquipmentKindSalesPlanItem>.Create()
				.AddColumn("Вид").AddTextRenderer(x => x.EquipmentKind != null ? x.EquipmentKind.Name : "")
				.AddColumn("План день").AddNumericRenderer(x => x.PlanDay).WidthChars(10).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.AddColumn("План месяц").AddNumericRenderer(x => x.PlanMonth).WidthChars(10).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.Finish();
			ytreeviewEquipmentKindSalesPlan.ItemsDataSource = ViewModel.Entity.ObservableEquipmentKindItemSalesPlans;

			ybuttonAddEquipmentKind.Clicked += (sender, e) => ViewModel.AddEquipmentKindItemCommand.Execute();
			ybuttonDeleteEquipmentKind.Clicked += (sender, e) =>
				ViewModel.RemoveEquipmentKindItemCommand.Execute(ytreeviewEquipmentKindSalesPlan.GetSelectedObject<EquipmentKindSalesPlanItem>());

			ytreeviewEquipmentTypeSalesPlan.ColumnsConfig = FluentColumnsConfig<EquipmentTypeSalesPlanItem>.Create()
				.AddColumn("Тип").AddTextRenderer(x => x.EquipmentType.GetEnumTitle())
				.AddColumn("План день").AddNumericRenderer(x => x.PlanDay).WidthChars(10).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.AddColumn("План месяц").AddNumericRenderer(x => x.PlanMonth).WidthChars(10).Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1)).Editing(true)
				.Finish();
			ytreeviewEquipmentTypeSalesPlan.ItemsDataSource = ViewModel.Entity.ObservableEquipmentTypeItemSalesPlans;

			ybuttonAddEquipmentType.Clicked += (sender, e) => ShowAttachmentForEquipmentType(true);
			ybuttonDeleteEquipmentType.Clicked += (sender, e) =>
				ViewModel.RemoveEquipmentTypeItemCommand.Execute(ytreeviewEquipmentTypeSalesPlan.GetSelectedObject<EquipmentTypeSalesPlanItem>());
			ybuttonSaveEquipmentType.Clicked += (sender, e) =>
			{
				ViewModel.AddEquipmentTypeItemCommand.Execute();
				ShowAttachmentForEquipmentType(false);
			};
			ybuttonCancelEquipmentType.Clicked += (sender, e) => ShowAttachmentForEquipmentType(false);

			yspeccomboboxEquipmentType.SetRenderTextFunc<EquipmentType>(s => s.GetEnumTitle());
			yspeccomboboxEquipmentType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.EquipmentTypes, w => w.ItemsList)
				.AddBinding(vm => vm.EquipmentType, w => w.SelectedItem).InitializeFromSource();
		}

		private void ShowAttachmentForEquipmentType(bool show)
		{
			ybuttonSaveEquipmentType.Visible = show;
			ybuttonCancelEquipmentType.Visible = show;
			yspeccomboboxEquipmentType.Visible = show;
		}
		
	}
}