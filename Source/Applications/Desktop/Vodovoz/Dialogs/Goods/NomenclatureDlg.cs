using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QS.Helpers;
using QS.BusinessCommon.Domain;
using QSOrmProject;
using QS.Validation;
using QSWidgetLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.ViewModel;
using Vodovoz.Domain.Logistic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.EntityRepositories;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Factories;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Representations;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz
{
	public partial class NomenclatureDlg : QS.Dialog.Gtk.EntityDialogBase<Nomenclature>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();
		
		private Warehouse _selectedWarehouse;
		private ValidationContext _validationContext;

		public NomenclatureDlg()
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Nomenclature>();
			TabName = "Новая номенклатура";
			ConfigureDlg();
		}

		public NomenclatureDlg(int id)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Nomenclature>(id);
			ConfigureDlg();
		}

		public NomenclatureDlg(Nomenclature sub) : this(sub.Id) { }

		private void ConfigureDlg()
		{
			notebook1.ShowTabs = false;
			spinWeight.Sensitive = false;
			spinVolume.Sensitive = false;
			lblPercentForMaster.Visible = spinPercentForMaster.Visible = false;
			entryName.IsEditable = true;
			radioInfo.Active = true;

			ylabelCreationDate.Binding.AddFuncBinding(Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();
			ylabelCreatedBy.Binding.AddFuncBinding<Nomenclature>(Entity, s => GetUserEmployeeName(s.CreatedBy), w => w.LabelProp).InitializeFromSource();

			enumVAT.ItemsEnum = typeof(VAT);
			enumVAT.Binding.AddBinding(Entity, e => e.VAT, w => w.SelectedItem).InitializeFromSource();

			enumType.ItemsEnum = typeof(NomenclatureCategory);
			enumType.Binding.AddBinding(Entity, e => e.Category, w => w.SelectedItem).InitializeFromSource();

			enumTareVolume.ItemsEnum = typeof(TareVolume);
			enumTareVolume.Binding.AddBinding(Entity, e => e.TareVolume, w => w.SelectedItemOrNull).InitializeFromSource();
			ycheckDisposableTare.Binding.AddBinding(Entity, e => e.IsDisposableTare, w => w.Active).InitializeFromSource();

			yСolorBtnBottleCapColor.Binding.AddBinding(Entity, e => e.BottleCapColor, w => w.Color, new ColorTextToGdkColorConverter()).InitializeFromSource();
			yСolorBtnBottleCapColor.ColorSet += YСolorBtnBottleCapColorOnColorSet;

			enumSaleCategory.Visible = Entity.Category == NomenclatureCategory.equipment;
			enumSaleCategory.ItemsEnum = typeof(SaleCategory);
			enumSaleCategory.Binding.AddBinding(Entity, e => e.SaleCategory, w => w.SelectedItemOrNull).InitializeFromSource();

			enumDepositType.Visible = Entity.Category == NomenclatureCategory.deposit;
			enumDepositType.ItemsEnum = typeof(TypeOfDepositCategory);
			enumDepositType.Binding.AddBinding(Entity, e => e.TypeOfDepositCategory, w => w.SelectedItemOrNull).InitializeFromSource();

			comboMobileCatalog.ItemsEnum = typeof(MobileCatalog);
			comboMobileCatalog.Binding.AddBinding(Entity, e => e.MobileCatalog, w => w.SelectedItem).InitializeFromSource();

			lblSaleCategory.Visible = Nomenclature.GetCategoriesWithSaleCategory().Contains(Entity.Category);
			lblSubType.Visible = Entity.Category == NomenclatureCategory.deposit;

			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			yentryOfficialName.Binding.AddBinding(Entity, e => e.OfficialName, w => w.Text).InitializeFromSource();
			var parallel = new ParallelEditing(yentryOfficialName);
			parallel.SubscribeOnChanges(entryName);
			parallel.GetParallelTextFunc = GenerateOfficialName;

			ycheckRentPriority.Binding.AddBinding(Entity, e => e.RentPriority, w => w.Active).InitializeFromSource();
			checkNotReserve.Binding.AddBinding(Entity, e => e.DoNotReserve, w => w.Active).InitializeFromSource();
			checkcanPrintPrice.Binding.AddBinding(Entity, e => e.CanPrintPrice, w => w.Active).InitializeFromSource();
			labelCanPrintPrice.Visible = checkcanPrintPrice.Visible = Entity.Category == NomenclatureCategory.water && !Entity.IsDisposableTare;
			checkHide.Binding.AddBinding(Entity, e => e.Hide, w => w.Active).InitializeFromSource();
			entryCode1c.Binding.AddBinding(Entity, e => e.Code1c, w => w.Text).InitializeFromSource();
			yspinSumOfDamage.Binding.AddBinding(Entity, e => e.SumOfDamage, w => w.ValueAsDecimal).InitializeFromSource();
			spinWeight.Binding.AddBinding(Entity, e => e.Weight, w => w.Value).InitializeFromSource();
			spinVolume.Binding.AddBinding(Entity, e => e.Volume, w => w.Value).InitializeFromSource();
			spinPercentForMaster.Binding.AddBinding(Entity, e => e.PercentForMaster, w => w.Value).InitializeFromSource();
			checkSerial.Binding.AddBinding(Entity, e => e.IsSerial, w => w.Active).InitializeFromSource();
			
			ycheckNewBottle.Binding.AddBinding(Entity, e => e.IsNewBottle, w => w.Active).InitializeFromSource();
			ycheckDefectiveBottle.Binding.AddBinding(Entity, e => e.IsDefectiveBottle, w => w.Active).InitializeFromSource();
			ycheckShabbyBottle.Binding.AddBinding(Entity, e => e.IsShabbyBottle, w => w.Active).InitializeFromSource();
			
			chkIsDiler.Binding.AddBinding(Entity, e => e.IsDiler, w => w.Active).InitializeFromSource();
			spinMinStockCount.Binding.AddBinding(Entity, e => e.MinStockCount, w => w.ValueAsDecimal).InitializeFromSource();

			ycomboFuelTypes.SetRenderTextFunc<FuelType>(x => x.Name);
			ycomboFuelTypes.ItemsList = UoW.GetAll<FuelType>();
			ycomboFuelTypes.Binding.AddBinding(Entity, e => e.FuelType, w => w.SelectedItem).InitializeFromSource();
			ycomboFuelTypes.Visible = Entity.Category == NomenclatureCategory.fuel;

			ylblOnlineStore.Text = Entity.OnlineStore?.Name;

			yentryFolder1c.SubjectType = typeof(Folder1c);
			yentryFolder1c.Binding.AddBinding(Entity, e => e.Folder1C, w => w.Subject).InitializeFromSource();
			
			yentryProductGroup.JournalButtons = Buttons.Add | Buttons.Edit;
			yentryProductGroup.RepresentationModel = new ProductGroupVM(UoW, new ProductGroupFilterViewModel());
			yentryProductGroup.Binding.AddBinding(Entity, e => e.ProductGroup, w => w.Subject).InitializeFromSource();
			
			referenceUnit.SubjectType = typeof(MeasurementUnits);
			referenceUnit.Binding.AddBinding(Entity, n => n.Unit, w => w.Subject).InitializeFromSource();
			yentryrefEqupmentType.SubjectType = typeof(EquipmentKind);
			yentryrefEqupmentType.Binding.AddBinding(Entity, e => e.Kind, w => w.Subject).InitializeFromSource();
			referenceColor.SubjectType = typeof(EquipmentColors);
			referenceColor.Binding.AddBinding(Entity, e => e.EquipmentColor, w => w.Subject).InitializeFromSource();
			referenceRouteColumn.SubjectType = typeof(Domain.Logistic.RouteColumn);
			referenceRouteColumn.Binding.AddBinding(Entity, n => n.RouteListColumn, w => w.Subject).InitializeFromSource();
			referenceManufacturer.SubjectType = typeof(Manufacturer);
			referenceManufacturer.Binding.AddBinding(Entity, e => e.Manufacturer, w => w.Subject).InitializeFromSource();
			checkNoDeliver.Binding.AddBinding(Entity, e => e.NoDelivery, w => w.Active).InitializeFromSource();

			yentryShortName.Binding.AddBinding(Entity, e => e.ShortName, w => w.Text, new NullToEmptyStringConverter()).InitializeFromSource();
			yentryShortName.MaxLength = 220;
			checkIsArchive.Binding.AddBinding(Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			checkIsArchive.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");

			entityviewmodelentryShipperCounterparty.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices)
			);
			entityviewmodelentryShipperCounterparty.Binding.AddBinding(Entity, s => s.ShipperCounterparty, w => w.Subject).InitializeFromSource();
			entityviewmodelentryShipperCounterparty.CanEditReference = true;
			yentryStorageCell.Binding.AddBinding(Entity, s => s.StorageCell, w => w.Text).InitializeFromSource();
			UpdateVisibilityForEshopParam();

			nomenclaturePurchasePricesView.ViewModel = new NomenclaturePurchasePricesViewModel(Entity, new Models.NomenclaturePurchasePriceModel(ServicesConfig.CommonServices.CurrentPermissionService));

			#region Вкладка характиристики

			ytextDescription.Binding.AddBinding(Entity, e => e.Description, w => w.Buffer.Text).InitializeFromSource();
			nomenclaturecharacteristicsview1.Uow = UoWGeneric;

			#endregion

			int currNomenclatureOfDependence = (Entity.DependsOnNomenclature == null ? 0 : Entity.DependsOnNomenclature.Id);

			dependsOnNomenclature.RepresentationModel = new NomenclatureDependsFromVM(Entity);
			dependsOnNomenclature.Binding.AddBinding(Entity, e => e.DependsOnNomenclature, w => w.Subject).InitializeFromSource();

			ConfigureInputs(Entity.Category, Entity.TareVolume);

			pricesView.UoWGeneric = UoWGeneric;
			pricesView.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");

			Imageslist.ImageButtonPressEvent += Imageslist_ImageButtonPressEvent;

			Entity.PropertyChanged += Entity_PropertyChanged;

			//make actions menu
			var menu = new Gtk.Menu();
			var menuItem = new Gtk.MenuItem("Заменить все ссылки на номенклатуру...");
			menuItem.Activated += MenuItem_ReplaceLinks_Activated; ;
			menu.Add(menuItem);
			menuActions.Menu = menu;
			menu.ShowAll();
			menuActions.Sensitive = !UoWGeneric.IsNew;

			ConfigureValidationContext();
		}

		private void ConfigureValidationContext()
		{
			_validationContext = _validationContextFactory.CreateNewValidationContext(Entity);
			
			_validationContext.ServiceContainer.AddService(typeof(INomenclatureRepository), _nomenclatureRepository);
		}

		private void YСolorBtnBottleCapColorOnColorSet(object sender, EventArgs e) {
			var color = (sender as yColorButton).Color;
			
			var colorRed = $"{color.Red:x4}".Remove(2);
			var colorBlue = $"{color.Blue:x4}".Remove(2);
			var colorGreen = $"{color.Green:x4}".Remove(2);

			Entity.BottleCapColor = $"#{colorRed}{colorGreen}{colorBlue}";
		}

		void UpdateVisibilityForEshopParam()
		{
			bool isEshopNomenclature = Entity?.ProductGroup?.ExportToOnlineStore ?? false;

			entityviewmodelentryShipperCounterparty.Visible = isEshopNomenclature;
			labelShipperCounterparty.Visible = isEshopNomenclature;
			yentryStorageCell.Visible = isEshopNomenclature;
			labelStorageCell.Visible = isEshopNomenclature;
			ylblOnlineStore.Visible = ylblOnlineStoreStr.Visible = Entity?.OnlineStore != null;
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.ProductGroup))
				nomenclaturecharacteristicsview1.RefreshWidgets();
		}

		void MenuItem_ReplaceLinks_Activated(object sender, EventArgs e)
		{
			var replaceDlg = new ReplaceEntityLinksDlg(Entity);
			OpenTab(replaceDlg);
		}

		private string GetUserEmployeeName(User s)
		{
			if(Entity.CreatedBy == null) {
				return "";
			}
			var employee = _employeeRepository.GetEmployeesForUser(UoW, s.Id).FirstOrDefault();
			if(employee == null) {
				return Entity.CreatedBy.Name;
			} else {
				return employee.ShortName;
			}
		}

		string GenerateOfficialName(object arg)
		{
			var widget = arg as Gtk.Entry;
			return widget.Text;
		}

		public override bool Save()
		{
			if(String.IsNullOrWhiteSpace(Entity.Code1c))
			{
				Entity.Code1c = _nomenclatureRepository.GetNextCode1c(UoW);
			}

			if(!ServicesConfig.ValidationService.Validate(Entity, _validationContext))
			{
				return false;
			}

			logger.Info("Сохраняем номенклатуру...");
			Entity.SetNomenclatureCreationInfo(_userRepository);
			pricesView.SaveChanges();
			UoWGeneric.Save();
			return true;
		}

		protected void OnEnumTypeChanged(object sender, EventArgs e)
		{
			ConfigureInputs(Entity.Category, Entity.TareVolume);

			if(Entity.Category != NomenclatureCategory.deposit) {
				Entity.TypeOfDepositCategory = null;
			}
		}

		protected void ConfigureInputs(NomenclatureCategory selected, TareVolume? tareVolume)
		{
			radioEquipment.Sensitive = selected == NomenclatureCategory.equipment;
			enumSaleCategory.Visible = lblSaleCategory.Visible = Nomenclature.GetCategoriesWithSaleCategory().Contains(selected);
			enumDepositType.Visible = lblSubType.Visible = selected == NomenclatureCategory.deposit;

			spinWeight.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.deposit);
			spinVolume.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.deposit);
			lblPercentForMaster.Visible = spinPercentForMaster.Visible = (selected == NomenclatureCategory.master);
			labelManufacturer.Sensitive = referenceManufacturer.Sensitive = (selected == NomenclatureCategory.equipment);
			labelColor.Sensitive = referenceColor.Sensitive = (selected == NomenclatureCategory.equipment);
			labelClass.Sensitive = yentryrefEqupmentType.Sensitive = (selected == NomenclatureCategory.equipment);
			labelModel.Sensitive = entryModel.Sensitive = (selected == NomenclatureCategory.equipment);
			labelSerial.Sensitive = checkSerial.Sensitive = (selected == NomenclatureCategory.equipment);
			labelRentPriority.Sensitive = ycheckRentPriority.Sensitive = (selected == NomenclatureCategory.equipment);
			labelReserve.Sensitive = checkNotReserve.Sensitive = !(selected == NomenclatureCategory.service || selected == NomenclatureCategory.deposit);
			labelCanPrintPrice.Visible = checkcanPrintPrice.Visible = Entity.Category == NomenclatureCategory.water && !Entity.IsDisposableTare;

			labelTypeTare.Visible = hboxTare.Visible = selected == NomenclatureCategory.water;
			hboxBottleCapColor.Visible = tareVolume == TareVolume.Vol19L;
			hboxTareChecks.Sensitive = selected == NomenclatureCategory.bottle;
			lblFuelType.Visible = ycomboFuelTypes.Visible = selected == NomenclatureCategory.fuel;
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

		protected void OnRadioEquipmentToggled(object sender, EventArgs e)
		{
			if(radioEquipment.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadioCharacteristicsToggled(object sender, EventArgs e)
		{
			if(radioCharacteristics.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnRadioImagesToggled(object sender, EventArgs e)
		{
			if(radioImages.Active) {
				notebook1.CurrentPage = 3;
				ImageTabOpen();
			}
		}

		protected void OnRadioPriceToggled(object sender, EventArgs e)
		{
			if(radioPrice.Active)
				notebook1.CurrentPage = 4;
		}

		protected void OnPurchasePriceToggled(object sender, EventArgs e)
		{
			if(radioPurchasePrice.Active)
			{
				notebook1.CurrentPage = 5;
			}
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

			if(Entity.Images == null)
				Entity.Images = new List<NomenclatureImage>();

			foreach(var imageSource in Entity.Images) {
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
				Entity.Images.Add(new NomenclatureImage(Entity, imageFile));
				ReloadImages();

				logger.Info("Ok");
			}
			Chooser.Destroy();
		}

		void Imageslist_ImageButtonPressEvent(object sender, ImageButtonPressEventArgs e)
		{
			if((int)e.eventArgs.Event.Button == 3) {
				popupMenuOn = (NomenclatureImage)e.Tag;
				Gtk.Menu jBox = new Gtk.Menu();
				Gtk.MenuItem MenuItem1 = new MenuItem("Удалить");
				MenuItem1.Activated += DeleteImage_Activated; ;
				jBox.Add(MenuItem1);
				jBox.ShowAll();
				jBox.Popup();
			}
		}

		void DeleteImage_Activated(object sender, EventArgs e)
		{
			Entity.Images.Remove(popupMenuOn);
			popupMenuOn = null;
			ReloadImages();
		}

		#endregion

		protected void OnDependsOnNomenclatureChanged(object sender, EventArgs e)
		{
			radioPrice.Sensitive = Entity.DependsOnNomenclature == null;
		}

		protected void OnEnumTypeChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Id == 0 && Nomenclature.GetCategoriesWithSaleCategory().Contains(Entity.Category))
				Entity.SaleCategory = SaleCategory.notForSale;
		}

		protected void OnYentryProductGroupChangedByUser(object sender, EventArgs e)
		{
			UpdateVisibilityForEshopParam();
		}

		protected void OnEnumTareVolumeChanged(object sender, EventArgs e)
		{
			hboxBottleCapColor.Visible = Entity.TareVolume == TareVolume.Vol19L;
		}
	}
}
