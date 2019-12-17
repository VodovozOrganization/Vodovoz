using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class PromotionalSetView : TabViewBase<PromotionalSetViewModel>
	{
		public PromotionalSetView(PromotionalSetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yCmbDiscountReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			yCmbDiscountReason.Binding.AddBinding(ViewModel, vm => vm.DiscountReasonSource, cmb => cmb.ItemsList).InitializeFromSource();
			yCmbDiscountReason.Binding.AddBinding(ViewModel.Entity, e => e.PromoSetName, w => w.SelectedItem).InitializeFromSource();

			yChkIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();

			ybtnAddNomenclature.Clicked += (sender, e) => { ViewModel.AddNomenculatureCommand.Execute(); };
			ybtnRemoveNomenclature.Clicked += (sender, e) => { ViewModel.RemoveNomenculatureCommand.Execute(); };
			ybtnRemoveNomenclature.Binding.AddBinding(ViewModel, vm => vm.CanRemove, b => b.Sensitive).InitializeFromSource();

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false); };

			buttonAddAction.ItemsEnum = typeof(PromosetActionType);
			buttonAddAction.EnumItemClicked += (sender, e) => { ViewModel.AddActionCommand.Execute((PromosetActionType)e.ItemEnum); };
			#region yTreePromoSetItems

			yTreePromoSetItems.ColumnsConfig = new FluentColumnsConfig<PromotionalSetItem>()
			.AddColumn("Код")
				.HeaderAlignment(0.5f)
				.AddTextRenderer(i => i.Nomenclature.Id.ToString())
			.AddColumn("Товар")
				.HeaderAlignment(0.5f)
				.AddTextRenderer(i => i.Nomenclature.Name)
			.AddColumn("Кол-во")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(i => i.Count)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
				.AddSetter((c, n) => c.Digits = n.Nomenclature.Unit == null ? 0 : (uint)n.Nomenclature.Unit.Digits)
				.WidthChars(10)
				.Editing()
			.AddTextRenderer(i => i.Nomenclature.Unit == null ? string.Empty : i.Nomenclature.Unit.Name, false)
			.AddColumn("Скидка")
				.HeaderAlignment(0.5f)
				.AddNumericRenderer(i => i.Discount).Editing(true)
				.Adjustment(new Adjustment(0, 0, 100, 1, 100, 1))
				.Digits(2)
				.WidthChars(10)
				.AddTextRenderer(n => "%", false)
			.AddColumn("")
			.Finish();

			yTreePromoSetItems.ItemsDataSource = ViewModel.Entity.ObservablePromotionalSetItems;

			yTreePromoSetItems.Selection.Changed += (sender, e) => {
				var selectedItem = yTreePromoSetItems.GetSelectedObject<PromotionalSetItem>();
				ViewModel.SelectedPromoItem = selectedItem;
			};

			#endregion

			if(ViewModel.Entity.Id > 0)
				ylblCreationDate.Text = ViewModel.Entity.CreateDate.ToString();
		}
	};
}
