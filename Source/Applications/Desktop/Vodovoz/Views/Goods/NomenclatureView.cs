using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gtk;
using QS.BusinessCommon.Domain;
using QS.Views.GtkUI;
using QS.Widgets;
using QSOrmProject;
using QSWidgetLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Dialogs.Nodes;
using VodovozBusiness.Domain.Goods.NomenclaturesOnlineParameters;
using Menu = Gtk.Menu;
using MenuItem = Gtk.MenuItem;

namespace Vodovoz.Views.Goods
{
	[ToolboxItem(true)]
	public partial class NomenclatureView : TabViewBase<NomenclatureViewModel>
	{
		private Entry _entry;
		private const int _maxWidthOnlineSizeWidget = 50;
		private const int _maxLenghtNumericEntry = 5;

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

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CloseCommand);

			ylabelCreationDate.Binding
				.AddFuncBinding(ViewModel.Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "",
					w => w.LabelProp)
				.InitializeFromSource();
			ylabelCreatedBy.Binding
				.AddFuncBinding(ViewModel.Entity, e => ViewModel.GetUserEmployeeName(), w => w.LabelProp)
				.InitializeFromSource();

			/*enumVAT.ItemsEnum = typeof(VAT);
			enumVAT.Binding
				.AddBinding(ViewModel.Entity, e => e.VAT, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();*/

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

			ylabelServiceType.Binding
				.AddBinding(ViewModel, vm => vm.IsMasterCategory, w => w.Visible)
				.InitializeFromSource();

			enumServiceType.ItemsEnum = typeof(MasterServiceType);
			enumServiceType.Binding
				.AddBinding(ViewModel.Entity, e => e.MasterServiceType, w => w.SelectedItemOrNull)
				.AddBinding(ViewModel, vm => vm.IsMasterCategory, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
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

			entryCode1c.IsEditable = false;
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

			entityentryProductGroup.ViewModel = ViewModel.ProductGroupEntityEntryViewModel;
			entityentryProductGroup.Sensitive = ViewModel.CanEdit;

			referenceUnit.SubjectType = typeof(MeasurementUnits);
			referenceUnit.Binding
				.AddBinding(ViewModel.Entity, n => n.Unit, w => w.Subject)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			entityentryRouteColumn.ViewModel = ViewModel.RouteColumnViewModel;
			entityentryRouteColumn.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			checkNoDeliver.Binding
				.AddBinding(ViewModel.Entity, e => e.NoDelivery, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			checkNeedSanitisation.Binding
				.AddBinding(ViewModel.Entity, e => e.IsNeedSanitisation, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEditNeedSanitisation, w => w.Sensitive)
				.InitializeFromSource();
			
			yentryShortName.Binding
				.AddBinding(ViewModel.Entity, e => e.ShortName, w => w.Text, new NullToEmptyStringConverter())
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			yentryShortName.MaxLength = 220;

			checkIsArchive.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanCreateAndArcNomenclatures && vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			checkIsArchive.Active = ViewModel.Entity.IsArchive;

			checkIsArchive.Released += OnCheckIsArchiveReleased;

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

			ybuttonEditGtins.BindCommand(ViewModel.EditGtinsCommand);

			yentryGtins.Binding
				.AddBinding(ViewModel, vm => vm.GtinsString, w => w.Text)
				.InitializeFromSource();

			ybuttonEditGroupGtins.BindCommand(ViewModel.EditGroupGtinsCommand);

			ytreeviewGroupGtins.CreateFluentColumnsConfig<GroupGtinEntity>()
				.AddColumn("Gtin").AddTextRenderer(x => x.GtinNumber)
				.AddColumn("Штук в упаковке").AddNumericRenderer(x => x.CodesCount)
				.AddColumn("")
				.Finish();

			ytreeviewGroupGtins.ItemsDataSource = ViewModel.Entity.GroupGtins;

			GtkScrolledWindowGroupGtins.HeightRequest = 80;

			chkInventoryAccounting.Binding
				.AddBinding(ViewModel.Entity, e => e.HasInventoryAccounting, w => w.Active)
				.AddBinding(ViewModel, vm => vm.UserCanCreateNomenclaturesWithInventoryAccounting, w => w.Sensitive)
				.InitializeFromSource();

			lblConditionAccounting.Binding
				.AddBinding(ViewModel, vm => vm.CanShowConditionAccounting, w => w.Visible)
				.InitializeFromSource();
			chkConditionAccounting.Binding
				.AddBinding(ViewModel.Entity, e => e.HasConditionAccounting, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanShowConditionAccounting, w => w.Visible)
				.AddBinding(ViewModel, vm => vm.UserCanEditConditionAccounting, w => w.Sensitive)
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

			entryDependsOnNomenclature.ViewModel = ViewModel.DependsOnNomenclatureEntryViewModel;
			entryDependsOnNomenclature.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

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

			nomenclatureMinimumBalanceByWarehouseView.ViewModel = ViewModel.NomenclatureMinimumBalanceByWarehouseViewModel;

			#region Вкладка изображения			

			attachedfileinformationsview1.ViewModel = ViewModel.AttachedFileInformationsViewModel;

			#endregion

			#region Вкладка Сайты и приложения

			ConfigureParametersForMobileApp();
			ConfigureParametersForVodovozWebSite();
			ConfigureParametersForKulerSaleWebSite();
			ConfigureParametersForRobotMia();

			ConfigureTreeOnlinePrices();
			ConfigureOnlineCharacteristics();

			#endregion

			ViewModel.Entity.PropertyChanged += OnEntityPropertyChanged;

			//make actions menu
			ConfigureActionsMenu();
		}

		private void OnCheckIsArchiveReleased(object sender, EventArgs e)
		{
			if(ViewModel.Entity.IsArchive)
			{
				ViewModel.UnArchiveCommand.Execute();
			}
			else
			{
				ViewModel.ArchiveCommand.Execute();
			}

			checkIsArchive.Active = ViewModel.Entity.IsArchive;
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
			enumCmbOnlineAvailabilityMobileApp.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityMobileApp.Binding
				.AddBinding(ViewModel.MobileAppNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			enumCmbOnlineMarkerMobileApp.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerMobileApp.ShowSpecialStateNot = true;
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
			enumCmbOnlineAvailabilityVodovozWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			enumCmbOnlineMarkerVodovozWebSite.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerVodovozWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineMarkerVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineMarker, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			entryOnlineDiscountVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineDiscount, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();
			entryOnlineDiscountVodovozWebSite.Changed += OnNumericEntryChanged;

			btnCopyPricesFromMobileAppToVodovozWebSite.BindCommand(ViewModel.CopyPricesWithoutDiscountFromMobileAppToVodovozWebSiteCommand);
		}

		private void ConfigureParametersForKulerSaleWebSite()
		{
			enumCmbOnlineAvailabilityKulerSaleWebSite.ItemsEnum = typeof(GoodsOnlineAvailability);
			enumCmbOnlineAvailabilityKulerSaleWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			enumCmbOnlineMarkerKulerSaleWebSite.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerKulerSaleWebSite.ShowSpecialStateNot = true;
			enumCmbOnlineMarkerKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineMarker, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			entryOnlineDiscountKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineDiscount, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();
			entryOnlineDiscountKulerSaleWebSite.Changed += OnNumericEntryChanged;
		}

		private void ConfigureParametersForRobotMia()
		{
			yTreeViewSlangWords.CreateFluentColumnsConfig<SlangWord>()
				.AddColumn("Номер")
					.AddNumericRenderer(x => x.Id)
				.AddColumn("Слово")
					.AddTextRenderer(x => x.Word)
				.Finish();

			yTreeViewSlangWords.Binding
				.AddBinding(ViewModel.RobotMiaParameters, vm => vm.SlangWords, w => w.ItemsDataSource)
				.AddBinding(ViewModel, vm => vm.SelectedSlangWordObject, w => w.SelectedRow)
				.InitializeFromSource();

			enumCmbOnlineAvailabilityRobotMia.ItemsEnum = typeof(GoodsOnlineAvailability);
			enumCmbOnlineAvailabilityRobotMia.ShowSpecialStateNot = true;
			enumCmbOnlineAvailabilityRobotMia.Binding
				.AddBinding(ViewModel.RobotMiaParameters, p => p.GoodsOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			btnAddSlangWord.BindCommand(ViewModel.AddSlangWordCommand);
			btnEditSlangWord.BindCommand(ViewModel.EditSlangWordCommand);
			btnRemoveSlangWord.BindCommand(ViewModel.RemoveSlangWordCommand);
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

			entryLengthOnline.WidthRequest = _maxWidthOnlineSizeWidget;
			entryLengthOnline.MaxLength = _maxLenghtNumericEntry;
			entryLengthOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.LengthOnline, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryLengthOnline.Changed += OnNumericEntryChanged;

			entryWidthOnline.WidthRequest = _maxWidthOnlineSizeWidget;
			entryWidthOnline.MaxLength = _maxLenghtNumericEntry;
			entryWidthOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.WidthOnline, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryWidthOnline.Changed += OnNumericEntryChanged;

			entryHeightOnline.WidthRequest = _maxWidthOnlineSizeWidget;
			entryHeightOnline.MaxLength = _maxLenghtNumericEntry;
			entryHeightOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.HeightOnline, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryHeightOnline.Changed += OnNumericEntryChanged;

			entryWeightOnline.MaxLength = 8;
			entryWeightOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.WeightOnline, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();
			entryWeightOnline.Changed += OnNumericWithDotFractionalPartChanged;

			#region Онлайн характеристики воды

			tableWaterOnlineCharacteristics.Binding
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

			#region Нагрев

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
			hboxHeatingPowerOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasHeating, w => w.Sensitive)
				.InitializeFromSource();

			entryHeatingPowerOnline.MaxLength = _maxLenghtNumericEntry;
			entryHeatingPowerOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.NewHeatingPower, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryHeatingPowerOnline.Changed += OnNumericEntryChanged;

			enumHeatingPowerUnitsOnline.ShowSpecialStateNot = true;
			enumHeatingPowerUnitsOnline.ItemsEnum = typeof(PowerUnits);
			enumHeatingPowerUnitsOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.HeatingPowerUnits, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			lblHeatingProductivityOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			hboxHeatingProductivityOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasHeating, w => w.Sensitive)
				.InitializeFromSource();

			enumHeatingProductivityFromToOnline.ShowSpecialStateNot = true;
			enumHeatingProductivityFromToOnline.ItemsEnum = typeof(ProductivityComparisionSign);
			enumHeatingProductivityFromToOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.HeatingProductivityComparisionSign, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			entryHeatingProductivityOnline.MaxLength = _maxLenghtNumericEntry;
			entryHeatingProductivityOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.HeatingProductivity, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();
			entryHeatingProductivityOnline.Changed += OnNumericWithDotFractionalPartChanged;

			enumHeatingProductivityUnitsOnline.ShowSpecialStateNot = true;
			enumHeatingProductivityUnitsOnline.ItemsEnum = typeof(ProductivityUnits);
			enumHeatingProductivityUnitsOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.HeatingProductivityUnits, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			lblHeatingTemperatureOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			hboxHeatingTemperatureOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasHeating, w => w.Sensitive)
				.InitializeFromSource();

			entryHeatingTemperatureOnlineFrom.MaxLength = _maxLenghtNumericEntry;
			entryHeatingTemperatureOnlineFrom.Binding
				.AddBinding(ViewModel.Entity, e => e.HeatingTemperatureFromOnline, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryHeatingTemperatureOnlineFrom.Changed += OnNumericEntryChanged;

			entryHeatingTemperatureOnlineTo.MaxLength = _maxLenghtNumericEntry;
			entryHeatingTemperatureOnlineTo.Binding
				.AddBinding(ViewModel.Entity, e => e.HeatingTemperatureToOnline, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryHeatingTemperatureOnlineTo.Changed += OnNumericEntryChanged;

			#endregion

			#region Охлаждение

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
			hboxCoolingPowerOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasCooling, w => w.Sensitive)
				.InitializeFromSource();

			entryCoolingPowerOnline.MaxLength = _maxLenghtNumericEntry;
			entryCoolingPowerOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.NewCoolingPower, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryCoolingPowerOnline.Changed += OnNumericEntryChanged;

			enumCoolingPowerUnitsOnline.ShowSpecialStateNot = true;
			enumCoolingPowerUnitsOnline.ItemsEnum = typeof(PowerUnits);
			enumCoolingPowerUnitsOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.CoolingPowerUnits, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			lblCoolingProductivityOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			hboxCoolingProductivityOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasCooling, w => w.Sensitive)
				.InitializeFromSource();

			enumCoolingProductivityFromToOnline.ShowSpecialStateNot = true;
			enumCoolingProductivityFromToOnline.ItemsEnum = typeof(ProductivityComparisionSign);
			enumCoolingProductivityFromToOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.CoolingProductivityComparisionSign, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			entryCoolingProductivityOnline.MaxLength = _maxLenghtNumericEntry;
			entryCoolingProductivityOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.CoolingProductivity, w => w.Text, new NullableDecimalToStringConverter())
				.InitializeFromSource();
			entryCoolingProductivityOnline.Changed += OnNumericWithDotFractionalPartChanged;

			enumCoolingProductivityUnitsOnline.ShowSpecialStateNot = true;
			enumCoolingProductivityUnitsOnline.ItemsEnum = typeof(ProductivityUnits);
			enumCoolingProductivityUnitsOnline.Binding
				.AddBinding(ViewModel.Entity, e => e.CoolingProductivityUnits, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			lblCoolingTemperatureOnlineTitle.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.InitializeFromSource();
			hboxCoolingTemperatureOnline.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsPurifierParameters || vm.IsWaterCoolerParameters, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.HasCooling, w => w.Sensitive)
				.InitializeFromSource();

			entryCoolingTemperatureOnlineFrom.MaxLength = _maxLenghtNumericEntry;
			entryCoolingTemperatureOnlineFrom.Binding
				.AddBinding(ViewModel.Entity, e => e.CoolingTemperatureFromOnline, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryCoolingTemperatureOnlineFrom.Changed += OnNumericEntryChanged;

			entryCoolingTemperatureOnlineTo.MaxLength = _maxLenghtNumericEntry;
			entryCoolingTemperatureOnlineTo.Binding
				.AddBinding(ViewModel.Entity, e => e.CoolingTemperatureToOnline, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryCoolingTemperatureOnlineTo.Changed += OnNumericEntryChanged;

			#endregion

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
			entryLockerRefrigeratorVolumeOnline.MaxLength = _maxLenghtNumericEntry;
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

			tablePumpCupHolderOnlineCharacteristics.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsWaterPumpParameters || vm.IsCupHolderParameters, w => w.Visible)
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

			lblCupHolderBracingTypeOnlineTitle.Binding
				.AddBinding(ViewModel, vm => vm.IsCupHolderParameters, w => w.Visible)
				.InitializeFromSource();

			enumCupHolderBracingTypeOnline.ShowSpecialStateNot = true;
			enumCupHolderBracingTypeOnline.ItemsEnum = typeof(CupHolderBracingType);
			enumCupHolderBracingTypeOnline.Binding
				.AddBinding(ViewModel, vm => vm.IsCupHolderParameters, w => w.Visible)
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

		private void OnPriceWithoutDiscountChanged(object sender, EventArgs e) => OnNumericWithFractionalPartChanged(sender, e, true);

		private void OnNumericWithDotFractionalPartChanged(object sender, EventArgs e) =>
			OnNumericWithFractionalPartChanged(sender, e, false);

		private void OnNumericWithFractionalPartChanged(object sender, EventArgs e, bool isCommaSeparator)
		{
			var entry = sender as Entry;
			var chars = entry.Text.ToCharArray();

			var text = ViewModel.StringHandler.ConvertCharsArrayToNumericString(chars, 2, isCommaSeparator);
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
			menuActions.Sensitive = ViewModel.Entity.Id != 0 && ViewModel.CanEdit;
		}

		private void YСolorBtnBottleCapColorOnColorSet(object sender, EventArgs e)
		{
			var color = (sender as yColorButton).Color;

			var colorRed = $"{color.Red:x4}".Remove(2);
			var colorBlue = $"{color.Blue:x4}".Remove(2);
			var colorGreen = $"{color.Green:x4}".Remove(2);

			ViewModel.Entity.BottleCapColor = $"#{colorRed}{colorGreen}{colorBlue}";
		}

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Entity.ProductGroup))
			{
				nomenclaturecharacteristicsview1.RefreshWidgets();
			}
		}

		private void OnReplaceLinksActivated(object sender, EventArgs e)
		{
			var replaceDlg = new ReplaceEntityLinksDlg(ViewModel.Entity);
			ViewModel.TabParent.AddSlaveTab(ViewModel, replaceDlg);
		}

		private string GenerateOfficialName(object arg)
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
			if(radioImages.Active)
			{
				notebook.CurrentPage = 3;
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

		public override void Destroy()
		{
			enumCategory.Changed -= ViewModel.OnEnumCategoryChanged;
			enumCategory.ChangedByUser -= ViewModel.OnEnumCategoryChangedByUser;
			ViewModel.Entity.PropertyChanged -= OnEntityPropertyChanged;
			UnsubscribePricesViews();
			UnsubscribeSitesAndAppsTabWidgets();
			base.Destroy();
		}

		private void UnsubscribeSitesAndAppsTabWidgets()
		{
			entryOnlineDiscountMobileApp.Changed -= OnNumericEntryChanged;
			entryOnlineDiscountVodovozWebSite.Changed -= OnNumericEntryChanged;
			entryOnlineDiscountKulerSaleWebSite.Changed -= OnNumericEntryChanged;
			listCmbOnlineCategory.Changed -= OnOnlineCategoryChanged;
			entryLengthOnline.Changed -= OnNumericEntryChanged;
			entryWidthOnline.Changed -= OnNumericEntryChanged;
			entryHeightOnline.Changed -= OnNumericEntryChanged;
			entryWeightOnline.Changed -= OnNumericWithDotFractionalPartChanged;
			entryHeatingPowerOnline.Changed -= OnNumericEntryChanged;
			entryHeatingProductivityOnline.Changed -= OnNumericWithDotFractionalPartChanged;
			entryHeatingTemperatureOnlineFrom.Changed -= OnNumericEntryChanged;
			entryHeatingTemperatureOnlineTo.Changed -= OnNumericEntryChanged;
			entryCoolingPowerOnline.Changed -= OnNumericEntryChanged;
			entryCoolingProductivityOnline.Changed -= OnNumericWithDotFractionalPartChanged;
			entryCoolingTemperatureOnlineFrom.Changed -= OnNumericEntryChanged;
			entryCoolingTemperatureOnlineTo.Changed -= OnNumericEntryChanged;
			entryLockerRefrigeratorVolumeOnline.Changed -= OnNumericEntryChanged;
		}

		private void UnsubscribePricesViews()
		{
			if(pricesView != null)
			{
				pricesView.PricesList.ElementAdded -= PriceAdded;
				pricesView.PricesList.ElementRemoved -= PriceRemoved;
				pricesView.PricesList.ElementChanged -= PriceRowChanged;
				pricesView.PricesList.PropertyOfElementChanged -= PricePropertyChanged;
			}

			if(alternativePricesView != null)
			{
				alternativePricesView.PricesList.ElementAdded -= PriceAdded;
				alternativePricesView.PricesList.ElementRemoved -= PriceRemoved;
				alternativePricesView.PricesList.ElementChanged -= PriceRowChanged;
				alternativePricesView.PricesList.PropertyOfElementChanged -= PricePropertyChanged;
			}
		}
	}
}
