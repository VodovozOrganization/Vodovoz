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

			buttonAdd.Clicked += OnButtonAddClicked;
			buttonChangeOld.Clicked += OnButtonChangeOldClicked;
			buttonChangeNew.Clicked += OnButtonChangeNewClicked;
			buttonDelete.Clicked += OnButtonDeleteClicked;
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			UnSubscribeUIEvents();

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<RegradingOfGoodsTemplateItem>()
				.AddColumn("Старая номенклатура").AddTextRenderer(x => x.NomenclatureOld.Name)
				.AddColumn("Новая номенклатура").AddTextRenderer(x => x.NomenclatureNew.Name)
				.Finish();

			ytreeviewItems.ItemsDataSource = ViewModel.Items;

			SubscribeUIEvents();
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

		private void SubscribeUIEvents()
		{
			buttonChangeNew.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeSelectedItem, w => w.Sensitive)
				.InitializeFromSource();

			buttonChangeOld.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeSelectedItem, w => w.Sensitive)
				.InitializeFromSource();

			buttonDelete.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeSelectedItem, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void UnSubscribeUIEvents()
		{
			buttonChangeNew.Binding.CleanSources();
			buttonChangeOld.Binding.CleanSources();
			buttonDelete.Binding.CleanSources();
		}
	}
}
