using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Autofac;
using FluentNHibernate.Data;
using Gamma.Widgets;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Data.V5;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Orders.V5;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderTemplateViewModel : DialogViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGoodsPriceCalculatorV5 _goodsPriceCalculator;
		private readonly ViewModelEEVMBuilder<DeliveryPoint> _deliveryPointViewModelBuilder;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel;
		private readonly Employee _currentEmployee;
		private DateTime _createdAt;
		private bool _isArchive;
		private bool _isSelfDelivery;
		private bool _dontArriveBeforeInterval;
		private int? _callBeforeArrivalMinutes;
		private int? _lastOnlineOrderIdFromThisTemplate;
		private string _comment;
		private int? _bottlesReturn;
		private string _contactPhone;
		private decimal _trifle;
		private OnlineOrderTemplateStatus _status;
		private bool _canSelectPaymentType;
		private OnlineOrderPaymentType _paymentType;
		private OnlineOrderDeliveryFrequency _deliveryFrequency;
		private Domain.Client.Counterparty _counterparty;
		private DeliveryPoint _deliveryPoint;
		private DeliverySchedule _deliverySchedule;
		private Employee _author;

		public OnlineOrderTemplateViewModel(
			ILogger<OnlineOrderTemplateViewModel> logger,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IGoodsPriceCalculatorV5 goodsPriceCalculator,
			ViewModelEEVMBuilder<DeliveryPoint> deliveryPointViewModelBuilder,
			DeliveryPointJournalFilterViewModel deliveryPointJournalFilterViewModel
			) : base(navigationManager)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_goodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_deliveryPointViewModelBuilder = deliveryPointViewModelBuilder ?? throw new ArgumentNullException(nameof(deliveryPointViewModelBuilder));
			_deliveryPointJournalFilterViewModel =
				deliveryPointJournalFilterViewModel ?? throw new ArgumentNullException(nameof(deliveryPointJournalFilterViewModel));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			UoW = _unitOfWorkFactory.CreateWithoutRoot();
			
			_currentEmployee =
				(employeeService ?? throw new ArgumentNullException(nameof(employeeService)))
				.GetEmployeeForUser(UoW, _commonServices.UserService.CurrentUserId);

			if(_currentEmployee is null)
			{
				//Dispose();
				throw new AbortCreatingPageException("Ваш пользователь не привязан к сотруднику. Дальнейшая работа не возможна", "Ошибка");
			}

			//Title = Entity.ToString();

			SetPermissions();
			CreateCommands();
			//CreatePropertyChangeRelations();
			ConfigureEntryViewModels();
		}

		public ILifetimeScope LifetimeScope { get; }
		public IUnitOfWork UoW { get; }
		
		public bool CanEdit => true;
		public bool HasPermissionsForAlternativePrice { get; private set; }
		
		public ICommand AddForSaleCommand { get; private set; }
		public ICommand SelectPaymentTypeCommand { get; private set; }
		
		public IEntityEntryViewModel DeliveryPointViewModel { get; private set; }

		public DateTime CreatedAt
		{
			get => _createdAt;
			set => SetField(ref _createdAt, value);
		}
		
		public bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		
		public bool IsSelfDelivery
		{
			get => _isSelfDelivery;
			set => SetField(ref _isSelfDelivery, value);
		}
		
		public bool DontArriveBeforeInterval
		{
			get => _dontArriveBeforeInterval;
			set => SetField(ref _dontArriveBeforeInterval, value);
		}
		
		public int? CallBeforeArrivalMinutes
		{
			get => _callBeforeArrivalMinutes;
			set => SetField(ref _callBeforeArrivalMinutes, value);
		}
		
		public string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}
		
		public virtual int? BottlesReturn
		{
			get => _bottlesReturn;
			set => SetField(ref _bottlesReturn, value);
		}
		
		public string ContactPhone
		{
			get => _contactPhone;
			set => SetField(ref _contactPhone, value);
		}
		
		public decimal Trifle
		{
			get => _trifle;
			set => SetField(ref _trifle, value);
		}

		public OnlineOrderTemplateStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		public bool CanSelectPaymentType
		{
			get => _canSelectPaymentType;
			set => SetField(ref _canSelectPaymentType, value);
		}
		
		public OnlineOrderPaymentType PaymentType
		{
			get => _paymentType;
			set => SetField(ref _paymentType, value);
		}
		
		public IList<OnlineOrderPaymentType> AvailablePaymentTypes { get; private set; }
		
		public OnlineOrderDeliveryFrequency DeliveryFrequency
		{
			get => _deliveryFrequency;
			set => SetField(ref _deliveryFrequency, value);
		}

		public Domain.Client.Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		public Domain.Client.DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}
		
		public DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule, value);
		}
		
		public Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}
		
		public int? LastOnlineOrderIdFromThisTemplate
		{
			get => _lastOnlineOrderIdFromThisTemplate;
			set => SetField(ref _lastOnlineOrderIdFromThisTemplate, value);
		}

		public IObservableList<OnlineOrderTemplateWeekday> Weekdays { get; }
		public IObservableList<OnlineOrderTemplateProduct> Products { get; }
		
		private void Initialize()
		{
			AvailablePaymentTypes = Enum.GetValues(typeof(OnlineOrderPaymentType))
				.Cast<OnlineOrderPaymentType>()
				.ToList();
		}
		
		private void SetPermissions()
		{
			HasPermissionsForAlternativePrice = false;
		}
		
		private void CreateCommands()
		{
			AddForSaleCommand = new DelegateCommand(AddForSale);
			SelectPaymentTypeCommand = new DelegateCommand(SelectPaymentType);
		}
		
		private void ConfigureEntryViewModels()
		{
			if(Counterparty != null)
			{
				_deliveryPointJournalFilterViewModel.Counterparty = Counterparty;
			}

			var deliveryPointViewModel =  _deliveryPointViewModelBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.DeliveryPoint)
				.UseViewModelJournalAndAutocompleter<DeliveryPointByClientJournalViewModel, DeliveryPointJournalFilterViewModel>(
					_deliveryPointJournalFilterViewModel)
				.UseViewModelDialog<DeliveryPointViewModel>()
				.Finish();

			deliveryPointViewModel.CanViewEntity = false;
			DeliveryPointViewModel = deliveryPointViewModel;
		}

		private void AddForSale()
		{
			if(!CanAddProductsToTemplate())
			{
				return;
			}
			
			var defaultCategory = NomenclatureCategory.water;
			//уточнить по поводу этой настройки
			/*if(CurrentUserSettings.Settings.DefaultSaleCategory.HasValue)
			{
				defaultCategory = CurrentUserSettings.Settings.DefaultSaleCategory.Value;
			}*/

			var journalViewModel =
				NavigationManager.OpenViewModel<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
					this,
					f =>
					{
						f.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder();
						f.SelectCategory = defaultCategory;
						f.SelectSaleCategory = SaleCategory.forSale;
						f.RestrictArchive = false;
						f.CanChangeShowArchive = false;
						f.CanChangeOnlyOnlineNomenclatures = false;
						f.OnlyOnlineNomenclatures = true;
					},
					OpenPageOptions.AsSlaveIgnoreHash,
					vm =>
					{
						vm.SelectionMode = JournalSelectionMode.Single;
						vm.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(_commonServices);
						vm.TabName = "Номенклатура на продажу";
						vm.CalculateQuantityOnStock = true;
					})
				.ViewModel;

			journalViewModel.SelectionMode = JournalSelectionMode.Multiple;
			journalViewModel.OnSelectResult += OnSaleProductsSelectResult;
		}
		
		private bool CanAddProductsToTemplate()
		{
			if(Counterparty == null)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Для добавления товара на продажу должен быть выбран клиент");
				return false;
			}

			if(DeliveryPoint == null && IsSelfDelivery)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Для добавления товара на продажу должна быть выбрана точка доставки");
				return false;
			}

			return true;
		}
		
		private void OnSaleProductsSelectResult(object sender, JournalSelectedEventArgs args)
		{
			(sender as JournalViewModelBase).OnSelectResult -= OnSaleProductsSelectResult;
			
			var selectedNodes = args.SelectedObjects.Cast<NomenclatureJournalNode>().ToList();

			if(!selectedNodes.Any())
			{
				return;
			}

			foreach(var node in selectedNodes)
			{
				TryAddProduct(UoW.Session.Get<Nomenclature>(node.Id));
			}
		}
		
		private void TryAddProduct(
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			IEnumerable<DiscountReason> discountReasons = null
			)
		{
			/*if(PaymentType == PaymentType.Cashless)
			{
				if(nomenclature.Category == NomenclatureCategory.deposit
					&& !Order.HasDepositItems()
					&& Order.HasNonPaidDeliveryItems())
				{
					MessageDialogHelper.RunWarningDialog("Нельзя добавить залоговую позицию, если в заказе уже есть незалоговые позиции.");
					return;
				}

				if(nomenclature.Category != NomenclatureCategory.deposit 
					&& Order.HasDepositItems()
					&& Order.HasNonPaidDeliveryItems())
				{
					MessageDialogHelper.RunWarningDialog("Нельзя добавить незалоговую позицию, если в заказе уже есть залоговые позиции.");
					return;
				}
			}*/

			/*
			if(Entity.OrderItems.Any(x => !Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
				&& nomenclature.Category == NomenclatureCategory.master)
			{
				MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
				return;
			}

			if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
				&& !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category))
			{
				MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
				return;
			}
			*/
			
			/*if(nomenclature.OnlineStore != null && !_canAddOnlineStoreNomenclaturesToOrder)
			{
				MessageDialogHelper.RunWarningDialog("У вас недостаточно прав для добавления на продажу номенклатуры интернет магазина");
				return;
			}*/

			AddProduct(UoW, nomenclature, count, discount);
		}
		
		public virtual void AddProduct(
			IUnitOfWork uow,
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			bool discountInMoney = false,
			bool needGetFixedPrice = true,
			IEnumerable<DiscountReason> discountReasons = null,
			PromotionalSet proSet = null)
		{
			switch(nomenclature.Category) {
				case NomenclatureCategory.water:
					AddWaterForSale(
						uow,
						nomenclature,
						count,
						discount,
						discountInMoney,
						needGetFixedPrice,
						discountReasons?.FirstOrDefault(),
						proSet);
					break;
				case NomenclatureCategory.master:
					return;
					break;
				default:
					var canApplyAlternativePrice = HasPermissionsForAlternativePrice && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

					var product = OnlineOrderTemplateProduct.Create(
						count,
						nomenclature.GetPrice(1, canApplyAlternativePrice),
						nomenclature,
						proSet,
						0,//templateId,
						new ObservableList<OnlineOrderTemplateProductDiscount>()
						);

					var acceptableCategories = Nomenclature.GetCategoriesForSale();
					
					if(product?.Nomenclature is null || !acceptableCategories.Contains(product.Nomenclature.Category))
					{
						return;
					}
					
					AddOrderItem(uow, product);

					break;
			}
		}
		
		public virtual void AddWaterForSale(
			IUnitOfWork uow,
			Nomenclature nomenclature,
			decimal count,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			bool needGetFixedPrice = true,
			DiscountReason reason = null,
			PromotionalSet proSet = null)
		{
			if(nomenclature.Category != NomenclatureCategory.water && !nomenclature.IsDisposableTare)
			{
				return;
			}

			//Если номенклатура промонабора добавляется по фиксе (без скидки), то у нового OrderItem убирается поле discountReason
			if(proSet != null && discount == 0)
			{
				var fixPricedNomenclaturesId = new[] { 1 };//GetNomenclaturesWithFixPrices.Select(n => n.Id);
				if(fixPricedNomenclaturesId.Contains(nomenclature.Id))
				{
					reason = null;
				}
			}

			if(discount > 0 && reason == null && proSet == null)
			{
				throw new ArgumentException("Требуется указать причину скидки (reason), если она (discount) больше 0!");
			}

			var price = _goodsPriceCalculator.CalculatePrice(
				Products,
				Counterparty,
				DeliveryPoint,
				nomenclature,
				proSet != null,
				HasPermissionsForAlternativePrice,
				count,
				needGetFixedPrice);
			
			AddOrderItem(
				uow,
				OnlineOrderTemplateProduct.Create(
					count,
					price,
					nomenclature,
					proSet,
					0,//templateId,
					new ObservableList<OnlineOrderTemplateProductDiscount>()
					)
				);
		}
		
		public virtual void AddOrderItem(
			IUnitOfWork uow,
			OnlineOrderTemplateProduct product,
			bool forceUseAlternativePrice = false)
		{
			if(Products.Contains(product))
			{
				return;
			}
			
			Products.Add(product);
			Recalculate();
		}
		
		private void Recalculate()
		{
			RecalculateWaterPrices();
		}
		
		public virtual void RecalculateWaterPrices()
		{
			/*for(var i = 0; i < Products.Count; i++)
			{
				if(Products[i].Nomenclature.Category == NomenclatureCategory.water)
				{
					Products[i].RecalculatePrice();
				}
			}*/
		}
		
		public virtual void RecalculatePrice()
		{
			/*if(IsUserPrice || PromoSet != null || Order.OrderStatus == OrderStatus.Closed || CopiedFromUndelivery != null)
			{
				return;
			}

			//TODO надо переделать подбор фиксы с учетом создания заказа из онлайна и установки промокода на позицию с фиксой
			var fixedPrice = Order.GetFixedPriceOrNull(Nomenclature, TotalCountInOrder);

			if(fixedPrice != null && CopiedFromUndelivery == null)
			{
				IsFixedPrice = true;
				if(Price != fixedPrice.Price)
				{
					SetPrice(fixedPrice.Price);
				}
				return;
			}

			IsFixedPrice = false;

			SetPrice(GetPriceByTotalCount());*/
		}
		
		public virtual void SetPrice(decimal price)
		{
			//Если цена не отличается от той которая должна быть по прайсам в 
			//номенклатуре, то цена не изменена пользователем и сможет расчитываться автоматически
			/*IsUserPrice = (price != GetPriceByTotalCount() && price != 0 && !IsFixedPrice) || CopiedFromUndelivery != null;

			price = decimal.Round(price, 2);

			if(Price != price)
			{
				Price = price;

				RecalculateDiscount();
				RecalculateVAT();
			}*/
		}

		private void SelectPaymentType()
		{
			
		}
		
		private void YCmbPromoSets_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			/*if(!(e.SelectedItem is PromotionalSet proSet))
			{
				return;
			}

			if(CanAddProductsToTemplate() && CanAddPromotionalSet(proSet, _freeLoaderChecker, _promotionalSetRepository))
			{
				ActivatePromotionalSet(proSet);
			}

			if(!yCmbPromoSets.IsSelectedNot)
			{
				yCmbPromoSets.SelectedItem = SpecialComboState.Not;
			}*/
		}
		
		private void ActivatePromotionalSet(PromotionalSet proSet)
		{
			//Добавление спец. действий промонабора
			/*
			foreach(var action in proSet.PromotionalSetActions)
			{
				action.Activate(Entity);
			}
			*/
			//Добавление номенклатур из промонабора
			TryAddNomenclatureFromPromoSet(proSet);

			//Entity.ObservablePromotionalSets.Add(proSet);
		}
		
		private void TryAddNomenclatureFromPromoSet(PromotionalSet proSet)
		{
			/*if(proSet == null || proSet.IsArchive || !proSet.PromotionalSetItems.Any())
			{
				return;
			}

			foreach(var proSetItem in proSet.PromotionalSetItems)
			{
				var nomenclature = proSetItem.Nomenclature;
				if(Entity.OrderItems.Any(x =>
						!Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
					&& nomenclature.Category == NomenclatureCategory.master)
				{
					MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
					return;
				}

				if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
					&& !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category))
				{
					MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
					return;
				}

				AddProduct(
					UoW,
					proSetItem.Nomenclature,
					proSetItem.Count,
					proSetItem.IsDiscountInMoney ? proSetItem.DiscountMoney : proSetItem.Discount,
					proSetItem.IsDiscountInMoney,
					true,
					null,
					proSetItem.PromoSet
				);
			}*/
			
			//TODO уточнить по поводу расчета стоимости доставки
		}
		
		/// <summary>
		/// Проверка на возможность добавления промонабора в заказ
		/// </summary>
		/// <returns><c>true</c>, если можно добавить промонабор,
		/// <c>false</c> если нельзя.</returns>
		/// <param name="proSet">Промонабор (промонабор)</param>
		public virtual bool CanAddPromotionalSet(
			PromotionalSet proSet,
			IFreeLoaderChecker freeLoaderChecker,
			IPromotionalSetRepository promotionalSetRepository)
		{
			/*if(PromotionalSets.Any(x => x.PromotionalSetForNewClients && proSet.PromotionalSetForNewClients))
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"В заказ нельзя добавить два промо-набора для новых клиентов");
				return false;
			}*/

			if(IsSelfDelivery)
			{
				return true;
			}

			if(proSet.PromotionalSetForNewClients
				&& freeLoaderChecker.CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(UoW, IsSelfDelivery, Counterparty, DeliveryPoint))
			{
				var message = "По этому адресу уже была ранее отгрузка промонабора на другое физ.лицо.";
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
				
				return false;
			}

			var proSetDict = new Dictionary<int, int[]>();//promotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(UoW);

			if(!proSet.PromotionalSetForNewClients | !proSetDict.Any())
			{
				return true;
			}

			var address = string.Join(", ", DeliveryPoint.City, DeliveryPoint.Street, DeliveryPoint.Building, DeliveryPoint.Room);
			var sb = new StringBuilder(
				$"Для адреса \"{address}\", найдены схожие точки доставки, на которые уже создавались заказы с промо-наборами:\n");
			
			foreach(var d in proSetDict)
			{
				var proSetTitle = UoW.GetById<PromotionalSet>(d.Key).ShortTitle;
				var orders = string.Join(
					" ,",
					UoW.GetById<Order>(d.Value).Select(o => o.Title)
				);
				sb.AppendLine($"– {proSetTitle}: {orders}");
			}
			
			sb.AppendLine($"Вы уверены, что хотите добавить \"{proSet.Title}\"");
			
			return _commonServices.InteractiveService.Question(sb.ToString());
		}
	}
}
