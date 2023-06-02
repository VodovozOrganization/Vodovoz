using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Navigation;
using QSOrmProject;
using QSProjectsLib;
using QS.Tdi;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.EntityRepositories.Store;
using QS.Project.Services;
using QS.Project.Journal;
using Vodovoz.JournalViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.JournalSelector;
using Vodovoz.Domain.Client;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Parameters;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RegradingOfGoodsDocumentItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private readonly IStockRepository _stockRepository = new StockRepository();
		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		
		RegradingOfGoodsDocumentItem newRow;
		RegradingOfGoodsDocumentItem FineEditItem;

		public RegradingOfGoodsDocumentItemsView()
		{
			this.Build();
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

			List<CullingCategory> types;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				types = uow.GetAll<CullingCategory>().OrderBy(c => c.Name).ToList();
			}

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<RegradingOfGoodsDocumentItem>()
				.AddColumn("Старая номенклатура").AddTextRenderer(x => x.NomenclatureOld.Name)
				.AddColumn("Кол-во на складе").AddTextRenderer(x => x.NomenclatureOld.Unit.MakeAmountShortStr(x.AmountInStock))
				.AddColumn("Новая номенклатура").AddTextRenderer(x => x.NomenclatureNew.Name)
				.AddColumn("Кол-во пересортицы").AddNumericRenderer(x => x.Amount).Editing()
				.AddSetter(
					(w, x) => w.Adjustment = new Gtk.Adjustment(
						0,
						0,
						GetMaxValueForAdjustmentSetting(x),
						1,
						10,
						10
					)
				)
				.AddSetter((w, x) => w.Digits = (uint)x.NomenclatureNew.Unit.Digits)
				.AddSetter(
					(w, x) => x.Amount = x.Amount > (decimal)GetMaxValueForAdjustmentSetting(x)
					? (decimal)GetMaxValueForAdjustmentSetting(x)
					: x.Amount
				)
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : String.Empty)
				.AddColumn("Тип брака")
					.AddComboRenderer(x => x.TypeOfDefect)
					.SetDisplayFunc(x => x.Name)
					.FillItems(types)
					.AddSetter(
						(c, n) =>
						{
							if(!n.NomenclatureNew.IsDefectiveBottle)
								n.TypeOfDefect = null;
							c.Editable = n.NomenclatureNew.IsDefectiveBottle;
							c.BackgroundGdk = n.NomenclatureNew.IsDefectiveBottle && n.TypeOfDefect == null
								? colorLightRed
								: colorWhite;
						}
					)
				.AddColumn("Источник\nбрака")
					.AddEnumRenderer(x => x.Source, true, new Enum[] { DefectSource.None })
					.AddSetter(
						(c, n) =>
						{
							if(!n.NomenclatureNew.IsDefectiveBottle)
								n.Source = DefectSource.None;
							c.Editable = n.NomenclatureNew.IsDefectiveBottle;
							c.BackgroundGdk = n.NomenclatureNew.IsDefectiveBottle && n.Source == DefectSource.None
								? colorLightRed
								: colorWhite;
						}
					)
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.Finish();
			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		double GetMaxValueForAdjustmentSetting(RegradingOfGoodsDocumentItem item){
			if(item.NomenclatureOld.Category == NomenclatureCategory.bottle
			   && item.NomenclatureNew.Category == NomenclatureCategory.water)
				return 39;
			return (double)item.AmountInStock;
		}

		void YtreeviewItems_Selection_Changed (object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		private IUnitOfWorkGeneric<RegradingOfGoodsDocument> documentUoW;

		public IUnitOfWorkGeneric<RegradingOfGoodsDocument> DocumentUoW {
			get { return documentUoW; }
			set {
				if (documentUoW == value)
					return;
				documentUoW = value;
				if (DocumentUoW.Root.Items == null)
					DocumentUoW.Root.Items = new List<RegradingOfGoodsDocumentItem> ();

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				UpdateButtonState();
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;
				if (!DocumentUoW.IsNew)
					LoadStock();
			}
		}

		private void UpdateButtonState()
		{
			var selected = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			buttonChangeNew.Sensitive = buttonDelete.Sensitive = selected != null;
			buttonChangeOld.Sensitive = selected != null && DocumentUoW.Root.Warehouse != null;
			buttonAdd.Sensitive = buttonFromTemplate.Sensitive = DocumentUoW.Root.Warehouse != null;

			buttonFine.Sensitive = selected != null;
			if(selected != null)
			{
				if (selected.Fine != null)
					buttonFine.Label = "Изменить штраф";
				else
					buttonFine.Label = "Добавить штраф";
			}
			buttonDeleteFine.Sensitive = selected != null && selected.Fine != null;
		}

		void DocumentUoW_Root_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == DocumentUoW.Root.GetPropertyName(x => x.Warehouse))
				UpdateButtonState();
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			Action<NomenclatureStockFilterViewModel> filterParams = f => f.RestrictWarehouse = DocumentUoW.Root.Warehouse;

			var vm = MainClass.MainWin.NavigationManager
				.OpenViewModel<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(null, filterParams)
				.ViewModel;
			
			vm.SelectionMode = JournalSelectionMode.Single;
			vm.TabName = "Выберите номенклатуру на замену";
			vm.OnEntitySelectedResult += (s, ea) => {
				var selectedNode = ea.SelectedNodes.Cast<NomenclatureStockJournalNode>().FirstOrDefault();
				if(selectedNode == null) {
					return;
				}
				var nomenclature = DocumentUoW.GetById<Nomenclature>(selectedNode.Id);

				newRow = new RegradingOfGoodsDocumentItem() {
					NomenclatureOld = nomenclature,
					AmountInStock = selectedNode.StockAmount
				};

				var nomenclatureFilter = new NomenclatureFilterViewModel();

				var userRepository = new UserRepository();

				var employeeService = VodovozGtkServicesConfig.EmployeeService;

				var counterpartySelectorFactory = new CounterpartyJournalFactory();

				var nomenclatureAutoCompleteSelectorFactory =
					new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
						ServicesConfig.CommonServices,
						nomenclatureFilter,
						counterpartySelectorFactory,
						_nomenclatureRepository,
						userRepository
						);

				var nomenclaturesJournalViewModel =
				new NomenclaturesJournalViewModel(
					nomenclatureFilter,
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					employeeService,
					new NomenclatureJournalFactory(),
					counterpartySelectorFactory,
					_nomenclatureRepository,
					userRepository
					);

				nomenclaturesJournalViewModel.SelectionMode = JournalSelectionMode.Single;
                nomenclaturesJournalViewModel.OnEntitySelectedResult += SelectNewNomenclature_ObjectSelected;

				MyTab.TabParent.AddSlaveTab(MyTab, nomenclaturesJournalViewModel);
			};
		}

        void SelectNewNomenclature_ObjectSelected (object sender, JournalSelectedNodesEventArgs e)
		{
			var journalNode = e?.SelectedNodes?.FirstOrDefault();
			if (journalNode != null)
            {
				var nomenclature = DocumentUoW.GetById<Nomenclature>(journalNode.Id);

				if (!nomenclature.IsDefectiveBottle)
				{
					newRow.Source = DefectSource.None;
					newRow.TypeOfDefect = null;
				}

				newRow.NomenclatureNew = nomenclature;
				DocumentUoW.Root.AddItem(newRow);
			}
		}

		private void LoadStock()
		{
			var nomenclatureIds = DocumentUoW.Root.Items.Select(x => x.NomenclatureOld.Id).ToArray();
			var inStock = _stockRepository.NomenclatureInStock(DocumentUoW, nomenclatureIds, DocumentUoW.Root.Warehouse.Id,
				DocumentUoW.Root.TimeStamp);

			foreach(var item in DocumentUoW.Root.Items)
			{
				item.AmountInStock = inStock[item.NomenclatureOld.Id];
			}
		}

		protected void OnButtonChangeOldClicked(object sender, EventArgs e)
		{
			Action<NomenclatureStockFilterViewModel> filterParams = f => f.RestrictWarehouse = DocumentUoW.Root.Warehouse;

			var vm = MainClass.MainWin.NavigationManager
				.OpenViewModel<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(null, filterParams)
				.ViewModel;

			vm.SelectionMode = JournalSelectionMode.Single;
			vm.TabName = "Изменить старую номенклатуру";
			vm.OnEntitySelectedResult += (s, ea) => {
				var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
				var selectedNode = ea.SelectedNodes.Cast<NomenclatureStockJournalNode>().FirstOrDefault();
				if(selectedNode == null) {
					return;
				}
				var nomenclature = DocumentUoW.GetById<Nomenclature>(selectedNode.Id);
				row.NomenclatureOld = nomenclature;
				row.AmountInStock = selectedNode.StockAmount;
			};
		}

		protected void OnButtonChangeNewClicked(object sender, EventArgs e)
		{
			var filter = new NomenclatureFilterViewModel();

			var userRepository = new UserRepository();

			var employeeService = VodovozGtkServicesConfig.EmployeeService;
			var counterpartyJournalFactory = new CounterpartyJournalFactory();

			var nomenclatureAutoCompleteSelectorFactory = 
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices,
					filter,
					counterpartyJournalFactory,
					_nomenclatureRepository,
					userRepository
					);

			var nomenclaturesJournalViewModel = 
				new NomenclaturesJournalViewModel(
					filter,
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					employeeService,
					new NomenclatureJournalFactory(),
					counterpartyJournalFactory,
					_nomenclatureRepository,
					userRepository
					);

			nomenclaturesJournalViewModel.SelectionMode = JournalSelectionMode.Single;
			nomenclaturesJournalViewModel.OnEntitySelectedResult += ChangeNewNomenclature_OnEntitySelectedResult;

			MyTab.TabParent.AddSlaveTab(MyTab, nomenclaturesJournalViewModel);
		}

		private void ChangeNewNomenclature_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
        {
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			if (row == null)
            {
				return;
			}

			var id = e.SelectedNodes.FirstOrDefault()?.Id;

			if (id == null)
            {
				return;
            }

			var nomenclature = UoW.Session.Get<Nomenclature>(id);
			row.NomenclatureNew = nomenclature;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			if (row.WarehouseIncomeOperation.Id == 0)
				DocumentUoW.Delete(row.WarehouseIncomeOperation);
			if (row.WarehouseWriteOffOperation.Id == 0)
				DocumentUoW.Delete(row.WarehouseWriteOffOperation);
			if(row.Id != 0)
				DocumentUoW.Delete(row);
			DocumentUoW.Root.ObservableItems.Remove(row);
		}

		protected void OnYtreeviewItemsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if (args.Column.Title == "Старая номенклатура")
				buttonChangeOld.Click();
			if (args.Column.Title == "Новая номенклатура")
				buttonChangeNew.Click();
		}

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			FineDlg fineDlg;
			if (selected.Fine != null)
			{
				fineDlg = new FineDlg(selected.Fine);
				fineDlg.EntitySaved += FineDlgExist_EntitySaved;
			}
			else
			{
				fineDlg = new FineDlg("Недостача");
				fineDlg.EntitySaved += FineDlgNew_EntitySaved;
			}
			fineDlg.Entity.TotalMoney = selected.SumOfDamage;
			FineEditItem = selected;
			MyTab.TabParent.AddSlaveTab(MyTab, fineDlg);
		}

		void FineDlgNew_EntitySaved (object sender, EntitySavedEventArgs e)
		{
			FineEditItem.Fine = e.Entity as Fine;
			FineEditItem = null;
		}

		void FineDlgExist_EntitySaved (object sender, EntitySavedEventArgs e)
		{
			DocumentUoW.Session.Refresh(FineEditItem.Fine);
		}

		protected void OnButtonDeleteFineClicked(object sender, EventArgs e)
		{
			var item = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			DocumentUoW.Delete(item.Fine);
			item.Fine = null;
			UpdateButtonState();
		}

		protected void OnButtonFromTemplateClicked(object sender, EventArgs e)
		{
			var selectTemplate = new OrmReference(typeof(RegradingOfGoodsTemplate));
			selectTemplate.Mode = OrmReferenceMode.Select;
			selectTemplate.ObjectSelected += SelectTemplate_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, selectTemplate);
		}

		void SelectTemplate_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if (DocumentUoW.Root.Items.Count > 0)
			{
				if (MessageDialogWorks.RunQuestionDialog("Текущий список будет очищен. Продолжить?"))
					DocumentUoW.Root.ObservableItems.Clear();
				else
					return;
			}

			var template = DocumentUoW.GetById<RegradingOfGoodsTemplate>((e.Subject as RegradingOfGoodsTemplate).Id);
			foreach(var item in template.Items)
			{
				DocumentUoW.Root.AddItem(new RegradingOfGoodsDocumentItem()
					{
						NomenclatureNew = item.NomenclatureNew,
						NomenclatureOld = item.NomenclatureOld
					});
			}
			LoadStock();
		}
	}
}

