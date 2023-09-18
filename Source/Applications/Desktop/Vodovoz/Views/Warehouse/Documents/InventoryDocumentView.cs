using Gamma.GtkWidgets;
using Gtk;
using QS.Navigation;
using QS.Views.GtkUI;
using QSOrmProject;
using QSProjectsLib;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.InventoryDocuments;
using Vodovoz.Domain.Goods;
using Vodovoz.Infrastructure;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.ViewModels.Warehouses.Documents;

namespace Vodovoz.Views.Warehouse.Documents
{
	[Obsolete("Снести после обновления 29.05.23")]
	public partial class InventoryDocumentView : TabViewBase<InventoryDocumentViewModel>
	{
		public InventoryDocumentView(InventoryDocumentViewModel viewModel)
			: base(viewModel)
		{
			Build();
			ConfigureView();
		}

		private void ConfigureView()
		{
			var filterWidget = new SelectableParameterReportFilterView(ViewModel.FilterViewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();

			BindDocumentFields();
			BindItemsButtons();
			ConfigureNomenclatureColumns();
			BindCommonButtons();
		}

		private void BindCommonButtons()
		{
			buttonSave.Sensitive = ViewModel.CanSave;
			buttonSave.Clicked += OnButtonSaveClicked;
			buttonCancel.Clicked += OnButtonCancelClicked;
			buttonPrint.Clicked += OnButtonPrintClicked;
		}

		private void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.Close(true, CloseSource.Cancel);
		}

		private void OnButtonSaveClicked(object sender, EventArgs e)
		{
			ViewModel.SaveAndClose();
		}

		private void BindDocumentFields()
		{
			ydatepickerDocDate.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.TimeStamp, w => w.Date)
				.AddBinding(e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			yentryrefWarehouse.ItemsQuery = ViewModel.GetRestrictedWarehouseQuery();

			yentryrefWarehouse.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.CanEdit, w => w.Sensitive)
				.AddBinding(e => e.Warehouse, w => w.Subject)
				.InitializeFromSource();

			yentryrefWarehouse.BeforeChangeByUser += OnYentryrefWarehouseBeforeChangeByUser;

			ychkSortNomenclaturesByTitle.Binding
				.AddBinding(ViewModel.Entity, e => e.SortedByNomenclatureName, w => w.Active)
				.InitializeFromSource();

			ytextviewCommnet.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.Comment, w => w.Buffer.Text)
				.AddFuncBinding(e => e.CanEdit, w => w.Editable)
				.InitializeFromSource();
		}

		private void BindItemsButtons()
		{
			ybtnFillItems.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanFillItems, w => w.Sensitive)
				.AddBinding(vm => vm.FillItemsButtonTitle, w => w.Label)
				.InitializeFromSource();

			ybtnFillItems.Clicked += OnButtonFillItemsClicked;

			ybtnAdd.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanAddItem, w => w.Sensitive)
				.InitializeFromSource();

			ybtnAdd.Clicked += OnButtonAddClicked;

			ybtnAddFine.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanAddFine, w => w.Sensitive)
				.InitializeFromSource();

			ybtnAddFine.Clicked += OnButtonFineClicked;

			ybtnDeleteFine.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanDeleteFine, w => w.Sensitive)
				.InitializeFromSource();

			ybtnDeleteFine.Clicked += OnButtonDeleteFineClicked;

			ybtnFillByAccounting.Binding.AddSource(ViewModel.Entity)
				.AddBinding(e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ybtnFillByAccounting.Clicked += OnYbtnFillByAccountingClicked;
		}

		private void ConfigureNomenclatureColumns()
		{
			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<InventoryDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => GetNomenclatureName(x.Nomenclature), useMarkup: true)
				.AddColumn("Кол-во в учёте").AddTextRenderer(x => x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB) : x.AmountInDB.ToString())
				.AddColumn("Кол-во по факту").AddNumericRenderer(x => x.AmountInFact).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (x.Nomenclature.Unit != null ? (uint)x.Nomenclature.Unit.Digits : 1))
				.AddColumn("Разница").AddTextRenderer(x => x.Difference != 0 && x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : string.Empty)
				.AddSetter((w, x) => w.ForegroundGdk = x.Difference < 0 ? GdkColors.DangerText : GdkColors.InfoText)
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : string.Empty)
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) =>
				{
					Gdk.Color color = new Gdk.Color(255, 255, 255);
					if(ViewModel.NomenclaturesWithDiscrepancies.Any(x => x.Id == node.Nomenclature.Id))
					{
						color = new Gdk.Color(255, 125, 125);
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();

			ytreeviewItems.Binding
				.AddBinding(
					ViewModel,
					vm => vm.SelectedInventoryDocumentItem,
					w => w.SelectedRow)
				.AddBinding(ViewModel.Entity, e => e.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.ObservableNomenclatureItems, w => w.ItemsDataSource)
				.InitializeFromSource();
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			ViewModel.PrintCommand.Execute();
		}

		protected void OnYentryrefWarehouseBeforeChangeByUser(object sender, EntryReferenceBeforeChangeEventArgs e)
		{
			if(ViewModel.Entity.Warehouse != null && ViewModel.Entity.ObservableNomenclatureItems.Count > 0)
			{
				if(ViewModel.AskQuestion("При изменении склада табличная часть документа будет очищена. Продолжить?"))
				{
					ViewModel.ClearItemsCommand.Execute();
				}
				else
				{
					e.CanChange = false;
				}
			}
		}

		private string GetNomenclatureName(Nomenclature nomenclature)
		{
			if(ViewModel.NomenclaturesWithDiscrepancies.Any(x => x.Id == nomenclature.Id))
			{
				return $"<b>{nomenclature.Name}</b>";
			}

			return nomenclature.Name;
		}

		protected void OnButtonFillItemsClicked(object sender, EventArgs e)
		{
			ViewModel.FillItemsCommand.Execute();
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItemCommand.Execute();
		}

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			ViewModel.AddOrEditFineCommand.Execute();
		}

		protected void OnButtonDeleteFineClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteFineCommand.Execute();
		}

		protected void OnYbtnFillByAccountingClicked(object sender, EventArgs e)
		{
			ViewModel.FillFactByAccountingCommand.Execute();
		}

		public override void Dispose()
		{
			buttonSave.Clicked -= OnButtonSaveClicked;
			buttonCancel.Clicked -= OnButtonCancelClicked;
			buttonPrint.Clicked -= OnButtonPrintClicked;
			ybtnFillItems.Clicked -= OnButtonFillItemsClicked;
			ybtnAdd.Clicked -= OnButtonAddClicked;
			ybtnAddFine.Clicked -= OnButtonFineClicked;
			ybtnDeleteFine.Clicked -= OnButtonDeleteFineClicked;
			ybtnFillByAccounting.Clicked -= OnYbtnFillByAccountingClicked;

			base.Dispose();
		}
	}
}
