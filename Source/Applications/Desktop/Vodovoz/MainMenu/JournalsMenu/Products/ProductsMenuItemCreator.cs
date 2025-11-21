using System;
using Gtk;
using QS.BusinessCommon.Domain;
using QS.Navigation;
using QS.Project.Services;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Journals;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Goods;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Vodovoz.ViewModels.Journals.JournalViewModels.Flyers;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;

namespace Vodovoz.MainMenu.JournalsMenu.Products
{
	/// <summary>
	/// Создатель меню Справочники - ТМЦ
	/// </summary>
	public class ProductsMenuItemCreator : MenuItemCreator
	{
		private readonly ConcreteMenuItemCreator _concreteMenuItemCreator;
		private readonly InventoryAccountingMenuItemCreator _inventoryAccountingMenuItemCreator;
		private readonly ExternalSourcesMenuItemCreator _externalSourcesMenuItemCreator;
		private MenuItem _additionalLoadSettingsMenuItem;

		public ProductsMenuItemCreator(
			ConcreteMenuItemCreator concreteMenuItemCreator,
			InventoryAccountingMenuItemCreator inventoryAccountingMenuItemCreator,
			ExternalSourcesMenuItemCreator externalSourcesMenuItemCreator)
		{
			_concreteMenuItemCreator = concreteMenuItemCreator ?? throw new ArgumentNullException(nameof(concreteMenuItemCreator));
			_inventoryAccountingMenuItemCreator =
				inventoryAccountingMenuItemCreator ?? throw new ArgumentNullException(nameof(inventoryAccountingMenuItemCreator));
			_externalSourcesMenuItemCreator =
				externalSourcesMenuItemCreator ?? throw new ArgumentNullException(nameof(externalSourcesMenuItemCreator));
		}

		/// <inheritdoc/>
		public override MenuItem Create()
		{
			var productsMenuItem = _concreteMenuItemCreator.CreateMenuItem("ТМЦ");
			var productsMenu = new Menu();
			productsMenuItem.Submenu = productsMenu;

			AddFirstSection(productsMenu);
			productsMenu.Add(CreateSeparatorMenuItem());
			AddSecondSection(productsMenu);
			productsMenu.Add(CreateSeparatorMenuItem());
			AddThirdSection(productsMenu);
			productsMenu.Add(CreateSeparatorMenuItem());
			AddFourthSection(productsMenu);

			Configure();

			return productsMenuItem;
		}

		#region FirstSection

		private void AddFirstSection(Menu productsMenu)
		{
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Номенклатуры", OnNomenclaturesPressed));
			productsMenu.Add(_inventoryAccountingMenuItemCreator.Create());
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Единицы измерения", OnUnitsPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Группы товаров", OnProductGroupsPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Папки номенклатуры в 1с", OnFolders1cPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Промонаборы", OnPromotionalSetsPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Рекомендации", OnActionRecomendationsActivated));

			_additionalLoadSettingsMenuItem =
				_concreteMenuItemCreator.CreateMenuItem("Настройка запаса и радиуса", OnAdditionalLoadSettingsPressed);
			productsMenu.Add(_additionalLoadSettingsMenuItem);
			
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Групповое заполнение себестоимости", OnGroupPricingPressed));
		}
		
		/// <summary>
		/// Номенклатура
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNomenclaturesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<NomenclaturesJournalViewModel>(null);
		}

		/// <summary>
		/// Единицы измерения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnUnitsPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(MeasurementUnits));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Группы товаров
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnProductGroupsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenTdiTab<ProductGroupsJournalViewModel>(null);
		}

		/// <summary>
		/// Папки номенклатуры в 1с
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnFolders1cPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<Folder1c>(),
				() => new OrmReference(typeof(Folder1c))
			);
		}

		/// <summary>
		/// Промонаборы
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPromotionalSetsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<PromotionalSetsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Рекомендации
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnActionRecomendationsActivated(object sender, EventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RecomendationsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Настройка запаса и радиуса
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAdditionalLoadSettingsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<AdditionalLoadingSettingsViewModel>(null);
		}

		/// <summary>
		/// Групповое заполнение себестоимости
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnGroupPricingPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<NomenclatureGroupPricingViewModel>(null);
		}

		#endregion

		#region SecondSection

		private void AddSecondSection(Menu productsMenu)
		{
			var equipmentsMenuItem = _concreteMenuItemCreator.CreateMenuItem("Оборудование", OnEquipmentsPressed);
			equipmentsMenuItem.Sensitive = false;

			productsMenu.Add(equipmentsMenuItem);
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Виды оборудования", OnEquipmentKindsPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Производители оборудования", OnManufacturersPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Цвета оборудования", OnEquipmentColorsPressed));
		}

		/// <summary>
		/// Оборудование
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить или удалить, не активно")]
		private void OnEquipmentsPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(Equipment));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Виды оборудования
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEquipmentKindsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<EquipmentKindJournalViewModel>(null);
		}

		/// <summary>
		/// Производители оборудования
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnManufacturersPressed(object sender, ButtonPressEventArgs e)
		{
			OrmReference refWin = new OrmReference(typeof(Manufacturer));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Цвета оборудования
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnEquipmentColorsPressed(object sender, ButtonPressEventArgs e)
		{
			OrmReference refWin = new OrmReference(typeof(EquipmentColors));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}
		
		#endregion
		
		#region ThirdSection

		private void AddThirdSection(Menu productsMenu)
		{
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Спецификация продукции", OnProductSpecificationPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Сертификаты продукции", OnCertificatesPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Шаблоны для пересортицы", OnRegardingOfGoodsTemplatesPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Категории выбраковки", OnCullingCategoriesPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Фуры", OnTransportationWagonPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины пересортицы", OnRegradingOfGoodsReasonsPressed));
		}
		
		/// <summary>
		/// Спецификация продукции
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnProductSpecificationPressed(object sender, ButtonPressEventArgs e)
		{
			OrmReference refWin = new OrmReference(typeof(ProductSpecification));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Сертификаты продукции
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnCertificatesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<Certificate>(),
				() => new OrmReference(typeof(Certificate))
			);
		}

		/// <summary>
		/// Шаблоны для пересортицы
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnRegardingOfGoodsTemplatesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<RegradingOfGoodsTemplate>(),
				() => new OrmReference(typeof(RegradingOfGoodsTemplate))
			);
		}

		/// <summary>
		/// Категории выбраковки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnCullingCategoriesPressed(object sender, ButtonPressEventArgs e)
		{
			var refWin = new OrmReference(typeof(CullingCategory));
			Startup.MainWin.TdiMain.AddTab(refWin);
		}

		/// <summary>
		/// Фуры
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTransportationWagonPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<MovementWagonJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		/// <summary>
		/// Причины пересортицы
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRegradingOfGoodsReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<RegradingOfGoodsReasonsJournalViewModel>(null);
		}

		#endregion
		
		#region FourthSection

		private void AddFourthSection(Menu productsMenu)
		{
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Пакеты бесплатной аренды", OnFreeRentPackagesPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Условия платной аренды", OnPaidRentPackagesPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Основания для скидок", OnDiscountReasonsPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины несдачи тары", OnNonReturnReasonsPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Причины забора тары", OnReturnTareReasonsPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Категории забора тары", OnReturnTareReasonCategoriesPressed));
			productsMenu.Add(_concreteMenuItemCreator.CreateMenuItem("Рекламные листовки", OnFlyersPressed));
			productsMenu.Add(_externalSourcesMenuItemCreator.Create());
		}

		/// <summary>
		/// Пакеты бесплатной аренды
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFreeRentPackagesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FreeRentPackagesJournalViewModel>(null);
		}

		/// <summary>
		/// Условия платной аренды
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPaidRentPackagesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<PaidRentPackagesJournalViewModel>(null);
		}

		/// <summary>
		/// Основания для скидок
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDiscountReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<DiscountReasonJournalViewModel>(null);
		}

		/// <summary>
		/// Причины несдачи тары
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete("Старый диалог, заменить")]
		private void OnNonReturnReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.TdiMain.OpenTab(
				OrmReference.GenerateHashName<NonReturnReason>(),
				() => new OrmReference(typeof(NonReturnReason))
			);
		}

		/// <summary>
		/// Причины забора тары
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReturnTareReasonsPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ReturnTareReasonsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Категории забора тары
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReturnTareReasonCategoriesPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<ReturnTareReasonCategoriesJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}

		/// <summary>
		/// Рекламные листовки
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFlyersPressed(object sender, ButtonPressEventArgs e)
		{
			Startup.MainWin.NavigationManager.OpenViewModel<FlyersJournalViewModel>(null, OpenPageOptions.IgnoreHash);
		}
		
		#endregion

		private void Configure()
		{
			_additionalLoadSettingsMenuItem.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(AdditionalLoadingNomenclatureDistribution)).CanRead;
		}
	}
}
