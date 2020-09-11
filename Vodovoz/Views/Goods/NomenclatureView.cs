using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.BusinessCommon.Domain;
using QS.Helpers;
using QS.Navigation;
using QS.Project.Dialogs;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Validation;
using QS.Views.GtkUI;
using QSOrmProject;
using QSWidgetLib;
using Vodovoz.Additions.Store;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.JournalViewModels;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Goods;
using Vodovoz.FilterViewModels.Goods;

namespace Vodovoz.Views.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureView : TabViewBase<NomenclatureViewModel>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		Domain.Store.Warehouse selectedWarehouse;
		
		public NomenclatureView(NomenclatureViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure() {
			notebook1.ShowTabs = false;
			spinWeight.Sensitive = false;
			spinVolume.Sensitive = false;
			lblPercentForMaster.Visible = spinPercentForMaster.Visible = false;
			entryName.IsEditable = true;
			radioInfo.Active = true;

			#region RadioButtons

			radioEquipment.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();
			radioPrice.Binding.AddBinding(ViewModel, vm => vm.SensitivityRadioPriceButton, w => w.Sensitive).InitializeFromSource();

			#endregion

			//buttonSave
			buttonCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);	
			ylabelCreationDate.Binding.AddFuncBinding(ViewModel.Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();
			ylabelCreatedBy.Binding.AddFuncBinding<Nomenclature>(ViewModel.Entity, e => ViewModel.GetUserEmployeeName(), w => w.LabelProp).InitializeFromSource();

			enumVAT.ItemsEnum = typeof(VAT);
			enumVAT.Binding.AddBinding(ViewModel.Entity, e => e.VAT, w => w.SelectedItem).InitializeFromSource();

			enumType.ItemsEnum = typeof(NomenclatureCategory);
			enumType.Binding.AddBinding(ViewModel.Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();

			enumTareVolume.ItemsEnum = typeof(TareVolume);
			enumTareVolume.Binding.AddBinding(ViewModel.Entity, e => e.TareVolume, w => w.SelectedItemOrNull).InitializeFromSource();
			ycheckDisposableTare.Binding.AddBinding(ViewModel.Entity, e => e.IsDisposableTare, w => w.Active).InitializeFromSource();

			yСolorBtnBottleCapColor.Binding.AddBinding(ViewModel.Entity, e => e.BottleCapColor, w => w.Color, new ColorTextToGdkColorConverter()).InitializeFromSource();
			yСolorBtnBottleCapColor.ColorSet += YСolorBtnBottleCapColorOnColorSet;

			enumSaleCategory.ItemsEnum = typeof(SaleCategory);
			enumSaleCategory.Binding.AddBinding(ViewModel.Entity, e => e.SaleCategory, w => w.SelectedItemOrNull).InitializeFromSource();
			enumSaleCategory.Binding.AddBinding(ViewModel, vm => vm.VisibilitySalesCategoriesItems, w => w.Visible).InitializeFromSource();
			lblSaleCategory.Binding.AddBinding(ViewModel, vm => vm.VisibilitySalesCategoriesItems, w => w.Visible).InitializeFromSource();

			enumDepositType.ItemsEnum = typeof(TypeOfDepositCategory);
			enumDepositType.Binding.AddBinding(ViewModel.Entity, e => e.TypeOfDepositCategory, w => w.SelectedItemOrNull).InitializeFromSource();
			enumDepositType.Binding.AddBinding(ViewModel, vm => vm.VisibilityDepositCategoryItems, w => w.Visible).InitializeFromSource();
			lblSubType.Binding.AddBinding(ViewModel, vm => vm.VisibilityDepositCategoryItems, w => w.Visible).InitializeFromSource();

			comboMobileCatalog.ItemsEnum = typeof(MobileCatalog);
			comboMobileCatalog.Binding.AddBinding(ViewModel.Entity, e => e.MobileCatalog, w => w.SelectedItem).InitializeFromSource();

			entryName.Binding.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryOfficialName.Binding.AddBinding(ViewModel.Entity, e => e.OfficialName, w => w.Text).InitializeFromSource();
			var parallel = new ParallelEditing(yentryOfficialName);
			parallel.SubscribeOnChanges(entryName);
			parallel.GetParallelTextFunc = GenerateOfficialName;

			checkNotReserve.Binding.AddBinding(ViewModel.Entity, e => e.DoNotReserve, w => w.Active).InitializeFromSource();
			checkNotReserve.Binding.AddBinding(ViewModel, vm => vm.SensitivityNotServiceOrDepositCategoryItems, w => w.Sensitive).InitializeFromSource();
			labelReserve.Binding.AddBinding(ViewModel, vm => vm.SensitivityNotServiceOrDepositCategoryItems, w => w.Sensitive).InitializeFromSource();
			checkcanPrintPrice.Binding.AddBinding(ViewModel.Entity, e => e.CanPrintPrice, w => w.Active).InitializeFromSource();
			checkcanPrintPrice.Binding.AddFuncBinding(ViewModel, vm => vm.VisibilityWaterCategoryItems, w => w.Visible).InitializeFromSource();
			labelCanPrintPrice.Binding.AddBinding(ViewModel, vm => vm.VisibilityWaterCategoryItems, w => w.Visible).InitializeFromSource();

			checkHide.Binding.AddBinding(ViewModel.Entity, e => e.Hide, w => w.Active).InitializeFromSource();
			entryCode1c.Binding.AddBinding(ViewModel.Entity, e => e.Code1c, w => w.Text).InitializeFromSource();
			yspinSumOfDamage.Binding.AddBinding(ViewModel.Entity, e => e.SumOfDamage, w => w.ValueAsDecimal).InitializeFromSource();
			spinWeight.Binding.AddBinding(ViewModel.Entity, e => e.Weight, w => w.Value).InitializeFromSource();
			spinWeight.Binding.AddBinding(ViewModel, vm => vm.SensitivityNotServiceOrDepositCategoryItems, w => w.Sensitive).InitializeFromSource();
			spinVolume.Binding.AddBinding(ViewModel.Entity, e => e.Volume, w => w.Value).InitializeFromSource();
			spinVolume.Binding.AddBinding(ViewModel, vm => vm.SensitivityNotServiceOrDepositCategoryItems, w => w.Sensitive).InitializeFromSource();
			spinPercentForMaster.Binding.AddBinding(ViewModel.Entity, e => e.PercentForMaster, w => w.Value).InitializeFromSource();
			spinPercentForMaster.Binding.AddBinding(ViewModel, vm => vm.VisibilityMasterCategoryItems, w => w.Visible).InitializeFromSource();
			lblPercentForMaster.Binding.AddBinding(ViewModel, vm => vm.VisibilityMasterCategoryItems, w => w.Visible).InitializeFromSource();

			ycheckNewBottle.Binding.AddBinding(ViewModel.Entity, e => e.IsNewBottle, w => w.Active).InitializeFromSource();
			ycheckDefectiveBottle.Binding.AddBinding(ViewModel.Entity, e => e.IsDefectiveBottle, w => w.Active).InitializeFromSource();
			ycheckShabbyBottle.Binding.AddBinding(ViewModel.Entity, e => e.IsShabbyBottle, w => w.Active).InitializeFromSource();
			
			chkIsDiler.Binding.AddBinding(ViewModel.Entity, e => e.IsDiler, w => w.Active).InitializeFromSource();
			spinMinStockCount.Binding.AddBinding(ViewModel.Entity, e => e.MinStockCount, w => w.ValueAsDecimal).InitializeFromSource();

			ycomboFuelTypes.SetRenderTextFunc<FuelType>(x => x.Name);
			ycomboFuelTypes.ItemsList = ViewModel.UoW.GetAll<FuelType>();
			ycomboFuelTypes.Binding.AddBinding(ViewModel.Entity, e => e.FuelType, w => w.SelectedItem).InitializeFromSource();
			ycomboFuelTypes.Binding.AddBinding(ViewModel, vm => vm.VisibilityFuelCategoryItems, w => w.Visible).InitializeFromSource();
			lblFuelType.Binding.AddBinding(ViewModel, vm => vm.VisibilityFuelCategoryItems, w => w.Visible).InitializeFromSource();

			ylblOnlineStore.Text = ViewModel.Entity.OnlineStore?.Name;
			ylblOnlineStore.Binding.AddBinding(ViewModel, vm => vm.VisibilityAdditionalCategoryItems, w => w.Visible).InitializeFromSource();
			ylblOnlineStoreStr.Binding.AddBinding(ViewModel, vm => vm.VisibilityAdditionalCategoryItems, w => w.Visible).InitializeFromSource();

			yentryFolder1c.SubjectType = typeof(Folder1c);
			yentryFolder1c.Binding.AddBinding(ViewModel.Entity, e => e.Folder1C, w => w.Subject).InitializeFromSource();
			yentryProductGroup.SubjectType = typeof(ProductGroup);
			yentryProductGroup.Binding.AddBinding(ViewModel.Entity, e => e.ProductGroup, w => w.Subject).InitializeFromSource();
			referenceUnit.SubjectType = typeof(MeasurementUnits);
			referenceUnit.Binding.AddBinding(ViewModel.Entity, n => n.Unit, w => w.Subject).InitializeFromSource();


			referenceRouteColumn.SubjectType = typeof(RouteColumn);
			referenceRouteColumn.Binding.AddBinding(ViewModel.Entity, n => n.RouteListColumn, w => w.Subject).InitializeFromSource();

			checkNoDeliver.Binding.AddBinding(ViewModel.Entity, e => e.NoDelivey, w => w.Active).InitializeFromSource();

			yentryShortName.Binding.AddBinding(ViewModel.Entity, e => e.ShortName, w => w.Text, new NullToEmptyStringConverter()).InitializeFromSource();
			yentryShortName.MaxLength = 220;
			checkIsArchive.Binding.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			checkIsArchive.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");

			entityviewmodelentryShipperCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entityviewmodelentryShipperCounterparty.Binding.AddBinding(ViewModel.Entity, s => s.ShipperCounterparty, w => w.Subject).InitializeFromSource();
			entityviewmodelentryShipperCounterparty.Binding.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible).InitializeFromSource();
			entityviewmodelentryShipperCounterparty.CanEditReference = true;
			labelShipperCounterparty.Binding.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible).InitializeFromSource();
			yentryStorageCell.Binding.AddBinding(ViewModel.Entity, s => s.StorageCell, w => w.Text).InitializeFromSource();
			yentryStorageCell.Binding.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible).InitializeFromSource();
			labelStorageCell.Binding.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible).InitializeFromSource();
			yspinbuttonPurchasePrice.Binding.AddBinding(ViewModel.Entity, s => s.PurchasePrice, w => w.ValueAsDecimal).InitializeFromSource();
			yspinbuttonPurchasePrice.Binding.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible).InitializeFromSource();
			labelPurchasePrice.Binding.AddBinding(ViewModel, vm => vm.IsEshopNomenclature, w => w.Visible).InitializeFromSource();
			//UpdateVisibilityForEshopParam();

			#region Вкладка Оборудование

			labelManufacturer.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();
			referenceManufacturer.SubjectType = typeof(Manufacturer);
			referenceManufacturer.Binding.AddBinding(ViewModel.Entity, e => e.Manufacturer, w => w.Subject).InitializeFromSource();
			referenceManufacturer.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();

			labelColor.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();
			referenceColor.SubjectType = typeof(EquipmentColors);
			referenceColor.Binding.AddBinding(ViewModel.Entity, e => e.EquipmentColor, w => w.Subject).InitializeFromSource();
			referenceColor.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();

			labelClass.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();
			yentryrefEqupmentType.SubjectType = typeof(EquipmentType);
			yentryrefEqupmentType.Binding.AddBinding(ViewModel.Entity, e => e.Type, w => w.Subject).InitializeFromSource();
			yentryrefEqupmentType.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();

			labelModel.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();
			entryModel.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();

			labelSerial.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();
			checkSerial.Binding.AddBinding(ViewModel.Entity, e => e.IsSerial, w => w.Active).InitializeFromSource();
			checkSerial.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();

			labelRentPriority.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();
			ycheckRentPriority.Binding.AddBinding(ViewModel.Entity, e => e.RentPriority, w => w.Active).InitializeFromSource();
			ycheckRentPriority.Binding.AddBinding(ViewModel, vm => vm.SensitivityEquipmentCategoryItems, w => w.Sensitive).InitializeFromSource();

			#endregion

			#region Вкладка "Склады отгрузки"

			repTreeViewWarehouses.ColumnsConfig = ColumnsConfigFactory.Create<Domain.Store.Warehouse>()
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
				.Finish();
			repTreeViewWarehouses.SetItemsSource(ViewModel.Entity.ObservableWarehouses);
			repTreeViewWarehouses.Selection.Changed += (sender, e) => {
				ViewModel.SelectedWarehouse = repTreeViewWarehouses.GetSelectedObject<Domain.Store.Warehouse>();
				//btnRemoveWarehouse.Sensitive = selectedWarehouse != null;
			};
			//btnRemoveWarehouse.Sensitive = selectedWarehouse != null;

			//btnAddWarehouse.Clicked += (sender, args) => ViewModel.AddWarehouseCommand.Execute();
			btnRemoveWarehouse.Clicked += (sender, args) => ViewModel.RemoveWarehouseCommand.Execute();

			#endregion

			#region Вкладка характиристики

			ytextDescription.Binding.AddBinding(ViewModel.Entity, e => e.Description, w => w.Buffer.Text).InitializeFromSource();
			nomenclaturecharacteristicsview1.Uow = ViewModel.UoWGeneric;

			#endregion

			//int currNomenclatureOfDependence = (ViewModel.Entity.DependsOnNomenclature == null ? 0 : ViewModel.Entity.DependsOnNomenclature.Id);

			dependsOnNomenclature.RepresentationModel = new NomenclatureDependsFromVM(ViewModel.Entity);
			dependsOnNomenclature.Binding.AddBinding(ViewModel.Entity, e => e.DependsOnNomenclature, w => w.Subject).InitializeFromSource();

			entityviewmodelentry1.SetEntityAutocompleteSelectorFactory(ViewModel.NomenclatureSelectorFactory);

			entityviewmodelentry1.Binding.AddBinding(ViewModel.Entity, s => s.DependsOnNomenclature, w => w.Subject).InitializeFromSource();
			entityviewmodelentry1.CanEditReference = true;

			ConfigureInputs(ViewModel.Entity.Category, ViewModel.Entity.TareVolume);

			pricesView.UoWGeneric = ViewModel.UoWGeneric;
			pricesView.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");

			Imageslist.ImageButtonPressEvent += Imageslist_ImageButtonPressEvent;

			ViewModel.Entity.PropertyChanged += Entity_PropertyChanged;

			//make actions menu
			var menu = new Gtk.Menu();
			var menuItem = new Gtk.MenuItem("Заменить все ссылки на номенклатуру...");
			menuItem.Activated += MenuItem_ReplaceLinks_Activated; ;
			menu.Add(menuItem);
			menuActions.Menu = menu;
			menu.ShowAll();
			menuActions.Sensitive = !ViewModel.UoWGeneric.IsNew;
		}
		
		private void YСolorBtnBottleCapColorOnColorSet(object sender, EventArgs e) {
			var color = (sender as yColorButton).Color;

			var colorRed = $"{color.Red:x4}".Remove(2);
			var colorBlue = $"{color.Blue:x4}".Remove(2);
			var colorGreen = $"{color.Green:x4}".Remove(2);

			ViewModel.Entity.BottleCapColor = $"#{colorRed}{colorGreen}{colorBlue}";
		}

		void UpdateVisibilityForEshopParam()
		{
			bool isEshopNomenclature = ViewModel.Entity?.ProductGroup?.ExportToOnlineStore ?? false;

			//entityviewmodelentryShipperCounterparty.Visible = isEshopNomenclature;
			//labelShipperCounterparty.Visible = isEshopNomenclature;
			//yentryStorageCell.Visible = isEshopNomenclature;
			//labelStorageCell.Visible = isEshopNomenclature;
			//yspinbuttonPurchasePrice.Visible = isEshopNomenclature;
			//labelPurchasePrice.Visible = isEshopNomenclature;
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Entity.ProductGroup))
				nomenclaturecharacteristicsview1.RefreshWidgets();
		}

		void MenuItem_ReplaceLinks_Activated(object sender, EventArgs e)
		{
			var replaceDlg = new ReplaceEntityLinksDlg(ViewModel.Entity);
			ViewModel.TabParent.OpenTab(() => replaceDlg);
		}

		string GenerateOfficialName(object arg)
		{
			var widget = arg as Gtk.Entry;
			return widget.Text;
		}

		public /*override*/ bool Save()
		{
			if(String.IsNullOrWhiteSpace(ViewModel.Entity.Code1c)) {
				ViewModel.Entity.Code1c = new NomenclatureRepository().GetNextCode1c(ViewModel.UoW);
			}

			var valid = new QSValidator<Nomenclature>(ViewModel.UoWGeneric.Root);
			
			if(valid.RunDlgIfNotValid((Window)this.Toplevel))
				return false;
			
			logger.Info("Сохраняем номенклатуру...");
			ViewModel.Entity.SetNomenclatureCreationInfo(UserSingletonRepository.GetInstance());
			pricesView.SaveChanges();
			ViewModel.UoWGeneric.Save();
			return true;
		}

		protected void OnEnumTypeChanged(object sender, EventArgs e)
		{
			ConfigureInputs(ViewModel.Entity.Category, ViewModel.Entity.TareVolume);

			if(ViewModel.Entity.Category != NomenclatureCategory.deposit) {
				ViewModel.Entity.TypeOfDepositCategory = null;
			}
		}

		protected void ConfigureInputs(NomenclatureCategory selected, TareVolume? tareVolume)
		{
			//radioEquipment.Sensitive = selected == NomenclatureCategory.equipment;
			//enumSaleCategory.Visible = lblSaleCategory.Visible = Nomenclature.GetCategoriesWithSaleCategory().Contains(selected);
			//enumDepositType.Visible = lblSubType.Visible = selected == NomenclatureCategory.deposit;
			//ylblOnlineStore.Visible = ylblOnlineStoreStr.Visible = selected == NomenclatureCategory.additional;

			//spinWeight.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.deposit);
			//spinVolume.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.deposit);
			//lblPercentForMaster.Visible = spinPercentForMaster.Visible = (selected == NomenclatureCategory.master);

			//labelManufacturer.Sensitive = referenceManufacturer.Sensitive = (selected == NomenclatureCategory.equipment);
			//labelColor.Sensitive = referenceColor.Sensitive = (selected == NomenclatureCategory.equipment);
			//labelClass.Sensitive = yentryrefEqupmentType.Sensitive = (selected == NomenclatureCategory.equipment);
			//labelModel.Sensitive = entryModel.Sensitive = (selected == NomenclatureCategory.equipment);
			//labelSerial.Sensitive = checkSerial.Sensitive = (selected == NomenclatureCategory.equipment);
			//labelRentPriority.Sensitive = ycheckRentPriority.Sensitive = (selected == NomenclatureCategory.equipment);

			//labelReserve.Sensitive = checkNotReserve.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.deposit);
			//labelCanPrintPrice.Visible = checkcanPrintPrice.Visible = ViewModel.Entity.Category == NomenclatureCategory.water && !ViewModel.Entity.IsDisposableTare;

			labelTypeTare.Visible = hboxTare.Visible = selected == NomenclatureCategory.water;
			hboxBottleCapColor.Visible = tareVolume == TareVolume.Vol19L;
			hboxTareChecks.Sensitive = selected == NomenclatureCategory.bottle;
			//lblFuelType.Visible = ycomboFuelTypes.Visible = selected == NomenclatureCategory.fuel;
			//FIXME запуск оборудования - временный фикс
			//if (Entity.Category == NomenclatureCategory.equipment)
			//Entity.Serial = true;
		}

		#region Переключение вкладок

		protected void OnRadioInfoToggled(object sender, EventArgs e)
		{
			if(radioInfo.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnRadioWarehousesToggled(object sender, EventArgs e)
		{
			if(radioWarehouses.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadioEquipmentToggled(object sender, EventArgs e)
		{
			if(radioEquipment.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnRadioCharacteristicsToggled(object sender, EventArgs e)
		{
			if(radioCharacteristics.Active)
				notebook1.CurrentPage = 3;
		}

		protected void OnRadioImagesToggled(object sender, EventArgs e)
		{
			if(radioImages.Active) {
				notebook1.CurrentPage = 4;
				ImageTabOpen();
			}
		}

		protected void OnRadioPriceToggled(object sender, EventArgs e)
		{
			if(radioPrice.Active)
				notebook1.CurrentPage = 5;
		}

		#endregion

		#region Вкладка изображений

		bool imageLoaded = false;
		NomenclatureImage popupMenuOn;

		private void ImageTabOpen()
		{
			if(!imageLoaded) {
				ReloadImages();
				imageLoaded = true;
			}
		}

		private void ReloadImages()
		{
			Imageslist.Images.Clear();

			if(ViewModel.Entity.Images == null)
				ViewModel.Entity.Images = new List<NomenclatureImage>();

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
				popupMenuOn = (NomenclatureImage)e.Tag;
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
			ViewModel.Entity.Images.Remove(popupMenuOn);
			popupMenuOn = null;
			ReloadImages();
		}

		#endregion

		/*protected void OnDependsOnNomenclatureChanged(object sender, EventArgs e)
		{
			radioPrice.Sensitive = ViewModel.Entity.DependsOnNomenclature == null;
		}*/

		protected void OnBtnAddWarehouseClicked(object sender, EventArgs e)
		{
			var refWin = new OrmReference(StoreDocumentHelper.GetWarehouseQuery()) {
				ButtonMode = ReferenceButtonMode.None,
				Mode = OrmReferenceMode.MultiSelect
			};
			refWin.ObjectSelected += RefWin_ObjectSelected;
			ViewModel.TabParent.AddSlaveTab(ViewModel, refWin);
		}

		void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var warehouses = e.Subjects.OfType<Domain.Store.Warehouse>();
			foreach(var w in warehouses) {
				if(w != null && !ViewModel.Entity.ObservableWarehouses.Any(x => x.Id == w.Id))
					ViewModel.Entity.ObservableWarehouses.Add(w);
			}
		}

		protected void OnEnumTypeChangedByUser(object sender, EventArgs e)
		{
			if(ViewModel.Entity.Id == 0 && Nomenclature.GetCategoriesWithSaleCategory().Contains(ViewModel.Entity.Category))
				ViewModel.Entity.SaleCategory = SaleCategory.notForSale;
		}

		protected void OnYentryProductGroupChangedByUser(object sender, EventArgs e)
		{
			UpdateVisibilityForEshopParam();
		}

		protected void OnEnumTareVolumeChanged(object sender, EventArgs e)
		{
			hboxBottleCapColor.Visible = ViewModel.Entity.TareVolume == TareVolume.Vol19L;
		}
	}
}
