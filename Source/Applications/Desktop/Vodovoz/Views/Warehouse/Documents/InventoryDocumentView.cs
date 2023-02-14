using Gamma.GtkWidgets;
using Gtk;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.Views.GtkUI;
using QSOrmProject;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.ViewModels.Warehouses.Documents;

namespace Vodovoz.Views.Warehouse.Documents
{
	public partial class InventoryDocumentView : TabViewBase<InventoryDocumentViewModel>
	{
		private InventoryDocumentItem _fineEditItem;

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

			ychkSortNomenclaturesByTitle.Binding
				.AddBinding(ViewModel, vm => vm.SortByNomenclatureTitle, w => w.Active)
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
				.AddBinding(vm => vm.CanAddNomenclature, w => w.Sensitive)
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
				.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
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

			ytreeviewItems.ItemsDataSource = ViewModel.Entity.Items;

			ytreeviewItems.Binding
				.AddBinding(
					ViewModel,
					vm => vm.SelectedInventoryDocumentItem,
					w => w.SelectedRow)
				.AddBinding(ViewModel.Entity, e => e.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ytreeviewItems.YTreeModel?.EmitModelChanged();
			ViewModel.Entity.PropertyChanged += EntityPropertyChanged;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(ViewModel.Print())
			{
				var reportInfo = new QS.Report.ReportInfo
				{
					Title = $"Акт инвентаризации №{ViewModel.Entity.Id} от {ViewModel.Entity.TimeStamp:d}",
					Identifier = "Store.InventoryDoc",
					Parameters = new Dictionary<string, object>
					{
						{ "inventory_id",  ViewModel.Entity.Id }
					}
				};

				Tab.TabParent.OpenTab(
					QSReport.ReportViewDlg.GenerateHashName(reportInfo),
					() => new QSReport.ReportViewDlg(reportInfo));
			}
		}

		protected void OnYentryrefWarehouseBeforeChangeByUser(object sender, EntryReferenceBeforeChangeEventArgs e)
		{
			if(ViewModel.Entity.Warehouse != null && ViewModel.Entity.Items.Count > 0)
			{
				if(ViewModel.AskQuestion("При изменении склада табличная часть документа будет очищена. Продолжить?"))
				{
					ViewModel.Entity.ClearItems();
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

		private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Entity.Items))
			{
				ytreeviewItems.YTreeModel?.EmitModelChanged();
			}
		}

		private void FillDiscrepancies()
		{
			ViewModel.FillDiscrepancies();
		}

		protected void OnButtonFillItemsClicked(object sender, EventArgs e)
		{
			// Костыль для передачи из фильтра предназначенного только для отчетов данных в подходящем виде
			var nomenclaturesToInclude = new List<int>();
			var nomenclaturesToExclude = new List<int>();
			var nomenclatureCategoryToInclude = new List<string>();
			var nomenclatureCategoryToExclude = new List<string>();
			var productGroupToInclude = new List<int>();
			var productGroupToExclude = new List<int>();

			foreach(SelectableParameterSet parameterSet in ViewModel.Filter.ParameterSets)
			{
				switch(parameterSet.ParameterName)
				{
					case nameof(Nomenclature):
						if(parameterSet.FilterType == SelectableFilterType.Include)
						{
							foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								nomenclaturesToInclude.Add(value.EntityId);
							}
						}
						else
						{
							foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								nomenclaturesToExclude.Add(value.EntityId);
							}
						}
						break;
					case nameof(NomenclatureCategory):
						if(parameterSet.FilterType == SelectableFilterType.Include)
						{
							foreach(SelectableEnumParameter<NomenclatureCategory> value in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								nomenclatureCategoryToInclude.Add(value.Value.ToString());
							}
						}
						else
						{
							foreach(SelectableEnumParameter<NomenclatureCategory> value in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								nomenclatureCategoryToExclude.Add(value.Value.ToString());
							}
						}
						break;
					case nameof(ProductGroup):
						if(parameterSet.FilterType == SelectableFilterType.Include)
						{
							foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								productGroupToInclude.Add(value.EntityId);
							}
						}
						else
						{
							foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								productGroupToExclude.Add(value.EntityId);
							}
						}
						break;
				}
			}

			FillDiscrepancies();

			ViewModel.FillItems(
				nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
				nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
				nomenclatureCategoryToInclude: nomenclatureCategoryToInclude.ToArray(),
				nomenclatureCategoryToExclude: nomenclatureCategoryToExclude.ToArray(),
				productGroupToInclude: productGroupToInclude.ToArray(),
				productGroupToExclude: productGroupToExclude.ToArray());
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var nomenclatureSelector = ViewModel.NomenclatureJournalFactory.CreateNomenclatureSelector();
			nomenclatureSelector.OnEntitySelectedResult += NomenclatureSelectorOnEntitySelectedResult;
			Tab.TabParent.AddSlaveTab(Tab, nomenclatureSelector);
		}

		private void NomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			if(e.SelectedNodes.Any())
			{
				foreach(var node in e.SelectedNodes)
				{
					if(ViewModel.Entity.Items.Any(x => x.Nomenclature.Id == node.Id))
					{
						continue;
					}

					var nomenclature = ViewModel.UoW.GetById<Nomenclature>(node.Id);
					ViewModel.Entity.AddItem(nomenclature, 0, 0);
				}

				ViewModel.SortDocumentItems();
			}
		}

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			FineDlg fineDlg;

			if(ViewModel.SelectedInventoryDocumentItem.Fine != null)
			{
				fineDlg = new FineDlg(ViewModel.SelectedInventoryDocumentItem.Fine);
				fineDlg.EntitySaved += FineDlgExist_EntitySaved;
			}
			else
			{
				fineDlg = new FineDlg("Недостача");
				fineDlg.EntitySaved += FineDlgNew_EntitySaved;
			}

			fineDlg.Entity.TotalMoney = ViewModel.SelectedInventoryDocumentItem.SumOfDamage;
			_fineEditItem = ViewModel.SelectedInventoryDocumentItem;
			Tab.TabParent.AddSlaveTab(Tab, fineDlg);
		}

		private void FineDlgNew_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			_fineEditItem.Fine = e.Entity as Fine;
			_fineEditItem = null;
		}

		protected void OnButtonDeleteFineClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteFine();
		}

		private void FineDlgExist_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = _fineEditItem.Fine.Id;
			ViewModel.UoW.Session.Evict(_fineEditItem.Fine);
			_fineEditItem.Fine = ViewModel.UoW.GetById<Fine>(id);
		}

		protected void OnYbtnFillByAccountingClicked(object sender, EventArgs e)
		{
			ViewModel.FillByAccounting();
		}
	}
}
