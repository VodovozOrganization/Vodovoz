using Gamma.ColumnConfig;
using Gamma.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain;
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
			//labelName.Visible = yentryName.Visible = false;
			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(true, QS.Navigation.CloseSource.Cancel);
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			chkIsArchive.Binding.AddBinding(ViewModel.Entity, s => s.IsArchive, w => w.Active).InitializeFromSource();
			entryFullBottlesToSell.Binding.AddBinding(ViewModel.Entity, e => e.FullBottleToSell, w => w.ValueAsInt).InitializeFromSource();
			entryEmptyBottlesToTake.Binding.AddBinding(ViewModel.Entity, e => e.EmptyBottlesToTake, w => w.ValueAsInt).InitializeFromSource();

			ybuttonAddNomenclature.Clicked += (sender, e) => { ViewModel.AddNomenclatureCommand.Execute(); };

			//ytreeviewNomenclatureSalesPlan.ColumnsConfig = FluentColumnsConfig<Nomenclature>.Create()
			//	.AddColumn("Подразделение").AddTextRenderer(x => x.Name)
			//	.Finish();
			ytreeviewNomenclatureSalesPlan.ColumnsConfig = FluentColumnsConfig<NomenclatureItemSalesPlan>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature != null ?x.Nomenclature.Name:"")
				.Finish();
			ytreeviewNomenclatureSalesPlan.ItemsDataSource = ViewModel.Entity.ObservableNomenclatureItemSalesPlans;
			ytreeviewNomenclatureSalesPlan.HeadersVisible = false;

			ytreeviewEquipmentTypeSalesPlan.ColumnsConfig = FluentColumnsConfig<EquipmentTypeItemSalesPlan>.Create()
				.AddColumn("Тип").AddTextRenderer(x => x.EquipmentType !=null? x.EquipmentType.GetEnumTitle():"")
				.Finish();
			ytreeviewEquipmentTypeSalesPlan.ItemsDataSource = ViewModel.Entity.ObservableEquipmentTypeItemSalesPlans;
			ytreeviewEquipmentTypeSalesPlan.HeadersVisible = false;

			ytreeviewEquipmentKindSalesPlan.ColumnsConfig = FluentColumnsConfig<EquipmentKindItemSalesPlan>.Create()
				.AddColumn("Вид").AddTextRenderer(x => x.EquipmentKind != null ? x.EquipmentKind.Name : "")
				.Finish();
			ytreeviewEquipmentKindSalesPlan.ItemsDataSource = ViewModel.Entity.ObservableEquipmentKindItemSalesPlans;
			ytreeviewEquipmentKindSalesPlan.HeadersVisible = false;

			ybuttonAddEquipmentType.Clicked += (sender, e) => ShowAttachmentForEquipmentType(true);

			ybuttonSaveEquipmentType.Clicked += (sender, e) => { ViewModel.AddEquipmentTypeCommand.Execute(); };
			ybuttonCancelEquipmentType.Clicked += (sender, e) => ShowAttachmentForEquipmentType(false);


			yspeccomboboxEquipmentType.Binding.AddSource(ViewModel) 
				.AddBinding( vm => vm.EquipmentTypes, w => w.ItemsList)
				.AddBinding( vm => vm.EquipmentType, w => w.SelectedItem).InitializeFromSource();
		}

		private void ShowAttachmentForEquipmentType(bool show)
		{
			ybuttonSaveEquipmentType.Visible = show;
			ybuttonCancelEquipmentType.Visible = show;
			yspeccomboboxEquipmentType.Visible = show;
		}
		
	}
}