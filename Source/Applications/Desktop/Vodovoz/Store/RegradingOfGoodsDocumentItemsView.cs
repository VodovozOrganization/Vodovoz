using Gamma.GtkWidgets;
using QS.Utilities;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Documents;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Store;
using VodovozBusiness.Domain.Documents;

namespace Vodovoz
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

			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;

			var basePrimary = GdkColors.PrimaryBase;
			var colorLightRed = GdkColors.DangerBase;

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

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			SubscribeOnUIEvents();
			UpdateButtonState();
		}

		public override RegradingOfGoodsDocumentItemsViewModel ViewModel
		{
			get => base.ViewModel;
			set
			{
				if(base.ViewModel != value)
				{
					base.ViewModel = value;
				}
			}
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

		private void YtreeviewItems_Selection_Changed(object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		private void UpdateButtonState()
		{
			var selected = ytreeviewItems.GetSelectedObject<RegradingOfGoodsDocumentItem>();

			buttonChangeNew.Sensitive = buttonDelete.Sensitive = selected != null;
			buttonChangeOld.Sensitive = selected != null
				&& ViewModel.CurrentWarehouse != null;
			buttonAdd.Sensitive = buttonFromTemplate.Sensitive = ViewModel.CurrentWarehouse != null;

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

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(RegradingOfGoodsDocumentItemsViewModel.CurrentWarehouse))
			{
				UpdateButtonState();
			}
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
			ViewModel.AddFineCommand.Execute();
		}

		protected void OnButtonDeleteFineClicked(object sender, EventArgs e)
		{
			ViewModel.DeleteFineCommand.Execute();
			UpdateButtonState();
		}

		protected void OnButtonFromTemplateClicked(object sender, EventArgs e)
		{
			ViewModel.FillFromTemplateCommand.Execute();
		}

		private void SubscribeOnUIEvents()
		{
			ytreeviewItems.Selection.Changed += YtreeviewItems_Selection_Changed;
			ytreeviewItems.RowActivated += OnYtreeviewItemsRowActivated;
		}

		private void UnSubscribeOnUIEvents()
		{
			ytreeviewItems.Selection.Changed -= YtreeviewItems_Selection_Changed;
			ytreeviewItems.RowActivated -= OnYtreeviewItemsRowActivated;
		}

		public override void Destroy()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			UnSubscribeOnUIEvents();
			base.Destroy();
		}
	}
}
