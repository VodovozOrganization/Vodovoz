using System;
using Gamma.ColumnConfig;
using Gtk;
using QS.Navigation;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	public partial class PromotionalSetView : TabViewBase<PromotionalSetViewModel>
	{
		public PromotionalSetView(PromotionalSetViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			notebook.Sensitive = ViewModel.CanCreateOrUpdate;
			notebook.ShowTabs = false;
			notebook.Binding
				.AddBinding(ViewModel, vm => vm.CurrentPage, w => w.CurrentPage)
				.InitializeFromSource();
			
			radioBtnInformation.Binding
				.AddBinding(ViewModel, vm => vm.InformationTabActive, w => w.Active)
				.InitializeFromSource();
			
			radioBtnSitesAndApps.Binding
				.AddBinding(ViewModel, vm => vm.SitesAndAppsTabActive, w => w.Active)
				.InitializeFromSource();
			
			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnSave.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			btnCancel.Clicked += (sender, e) => ViewModel.Close(ViewModel.CanCreateOrUpdate, CloseSource.Cancel);

			yentryPromotionalSetName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			yChkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			yentryDiscountReason.Binding
				.AddBinding(ViewModel.Entity, e => e.DiscountReasonInfo, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbCanEditNomCount.Binding
				.AddBinding(ViewModel.Entity, e => e.CanEditNomenclatureCount, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			chkPromoSetForNewClients.Binding
				.AddBinding(ViewModel.Entity, p => p.PromotionalSetForNewClients, w => w.Active)
				.InitializeFromSource();
			chkPromoSetForNewClients.Sensitive = ViewModel.CanChangeType;

			chkBtnShowSpecialBottlesCountForDeliveryPrice.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowSpecialBottlesCountForDeliveryPrice, w => w.Active)
				.AddBinding(vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			entrySpecialBottlesCountForDeliveryPrice.MaxLength = 3;
			entrySpecialBottlesCountForDeliveryPrice.Binding
				.AddBinding(ViewModel.Entity, p => p.BottlesCountForCalculatingDeliveryPrice, w => w.Text, new NullableIntToStringConverter())
				.AddBinding(ViewModel, vm => vm.ShowSpecialBottlesCountForDeliveryPrice, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();
			entrySpecialBottlesCountForDeliveryPrice.Changed += OnSpecialBottlesCountForDeliveryPriceChanged;

			widgetcontainerview.Binding
				.AddBinding(ViewModel, vm => vm.SelectedActionViewModel, w => w.WidgetViewModel);

			ybtnAddNomenclature.Clicked += (sender, e) => ViewModel.AddNomenclatureCommand.Execute();
			ybtnAddNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, b => b.Sensitive)
				.InitializeFromSource();

			ybtnRemoveNomenclature.Clicked += (sender, e) => ViewModel.RemoveNomenclatureCommand.Execute();
			ybtnRemoveNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveNomenclature, b => b.Sensitive)
				.InitializeFromSource();

			yEnumButtonAddAction.ItemsEnum = typeof(PromotionalSetActionType);
			yEnumButtonAddAction.EnumItemClicked += (sender, e) => ViewModel.AddActionCommand.Execute((PromotionalSetActionType)e.ItemEnum);
			yEnumButtonAddAction.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.InitializeFromSource();

			ybtnRemoveAction.Clicked += (sender, e) => ViewModel.RemoveActionCommand.Execute();
			ybtnRemoveAction.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveAction, w => w.Sensitive)
				.InitializeFromSource();

			ConfigureTreeActions();
			ConfigureTreePromoSetsItems();
			ConfigureSitesAndAppsTab();

			ylblCreationDate.Text = ViewModel.CreationDate;
		}
		
		private void OnSpecialBottlesCountForDeliveryPriceChanged(object sender, EventArgs e)
		{
			var entry = sender as Entry;
			var chars = entry.Text.ToCharArray();
			
			var text = ViewModel.StringHandler.ConvertCharsArrayToNumericString(chars);
			entry.Text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
		}

		private void ConfigureTreePromoSetsItems()
		{
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
					.AddNumericRenderer(i => i.ManualChangingDiscount).Editing(true)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
							? new Adjustment(0, 0, 1000000000, 1, 100, 1)
							: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney)
				.AddColumn("")
				.Finish();

			yTreePromoSetItems.ItemsDataSource = ViewModel.Entity.ObservablePromotionalSetItems;
			yTreePromoSetItems.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.AddBinding(vm => vm.SelectedPromoItem, w => w.SelectedRow)
				.InitializeFromSource();
		}

		private void ConfigureTreeActions()
		{
			yTreeActionsItems.ItemsDataSource = ViewModel.Entity.ObservablePromotionalSetActions;
			yTreeActionsItems.ColumnsConfig = new FluentColumnsConfig<PromotionalSetActionBase>()
				.AddColumn("Действие")
					.AddTextRenderer(x => x.Title)
				.Finish();
			yTreeActionsItems.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCreateOrUpdate, w => w.Sensitive)
				.AddBinding(vm => vm.SelectedAction, w => w.SelectedRow)
				.InitializeFromSource();
		}
		
		private void ConfigureSitesAndAppsTab()
		{
			lblErpIdTitle.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Id > 0, w => w.Visible)
				.InitializeFromSource();
			lblErpId.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.Id > 0, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.Id, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();
			
			entryOnlineName.Binding
				.AddBinding(ViewModel.Entity, e => e.OnlineName, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, b => b.Sensitive)
				.InitializeFromSource();

			ConfigureParametersForMobileApp();
			ConfigureParametersForVodovozWebSite();
			ConfigureParametersForKulerSaleWebSite();
		}
		
		private void ConfigureParametersForMobileApp()
		{
			enumCmbOnlineAvailabilityMobileApp.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityMobileApp.ItemsEnum = typeof(GoodsOnlineAvailability);
			enumCmbOnlineAvailabilityMobileApp.Binding
				.AddBinding(ViewModel.MobileAppPromotionalSetOnlineParameters, p => p.PromotionalSetOnlineAvailability, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, b => b.Sensitive)
				.InitializeFromSource();
		}
		
		private void ConfigureParametersForVodovozWebSite()
		{
			enumCmbOnlineAvailabilityVodovozWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityVodovozWebSite.ItemsEnum = typeof(GoodsOnlineAvailability);
			enumCmbOnlineAvailabilityVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSitePromotionalSetOnlineParameters, p => p.PromotionalSetOnlineAvailability, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, b => b.Sensitive)
				.InitializeFromSource();
		}
		
		private void ConfigureParametersForKulerSaleWebSite()
		{
			enumCmbOnlineAvailabilityKulerSaleWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityKulerSaleWebSite.ItemsEnum = typeof(GoodsOnlineAvailability);
			enumCmbOnlineAvailabilityKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSitePromotionalSetOnlineParameters, p => p.PromotionalSetOnlineAvailability, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.CanCreateOrUpdate, b => b.Sensitive)
				.InitializeFromSource();
		}
	};
}
