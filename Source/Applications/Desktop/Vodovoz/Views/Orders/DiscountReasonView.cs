using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class DiscountReasonView : TabViewBase<DiscountReasonViewModel>
	{
		private Frame _frameApplicabilities;
		
		public DiscountReasonView(DiscountReasonViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			buttonSave.BindCommand(ViewModel.SaveCommand);
			
			buttonSave.Binding
				.AddBinding(ViewModel, vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.InitializeFromSource();
			
			buttonCancel.BindCommand(ViewModel.CloseCommand);
			
			radioDiscountInfo.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DiscountInfoTabActive, w => w.Active)
				.InitializeFromSource();
			
			radioPromoCodeSettings.Binding
				.AddBinding(ViewModel, vm => vm.PromoCodeSettingsTabActive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.IsPromoCode, w => w.Sensitive)
				.InitializeFromSource();

			enumDiscountType.ShowSpecialStateNot = false;
			enumDiscountType.ItemsEnum = typeof(DiscountReasonType);
			enumDiscountType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedDiscountReasonType, w => w.SelectedItem)
				.AddBinding(vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.InitializeFromSource();

			notebook.ShowTabs = false;
			notebook.Binding
				.AddBinding(ViewModel, vm => vm.CurrentPage, w => w.CurrentPage)
				.InitializeFromSource();
			
			ConfigureDiscountInfoTab();
			ConfigurePromoCodeTab();
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
			SizeAllocated += OnSizeAllocated;
		}

		#region Вкладка Информация о скидке

		private void ConfigureDiscountInfoTab()
		{
			entryName.Binding
				.AddBinding(ViewModel, vm => vm.EntityName, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanChangeDiscountReasonName, w => w.Sensitive)
				.InitializeFromSource();
			
			checkIsArchive.Binding
				.AddBinding(ViewModel, vm => vm.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanArchive, w => w.Sensitive)
				.InitializeFromSource();
			
			spinDiscount.Binding
				.AddBinding(ViewModel, vm => vm.DiscountValue, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.InitializeFromSource();
			
			enumDiscountValueType.ItemsEnum = typeof(DiscountUnits);
			enumDiscountValueType.Binding
				.AddBinding(ViewModel, vm => vm.DiscountValueType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.InitializeFromSource();
			
			chkBtnPremiumDiscount.Binding
				.AddBinding(ViewModel, vm => vm.IsPremiumDiscount, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.InitializeFromSource();
			
			chkBtnPresent.Binding
				.AddBinding(ViewModel, vm => vm.IsPresent, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.InitializeFromSource();
			
			chkBtnSelectAll.Binding
				.AddBinding(ViewModel, vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.SelectedAllCategories, w => w.Active)
				.InitializeFromSource();
			
			ConfigureApplicabilityDiscountWidgets();
		}

		private void ConfigureApplicabilityDiscountWidgets()
		{
			ConfigureApplicabilities();
			ConfigureDiscountNomenclatureCategoriesWidgets();
			ConfigureDiscountNomenclaturesWidgets();
			ConfigureDiscountProductGroupsWidgets();
			ConfigurePromoSetsWidgets();
		}

		private void ConfigureApplicabilities()
		{
			if(!ViewModel.DiscountTypeUses.Any())
			{
				return;
			}
			
			_frameApplicabilities = new Frame();
			_frameApplicabilities.ShadowType = ShadowType.EtchedOut;

			var tableApplicabilities = new yTable();
			uint row = 0;

			foreach(var keyPairValue in ViewModel.DiscountTypeUses)
			{
				ConfigureApplicability(tableApplicabilities, keyPairValue, row);
				row++;
			}
			
			_frameApplicabilities.Add(tableApplicabilities);
			_frameApplicabilities.ShowAll();
			vboxDiscountInfo.Add(_frameApplicabilities);
			var frameBox = (Box.BoxChild)vboxDiscountInfo[_frameApplicabilities];
			frameBox.Expand = false;
			var tableDiscountInfoBox = (Box.BoxChild)vboxDiscountInfo[tableDiscountInfo];
			vboxDiscountInfo.ReorderChild(_frameApplicabilities, tableDiscountInfoBox.Position + 1);
		}
		
		private void ConfigureApplicability(
			yTable tableApplicabilities,
			KeyValuePair<DiscountType, UseDiscountType?> discountTypeUses,
			uint row)
		{
			uint column = 0;
			var labelApplicability = new yLabel();
			labelApplicability.LabelProp = $"{discountTypeUses.Key.GetEnumTitle()}:";
			labelApplicability.Xalign = 1f;
			tableApplicabilities.Attach(
				labelApplicability,
				column,
				++column,
				row,
				row + 1,
				AttachOptions.Fill,
				AttachOptions.Fill,
				0,
				0);
			
			var comboDiscountTypeUses = new yListComboBox();
			
			comboDiscountTypeUses.SetRenderTextFunc<KeyValuePair<DiscountType, UseDiscountType?>>(x =>
				!x.Value.HasValue
					? "Нет"
					: x.Value.Value.GetEnumTitle());
			
			comboDiscountTypeUses.DefaultFirst = true;
			comboDiscountTypeUses.ItemsList = GenerateDiscountTypeUsesList(discountTypeUses.Key);
			comboDiscountTypeUses.ItemSelected += OnDiscountTypeUseSelected;
			
			tableApplicabilities.Attach(
				comboDiscountTypeUses,
				column,
				++column,
				row,
				row + 1,
				AttachOptions.Fill,
				AttachOptions.Fill,
				0,
				0);
		}

		private void OnDiscountTypeUseSelected(object sender, ItemSelectedEventArgs e)
		{
			var selectedValue = (KeyValuePair<DiscountType, UseDiscountType?>)e.SelectedItem;
			
			if(ViewModel.DiscountTypeUses.ContainsKey(selectedValue.Key))
			{
				ViewModel.DiscountTypeUses[selectedValue.Key] = selectedValue.Value;
			}
		}

		private static List<KeyValuePair<DiscountType, UseDiscountType?>> GenerateDiscountTypeUsesList(DiscountType discountType)
		{
			var list = new List<KeyValuePair<DiscountType, UseDiscountType?>>
			{
				new KeyValuePair<DiscountType, UseDiscountType?>(discountType, null)
			};

			list.AddRange(
				from UseDiscountType useDiscountType in Enum.GetValues(typeof(UseDiscountType))
				select new KeyValuePair<DiscountType, UseDiscountType?>(discountType, useDiscountType)
				);

			return list;
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
					.AddSetter((c, n) => c.Activatable = ViewModel.CanEditDiscountReason)
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
			treeViewNomenclatures.ItemsDataSource = ViewModel.Entity.Nomenclatures;
			treeViewNomenclatures.Binding
				.AddBinding(ViewModel, vm => vm.SelectedNomenclature, w => w.SelectedRow)
				.InitializeFromSource();
			
			btnAddNomenclature.BindCommand(ViewModel.AddNomenclatureCommand);
			btnRemoveNomenclature.BindCommand(ViewModel.RemoveNomenclatureCommand);
			
			btnAddNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.InitializeFromSource();
			
			btnRemoveNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveNomenclature, w => w.Sensitive)
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

			treeViewProductGroups.ItemsDataSource = ViewModel.Entity.ProductGroups;
			treeViewProductGroups.Binding
				.AddBinding(ViewModel, vm => vm.SelectedProductGroup, w => w.SelectedRow)
				.InitializeFromSource();
			
			btnAddProductGroup.BindCommand(ViewModel.AddProductGroupCommand);
			btnRemoveProductGroup.BindCommand(ViewModel.RemoveProductGroupCommand);
			
			btnAddProductGroup.Binding
				.AddBinding(ViewModel, vm => vm.CanEditDiscountReason, w => w.Sensitive)
				.InitializeFromSource();
			
			btnRemoveProductGroup.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveProductGroup, w => w.Sensitive)
				.InitializeFromSource();
		}
		
		private void ConfigurePromoSetsWidgets()
		{
			addOrRemovePromoSetsView.ViewModel = ViewModel.AddOrRemovePromoSetsViewModel;
			addOrRemovePromoSetsView.HeightRequest = 150;
		}

		#endregion

		#region Настройки промокода

		private void ConfigurePromoCodeTab()
		{
			entryPromoCodeName.Visible = false;
			
			datePromoCodeDuration.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.AddBinding(vm => vm.CanEditPromoCode, w => w.Sensitive)
				.InitializeFromSource();
			
			chkPromoCodeTimeDuration.Binding
				.AddBinding(ViewModel, vm => vm.HasPromoCodeDurationTime, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditPromoCode, w => w.Sensitive)
				.InitializeFromSource();

			timePromoCodeDuration.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartTime, w => w.TimeStart)
				.AddBinding(vm => vm.EndTime, w => w.TimeEnd)
				.AddBinding(vm => vm.HasPromoCodeDurationTime, w => w.Visible)
				.AddBinding(vm => vm.CanEditPromoCode, w => w.Sensitive)
				.InitializeFromSource();
			
			chkOrderMinSum.Binding
				.AddBinding(ViewModel, vm => vm.HasOrderMinSum, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditPromoCode, w => w.Sensitive)
				.InitializeFromSource();

			spinMinOrderSum.Adjustment = new Adjustment(0, 0, PromoCodeDiscount.OrderMinSumLimit, 100, 1000, 0);
			spinMinOrderSum.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderMinSum, w => w.ValueAsDecimal)
				.AddBinding(vm => vm.HasOrderMinSum, w => w.Visible)
				.AddBinding(vm => vm.CanEditPromoCode, w => w.Sensitive)
				.InitializeFromSource();
			
			lblRubles.Binding
				.AddBinding(ViewModel, vm => vm.HasOrderMinSum, w => w.Visible)
				.InitializeFromSource();
			
			chkOneTimePromoCode.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsOneTimePromoCode, w => w.Active)
				.AddBinding(vm => vm.CanEditPromoCode, w => w.Sensitive)
				.InitializeFromSource();
		}

		#endregion
		
		private void OnSizeAllocated(object o, SizeAllocatedArgs args)
		{
			hpaned1.Position = args.Allocation.Width / 2;
		}
		
		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.SelectedDiscountReasonType))
			{
				UpdateFrameApplicabilities();
			}
		}

		private void UpdateFrameApplicabilities()
		{
			_frameApplicabilities?.Destroy();

			if(ViewModel.Entity.DiscountReasonType != DiscountReasonType.Discount)
			{
				ConfigureApplicabilities();
			}
		}

		protected override void OnDestroyed()
		{
			ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
			base.OnDestroyed();
		}
	}
}
