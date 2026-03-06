using Gamma.GtkWidgets;
using QS.Utilities;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using System.Globalization;
using Gtk;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Documents;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Store;

namespace Vodovoz.Store
{
	[ToolboxItem(true)]
	public partial class RegradingOfGoodsDocumentItemsView : WidgetViewBase<RegradingOfGoodsDocumentItemsViewModel>
	{
		public RegradingOfGoodsDocumentItemsView()
		{
			Build();

			buttonFromTemplate.Clicked += OnButtonFromTemplateClicked;
			buttonAdd.Clicked += OnAddButtonClicked;
			buttonChangeOld.Clicked += OnButtonChangeOldClicked;
			buttonChangeNew.Clicked += OnButtonChangeNewClicked;
			buttonDelete.Clicked += OnButtonDeleteClicked;
			buttonFine.Clicked += OnButtonFineClicked;
			buttonDeleteFine.Clicked += OnButtonDeleteFineClicked;
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			UnSubscribeOnUIEvents();

			var basePrimary = GdkColors.PrimaryBase;
			var colorLightRed = GdkColors.DangerBase;

			ytreeviewItems.ColumnsConfig = ColumnsConfigFactory.Create<RegradingOfGoodsDocumentItem>()
				.AddColumn("Старая номенклатура").AddTextRenderer(x => x.NomenclatureOld.Name)
				.AddColumn("Кол-во на складе").AddTextRenderer(x => x.NomenclatureOld.Unit.MakeAmountShortStr(x.AmountInStock))
				.AddColumn("Новая номенклатура").AddTextRenderer(x => x.NomenclatureNew.Name)
				.AddColumn("Кол-во пересортицы")
					.AddNumericRenderer(x => x.Amount, ItemCountEditedHandler)
					.Editing()
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
				.AddColumn("Сумма ущерба").AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.SumOfDamage))
				.AddColumn("Штраф").AddTextRenderer(x => x.Fine != null ? x.Fine.Description : string.Empty)
				.AddColumn("Тип брака")
					.AddComboRenderer(x => x.TypeOfDefect)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.DefectTypesCache)
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
					.FillItems(ViewModel.RegradingReasonsCache)
					.Editing()
				.Finish();

			ytreeviewItems.ItemsDataSource = ViewModel.Items;

			ytreeviewItems.Binding.AddBinding(ViewModel, vm => vm.SelectedItemObject, w => w.SelectedRow);

			SubscribeOnUIEvents();
		}

		private void ItemCountEditedHandler(object o, EditedArgs args)
		{
			var node = ytreeviewItems.YTreeModel.NodeAtPath(new TreePath(args.Path));
			if(!(node is RegradingOfGoodsDocumentItem item))
			{
				return;
			}

			var maxValue = (decimal)GetMaxValueForAdjustmentSetting(item);

			if(maxValue > 0)
			{
				if(item.Amount > maxValue)
				{
					item.Amount = maxValue;
				}
			}

			var newValue = args.NewText.Replace(',', '.');
			decimal.TryParse(newValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var newAmount);
			item.Amount = Math.Round(newAmount, item.NomenclatureNew.Unit.Digits);
		}

		private void OnAddButtonClicked(object sender, EventArgs e)
		{
			ViewModel.AddItemCommand.Execute();
		}

		private double GetMaxValueForAdjustmentSetting(RegradingOfGoodsDocumentItem item)
		{
			if(item.NomenclatureOld.Category == NomenclatureCategory.bottle
			   && item.NomenclatureNew.Category == NomenclatureCategory.water)
			{
				return 39;
			}

			return (double)item.AmountInStock;
		}

		protected void OnButtonChangeOldClicked(object sender, EventArgs e)
		{
			ViewModel.ChangeOldNomenclatureCommand.Execute();
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

		protected void OnButtonFineClicked(object sender, EventArgs e)
		{
			ViewModel.ActionFineCommand.Execute();
		}

		protected void OnButtonDeleteFineClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteFineCommand.Execute();
			ytreeviewItems.YTreeModel.EmitModelChanged();
		}

		protected void OnButtonFromTemplateClicked(object sender, EventArgs e)
		{
			ViewModel.FillFromTemplateCommand.Execute();
		}

		private void SubscribeOnUIEvents()
		{
			ytreeviewItems.RowActivated += OnYtreeviewItemsRowActivated;

			buttonAdd.Binding
				.AddBinding(ViewModel, vm => vm.CanAddItem, w => w.Sensitive)
				.InitializeFromSource();

			buttonFromTemplate.Binding
				.AddBinding(ViewModel, vm => vm.CanFillFromTemplate, w => w.Sensitive)
				.InitializeFromSource();

			buttonChangeOld.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeOldNomenclature, w => w.Sensitive)
				.InitializeFromSource();

			buttonChangeNew.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeSelectedItem, w => w.Sensitive)
				.InitializeFromSource();

			buttonDelete.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeSelectedItem, w => w.Sensitive)
				.InitializeFromSource();

			buttonFine.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeSelectedItem, w => w.Sensitive)
				.AddBinding(vm => vm.FineButtonText, w => w.Label)
				.InitializeFromSource();

			buttonDeleteFine.Binding
				.AddBinding(ViewModel, vm => vm.CanDeleteFine, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void UnSubscribeOnUIEvents()
		{
			ytreeviewItems.RowActivated -= OnYtreeviewItemsRowActivated;

			buttonAdd.Binding.CleanSources();
			buttonFromTemplate.Binding.CleanSources();
			buttonChangeOld.Binding.CleanSources();
			buttonChangeNew.Binding.CleanSources();
			buttonDelete.Binding.CleanSources();
			buttonFine.Binding.CleanSources();
			buttonDeleteFine.Binding.CleanSources();
		}
	}
}
