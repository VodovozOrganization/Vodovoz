using Autofac;
using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RegradingOfGoodsTemplateItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private RegradingOfGoodsTemplateItem _newRow;

		public RegradingOfGoodsTemplateItemsView()
		{
			Build();

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
			var page = Startup.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel>(
				MyTab,
				QS.Navigation.OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.TabName = "Выберите номенклатуру на замену";
					vievModel.OnSelectResult += OldNomenclatureSelectorOnEntitySelectedResult;
				});
		}

		private void OldNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var node = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();

			if(node == null)
			{
				return;
			}

			var nomenclature = TemplateUoW.GetById<Nomenclature>(node.Id);
			
			_newRow = new RegradingOfGoodsTemplateItem()
			{
				NomenclatureOld = nomenclature
			};

			var page = Startup.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
				MyTab,
				filter =>
				{
					filter.RestrictArchive = true;
					filter.AvailableCategories = Nomenclature.GetCategoriesForGoods();
				},
				QS.Navigation.OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.TabName = "Выберите новую номенклатуру";
					vievModel.OnSelectResult += NewNomenclatureSelectorOnEntitySelectedResult;
				});
		}

		private void NewNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var node = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();

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
			var page = Startup.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel>(
				MyTab,
				QS.Navigation.OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.TabName = "Изменить старую номенклатуру";
					vievModel.OnSelectResult += OldNomenclatureSelectorOnEntitySelectedResult;
				});
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
			var page = Startup.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel>(
				MyTab,
				QS.Navigation.OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.TabName = "Изменить новую номенклатуру";
					vievModel.OnSelectResult += ChangeNewNomenclatureSelectorOnEntitySelectedResult;

				});
		}
		
		private void ChangeNewNomenclatureSelectorOnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var row = ytreeviewItems.GetSelectedObject<RegradingOfGoodsTemplateItem>();
			var node = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();

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

