using Gamma.Binding;
using Gamma.ColumnConfig;
using Gtk;
using Pango;
using QS.Views.Dialog;
using QS.Views.GtkUI;
using System;
using System.Linq;
using Vodovoz.ViewModels.Dialogs.Goods;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GroupNomenclaturePriceView : DialogViewBase<NomenclatureGroupPricingViewModel>
	{
		private static Gdk.Color _white = new Gdk.Color(255, 255, 255);
		private static Gdk.Color _red = new Gdk.Color(237, 55, 55);

		public GroupNomenclaturePriceView(NomenclatureGroupPricingViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			datePicker.Binding.AddBinding(ViewModel, vm => vm.Date, w => w.Date).InitializeFromSource();
			datePicker.IsEditable = true;

			buttonSave.Clicked += (s, e) => ViewModel.SaveCommand.Execute();
			ViewModel.SaveCommand.CanExecuteChanged += (s, e) => buttonSave.Sensitive = ViewModel.SaveCommand.CanExecute();
			ViewModel.SaveCommand.RaiseCanExecuteChanged();

			buttonCancel.Clicked += (s, e) => ViewModel.CloseCommand.Execute();
			ViewModel.CloseCommand.CanExecuteChanged += (s, e) => buttonCancel.Sensitive = ViewModel.CloseCommand.CanExecute();
			ViewModel.CloseCommand.RaiseCanExecuteChanged();

			ytreeviewPrices.ColumnsConfig = FluentColumnsConfig<INomenclatureGroupPricingItemViewModel>.Create()
				.AddColumn("Товар")
					.MinWidth(400)
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Name)
					.AddSetter((c, n) =>
					{
						if(n.IsGroup)
						{
							c.Xalign = 0f;
							c.Alignment = Pango.Alignment.Left;
							c.Weight = (int)Weight.Bold;
							
						}
						else
						{
							c.Xalign = 1f;
							c.Alignment = Pango.Alignment.Right;
							c.Weight = (int)Weight.Normal;
						}

					})
				.AddColumn("Себестоимость\nпроизводства")
					.MinWidth(180)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.CostPurchasePrice)
					.Editing(x => !x.IsGroup)
					.Adjustment(new Adjustment(0, 0, 99999999, 1, 10, 10))
					.AddSetter((c, n) =>
					{
						c.Xalign = 0.5f;
						c.Alignment = Pango.Alignment.Center;
						if(n.InvalidCostPurchasePrice)
						{
							c.BackgroundGdk = _red;
						}
						else
						{
							c.BackgroundGdk = _white;
						}
					})
					.AddSetter((c, n) =>
					{
						c.Visible = !n.IsGroup;
					})
				.AddColumn("Стоимость доставки\nдо склада")
					.MinWidth(180)
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.InnerDeliveryPrice)
					.Editing(x => !x.IsGroup)
					.Adjustment(new Adjustment(0, 0, 99999999, 1, 10, 10))
					.AddSetter((c, n) =>
					{
						c.Xalign = 0.5f;
						c.Alignment = Pango.Alignment.Center;
						if(n.InvalidInnerDeliveryPrice)
						{
							c.BackgroundGdk = _red;
						}
						else
						{
							c.BackgroundGdk = _white;
						}
					})
					.AddSetter((c, n) =>
					{
						c.Visible = !n.IsGroup;
					})
				.AddColumn("")
				.Finish();

			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
			ReloadItemSource();
		}

		private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.PriceViewModels):
					ReloadItemSource();
					break;
				default:
					break;
			}
		}

		private void ReloadItemSource()
		{
			ytreeviewPrices.YTreeModel = new LevelTreeModel<NomenclatureGroupPricingProductGroupViewModel>(ViewModel.PriceViewModels, ViewModel.LevelConfig);
			ytreeviewPrices.ExpandAll();
		}
	}
}
