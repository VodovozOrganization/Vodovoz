using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Views.GtkUI;
using QSProjectsLib;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.Views.Warehouse
{
	public partial class WriteoffDocumentView : TabViewBase<WriteOffDocumentViewModel>
	{
		public WriteoffDocumentView(WriteOffDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			buttonSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			buttonPrint.Clicked += (sender, args) => ViewModel.PrintCommand.Execute();

			labelTimeStamp.Binding
				.AddBinding(ViewModel.Entity, e => e.DateString, w => w.LabelProp)
				.InitializeFromSource();
			
			responsibleEmployeeEntry.ViewModel = ViewModel.ResponsibleEmployeeViewModel;
			responsibleEmployeeEntry.ViewModel.IsEditable = textComment.Editable = ViewModel.CanEditDocument;
			
			comboType.ItemsEnum = typeof(WriteOffType);
			comboType.AddEnumToHideList(WriteOffType.counterparty);
			comboType.Binding
				.AddBinding(ViewModel.Entity, e => e.WriteOffType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanChangeStorage, w => w.Sensitive)
				.InitializeFromSource();
			
			ySpecCmbWarehouses.ItemsList = ViewModel.Warehouses;
			ySpecCmbWarehouses.Binding
				.AddBinding(ViewModel.Entity, e => e.WriteOffFromWarehouse, w => w.SelectedItem)
				.InitializeFromSource();
			
			hboxStorages.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeStorage, w => w.Sensitive)
				.InitializeFromSource();
			hboxWarehouseStorage.Binding
				.AddBinding(ViewModel, vm => vm.CanShowWarehouseStorage, w => w.Visible)
				.InitializeFromSource();
			hboxEmployeeStorage.Binding
				.AddBinding(ViewModel, vm => vm.CanShowEmployeeStorage, w => w.Visible)
				.InitializeFromSource();
			hboxCarStorage.Binding
				.AddBinding(ViewModel, vm => vm.CanShowCarStorage, w => w.Visible)
				.InitializeFromSource();

			employeeStorageEntry.ViewModel = ViewModel.WriteOffFromEmployeeViewModel;
			employeeStorageEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeStorage, w => w.Sensitive)
				.InitializeFromSource();
			
			carStorageEntry.ViewModel = ViewModel.CarViewModel;
			carStorageEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeStorage, w => w.Sensitive)
				.InitializeFromSource();

			textComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			if(ViewModel.UserHasOnlyAccessToWarehouseAndComplaints)
			{
				responsibleEmployeeEntry.ViewModel.IsEditable = false;
			}
			
			vboxItems.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeItems, w => w.Sensitive)
				.InitializeFromSource();

			if(!ViewModel.Entity.CanEdit && ViewModel.Entity.TimeStamp.Date != DateTime.Now.Date)
			{
				/*ySpecCmbWarehouses.Binding
					.AddFuncBinding(ViewModel.Entity, e => e.CanEdit, w => w.Sensitive)
					.InitializeFromSource();
				*/
				comboType.Sensitive = false;
				textComment.Sensitive = false;
				hboxStorages.Sensitive = false;
				vboxItems.Sensitive = false;
				buttonSave.Sensitive = false;
			}
			else
			{
				ViewModel.Entity.CanEdit = true;
			}

			ConfigureTreeItems();

			btnAddNomenclature.Clicked += (sender, args) => ViewModel.AddNomenclatureCommand.Execute();
			btnAddNomenclatureInstance.Clicked += (sender, args) => ViewModel.AddInventoryInstanceCommand.Execute();
			btnDelete.Clicked += (sender, args) => ViewModel.DeleteItemCommand.Execute();
			btnAddFine.Clicked += (sender, args) => ViewModel.AddOrEditFineCommand.Execute();
			btnDeleteFine.Clicked += (sender, args) => ViewModel.DeleteFineCommand.Execute();
			
			btnDelete.Binding
				.AddBinding(ViewModel, vm => vm.CanDeleteItem, w => w.Sensitive)
				.InitializeFromSource();
			btnAddFine.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.AddOrEditFineTitle, w => w.Label)
				.AddBinding(vm => vm.CanAddOrDeleteFine, w => w.Sensitive)
				.InitializeFromSource();
			btnDeleteFine.Binding
				.AddBinding(ViewModel, vm => vm.CanAddOrDeleteFine, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void ConfigureTreeItems()
		{
			treeItemsList.ColumnsConfig = ColumnsConfigFactory.Create<WriteoffDocumentItem>()
				.AddColumn("Наименование")
					.AddTextRenderer (i => i.Name)
				.AddColumn("С/Н оборудования")
					.AddTextRenderer (i => i.EquipmentString)
				.AddColumn("Количество")
					.AddNumericRenderer(i => i.Amount)
					.Editing()
					.WidthChars(10)
					.AddSetter((c, i) => c.Digits = (uint)i.Nomenclature.Unit.Digits)
					.AddSetter((c, i) => c.Editable = i.CanEditAmount)
					.AddSetter((c, i) => c.Adjustment = new Adjustment(0, 0, (double)i.AmountOnStock, 1, 100, 0))
					.AddTextRenderer(i => i.Nomenclature.Unit.Name, false)
				.AddColumn("Причина выбраковки")
					.AddComboRenderer(i => i.CullingCategory)
					.SetDisplayFunc(DomainHelper.GetTitle)
					.Editing()
					.FillItems(ViewModel.CullingCategories)
				.AddColumn("Сумма ущерба")
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф")
					.AddTextRenderer(x => x.Fine != null ? x.Fine.Description : string.Empty)
				.AddColumn("Выявлено в процессе")
					.AddTextRenderer(i => i.Comment)
					.Editable()
				.Finish();
			
			treeItemsList.ItemsDataSource = ViewModel.Entity.ObservableItems;
			treeItemsList.Binding
				.AddBinding(ViewModel, vm => vm.SelectedItem, w => w.SelectedRow)
				.InitializeFromSource();
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintCommand.Execute();
		}
	}
}
