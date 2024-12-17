using Gamma.GtkWidgets;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Store;

namespace Vodovoz.Store
{
	[ToolboxItem(true)]
	public partial class RegradingOfGoodsTemplateItemsView : WidgetViewBase<RegradingOfGoodsTemplateItemsViewModel>
	{

		public RegradingOfGoodsTemplateItemsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ytreeviewItems.Selection.Changed -= YtreeviewItems_Selection_Changed;

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<RegradingOfGoodsTemplateItem>()
				.AddColumn("Старая номенклатура").AddTextRenderer(x => x.NomenclatureOld.Name)
				.AddColumn("Новая номенклатура").AddTextRenderer(x => x.NomenclatureNew.Name)
				.Finish();

			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;

			UpdateButtonState();
		}

		void YtreeviewItems_Selection_Changed(object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		private void UpdateButtonState()
		{
			var selected = ViewModel.SelectedItem;
			buttonChangeNew.Sensitive = buttonDelete.Sensitive = buttonChangeOld.Sensitive = selected != null;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItemCommand.Execute();
		}

		protected void OnButtonChangeOldClicked(object sender, EventArgs e)
		{
			ViewModel.ChangeNewNomenclatureCommand.Execute();
		}


		protected void OnButtonChangeNewClicked(object sender, EventArgs e)
		{
			ViewModel.ChangeNewNomenclatureCommand.Execute();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteItemCommand.Execute();
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

		public override void Destroy()
		{
			ytreeviewItems.Selection.Changed -= YtreeviewItems_Selection_Changed;
			base.Destroy();
		}
	}
}
