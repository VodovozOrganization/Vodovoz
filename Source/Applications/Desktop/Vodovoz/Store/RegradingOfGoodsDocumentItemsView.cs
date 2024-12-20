﻿using Autofac;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Tdi;
using QS.Utilities;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using VodovozBusiness.Domain.Documents;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RegradingOfGoodsDocumentItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private IStockRepository _stockRepository;
		private ILifetimeScope _lifetimeScope;
		private RegradingOfGoodsDocumentItem _newRow;
		private RegradingOfGoodsDocumentItem _fineEditItem;

		public RegradingOfGoodsDocumentItemsView()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
			var unitOfWorkFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();

			_stockRepository = _lifetimeScope.Resolve<IStockRepository>();
			Build();
			var basePrimary = GdkColors.PrimaryBase;
			var colorLightRed = GdkColors.DangerBase;

			List<CullingCategory> types;
			List<RegradingOfGoodsReason> regradingReasons;

			using(IUnitOfWork uow = unitOfWorkFactory.CreateWithoutRoot())
			{
				types = uow.GetAll<CullingCategory>().OrderBy(c => c.Name).ToList();
				regradingReasons = uow.GetAll<RegradingOfGoodsReason>().OrderBy(c => c.Name).ToList();
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
				.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : string.Empty)
				.AddColumn("Тип брака")
					.AddComboRenderer(x => x.TypeOfDefect)
					.SetDisplayFunc(x => x.Name)
					.FillItems(types)
					.AddSetter(
						(c, n) =>
						{
							if(!n.IsDefective)
							{
								n.TypeOfDefect = null;
							}

							c.Editable = n.IsDefective;
							c.BackgroundGdk =
								n.IsDefective
								&& n.TypeOfDefect == null
									? colorLightRed
									: basePrimary;
						}
					)
				.AddColumn("Источник\nбрака")
					.AddEnumRenderer(x => x.Source, true, new Enum[] { DefectSource.None })
					.AddSetter(
						(c, n) =>
						{
							if(!n.IsDefective)
							{
								n.Source = DefectSource.None;
							}

							c.Editable = n.IsDefective;
							c.BackgroundGdk =
								n.IsDefective
								&& n.Source == DefectSource.None
									? colorLightRed
									: basePrimary;
						}
					)
				.AddColumn("Что произошло").AddTextRenderer(x => x.Comment).Editable()
				.AddColumn("Причина пересортицы")
					.AddComboRenderer(x => x.RegradingOfGoodsReason)
					.SetDisplayFunc(x => x.Name)
					.FillItems(regradingReasons)
					.Editing()
				.Finish();
			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		public ITdiTab ParrentDlg { get; set; }

		public ITdiCompatibilityNavigation NavigationManager { get; set; }

		private double GetMaxValueForAdjustmentSetting(RegradingOfGoodsDocumentItem item)
		{
			if(item.NomenclatureOld.Category == NomenclatureCategory.bottle
			   && item.NomenclatureNew.Category == NomenclatureCategory.water)
			{
				return 39;
			}

			return (double)item.AmountInStock;
		}

		private void YtreeviewItems_Selection_Changed(object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		private IUnitOfWorkGeneric<RegradingOfGoodsDocument> _documentUoW;

		public IUnitOfWorkGeneric<RegradingOfGoodsDocument> DocumentUoW
		{
			get => _documentUoW;
			set
			{
				if(_documentUoW == value)
				{
					return;
				}

				_documentUoW = value;

				if(DocumentUoW.Root.Items == null)
				{
					DocumentUoW.Root.Items = new List<RegradingOfGoodsDocumentItem>();
				}

				ytreeviewItems.ItemsDataSource = DocumentUoW.Root.ObservableItems;
				UpdateButtonState();
				DocumentUoW.Root.PropertyChanged += DocumentUoW_Root_PropertyChanged;

				if(!DocumentUoW.IsNew)
				{
					LoadStock();
				}
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
				if(selected.Fine != null)
				{
					buttonFine.Label = "Изменить штраф";
				}
				else
				{
					buttonFine.Label = "Добавить штраф";
				}
			}

			buttonDeleteFine.Sensitive = selected != null && selected.Fine != null;
		}

		private void DocumentUoW_Root_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == DocumentUoW.Root.GetPropertyName(x => x.Warehouse))
			{
				UpdateButtonState();
			}
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			Action<NomenclatureStockFilterViewModel> filterParams = f => f.RestrictWarehouse = DocumentUoW.Root.Warehouse;

			var vm = NavigationManager.OpenViewModelOnTdi<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(ParrentDlg, filterParams)
				.ViewModel;

			vm.SelectionMode = JournalSelectionMode.Single;
			vm.TabName = "Выберите номенклатуру на замену";
			vm.OnEntitySelectedResult += (s, ea) =>
			{
				var selectedNode = ea.SelectedNodes.Cast<NomenclatureStockJournalNode>().FirstOrDefault();

				if(selectedNode == null)
				{
					return;
				}
				var nomenclature = DocumentUoW.GetById<Nomenclature>(selectedNode.Id);

				_newRow = new RegradingOfGoodsDocumentItem()
				{
					NomenclatureOld = nomenclature,
					AmountInStock = selectedNode.StockAmount
				};

				var nomenclaturesJournalViewModel = _lifetimeScope.Resolve<NomenclaturesJournalViewModel>();
				nomenclaturesJournalViewModel.SelectionMode = JournalSelectionMode.Single;
				nomenclaturesJournalViewModel.OnSelectResult += SelectNewNomenclature_ObjectSelected;

				MyTab.TabParent.AddSlaveTab(MyTab, nomenclaturesJournalViewModel);
			};
		}

		private void SelectNewNomenclature_ObjectSelected(object sender, JournalSelectedEventArgs e)
		{
			var journalNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();

			if(journalNode != null)
			{
				var nomenclature = DocumentUoW.GetById<Nomenclature>(journalNode.Id);

				if(!nomenclature.IsDefectiveBottle)
				{
					_newRow.Source = DefectSource.None;
					_newRow.TypeOfDefect = null;
				}

				_newRow.NomenclatureNew = nomenclature;
				DocumentUoW.Root.AddItem(_newRow);
			}
		}

		private void LoadStock()
		{
			var nomenclatureIds = DocumentUoW.Root.Items.Select(x => x.NomenclatureOld.Id).ToArray();
			var inStock =
				_stockRepository.NomenclatureInStock(
					DocumentUoW,
					nomenclatureIds,
					new[] { DocumentUoW.Root.Warehouse.Id },
					DocumentUoW.Root.TimeStamp);

			foreach(var item in DocumentUoW.Root.Items)
			{
				if(inStock.ContainsKey(item.NomenclatureOld.Id))
				{
					item.AmountInStock = inStock[item.NomenclatureOld.Id];
				}
			}
		}

		protected void OnButtonChangeOldClicked(object sender, EventArgs e)
		{
			Action<NomenclatureStockFilterViewModel> filterParams = f => f.RestrictWarehouse = DocumentUoW.Root.Warehouse;

			var vm = Startup.MainWin.NavigationManager
				.OpenViewModel<NomenclatureStockBalanceJournalViewModel, Action<NomenclatureStockFilterViewModel>>(null, filterParams)
				.ViewModel;

			vm.SelectionMode = JournalSelectionMode.Single;
			vm.TabName = "Изменить старую номенклатуру";
			vm.OnEntitySelectedResult += (s, ea) =>
			{
				var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();

				var selectedNode = ea.SelectedNodes
					.Cast<NomenclatureStockJournalNode>()
					.FirstOrDefault();

				if(selectedNode == null)
				{
					return;
				}

				var nomenclature = DocumentUoW.GetById<Nomenclature>(selectedNode.Id);
				row.NomenclatureOld = nomenclature;
				row.AmountInStock = selectedNode.StockAmount;
			};
		}

		protected void OnButtonChangeNewClicked(object sender, EventArgs e)
		{
			var nomenclaturesJournalViewModel = _lifetimeScope.Resolve<NomenclaturesJournalViewModel>();
			nomenclaturesJournalViewModel.SelectionMode = JournalSelectionMode.Single;
			nomenclaturesJournalViewModel.OnSelectResult += ChangeNewNomenclature_OnEntitySelectedResult;

			MyTab.TabParent.AddSlaveTab(MyTab, nomenclaturesJournalViewModel);
		}

		private void ChangeNewNomenclature_OnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();
			if(row == null)
			{
				return;
			}

			var id = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault()?.Id;

			if(id == null)
			{
				return;
			}

			var nomenclature = UoW.Session.Get<Nomenclature>(id);
			row.NomenclatureNew = nomenclature;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();

			if(row.WarehouseIncomeOperation.Id == 0)
			{
				DocumentUoW.Delete(row.WarehouseIncomeOperation);
			}

			if(row.WarehouseWriteOffOperation.Id == 0)
			{
				DocumentUoW.Delete(row.WarehouseWriteOffOperation);
			}

			if(row.Id != 0)
			{
				DocumentUoW.Delete(row);
			}

			DocumentUoW.Root.ObservableItems.Remove(row);
		}

		protected void OnYtreeviewItemsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if(args.Column.Title == "Старая номенклатура")
			{
				buttonChangeOld.Click();
			}

			if(args.Column.Title == "Новая номенклатура")
			{
				buttonChangeNew.Click();
			}
		}

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();

			if(selected.Fine != null)
			{
				var page = NavigationManager.OpenViewModelOnTdi<FineViewModel, IEntityUoWBuilder>(ParrentDlg, EntityUoWBuilder.ForOpen(selected.Fine.Id), OpenPageOptions.AsSlave);

				page.ViewModel.Entity.TotalMoney = selected.SumOfDamage;
				page.ViewModel.EntitySaved += OnFineDlgExistEntitySaved;
			}
			else
			{
				var page = NavigationManager.OpenViewModelOnTdi<FineViewModel, IEntityUoWBuilder>(ParrentDlg, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

				page.ViewModel.Entity.FineReasonString = "Недостача";
				page.ViewModel.Entity.TotalMoney = selected.SumOfDamage;
				page.ViewModel.EntitySaved += OnFineDlgNewEntitySaved;
			}

			_fineEditItem = selected;
		}

		private void OnFineDlgNewEntitySaved(object sender, EntitySavedEventArgs e)
		{
			_fineEditItem.Fine = e.Entity as Fine;
			_fineEditItem = null;
		}

		private void OnFineDlgExistEntitySaved(object sender, EntitySavedEventArgs e)
		{
			DocumentUoW.Session.Refresh(_fineEditItem.Fine);
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

		private void SelectTemplate_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if(DocumentUoW.Root.Items.Count > 0)
			{
				if(MessageDialogHelper.RunQuestionDialog("Текущий список будет очищен. Продолжить?"))
				{
					DocumentUoW.Root.ObservableItems.Clear();
				}
				else
				{
					return;
				}
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

		public override void Destroy()
		{
			_stockRepository = null;

			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}

			base.Destroy();
		}
	}
}
