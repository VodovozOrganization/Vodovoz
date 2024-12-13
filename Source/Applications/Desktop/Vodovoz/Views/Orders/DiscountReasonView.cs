using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class DiscountReasonView : TabViewBase<DiscountReasonViewModel>
	{
		public DiscountReasonView(DiscountReasonViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseCommand);
			
			radioDiscountInfo.Binding
				.AddBinding(ViewModel, vm => vm.DiscountInfoTabActive, w => w.Active)
				.InitializeFromSource();
			
			radioPromoCodeSettings.Binding
				.AddBinding(ViewModel, vm => vm.PromoCodeSettingsTabActive, w => w.Active)
				.InitializeFromSource();

			notebook.ShowTabs = false;
			notebook.Binding
				.AddBinding(ViewModel, vm => vm.CurrentPage, w => w.CurrentPage)
				.InitializeFromSource();
			
			ConfigureDiscountInfoTab();
			ConfigurePromoCodeTab();
		}

		#region Вкладка Информация о скидке

		private void ConfigureDiscountInfoTab()
		{
			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanChangeDiscountReasonName, w => w.Sensitive)
				.InitializeFromSource();
			
			checkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();
			
			spinDiscount.Binding
				.AddBinding(ViewModel.Entity, e => e.Value, w => w.ValueAsDecimal)
				.InitializeFromSource();
			
			enumDiscountValueType.ItemsEnum = typeof(DiscountUnits);
			enumDiscountValueType.Binding
				.AddBinding(ViewModel.Entity, e => e.ValueType, w => w.SelectedItem)
				.InitializeFromSource();
			
			chkBtnPremiumDiscount.Binding
				.AddBinding(ViewModel.Entity, e => e.IsPremiumDiscount, w => w.Active)
				.InitializeFromSource();
			
			chkBtnPresent.Binding
				.AddBinding(ViewModel.Entity, e => e.IsPresent, w => w.Active)
				.InitializeFromSource();
			chkBtnSelectAll.Toggled += (s, e) => ViewModel.UpdateSelectedCategoriesCommand.Execute(chkBtnSelectAll.Active);
			
			chkPromoCode.Binding
				.AddBinding(ViewModel.Entity, e => e.IsPromoCode, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanChangeIsPromoCode, w => w.Sensitive)
				.InitializeFromSource();
			
			ConfigureApplicabilityDiscountWidgets();
		}

		private void ConfigureApplicabilityDiscountWidgets()
		{
			ConfigureDiscountNomenclatureCategoriesWidgets();
			ConfigureDiscountNomenclaturesWidgets();
			ConfigureDiscountProductGroupsWidgets();
		}

		private void ConfigureDiscountNomenclatureCategoriesWidgets()
		{
			treeViewNomenclatureCategories.CreateFluentColumnsConfig<SelectableNomenclatureCategoryNode>()
				.AddColumn("")
					.AddTextRenderer(x => x.DiscountReasonNomenclatureCategory != null ? x.DiscountReasonNomenclatureCategory.Id.ToString() : "")
				.AddColumn("")
					.AddTextRenderer(x =>
						x.DiscountReasonNomenclatureCategory.NomenclatureCategory.GetEnumTitle())
				.AddColumn("")
					.AddToggleRenderer(x => x.IsSelected)
					.ToggledEvent(OnDiscountNomenclatureCategorySelected)
				.AddColumn("")
				.Finish();

			treeViewNomenclatureCategories.HeadersVisible = false;
			treeViewNomenclatureCategories.ItemsDataSource = ViewModel.SelectableNomenclatureCategoryNodes;
		}

		private void OnDiscountNomenclatureCategorySelected(object o, ToggledArgs args)
		{
			Gtk.Application.Invoke((s, e) =>
			{
				var selectedCategory = treeViewNomenclatureCategories.GetSelectedObject<SelectableNomenclatureCategoryNode>();

				if(selectedCategory == null)
				{
					return;
				}
				
				ViewModel.UpdateNomenclatureCategories(selectedCategory);
			});
		}

		private void ConfigureDiscountNomenclaturesWidgets()
		{
			treeViewNomenclatures.CreateFluentColumnsConfig<Nomenclature>()
				.AddColumn("")
					.AddNumericRenderer(x => x.Id)
				.AddColumn("")
					.AddTextRenderer(x => x.Name)
				.AddColumn("")
				.Finish();
			
			treeViewNomenclatures.HeadersVisible = false;
			treeViewNomenclatures.ItemsDataSource = ViewModel.Entity.ObservableNomenclatures;
			treeViewNomenclatures.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNomenclature, w => w.SelectedRow)
				.InitializeFromSource();
			
			btnAddNomenclature.BindCommand(ViewModel.AddNomenclatureCommand);
			btnRemoveNomenclature.BindCommand(ViewModel.RemoveNomenclatureCommand);
			
			btnRemoveNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.IsNomenclatureSelected, w => w.Sensitive)
				.InitializeFromSource();
		}

		private void ConfigureDiscountProductGroupsWidgets()
		{
			treeViewProductGroups.CreateFluentColumnsConfig<ProductGroup>()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => ViewModel.Entity.ProductGroups.IndexOf(node) + 1)
				.AddColumn("Группа товаров")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.Name)
				.AddColumn("")
				.Finish();

			treeViewProductGroups.ItemsDataSource = ViewModel.Entity.ObservableProductGroups;
			treeViewProductGroups.Binding
				.AddBinding(ViewModel, vm => vm.SelectedProductGroup, w => w.SelectedRow)
				.InitializeFromSource();
			
			btnAddProductGroup.BindCommand(ViewModel.AddProductGroupCommand);
			btnRemoveProductGroup.BindCommand(ViewModel.RemoveProductGroupCommand);
			
			btnRemoveProductGroup.Binding
				.AddBinding(ViewModel, vm => vm.IsProductGroupSelected, w => w.Sensitive)
				.InitializeFromSource();
		}

		#endregion

		#region Настройки промокода

		private void ConfigurePromoCodeTab()
		{
			entryPromoCodeName.Binding
				.AddBinding(ViewModel.Entity, e => e.PromoCodeName, w => w.Text)
				.InitializeFromSource();
			
			datePromoCodeDuration.Binding
				.AddSource(ViewModel.Entity)
				.AddBinding(e => e.StartDatePromoCode, w => w.StartDateOrNull)
				.AddBinding(e => e.EndDatePromoCode, w => w.EndDateOrNull)
				.InitializeFromSource();
			
			chkPromoCodeTimeDuration.Binding
				.AddBinding(ViewModel, vm => vm.HasPromoCodeDurationTime, w => w.Active)
				.InitializeFromSource();

			timePromoCodeDuration.AutocompleteStepInMinutes = 60;
			timePromoCodeDuration.Binding
				.AddBinding(ViewModel.Entity, e => e.StartTimePromoCode, w => w.TimeStart)
				.AddBinding(ViewModel.Entity, e => e.EndTimePromoCode, w => w.TimeEnd)
				.AddBinding(ViewModel, vm => vm.HasPromoCodeDurationTime, w => w.Visible)
				.InitializeFromSource();
			
			chkOrderMinSum.Binding
				.AddBinding(ViewModel, vm => vm.HasOrderMinSum, w => w.Active)
				.InitializeFromSource();

			spinMinOrderSum.Adjustment = new Adjustment(0, 0, DiscountReason.PromoCodeOrderMinSumLimit, 100, 1000, 0);
			spinMinOrderSum.Binding
				.AddBinding(ViewModel.Entity, e => e.PromoCodeOrderMinSum, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.HasOrderMinSum, w => w.Visible)
				.InitializeFromSource();
			
			lblRubles.Binding
				.AddBinding(ViewModel, vm => vm.HasOrderMinSum, w => w.Visible)
				.InitializeFromSource();
			
			chkOneTimePromoCode.Binding
				.AddBinding(ViewModel.Entity, e => e.IsOneTimePromoCode, w => w.Active)
				.InitializeFromSource();
		}

		#endregion
	}
}
