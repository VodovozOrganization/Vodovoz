using Autofac;
using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.TempAdapters;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RegradingOfGoodsTemplateItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private RegradingOfGoodsTemplateItem _newRow;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;

		public RegradingOfGoodsTemplateItemsView()
		{
			Build();

			_nomenclatureSelectorFactory = new NomenclatureJournalFactory();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<RegradingOfGoodsTemplateItem>()
				.AddColumn("Старая номенклатура").AddTextRenderer(x => x.NomenclatureOld.Name)
				.AddColumn("Новая номенклатура").AddTextRenderer(x => x.NomenclatureNew.Name)
				.Finish();
			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
		}

		void YtreeviewItems_Selection_Changed (object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		private IUnitOfWorkGeneric<RegradingOfGoodsTemplate> templateUoW;

		public IUnitOfWorkGeneric<RegradingOfGoodsTemplate> TemplateUoW {
			get { return templateUoW; }
			set {
				if (templateUoW == value)
					return;
				templateUoW = value;
				if (TemplateUoW.Root.Items == null)
					TemplateUoW.Root.Items = new List<RegradingOfGoodsTemplateItem> ();

				ytreeviewItems.ItemsDataSource = TemplateUoW.Root.ObservableItems;
				UpdateButtonState();
			}
		}

		private void UpdateButtonState()
		{
			var selected = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			buttonChangeNew.Sensitive = buttonDelete.Sensitive = buttonChangeOld.Sensitive = selected != null;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var oldNomenclatureSelector =
				CreateNomenclatureSelector("Выберите номенклатуру на замену", OldNomenclatureSelectorOnEntitySelectedResult);
			MyTab.TabParent.AddSlaveTab(MyTab, oldNomenclatureSelector);
		}
		
		private IEntitySelector CreateNomenclatureSelector(string tabName, EventHandler<JournalSelectedNodesEventArgs> onEntitySelectResult)
		{
			var newNomenclatureSelector = _nomenclatureSelectorFactory.CreateNomenclatureSelector(_lifetimeScope, multipleSelect: false);
			(newNomenclatureSelector as JournalViewModelBase).TabName = tabName;
			newNomenclatureSelector.OnEntitySelectedResult += onEntitySelectResult;
			return newNomenclatureSelector;
		}

		private void OldNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var node = e.SelectedNodes.FirstOrDefault();

			if(node == null)
			{
				return;
			}

			var nomenclature = TemplateUoW.GetById<Nomenclature>(node.Id);
			
			_newRow = new RegradingOfGoodsTemplateItem()
			{
				NomenclatureOld = nomenclature
			};

			var newNomenclatureSelector =
				CreateNomenclatureSelector("Выберите новую номенклатуру", NewNomenclatureSelectorOnEntitySelectedResult);
			MyTab.TabParent.AddSlaveTab(MyTab, newNomenclatureSelector);
		}

		private void NewNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var node = e.SelectedNodes.FirstOrDefault();

			if(node == null)
			{
				_newRow = null;
				return;
			}

			var nomenclature = TemplateUoW.GetById<Nomenclature>(node.Id);
			_newRow.NomenclatureNew = nomenclature;
			TemplateUoW.Root.AddItem(_newRow);
		}

		protected void OnButtonChangeOldClicked(object sender, EventArgs e)
		{
			var changeOldNomenclatureSelector =
				CreateNomenclatureSelector("Изменить старую номенклатуру", ChangeOldNomenclatureSelectorOnEntitySelectedResult);
			MyTab.TabParent.AddSlaveTab(MyTab, changeOldNomenclatureSelector);
		}
		
		private void ChangeOldNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			var node = e.SelectedNodes.FirstOrDefault();

			if(node == null || row == null)
			{
				return;
			}

			var nomenclature = TemplateUoW.GetById<Nomenclature>(node.Id);
			row.NomenclatureOld = nomenclature;
		}

		protected void OnButtonChangeNewClicked(object sender, EventArgs e)
		{
			var changeNewNomenclatureSelector =
				CreateNomenclatureSelector("Изменить новую номенклатуру", ChangeNewNomenclatureSelectorOnEntitySelectedResult);
			MyTab.TabParent.AddSlaveTab(MyTab, changeNewNomenclatureSelector);
		}
		
		private void ChangeNewNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			var node = e.SelectedNodes.FirstOrDefault();

			if(node == null || row == null)
			{
				return;
			}

			var nomenclature = TemplateUoW.GetById<Nomenclature>(node.Id);
			row.NomenclatureNew = nomenclature;
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			if(row.Id != 0)
				TemplateUoW.Delete(row);
			TemplateUoW.Root.ObservableItems.Remove(row);
		}

		protected void OnYtreeviewItemsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if (args.Column.Title == "Старая номенклатура")
				buttonChangeOld.Click();
			if (args.Column.Title == "Новая номенклатура")
				buttonChangeNew.Click();
		}

		public override void Destroy()
		{
			base.Destroy();
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
		}
	}
}

