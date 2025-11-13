using Autofac;
using Gamma.GtkWidgets;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Report;
using QSOrmProject;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.PermissionExtensions;
using Vodovoz.ReportsParameters;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Dialogs.DocumentDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShiftChangeWarehouseDocumentDlg : QS.Dialog.Gtk.EntityDialogBase<ShiftChangeWarehouseDocument>
	{
		private static ILogger<ShiftChangeWarehouseDocumentDlg> _logger;

		private IEmployeeRepository _employeeRepository;
		private IStockRepository _stockRepository;

		private SelectableParametersReportFilter _filter;

		public ShiftChangeWarehouseDocumentDlg()
		{
			ResolveDependencies();
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<ShiftChangeWarehouseDocument>();
			Entity.AuthorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			if(Entity.AuthorId == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать складские документы, так как некого указывать в качестве кладовщика.");
				FailInitialize = true;
				return;
			}

			var storeDocument = new StoreDocumentHelper(new UserSettingsService());
			if(UoW.IsNew)
				Entity.Warehouse = storeDocument.GetDefaultWarehouse(UoW, WarehousePermissionsType.ShiftChangeCreate);
			if(!UoW.IsNew)
				Entity.Warehouse = storeDocument.GetDefaultWarehouse(UoW, WarehousePermissionsType.ShiftChangeEdit);

			ConfigureDlg(storeDocument);
		}

		public ShiftChangeWarehouseDocumentDlg(int id)
		{
			ResolveDependencies();
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<ShiftChangeWarehouseDocument>(id);
			
			var storeDocument = new StoreDocumentHelper(new UserSettingsService());
			ConfigureDlg(storeDocument);
		}

		private void ResolveDependencies()
		{
			_employeeRepository = ScopeProvider.Scope.Resolve<IEmployeeRepository>();
			_stockRepository = ScopeProvider.Scope.Resolve<IStockRepository>();
		}

		public ShiftChangeWarehouseDocumentDlg(ShiftChangeWarehouseDocument sub) : this (sub.Id)
		{
		}

		bool canCreate;
		bool canEdit;

		public bool CanSave => canCreate || canEdit;

		void ConfigureDlg(StoreDocumentHelper storeDocument)
		{
			canEdit = !UoW.IsNew && storeDocument.CanEditDocument(WarehousePermissionsType.ShiftChangeEdit, Entity.Warehouse);

			if(Entity.Id != 0 && Entity.TimeStamp < DateTime.Today)
			{
				var permissionValidator = 
					new EntityExtendedPermissionValidator(ServicesConfig.UnitOfWorkFactory, PermissionExtensionSingletonStore.GetInstance(), _employeeRepository);
				
				canEdit &= permissionValidator.Validate(
					typeof(ShiftChangeWarehouseDocument), ServicesConfig.UserService.CurrentUserId, nameof(RetroactivelyClosePermission));
			}

			canCreate = UoW.IsNew && !storeDocument.CheckCreateDocument(WarehousePermissionsType.ShiftChangeCreate, Entity.Warehouse);

			if(!canCreate && UoW.IsNew){
				FailInitialize = true;
				return;
			}

			if(!canEdit && !UoW.IsNew)
				MessageDialogHelper.RunWarningDialog("У вас нет прав на изменение этого документа.");

			ydatepickerDocDate.Sensitive = yentryrefWarehouse.IsEditable = ytextviewCommnet.Editable = canEdit || canCreate;

			ytreeviewNomenclatures.Sensitive = 
				buttonFillItems.Sensitive = 
				buttonAdd.Sensitive = canEdit || canCreate;

			ytreeviewNomenclatures.ItemsDataSource = Entity.ObservableNomenclatureItems;
			ytreeviewNomenclatures.YTreeModel?.EmitModelChanged();

			ydatepickerDocDate.Binding.AddBinding(Entity, e => e.TimeStamp, w => w.Date).InitializeFromSource();
			if(UoW.IsNew)
				yentryrefWarehouse.ItemsQuery = storeDocument.GetRestrictedWarehouseQuery(WarehousePermissionsType.ShiftChangeCreate);
			if(!UoW.IsNew)
				yentryrefWarehouse.ItemsQuery = storeDocument.GetRestrictedWarehouseQuery(WarehousePermissionsType.ShiftChangeEdit);
			yentryrefWarehouse.Binding.AddBinding(Entity, e => e.Warehouse, w => w.Subject).InitializeFromSource();
			yentryrefWarehouse.Changed += OnWarehouseChanged;

			ytextviewCommnet.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			string errorMessage = "Не установлены единицы измерения у следующих номенклатур :" + Environment.NewLine;
			int wrongNomenclatures = 0;
			foreach(var item in Entity.NomenclatureItems) {
				if(item.Nomenclature.Unit == null) {
					errorMessage += string.Format("Номер: {0}. Название: {1}{2}",
						item.Nomenclature.Id, item.Nomenclature.Name, Environment.NewLine);
					wrongNomenclatures++;
				}
			}
			if(wrongNomenclatures > 0) {
				MessageDialogHelper.RunErrorDialog(errorMessage);
				FailInitialize = true;
				return;
			}

			_filter = new SelectableParametersReportFilter(UoW);

			var nomenclatureParam = _filter.CreateParameterSet(
				"Номенклатуры",
				nameof(Nomenclature),
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

			var nomenclatureTypeParam = _filter.CreateParameterSet(
				"Типы номенклатур",
				nameof(NomenclatureCategory),
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

			ProductGroup productGroupChildAlias = null;
			//Предзагрузка. Для избежания ленивой загрузки
			UoW.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs,
					() => productGroupChildAlias,
					() => !productGroupChildAlias.IsArchive)
				.Fetch(SelectMode.Fetch, () => productGroupChildAlias)
				.List();

			_filter.CreateParameterSet(
				"Группы товаров",
				nameof(ProductGroup),
				new RecursiveParametersFactory<ProductGroup>(UoW,
				(filters) => {
					var query = UoW.Session.QueryOver<ProductGroup>()
						.Where(p => p.Parent == null)
						.And(p => !p.IsArchive);
					
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

			var filterViewModel = new SelectableParameterReportFilterViewModel(_filter);
			var filterWidget = new SelectableParameterReportFilterView(filterViewModel);
			vboxParameters.Add(filterWidget);
			filterWidget.Show();

			ConfigureNomenclaturesView();
		}

		private void ConfigureNomenclaturesView()
		{
			ytreeviewNomenclatures.ColumnsConfig = ColumnsConfigFactory.Create<ShiftChangeWarehouseDocumentItem>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Кол-во в учёте").AddTextRenderer(x => x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.AmountInDB) : x.AmountInDB.ToString())
				.AddColumn("Кол-во по факту").AddNumericRenderer(x => x.AmountInFact).Editing()
					.Adjustment(new Gtk.Adjustment(0, 0, 10000000, 1, 10, 10))
					.AddSetter((w, x) => w.Digits = (x.Nomenclature.Unit != null ? (uint)x.Nomenclature.Unit.Digits : 1))
				.AddColumn("Разница").AddTextRenderer(x => x.Difference != 0 && x.Nomenclature.Unit != null ? x.Nomenclature.Unit.MakeAmountShortStr(x.Difference) : String.Empty)
					.AddSetter((w, x) => w.ForegroundGdk = x.Difference < 0 ? GdkColors.DangerText : GdkColors.InfoText)
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.Finish();
		}

		public override bool Save()
		{
			if(!CanSave)
				return false;

			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			Entity.LastEditorId = _employeeRepository.GetEmployeeForCurrentUser(UoW)?.Id;
			Entity.LastEditedTime = DateTime.Now;
			if(Entity.LastEditorId == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете изменять складские документы, так как некого указывать в качестве кладовщика.");
				return false;
			}

			_logger.LogInformation("Сохраняем акт списания...");
			UoWGeneric.Save();
			_logger.LogInformation("Ok.");
			return true;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(ShiftChangeWarehouseDocument), "акта передачи склада"))
				Save();

			var reportInfoFactory = ScopeProvider.Scope.Resolve<IReportInfoFactory>();
			var reportInfo = reportInfoFactory.Create();
			reportInfo.Title = String.Format("Акт передачи склада №{0} от {1:d}", Entity.Id, Entity.TimeStamp);
			reportInfo.Identifier = "Store.ShiftChangeWarehouse";
			reportInfo.Parameters = new Dictionary<string, object> {
				{ "document_id",  Entity.Id }
			};

			TabParent.OpenTab(
				QSReport.ReportViewDlg.GenerateHashName(reportInfo),
				() => new QSReport.ReportViewDlg(reportInfo)
			);
		}

		protected void OnYentryrefWarehouseBeforeChangeByUser(object sender, EntryReferenceBeforeChangeEventArgs e)
		{
			if(Entity.Warehouse != null && Entity.NomenclatureItems.Count > 0) {
				if(MessageDialogHelper.RunQuestionDialog("При изменении склада табличная часть документа будет очищена. Продолжить?"))
					Entity.ObservableNomenclatureItems.Clear();
				else
					e.CanChange = false;
			}
		}

		#region NomenclaturesView

		protected void OnButtonFillItemsClicked(object sender, EventArgs e)
		{
			// Костыль для передачи из фильтра предназначенного только для отчетов данных в подходящем виде
			List<int> nomenclaturesToInclude = new List<int>();
			List<int> nomenclaturesToExclude = new List<int>();
			var nomenclatureCategoryToInclude = new List<NomenclatureCategory>();
			var nomenclatureCategoryToExclude = new List<NomenclatureCategory>();
			List<int> productGroupToInclude = new List<int>();
			List<int> productGroupToExclude = new List<int>();

			foreach (SelectableParameterSet parameterSet in _filter.ParameterSets) {
				switch(parameterSet.ParameterName) {
					case nameof(Nomenclature):
						if (parameterSet.FilterType == SelectableFilterType.Include) {
							foreach (SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
								nomenclaturesToInclude.Add(value.EntityId);
							}
						} else {
							foreach(SelectableEntityParameter<Nomenclature> value in parameterSet.OutputParameters.Where(x => x.Selected)) {
								nomenclaturesToExclude.Add(value.EntityId);
							}
						}
						break;
					case nameof(NomenclatureCategory):
						if(parameterSet.FilterType == SelectableFilterType.Include) {
							foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
								nomenclatureCategoryToInclude.Add((NomenclatureCategory)value.Value);
							}
						} else {
							foreach(var selectableParameter in parameterSet.OutputParameters.Where(x => x.Selected))
							{
								var value = (SelectableEnumParameter<NomenclatureCategory>)selectableParameter;
								nomenclatureCategoryToExclude.Add((NomenclatureCategory)value.Value);
							}
						}
						break;
					case nameof(ProductGroup):
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

			if(Entity.NomenclatureItems.Count == 0)
			{
				Entity.FillNomenclatureItemsFromStock(
					UoW,
					_stockRepository,
					nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
					nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
					nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
					nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
					productGroupToInclude: productGroupToInclude.ToArray(),
					productGroupToExclude: productGroupToExclude.ToArray());
			}
			else
			{
				Entity.UpdateItemsFromStock(
					UoW,
					_stockRepository,
					nomenclaturesToInclude: nomenclaturesToInclude.ToArray(),
					nomenclaturesToExclude: nomenclaturesToExclude.ToArray(),
					nomenclatureTypeToInclude: nomenclatureCategoryToInclude.ToArray(),
					nomenclatureTypeToExclude: nomenclatureCategoryToExclude.ToArray(),
					productGroupToInclude: productGroupToInclude.ToArray(),
					productGroupToExclude: productGroupToExclude.ToArray());
			}

			UpdateButtonState();
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var journal =
				Startup.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
					this,
					filter =>
					{
						filter.AvailableCategories = Nomenclature.GetCategoriesForGoods();
					},
					OpenPageOptions.AsSlave,
					vm => vm.SelectionMode = JournalSelectionMode.Single
				).ViewModel;
				
			journal.OnSelectResult += Journal_OnEntitySelectedResult;
		}

		private void Journal_OnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
			if(selectedNode == null)
			{
				return;
			}

			var selectedNomenclature = UoW.GetById<Nomenclature>(selectedNode.Id);
			if(Entity.NomenclatureItems.Any(x => x.Nomenclature.Id == selectedNomenclature.Id))
				return;

			Entity.AddItem(selectedNomenclature, 0, 0);
		}

		private void UpdateButtonState()
		{
			buttonFillItems.Sensitive = Entity.Warehouse != null;
			if(Entity.NomenclatureItems.Count == 0)
				buttonFillItems.Label = "Заполнить по складу";
			else
				buttonFillItems.Label = "Обновить остатки";
		}

		// change warehouse handler

		protected void OnWarehouseChanged(object sender, EventArgs e)
		{
			if(Entity.Warehouse != null)
				buttonFillItems.Click();
			UpdateButtonState();
		}

		#endregion
	}
}
