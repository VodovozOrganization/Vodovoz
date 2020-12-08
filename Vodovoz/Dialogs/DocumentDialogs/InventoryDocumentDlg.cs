using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Additions.Store;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.PermissionExtensions;
using Vodovoz.EntityRepositories;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using Gamma.GtkWidgets;
using Gtk;
using System.Linq;
using QSProjectsLib;
using Gdk;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.Reports;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Vodovoz.Domain.Employees;
using QS.Tdi;
using Vodovoz.Parameters;

namespace Vodovoz
{
	public partial class InventoryDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<InventoryDocument>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private IEmployeeRepository EmployeeRepository { get; } = EmployeeSingletonRepository.GetInstance();
		private INomenclatureRepository nomenclatureRepository { get; } = new NomenclatureRepository(new NomenclatureParametersProvider());
		private SelectableParametersReportFilter filter;
		private InventoryDocumentItem FineEditItem;

		public InventoryDocumentDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<InventoryDocument> ();
			Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if(Entity.Author == null)
			{
				MessageDialogHelper.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}
			Entity.Warehouse = StoreDocumentHelper.GetDefaultWarehouse(UoW, WarehousePermissions.InventoryEdit);

			ConfigureDlg ();
		}

		public InventoryDocumentDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<InventoryDocument> (id);
			ConfigureDlg ();
		}

		public InventoryDocumentDlg (InventoryDocument sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			if(StoreDocumentHelper.CheckAllPermissions(UoW.IsNew, WarehousePermissions.InventoryEdit, Entity.Warehouse)) {
				FailInitialize = true;
				return;
			}

			var editing = StoreDocumentHelper.CanEditDocument(WarehousePermissions.InventoryEdit, Entity.Warehouse);
			ydatepickerDocDate.Sensitive = yentryrefWarehouse.IsEditable = ytextviewCommnet.Editable = editing;

			ytreeviewItems.Sensitive =
				buttonAdd.Sensitive = 
				buttonFillItems.Sensitive =
				buttonFine.Sensitive =
				buttonDeleteFine.Sensitive = editing;

			ydatepickerDocDate.Binding.AddBinding(Entity, e => e.TimeStamp, w => w.Date).InitializeFromSource();
			yentryrefWarehouse.ItemsQuery = StoreDocumentHelper.GetRestrictedWarehouseQuery(WarehousePermissions.InventoryEdit);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();

			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			string errorMessage = "Не установлены единицы измерения у следующих номенклатур :" + Environment.NewLine;
			int wrongNomenclatures = 0;
			foreach (var item in UoWGeneric.Root.Items)
			{
				if(item.Nomenclature.Unit == null) {
					errorMessage += string.Format("Номер: {0}. Название: {1}{2}",
						item.Nomenclature.Id, item.Nomenclature.Name, Environment.NewLine);
					wrongNomenclatures++;
				}
			}
			if (wrongNomenclatures > 0) {
				MessageDialogHelper.RunErrorDialog(errorMessage);
				FailInitialize = true;
				return;
			}

			var permmissionValidator = new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), EmployeeSingletonRepository.GetInstance());
			Entity.CanEdit = permmissionValidator.Validate(typeof(InventoryDocument), UserSingletonRepository.GetInstance().GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));
			if(!Entity.CanEdit && Entity.TimeStamp.Date != DateTime.Now.Date) {
				ydatepickerDocDate.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				yentryrefWarehouse.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				ytextviewCommnet.Binding.AddFuncBinding(Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				buttonSave.Sensitive = false;
				ytreeviewItems.Sensitive =
					buttonAdd.Sensitive =
					buttonFillItems.Sensitive =
					buttonFine.Sensitive =
					buttonDeleteFine.Sensitive = false;
			} else {
				Entity.CanEdit = true;
			}

			filter = new SelectableParametersReportFilter(UoW);

			var nomenclatureParam = filter.CreateParameterSet(
				"Номенклатуры",
				"nomenclature",
				new ParametersFactory(UoW, (filters) => {
					SelectableEntityParameter<Nomenclature> resultAlias = null;
					var query = UoW.Session.QueryOver<Nomenclature>()
						.Where(x => !x.IsArchive);
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							var filterCriterion = f();
							if(filterCriterion != null) {
								query.Where(filterCriterion);
							}
						}
					}

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(x => x.OfficialName).WithAlias(() => resultAlias.EntityTitle)
						);
					query.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Nomenclature>>());
					return query.List<SelectableParameter>();
				})
			);

			var nomenclatureTypeParam = filter.CreateParameterSet(
				"Типы номенклатур",
				"nomenclature_type",
				new ParametersEnumFactory<NomenclatureCategory>()
			);

			nomenclatureParam.AddFilterOnSourceSelectionChanged(nomenclatureTypeParam,
				() => {
					var selectedValues = nomenclatureTypeParam.GetSelectedValues();
					if(!selectedValues.Any()) {
						return null;
					}
					return Restrictions.On<Nomenclature>(x => x.Category).IsIn(nomenclatureTypeParam.GetSelectedValues().ToArray());
				}
			);

			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();

			filter.CreateParameterSet(
				"Группы товаров",
				"product_group",
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) => {
					var query = UoW.Session.QueryOver<ProductGroup>();
					if(filters != null && filters.Any()) {
						foreach(var f in filters) {
							query.Where(f());
						}
					}
					return query.List();
				},
				x => x.Name,
				x => x.Childs)
			);

			var filterViewModel = new SelectableParameterReportFilterViewModel(filter);
			var filterWidget = new SelectableParameterReportFilterView(filterViewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();

			ConfigNomenclatureColumns();
		}

		public override bool Save ()
		{
			if(!Entity.CanEdit)
				return false;

			var valid = new QSValidator<InventoryDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.LastEditor = EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditor == null)
			{
				MessageDialogHelper.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			Entity.UpdateOperations(UoW);

			logger.Info ("Сохраняем акт списания...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint (typeof(InventoryDocument), "акта инвентаризации"))
				Save ();

			var reportInfo = new QS.Report.ReportInfo {
				Title = String.Format ("Акт инвентаризации №{0} от {1:d}", Entity.Id, Entity.TimeStamp),
				Identifier = "Store.InventoryDoc",
				Parameters = new Dictionary<string, object> {
					{ "inventory_id",  Entity.Id }
				}
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg (reportInfo)
			);
		}

		protected void OnYentryrefWarehouseBeforeChangeByUser(object sender, EntryReferenceBeforeChangeEventArgs e)
		{
			if(Entity.Warehouse != null && Entity.Items.Count > 0)
			{
				if (MessageDialogHelper.RunQuestionDialog("При изменении склада табличная часть документа будет очищена. Продолжить?"))
					Entity.ObservableItems.Clear();
				else
					e.CanChange = false;
			}
		}

		#region Nomenclatures

		IEnumerable<Nomenclature> nomenclaturesWithDiscrepancies = new List<Nomenclature>();

		private void ConfigNomenclatureColumns()
		{
			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<InventoryDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => GetNomenclatureName(x.Nomenclature), useMarkup: true)
				.AddColumn("Кол-во в учёте").AddTextRenderer(x => x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB) : x.AmountInDB.ToString())
				.AddColumn("Кол-во по факту").AddNumericRenderer(x => x.AmountInFact).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
				.AddSetter((w, x) => w.Digits = (x.Nomenclature.Unit != null ? (uint)x.Nomenclature.Unit.Digits : 1))
				.AddColumn("Разница").AddTextRenderer(x => x.Difference != 0 && x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : String.Empty)
				.AddSetter((w, x) => w.Foreground = x.Difference < 0 ? "red" : "blue")
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : String.Empty)
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.RowCells()
				.AddSetter<CellRenderer>((cell, node) => {
					Color color = new Color(255, 255, 255);
					if(nomenclaturesWithDiscrepancies.Any(x => x.Id == node.Nomenclature.Id)) {
						color = new Color(255, 125, 125);
					}
					cell.CellBackgroundGdk = color;
				})
				.Finish();

			ytreeviewItems.ItemsDataSource = Entity.ObservableItems;
			ytreeviewItems.YTreeModel?.EmitModelChanged();

			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		private string GetNomenclatureName(Nomenclature nomenclature)
		{
			if(nomenclaturesWithDiscrepancies.Any(x => x.Id == nomenclature.Id)) {
				return $"<b>{nomenclature.Name}</b>";
			}
			return nomenclature.Name;
		}

		void YtreeviewItems_Selection_Changed(object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<InventoryDocumentItem>();
			buttonFine.Sensitive = selected != null;
			if(selected != null) {
				if(selected.Fine != null)
					buttonFine.Label = "Изменить штраф";
				else
					buttonFine.Label = "Добавить штраф";
			}
			buttonDeleteFine.Sensitive = selected != null && selected.Fine != null;
		}

		private void FillDiscrepancies()
		{
			if(Entity.Warehouse != null && Entity.Warehouse.Id > 0) {
				var warehouseRepository = new WarehouseRepository();
				nomenclaturesWithDiscrepancies = warehouseRepository.GetDiscrepancyNomenclatures(UoW, Entity.Warehouse.Id);
			}
		}

		protected void OnButtonFillItemsClicked(object sender, EventArgs e)
		{
			// Костыль для передачи из фильтра предназначенного только для отчетов данных в подходящем виде
			List<int> nomenclaturesToInclude = new List<int>();
			List<int> nomenclaturesToExclude = new List<int>();
			List<string> nomenclatureCategoryToInclude = new List<string>();
			List<string> nomenclatureCategoryToExclude = new List<string>();
			List<int> productGroupToInclude = new List<int>();
			List<int> productGroupToExclude = new List<int>();

			foreach(SelectableParameterSet parameterSet in filter.ParameterSets) {
				switch(parameterSet.ParameterName) {
					case "nomenclature":
						if(parameterSet.FilterType == SelectableFilterType.Include) {
							foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
								nomenclaturesToInclude.Add(value.EntityId);
							}
						} else {
							foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
								nomenclaturesToExclude.Add(value.EntityId);
							}
						}
						break;
					case "nomenclature_type":
						if(parameterSet.FilterType == SelectableFilterType.Include) {
							foreach(SelectableEnumParameter<NomenclatureCategory> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
								nomenclatureCategoryToInclude.Add(value.Value.ToString());
							}
						} else {
							foreach(SelectableEnumParameter<NomenclatureCategory> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
								nomenclatureCategoryToExclude.Add(value.Value.ToString());
							}
						}
						break;
					case "product_group":
						if(parameterSet.FilterType == SelectableFilterType.Include) {
							foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
								productGroupToInclude.Add(value.EntityId);
							}
						} else {
							foreach(SelectableEntityParameter<ProductGroup> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
								productGroupToExclude.Add(value.EntityId);
							}
						}
						break;
				}
			}

			FillDiscrepancies();

			if(Entity.Items.Count == 0)
				Entity.FillItemsFromStock(UoW,
					nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
					nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
					nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
					nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
					productGroupToInclude: productGroupToInclude.ToArray(),
					productGroupToExclude: productGroupToExclude.ToArray());
			else
				Entity.UpdateItemsFromStock(UoW,
					nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
					nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
					nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
					nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
					productGroupToInclude: productGroupToInclude.ToArray(),
					productGroupToExclude: productGroupToExclude.ToArray());
			UpdateButtonState();
		}

		private void UpdateButtonState()
		{
			buttonFillItems.Sensitive = Entity.Warehouse != null;
			if(Entity.Items.Count == 0)
				buttonFillItems.Label = "Заполнить по складу";
			else
				buttonFillItems.Label = "Обновить остатки";
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var nomenclatureSelectDlg = new OrmReference(nomenclatureRepository.NomenclatureOfGoodsOnlyQuery());
			nomenclatureSelectDlg.Mode = OrmReferenceMode.Select;
			nomenclatureSelectDlg.ObjectSelected += NomenclatureSelectDlg_ObjectSelected;
			TabParent.AddSlaveTab(this, nomenclatureSelectDlg);
		}

		void NomenclatureSelectDlg_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var nomenclature = e.Subject as Nomenclature;
			if(Entity.Items.Any(x => x.Nomenclature.Id == nomenclature.Id))
				return;

			Entity.AddItem(nomenclature, 0, 0);
		}

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<InventoryDocumentItem>();
			FineDlg fineDlg;
			if(selected.Fine != null) {
				fineDlg = new FineDlg(selected.Fine);
				fineDlg.EntitySaved += FineDlgExist_EntitySaved;
			} else {
				fineDlg = new FineDlg("Недостача");
				fineDlg.EntitySaved += FineDlgNew_EntitySaved;
			}
			fineDlg.Entity.TotalMoney = selected.SumOfDamage;
			FineEditItem = selected;
			TabParent.AddSlaveTab(this, fineDlg);
		}

		protected void OnButtonDeleteFineClicked(object sender, EventArgs e)
		{
			var item = ytreeviewItems.GetSelectedObject<InventoryDocumentItem>();
			UoW.Delete(item.Fine);
			item.Fine = null;
			YtreeviewItems_Selection_Changed(null, EventArgs.Empty);
		}

		void FineDlgNew_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			FineEditItem.Fine = e.Entity as Fine;
			FineEditItem = null;
		}

		void FineDlgExist_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			//Мы здесь не можем выполнить просто рефреш, так как если удалить сотрудника из штрафа, получаем эксепшен.
			int id = FineEditItem.Fine.Id;
			UoW.Session.Evict(FineEditItem.Fine);
			FineEditItem.Fine = UoW.GetById<Fine>(id);
		}

		#endregion
	}
}

