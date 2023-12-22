using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.BusinessCommon.Domain;
using QS.Helpers;
using QS.Navigation;
using QS.Project.Dialogs.GtkUI;
using QS.Views.GtkUI;
using QSOrmProject;
using QSWidgetLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Gamma.Binding;
using Gamma.ColumnConfig;
using QS.Widgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Representations.ProductGroups;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Dialogs.Nodes;
using Menu = Gtk.Menu;
using MenuItem = Gtk.MenuItem;
using ValidationType = QSWidgetLib.ValidationType;

namespace Vodovoz.Views.Goods
{
	[ToolboxItem(true)]
	public partial class NomenclatureView : TabViewBase<NomenclatureViewModel>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private Entry _entry;
		
		public NomenclatureView(NomenclatureViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			notebook.ShowTabs = false;

			#region RadioButtons

			radioInfo.Active = true;
			radioEquipment.Binding
				.AddBinding(ViewModel, vm => vm.IsEquipmentCategory, w => w.Sensitive)
				.InitializeFromSource();
			radioEquipment.Toggled += OnRadioEquipmentToggled;

			radioPrice.Binding
				.AddBinding(ViewModel, vm => vm.WithoutDependsOnNomenclature, w => w.Sensitive)
				.InitializeFromSource();
			radioPrice.Toggled += OnRadioPriceToggled;

			radioSitesAndApps.Toggled += OnSitesAndAppsToggled;
			radioSitesAndApps.Binding
				.AddBinding(ViewModel, vm => vm.ActiveSitesAndAppsTab, w => w.Active)
				.AddBinding(ViewModel, vm => vm.HasAccessToSitesAndAppsTab, w => w.Visible)
				.InitializeFromSource();

			#endregion

			buttonSave.Clicked += (sender, args) =>
			{
				ViewModel.SaveCommand.Execute();
			};
			buttonSave.Sensitive = ViewModel.CanEdit;
			buttonCancel.Clicked += (sender, args) => ViewModel.Close(ViewModel.AskSaveOnClose, CloseSource.Cancel);
			ylabelCreationDate.Binding
				.AddFuncBinding(ViewModel.Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "",
					w => w.LabelProp)
				.InitializeFromSource();
			ylabelCreatedBy.Binding
				.AddFuncBinding(ViewModel.Entity, e => ViewModel.GetUserEmployeeName(), w => w.LabelProp)
				.InitializeFromSource();

			enumVAT.ItemsEnum = typeof(VAT);
			enumVAT.Binding
				.AddBinding(ViewModel.Entity, e => e.VAT, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			enumCategory.Changed += ViewModel.OnEnumCategoryChanged;
			enumCategory.ChangedByUser += ViewModel.OnEnumCategoryChangedByUser;
			enumCategory.ItemsEnum = typeof(NomenclatureCategory);
			enumCategory.Binding
				.AddBinding(ViewModel.Entity, e => e.Category, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			enumTareVolume.ItemsEnum = typeof(TareVolume);
			enumTareVolume.Binding
				.AddBinding(ViewModel.Entity, e => e.TareVolume, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.IsWaterCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ycheckDisposableTare.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDisposableTare, w => w.Active)
				.AddBinding(ViewModel, vm => vm.IsWaterOrBottleCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			labelTypeTare.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterOrBottleCategory, w => w.Visible)
				.InitializeFromSource();
			labelTareVolume.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterCategory, w => w.Visible)
				.InitializeFromSource();

			yСolorBtnBottleCapColor.Binding
				.AddBinding(ViewModel.Entity, e => e.BottleCapColor, w => w.Color, new ColorTextToGdkColorConverter())
				.AddBinding(ViewModel, vm => vm.Is19lTareVolume, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			yСolorBtnBottleCapColor.ColorSet += YСolorBtnBottleCapColorOnColorSet;
			ylblBottleCapColor.Binding
				.AddBinding(ViewModel, vm => vm.Is19lTareVolume, w => w.Visible)
				.InitializeFromSource();

			enumSaleCategory.ItemsEnum = typeof(SaleCategory);
			enumSaleCategory.Binding
				.AddBinding(ViewModel.Entity, e => e.SaleCategory, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.IsSaleCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			lblSaleCategory.Binding
				.AddBinding(ViewModel, vm => vm.IsSaleCategory, w => w.Visible)
				.InitializeFromSource();

			enumDepositType.ItemsEnum = typeof(TypeOfDepositCategory);
			enumDepositType.Binding
				.AddBinding(ViewModel.Entity, e => e.TypeOfDepositCategory, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.IsDepositCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			lblSubType.Binding
				.AddBinding(ViewModel, vm => vm.IsDepositCategory, w => w.Visible)
				.InitializeFromSource();

			comboMobileCatalog.ItemsEnum = typeof(MobileCatalog);
			comboMobileCatalog.Binding
				.AddBinding(ViewModel.Entity, e => e.MobileCatalog, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			yentryOfficialName.Binding
				.AddBinding(ViewModel.Entity, e => e.OfficialName, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			var parallel = new ParallelEditing(yentryOfficialName);
			parallel.SubscribeOnChanges(entryName);
			parallel.GetParallelTextFunc = GenerateOfficialName;

			checkNotReserve.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotServiceAndDepositCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.DoNotReserve, w => w.Active)
				.InitializeFromSource();

			checkcanPrintPrice.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.IsWaterInNotDisposableTare, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.CanPrintPrice, w => w.Active)
				.InitializeFromSource();
			labelCanPrintPrice.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterInNotDisposableTare, w => w.Visible)
				.InitializeFromSource();

			checkHide.Binding
				.AddBinding(ViewModel.Entity, e => e.Hide, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			entryCode1c.Binding
				.AddBinding(ViewModel.Entity, e => e.Code1c, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			yspinSumOfDamage.Binding
				.AddBinding(ViewModel.Entity, e => e.SumOfDamage, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			spinWeight.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotServiceAndDepositCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Weight, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinLength.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotServiceAndDepositCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Length, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinWidth.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotServiceAndDepositCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Width, w => w.ValueAsDecimal)
				.InitializeFromSource();

			spinHeight.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotServiceAndDepositCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Height, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ylabelVolume.Binding
				.AddBinding(ViewModel.Entity, e => e.Volume, w => w.Text, new DecimalToStringConverter())
				.InitializeFromSource();

			spinPercentForMaster.Binding
				.AddBinding(ViewModel.Entity, e => e.PercentForMaster, w => w.Value)
				.AddBinding(ViewModel, vm => vm.IsMasterCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			lblPercentForMaster.Binding
				.AddBinding(ViewModel, vm => vm.IsMasterCategory, w => w.Visible)
				.InitializeFromSource();

			labelBottle.Binding
				.AddBinding(ViewModel, vm => vm.IsBottleCategory, w => w.Visible)
				.InitializeFromSource();
			ycheckNewBottle.Binding
				.AddBinding(ViewModel.Entity, e => e.IsNewBottle, w => w.Active)
				.AddBinding(ViewModel, vm => vm.IsBottleCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ycheckDefectiveBottle.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDefectiveBottle, w => w.Active)
				.AddBinding(ViewModel, vm => vm.IsBottleCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			ycheckShabbyBottle.Binding
				.AddBinding(ViewModel.Entity, e => e.IsShabbyBottle, w => w.Active)
				.AddBinding(ViewModel, vm => vm.IsBottleCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			chkIsDiler.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDiler, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			spinMinStockCount.Binding
				.AddBinding(ViewModel.Entity, e => e.MinStockCount, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			ycomboFuelTypes.SetRenderTextFunc<FuelType>(x => x.Name);
			ycomboFuelTypes.ItemsList = ViewModel.UoW.GetAll<FuelType>();
			ycomboFuelTypes.Binding
				.AddBinding(ViewModel.Entity, e => e.FuelType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.IsFuelCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			lblFuelType.Binding
				.AddBinding(ViewModel, vm => vm.IsFuelCategory, w => w.Visible)
				.InitializeFromSource();

			ylblOnlineStore.Text = ViewModel.Entity.OnlineStore?.Name;
			ylblOnlineStore.Binding
				.AddBinding(ViewModel, vm => vm.IsOnlineStoreNomenclature, w => w.Visible)
				.InitializeFromSource();
			ylblOnlineStoreStr.Binding
				.AddBinding(ViewModel, vm => vm.IsOnlineStoreNomenclature, w => w.Visible)
				.InitializeFromSource();

			yentryFolder1c.SubjectType = typeof(Folder1c);
			yentryFolder1c.Binding
				.AddBinding(ViewModel.Entity, e => e.Folder1C, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			yentryProductGroup.JournalButtons = Buttons.Add | Buttons.Edit;
			yentryProductGroup.RepresentationModel = new ProductGroupVM(ViewModel.UoW, new ProductGroupFilterViewModel
			{
				HidenByDefault = false,
				HideArchive = true
			});
			yentryProductGroup.Binding
				.AddBinding(ViewModel.Entity, e => e.ProductGroup, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			referenceUnit.SubjectType = typeof(MeasurementUnits);
			referenceUnit.Binding
				.AddBinding(ViewModel.Entity, n => n.Unit, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			referenceRouteColumn.SubjectType = typeof(RouteColumn);
			referenceRouteColumn.Binding
				.AddBinding(ViewModel.Entity, n => n.RouteListColumn, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			checkNoDeliver.Binding
				.AddBinding(ViewModel.Entity, e => e.NoDelivery, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			yentryShortName.Binding
				.AddBinding(ViewModel.Entity, e => e.ShortName, w => w.Text, new NullToEmptyStringConverter())
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			yentryShortName.MaxLength = 220;
			checkIsArchive.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanCreateAndArcNomenclatures && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();

			entityviewmodelentryShipperCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entityviewmodelentryShipperCounterparty.Binding
				.AddBinding(ViewModel.Entity, e => e.ShipperCounterparty, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			entityviewmodelentryShipperCounterparty.CanEditReference = true;
			labelShipperCounterparty.Binding
				.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible)
				.InitializeFromSource();
			yentryStorageCell.Binding
				.AddBinding(ViewModel.Entity, e => e.StorageCell, w => w.Text)
				.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			labelStorageCell.Binding
				.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible)
				.InitializeFromSource();
			checkGroupPricing.Binding
				.AddBinding(ViewModel.Entity, e => e.UsingInGroupPriceSet, w => w.Active)
				.InitializeFromSource();
			ycheckIsAccountableInChestniyZnak.Binding.AddBinding(ViewModel.Entity, e => e.IsAccountableInTrueMark, w => w.Active)
				.InitializeFromSource();
			validatedGtin.ValidationMode = ValidationType.numeric;
			validatedGtin.MaxLength = 14;
			validatedGtin.Binding.AddBinding(ViewModel.Entity, e => e.Gtin, w => w.Text).InitializeFromSource();

			chkInventoryAccounting.Binding
				.AddBinding(ViewModel.Entity, e => e.HasInventoryAccounting, w => w.Active)
				.AddBinding(ViewModel, vm => vm.UserCanCreateNomenclaturesWithInventoryAccounting, w => w.Sensitive)
				.InitializeFromSource();

			#region Вкладка Оборудование

			referenceManufacturer.SubjectType = typeof(Manufacturer);
			referenceManufacturer.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsEquipmentCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Manufacturer, w => w.Subject)
				.InitializeFromSource();

			referenceColor.SubjectType = typeof(EquipmentColors);
			referenceColor.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsEquipmentCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.EquipmentColor, w => w.Subject)
				.InitializeFromSource();

			yentryrefEqupmentKind.SubjectType = typeof(EquipmentKind);
			yentryrefEqupmentKind.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsEquipmentCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Kind, w => w.Subject)
				.InitializeFromSource();

			entryModel.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsEquipmentCategory && vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			checkSerial.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsEquipmentCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.IsSerial, w => w.Active)
				.InitializeFromSource();

			ycheckRentPriority.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsEquipmentCategory && vm.CanEdit, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.RentPriority, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonMagnetGlassHolder.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsMagnetGlassHolderSelected, w => w.Active)
				.AddBinding(vm => vm.IsShowGlassHolderSelectionControls, w => w.Visible)
				.InitializeFromSource();

			ycheckbuttonScrewGlassHolder.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsScrewGlassHolderSelected, w => w.Active)
				.AddBinding(vm => vm.IsShowGlassHolderSelectionControls, w => w.Visible)
				.InitializeFromSource();

			ylabelGlassHolderType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsShowGlassHolderSelectionControls, w => w.Visible)
				.InitializeFromSource();

			#endregion

			#region Вкладка характиристики

			ytextDescription.Binding
				.AddBinding(ViewModel.Entity, e => e.Description, w => w.Buffer.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			nomenclaturecharacteristicsview1.Uow = ViewModel.UoWGeneric;
			nomenclaturecharacteristicsview1.Sensitive = ViewModel.CanEdit;

			#endregion

			#region Вкладка Цена закупки

			nomenclaturecostpricesview.ViewModel = ViewModel.NomenclatureCostPricesViewModel;
			nomenclaturecostpricesview.Sensitive = ViewModel.CanEdit;

			nomenclaturePurchasePricesView.ViewModel = ViewModel.NomenclaturePurchasePricesViewModel;
			nomenclaturePurchasePricesView.Sensitive = ViewModel.CanEdit;

			nomenclatureinnerdeliverypricesview1.ViewModel = ViewModel.NomenclatureInnerDeliveryPricesViewModel;
			nomenclatureinnerdeliverypricesview1.Sensitive = ViewModel.CanEdit;

			#endregion

			entityViewModelEntryNomenclature.SetEntityAutocompleteSelectorFactory(ViewModel.NomenclatureSelectorFactory);
			entityViewModelEntryNomenclature.Binding
				.AddBinding(ViewModel.Entity, e => e.DependsOnNomenclature, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			entityViewModelEntryNomenclature.CanEditReference = true;

			pricesView.Prices = ViewModel.Entity.NomenclaturePrice.Cast<NomenclaturePriceBase>().ToList();
			pricesView.PricesList.ElementAdded += PriceAdded;
			pricesView.PricesList.ElementRemoved += PriceRemoved;
			pricesView.PricesList.ElementChanged += PriceRowChanged;
			pricesView.PricesList.PropertyOfElementChanged += PricePropertyChanged;
			pricesView.Sensitive = ViewModel.CanCreateAndArcNomenclatures && ViewModel.CanEdit;
			pricesView.NomenclaturePriceType = NomenclaturePriceBase.NomenclaturePriceType.General;

			alternativePricesView.Prices = ViewModel.Entity.AlternativeNomenclaturePrices.Cast<NomenclaturePriceBase>().ToList();
			alternativePricesView.PricesList.ElementAdded += PriceAdded;
			alternativePricesView.PricesList.ElementRemoved += PriceRemoved;
			alternativePricesView.PricesList.ElementChanged += PriceRowChanged;
			alternativePricesView.PricesList.PropertyOfElementChanged += PricePropertyChanged;
			alternativePricesView.Sensitive =
				ViewModel.CanCreateAndArcNomenclatures
				&& ViewModel.CanEditAlternativeNomenclaturePrices
				&& ViewModel.CanEdit;
			alternativePricesView.NomenclaturePriceType = NomenclaturePriceBase.NomenclaturePriceType.Alternative;

			#region Вкладка изображения

			Imageslist.Sensitive = ViewModel.CanEdit;
			buttonAddImage.Sensitive = ViewModel.CanEdit;

			if(ViewModel.CanEdit)
			{
				Imageslist.ImageButtonPressEvent += Imageslist_ImageButtonPressEvent;
			}

			#endregion

			#region Вкладка Сайты и приложения

			ConfigureNotSpecialStateForOnlineAvailabilityWidgets();
			ConfigureNotSpecialStateForOnlineMarkerWidgets();
			ConfigureParametersForMobileApp();
			ConfigureParametersForVodovozWebSite();
			ConfigureParametersForKulerSaleWebSite();

			ConfigureTreeOnlinePrices();
			ConfigureOnlineCharacteristics();

			#endregion

			ViewModel.Entity.PropertyChanged += Entity_PropertyChanged;

			//make actions menu
			ConfigureActionsMenu();
		}

		private void PricePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(NomenclaturePrice.Price))
			{
				ViewModel.PriceChanged = true;
			}
		}

		private void PriceRowChanged(object alist, int[] aidx)
		{
			ViewModel.UpdateNomenclatureOnlinePricesNodes();
			ViewModel.SetNeedCheckOnlinePrices();
		}

		private void PriceAdded(object alist, int[] aidx)
		{
			var price = (alist as IList<NomenclaturePriceBase>)[aidx[0]];
			
			switch(price)
			{
				case NomenclaturePrice generalPrice:
					generalPrice.Nomenclature = ViewModel.Entity;
					ViewModel.Entity.NomenclaturePrice.Add(generalPrice);
					ViewModel.AddNotKulerSaleOnlinePrice(generalPrice);
					break;
				case AlternativeNomenclaturePrice alternativePrice:
					alternativePrice.Nomenclature = ViewModel.Entity;
					ViewModel.Entity.AlternativeNomenclaturePrices.Add(alternativePrice);
					ViewModel.AddKulerSaleOnlinePrice(alternativePrice);
					break;
			}

			ViewModel.UpdateNomenclatureOnlinePricesNodes();
		}
		
		private void PriceRemoved(object alist, int[] aidx, object aobject)
		{
			switch(aobject)
			{
				case NomenclaturePrice generalPrice:
					ViewModel.Entity.NomenclaturePrice.Remove(generalPrice);
					ViewModel.RemoveNotKulerSalePrices(generalPrice);
					break;
				case AlternativeNomenclaturePrice alternativePrice:
					ViewModel.Entity.AlternativeNomenclaturePrices.Remove(alternativePrice);
					ViewModel.RemoveKulerSalePrices(alternativePrice);
					break;
			}
		}

		private void ConfigureParametersForMobileApp()
		{
			enumCmbOnlineAvailabilityMobileApp.ItemsEnum = typeof(GoodsOnlineAvailability);
			enumCmbOnlineAvailabilityMobileApp.Binding
				.AddBinding(ViewModel.MobileAppNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumCmbOnlineMarkerMobileApp.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerMobileApp.Binding
				.AddBinding(ViewModel.MobileAppNomenclatureOnlineParameters, p => p.NomenclatureOnlineMarker, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			entryOnlineDiscountMobileApp.Binding
				.AddBinding(ViewModel.MobileAppNomenclatureOnlineParameters, p => p.NomenclatureOnlineDiscount, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();
			entryOnlineDiscountMobileApp.Changed += OnNumericEntryChanged;
		}
		
		private void ConfigureParametersForVodovozWebSite()
		{
			enumCmbOnlineAvailabilityVodovozWebSite.ItemsEnum = typeof(GoodsOnlineAvailability);
			enumCmbOnlineAvailabilityVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumCmbOnlineMarkerVodovozWebSite.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineMarker, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			entryOnlineDiscountVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineDiscount, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();
			entryOnlineDiscountVodovozWebSite.Changed += OnNumericEntryChanged;

			btnCopyPricesFromMobileAppToVodovozWebSite.Clicked += (sender, args) =>
				ViewModel.CopyPricesWithoutDiscountFromMobileAppToVodovozWebSiteCommand.Execute();
		}
		
		private void ConfigureParametersForKulerSaleWebSite()
		{
			enumCmbOnlineAvailabilityKulerSaleWebSite.ItemsEnum = typeof(GoodsOnlineAvailability);
			enumCmbOnlineAvailabilityKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumCmbOnlineMarkerKulerSaleWebSite.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineMarker, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			entryOnlineDiscountKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineDiscount, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();
			entryOnlineDiscountKulerSaleWebSite.Changed += OnNumericEntryChanged;
		}

		private void ConfigureNotSpecialStateForOnlineAvailabilityWidgets()
		{
			enumCmbOnlineAvailabilityMobileApp.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityVodovozWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityKulerSaleWebSite.ShowSpecialStateNot = true;
		}
		
		private void ConfigureNotSpecialStateForOnlineMarkerWidgets()
		{
			enumCmbOnlineMarkerMobileApp.ShowSpecialStateNot = true;
			enumCmbOnlineMarkerVodovozWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineMarkerKulerSaleWebSite.ShowSpecialStateNot = true;
		}
		
		private void ConfigureTreeOnlinePrices()
		{
			treeViewOnlinePrices.ColumnsConfig = FluentColumnsConfig<NomenclatureOnlinePricesNode>.Create()
				.AddColumn("Кол-во (от)")
					.AddNumericRenderer(x => x.MinCount)
				.AddColumn("Цена продажи")
					.AddTextRenderer(x => x.NomenclaturePrice.ToString())
				.AddColumn("Цена продажи\nКулер-Сейл")
					.AddTextRenderer(x => x.KulerSalePrice.ToString())
				.AddColumn("Приложение\n\nЦена без\nскидки")
					.AddTextRenderer(x => x.MobileAppPriceWithoutDiscountString)
					.EditingStartedEvent(OnPriceWithoutDiscountStartedEditing)
					.EditedEvent(OnPriceWithoutDiscountEdited)
					.AddSetter((cell, node) => cell.Editable = node.CanChangeMobileAppPriceWithoutDiscount)
				.AddColumn("Сайт ВВ\n\nЦена без\nскидки")
					.AddTextRenderer(x => x.VodovozWebSitePriceWithoutDiscountString)
					.EditingStartedEvent(OnPriceWithoutDiscountStartedEditing)
					.EditedEvent(OnPriceWithoutDiscountEdited)
					.AddSetter((cell, node) => cell.Editable = node.CanChangeVodovozWebSitePriceWithoutDiscount)
				.AddColumn("Кулер-Сейл\n\nЦена без\nскидки")
					.AddTextRenderer(x => x.KulerSaleWebSitePriceWithoutDiscountString)
					.EditingStartedEvent(OnPriceWithoutDiscountStartedEditing)
					.EditedEvent(OnPriceWithoutDiscountEdited)
					.AddSetter((cell, node) => cell.Editable = node.CanChangeKulerSaleWebSitePriceWithoutDiscount)
				.AddColumn("")
				.Finish();

			treeViewOnlinePrices.ItemsDataSource = ViewModel.NomenclatureOnlinePrices;
		}

		private void ConfigureOnlineCharacteristics()
		{
			lblErpIdValue.Text = ViewModel.Entity.Id.ToString();
			
			listCmbMobileAppOnlineCatalog.ShowSpecialStateNot = true;
			listCmbMobileAppOnlineCatalog.SetRenderTextFunc<MobileAppNomenclatureOnlineCatalog>(x => x.Name);
			listCmbMobileAppOnlineCatalog.ItemsList = ViewModel.MobileAppNomenclatureOnlineCatalogs;
			listCmbMobileAppOnlineCatalog.Binding
				.AddBinding(ViewModel.Entity, vm => vm.MobileAppNomenclatureOnlineCatalog, w => w.SelectedItem)
				.InitializeFromSource();
			
			listCmbVodovozWebSiteOnlineCatalog.ShowSpecialStateNot = true;
			listCmbVodovozWebSiteOnlineCatalog.SetRenderTextFunc<VodovozWebSiteNomenclatureOnlineCatalog>(x => x.Name);
			listCmbVodovozWebSiteOnlineCatalog.ItemsList = ViewModel.VodovozWebSiteNomenclatureOnlineCatalogs;
			listCmbVodovozWebSiteOnlineCatalog.Binding
				.AddBinding(ViewModel.Entity, vm => vm.VodovozWebSiteNomenclatureOnlineCatalog, w => w.SelectedItem)
				.InitializeFromSource();
			
			listCmbKulerSaleWebSiteOnlineCatalog.ShowSpecialStateNot = true;
			listCmbKulerSaleWebSiteOnlineCatalog.SetRenderTextFunc<KulerSaleWebSiteNomenclatureOnlineCatalog>(x => x.Name);
			listCmbKulerSaleWebSiteOnlineCatalog.ItemsList = ViewModel.KulerSaleWebSiteNomenclatureOnlineCatalogs;
			listCmbKulerSaleWebSiteOnlineCatalog.Binding
				.AddBinding(ViewModel.Entity, vm => vm.KulerSaleWebSiteNomenclatureOnlineCatalog, w => w.SelectedItem)
				.InitializeFromSource();
			
			entryNameOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.OnlineName, w => w.Text)
				.InitializeFromSource();
			
			listCmbOnlineGroup.ShowSpecialStateNot = true;
			listCmbOnlineGroup.SetRenderTextFunc<NomenclatureOnlineGroup>(x => x.Name);
			listCmbOnlineGroup.ItemsList = ViewModel.NomenclatureOnlineGroups;
			listCmbOnlineGroup.Binding
				.AddBinding(ViewModel, vm => vm.SelectedOnlineGroup, w => w.SelectedItem)
				.InitializeFromSource();
			
			listCmbOnlineCategory.ShowSpecialStateNot = true;
			listCmbOnlineCategory.SetRenderTextFunc<NomenclatureOnlineCategory>(x => x.Name);
			listCmbOnlineCategory.Changed += OnOnlineCategoryChanged;
			listCmbOnlineCategory.Binding
				.AddBinding(ViewModel, vm => vm.OnlineCategories, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.SelectedOnlineCategory, w => w.SelectedItem)
				.InitializeFromSource();

			#region Онлайн характеристики воды

			vboxWaterOnlineParameters.Binding
				.AddBinding(ViewModel, e => e.IsWaterParameters, w => w.Visible)
				.InitializeFromSource();
			
			enumTareVolumeOnline.Sensitive = false;
			enumTareVolumeOnline.ItemsEnum = typeof(TareVolume);
			enumTareVolumeOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.TareVolume, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			chkNewBottleOnline.Sensitive = false;
			chkNewBottleOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.IsNewBottle, w => w.Active)
				.InitializeFromSource();
			
			chkIsDisposableTareOnline.Sensitive = false;
			chkIsDisposableTareOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.IsDisposableTare, w => w.Active)
				.InitializeFromSource();

			chkSparklingWaterOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.IsSparklingWater, w => w.Active)
				.InitializeFromSource();

			#endregion

			lblInstallationTypeOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			enumCmbInstallationTypeOnline.ShowSpecialStateNot = true;
			enumCmbInstallationTypeOnline.ItemsEnum = typeof(EquipmentInstallationType);
			enumCmbInstallationTypeOnline.ShowSpecialStateNot = true;
			enumCmbInstallationTypeOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.EquipmentInstallationType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			lblWorkloadTypeOnlineTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			enumCmbWorkloadTypeOnline.ShowSpecialStateNot = true;
			enumCmbWorkloadTypeOnline.ItemsEnum = typeof(EquipmentWorkloadType);
			enumCmbWorkloadTypeOnline.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.EquipmentWorkloadType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			lblHeatingOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			nullableChkHeating.RenderMode = RenderMode.Icon;
			nullableChkHeating.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.HasHeating, w => w.Active)
				.InitializeFromSource();
			
			lblProtectionOnHotWaterTapOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			enumCmbProtectionOnHotWaterTapOnline.ShowSpecialStateNot = true;
			enumCmbProtectionOnHotWaterTapOnline.ItemsEnum = typeof(ProtectionOnHotWaterTap);
			enumCmbProtectionOnHotWaterTapOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasHeating, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.ProtectionOnHotWaterTap, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			lblHeatingPowerOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			entryHeatingPowerOnline.MaxLength = 5;
			entryHeatingPowerOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasHeating, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.NewHeatingPower, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryHeatingPowerOnline.Changed += OnNumericEntryChanged;
			
			lblHeatingProductivityOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			entryHeatingProductivityOnline.MaxLength = 5;
			entryHeatingProductivityOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasHeating, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.HeatingProductivity, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryHeatingProductivityOnline.Changed += OnNumericEntryChanged;

			lblCoolingOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			nullableChkCooling.RenderMode = RenderMode.Icon;
			nullableChkCooling.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.HasCooling, w => w.Active)
				.InitializeFromSource();
			
			lblCoolingTypeOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			enumCmbCoolingTypeOnline.ShowSpecialStateNot = true;
			enumCmbCoolingTypeOnline.ItemsEnum = typeof(CoolingType);
			enumCmbCoolingTypeOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasCooling, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.NewCoolingType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
				
			lblCoolingPowerOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			entryCoolingPowerOnline.MaxLength = 5;
			entryCoolingPowerOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasCooling, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.NewCoolingPower, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryCoolingPowerOnline.Changed += OnNumericEntryChanged;
			
			lblCoolingProductivityOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			entryCoolingProductivityOnline.MaxLength = 5;
			entryCoolingProductivityOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasCooling, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.CoolingProductivity, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryCoolingProductivityOnline.Changed += OnNumericEntryChanged;

			lblLockerRefrigeratorOnlineTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			enumCmbLockerRefrigeratorOnline.ShowSpecialStateNot = true;
			enumCmbLockerRefrigeratorOnline.ItemsEnum = typeof(LockerRefrigeratorType);
			enumCmbLockerRefrigeratorOnline.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.LockerRefrigeratorType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			lblLockerRefrigeratorVolumeOnlineTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			entryLockerRefrigeratorVolumeOnline.MaxLength = 5;
			entryLockerRefrigeratorVolumeOnline.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.LockerRefrigeratorType != null, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.LockerRefrigeratorVolume, w => w.Text, new NullableIntToStringConverter())
				.AddBinding(ViewModel, vm => vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			entryLockerRefrigeratorVolumeOnline.Changed += OnNumericEntryChanged;
			
			lblTapTypeOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			enumCmbTapTypeOnline.ShowSpecialStateNot = true;
			enumCmbTapTypeOnline.ItemsEnum = typeof(TapType);
			enumCmbTapTypeOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.TapType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			lblCupHolderBracingOnlineTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			
			enumCmbCupHolderBracing.ShowSpecialStateNot = true;
			enumCmbCupHolderBracing.Sensitive = false;
			enumCmbCupHolderBracing.ItemsEnum = typeof(GlassHolderType);
			enumCmbCupHolderBracing.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.GlassHolderType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			vboxPumpOnlineParameters.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterPumpParameters, w => w.Visible)
				.InitializeFromSource();
			
			lblPumpTypeOnlineTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterPumpParameters, w => w.Visible)
				.InitializeFromSource();
			
			enumPumpTypeOnline.ShowSpecialStateNot = true;
			enumPumpTypeOnline.ItemsEnum = typeof(PumpType);
			enumPumpTypeOnline.Binding
				.AddBinding(ViewModel, vm => vm.IsWaterPumpParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.PumpType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			vboxCupHolderOnlineParameters.Binding
				.AddBinding(ViewModel, vm => vm.IsCupHolderParameters, w => w.Visible)
				.InitializeFromSource();
			
			enumCupHolderBracingTypeOnline.ShowSpecialStateNot = true;
			enumCupHolderBracingTypeOnline.ItemsEnum = typeof(CupHolderBracingType);
			enumCupHolderBracingTypeOnline.Binding
				.AddBinding(ViewModel, vm => vm.IsCupHolderParameters, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.CupHolderBracingType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}

		private void OnOnlineCategoryChanged(object sender, EventArgs e)
		{
			enumCmbInstallationTypeOnline.ClearEnumHideList();
			
			if(ViewModel.IsWaterCoolerParameters)
			{
				enumCmbInstallationTypeOnline.AddEnumToHideList(EquipmentInstallationType.Embedded);
			}
		}

		private void OnNumericEntryChanged(object sender, EventArgs e)
		{
			var entry = sender as Entry;
			var chars = entry.Text.ToCharArray();
			
			var text = ViewModel.StringHandler.ConvertCharsArrayToNumericString(chars);
			entry.Text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
		}

		private void OnPriceWithoutDiscountStartedEditing(object o, EditingStartedArgs args)
		{
			if(args.Args.First() is Entry entry)
			{
				_entry = entry;
				_entry.Changed += OnPriceWithoutDiscountChanged;
			}
		}
		
		private void OnPriceWithoutDiscountEdited(object o, EditedArgs args)
		{
			if(_entry != null)
			{
				_entry.Changed -= OnPriceWithoutDiscountChanged;
				_entry = null;
			}
		}

		private void OnPriceWithoutDiscountChanged(object sender, EventArgs e)
		{
			var entry = sender as Entry;
			var chars = entry.Text.ToCharArray();
			
			var text = ViewModel.StringHandler.ConvertCharsArrayToNumericString(chars, 2);
			entry.Text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
		}

		private void ConfigureActionsMenu()
		{
			var menu = new Menu();
			var menuItem = new MenuItem("Заменить все ссылки на номенклатуру...");
			menuItem.Activated += OnReplaceLinksActivated;
			menu.Add(menuItem);
			menuActions.Menu = menu;
			menu.ShowAll();
			menuActions.Sensitive = !ViewModel.UoWGeneric.IsNew && ViewModel.CanEdit;
		}

		private void YСolorBtnBottleCapColorOnColorSet(object sender, EventArgs e) {
			var color = (sender as yColorButton).Color;

			var colorRed = $"{color.Red:x4}".Remove(2);
			var colorBlue = $"{color.Blue:x4}".Remove(2);
			var colorGreen = $"{color.Green:x4}".Remove(2);

			ViewModel.Entity.BottleCapColor = $"#{colorRed}{colorGreen}{colorBlue}";
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Entity.ProductGroup))
				nomenclaturecharacteristicsview1.RefreshWidgets();
		}

		void OnReplaceLinksActivated(object sender, EventArgs e)
		{
			var replaceDlg = new ReplaceEntityLinksDlg(ViewModel.Entity);
			ViewModel.TabParent.AddSlaveTab(ViewModel, replaceDlg);
		}

		string GenerateOfficialName(object arg)
		{
			var widget = arg as Entry;
			return widget.Text;
		}

		#region Переключение вкладок

		protected void OnRadioInfoToggled(object sender, EventArgs e)
		{
			if(radioInfo.Active)
			{
				notebook.CurrentPage = 0;
			}
		}

		protected void OnRadioEquipmentToggled(object sender, EventArgs e)
		{
			if(radioEquipment.Active)
			{
				notebook.CurrentPage = 1;
			}
		}

		protected void OnRadioCharacteristicsToggled(object sender, EventArgs e)
		{
			if(radioCharacteristics.Active)
			{
				notebook.CurrentPage = 2;
			}
		}

		protected void OnRadioImagesToggled(object sender, EventArgs e)
		{
			if(radioImages.Active) {
				notebook.CurrentPage = 3;
				ImageTabOpen();
			}
		}

		protected void OnRadioPriceToggled(object sender, EventArgs e)
		{
			if(radioPrice.Active)
			{
				notebook.CurrentPage = 4;
			}
		}

		protected void OnPurchasePriceToggled(object sender, EventArgs e)
		{
			if(radioPurchasePrice.Active)
			{
				notebook.CurrentPage = 5;
			}
		}
		
		private void OnSitesAndAppsToggled(object sender, EventArgs e)
		{
			if(radioSitesAndApps.Active)
			{
				notebook.CurrentPage = 6;
			}
		}

		#endregion

		#region Вкладка изображений

		private void ImageTabOpen()
		{
			if(!ViewModel.ImageLoaded) {
				ReloadImages();
				ViewModel.ImageLoaded = true;
			}
		}

		private void ReloadImages()
		{
			Imageslist.Images.Clear();

			foreach(var imageSource in ViewModel.Entity.Images) {
				Imageslist.AddImage(new Gdk.Pixbuf(imageSource.Image), imageSource);
			}
			Imageslist.UpdateList();
		}

		protected void OnButtonAddImageClicked(object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog("Выберите изображение...",
				(Window)this.Toplevel,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept);

			FileFilter Filter = new FileFilter();
			Filter.AddPixbufFormats();
			Filter.Name = "Все изображения";
			Chooser.AddFilter(Filter);

			if((ResponseType)Chooser.Run() == ResponseType.Accept) {
				Chooser.Hide();
				logger.Info("Загрузка изображения...");

				var imageFile = ImageHelper.LoadImageToJpgBytes(Chooser.Filename);
				ViewModel.Entity.Images.Add(new NomenclatureImage(ViewModel.Entity, imageFile));
				ReloadImages();

				logger.Info("Ok");
			}
			Chooser.Destroy();
		}

		void Imageslist_ImageButtonPressEvent(object sender, ImageButtonPressEventArgs e)
		{
			if((int)e.eventArgs.Event.Button == 3) {
				ViewModel.PopupMenuOn = (NomenclatureImage)e.Tag;
				Menu jBox = new Menu();
				MenuItem MenuItem1 = new MenuItem("Удалить");
				MenuItem1.Activated += DeleteImage_Activated; ;
				jBox.Add(MenuItem1);
				jBox.ShowAll();
				jBox.Popup();
			}
		}

		void DeleteImage_Activated(object sender, EventArgs e)
		{
			ViewModel.DeleteImage();
			ReloadImages();
		}

		#endregion

		public override void Dispose()
		{
			if(pricesView != null)
			{
				pricesView.PricesList.ElementAdded -= PriceAdded;
				pricesView.PricesList.ElementRemoved -= PriceRemoved;
				pricesView.PricesList.ElementChanged -= PriceRowChanged;
			}

			if(alternativePricesView != null)
			{
				alternativePricesView.PricesList.ElementAdded -= PriceAdded;
				alternativePricesView.PricesList.ElementRemoved -= PriceRemoved;
				alternativePricesView.PricesList.ElementChanged -= PriceRowChanged;
			}
			
			base.Dispose();
		}
	}
}
