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
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Logistic;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Representations.ProductGroups;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewModels.Dialogs.Goods;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureView : TabViewBase<NomenclatureViewModel>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		
		public NomenclatureView(NomenclatureViewModel viewModel) : base(viewModel)
		{
			this.Build();
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

			#endregion

			buttonSave.Clicked += (sender, args) =>
			{
				ViewModel.Entity.NomenclaturePrice.Clear();
				foreach(var item in pricesView.Prices.Cast<NomenclaturePrice>())
				{
					item.Nomenclature = ViewModel.Entity;
					ViewModel.Entity.NomenclaturePrice.Add(item);
				}

				ViewModel.Entity.AlternativeNomenclaturePrices.Clear();
				foreach(var item in alternativePricesView.Prices.Cast<AlternativeNomenclaturePrice>())
				{
					item.Nomenclature = ViewModel.Entity;
					ViewModel.Entity.AlternativeNomenclaturePrices.Add(item);
				}

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

			enumCategory.Changed += ViewModel.OnEnumKindChanged;
			enumCategory.ChangedByUser += ViewModel.OnEnumKindChangedByUser;
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
			pricesView.Sensitive = ViewModel.CanCreateAndArcNomenclatures && ViewModel.CanEdit;
			pricesView.NomenclaturePriceType = NomenclaturePriceBase.NomenclaturePriceType.General;

			alternativePricesView.Prices = ViewModel.Entity.AlternativeNomenclaturePrices.Cast<NomenclaturePriceBase>().ToList();
			alternativePricesView.Sensitive = ViewModel.CanCreateAndArcNomenclatures && ViewModel.CanEditAlternativeNomenclaturePrices &&
			                                  ViewModel.CanEdit;
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

			#endregion

			ViewModel.Entity.PropertyChanged += Entity_PropertyChanged;

			//make actions menu
			ConfigureActionsMenu();
		}

		private void ConfigureParametersForMobileApp()
		{
			enumCmbOnlineAvailabilityMobileApp.ItemsEnum = typeof(NomenclatureOnlineAvailability);
			enumCmbOnlineAvailabilityMobileApp.Binding
				.AddBinding(ViewModel.MobileAppNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumCmbOnlineMarkerMobileApp.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerMobileApp.Binding
				.AddBinding(ViewModel.MobileAppNomenclatureOnlineParameters, p => p.NomenclatureOnlineMarker, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			spinBtnOnlineDiscountMobileApp.Binding
				.AddBinding(ViewModel.MobileAppNomenclatureOnlineParameters, p => p.NomenclatureOnlineDiscount, w => w.ValueAsDecimal)
				.InitializeFromSource();
		}
		
		private void ConfigureParametersForVodovozWebSite()
		{
			enumCmbOnlineAvailabilityVodovozWebSite.ItemsEnum = typeof(NomenclatureOnlineAvailability);
			enumCmbOnlineAvailabilityVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumCmbOnlineMarkerVodovozWebSite.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineMarker, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			spinBtnOnlineDiscountVodovozWebSite.Binding
				.AddBinding(ViewModel.VodovozWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineDiscount, w => w.ValueAsDecimal)
				.InitializeFromSource();
		}
		
		private void ConfigureParametersForKulerSaleWebSite()
		{
			enumCmbOnlineAvailabilityKulerSaleWebSite.ItemsEnum = typeof(NomenclatureOnlineAvailability);
			enumCmbOnlineAvailabilityKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineAvailability, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumCmbOnlineMarkerKulerSaleWebSite.ItemsEnum = typeof(NomenclatureOnlineMarker);
			enumCmbOnlineMarkerKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineMarker, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			spinBtnOnlineDiscountKulerSaleWebSite.Binding
				.AddBinding(ViewModel.KulerSaleWebSiteNomenclatureOnlineParameters, p => p.NomenclatureOnlineDiscount, w => w.ValueAsDecimal)
				.InitializeFromSource();
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
			throw new NotImplementedException();
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

	}
}
