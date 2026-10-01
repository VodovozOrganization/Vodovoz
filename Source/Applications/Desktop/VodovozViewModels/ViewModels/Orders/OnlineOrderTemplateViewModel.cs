using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Autofac;
using Gamma.Widgets;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using RestSharp.Validation;
using Vodovoz.Core.Application.Orders.Validators;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Presentation.ViewModels.PaymentTypes;
using Vodovoz.Services;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.ViewModels.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Validation;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderTemplateViewModel : DialogTabViewModelBase
	{
		private readonly IEntityUoWBuilder _entityUoWBuilder;
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGoodsPriceCalculator _goodsPriceCalculator;
		private readonly ISaleWithTaxHandler _saleHandler;
		private readonly IAddNomenclatureToSaleValidator _addNomenclatureToSaleValidator;
		private readonly ViewModelEEVMBuilder<DeliveryPoint> _deliveryPointViewModelBuilder;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel;
		private readonly SelectPaymentTypeViewModel _selectPaymentTypeViewModel;
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
			IEntityUoWBuilder entityUoWBuilder,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IGoodsPriceCalculator goodsPriceCalculator,
			ISaleWithTaxHandler saleHandler,
			IAddNomenclatureToSaleValidator addNomenclatureToSaleValidator,
			ViewModelEEVMBuilder<DeliveryPoint> deliveryPointViewModelBuilder,
			DeliveryPointJournalFilterViewModel deliveryPointJournalFilterViewModel,
			SelectPaymentTypeViewModel selectPaymentTypeViewModel
			) : base(unitOfWorkFactory, commonServices?.InteractiveService, navigationManager)
		{
			_entityUoWBuilder = entityUoWBuilder ?? throw new ArgumentNullException(nameof(entityUoWBuilder));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_goodsPriceCalculator = goodsPriceCalculator ?? throw new ArgumentNullException(nameof(goodsPriceCalculator));
			_saleHandler = saleHandler ?? throw new ArgumentNullException(nameof(saleHandler));
			_addNomenclatureToSaleValidator =
				addNomenclatureToSaleValidator ?? throw new ArgumentNullException(nameof(addNomenclatureToSaleValidator));
			_deliveryPointViewModelBuilder = deliveryPointViewModelBuilder ?? throw new ArgumentNullException(nameof(deliveryPointViewModelBuilder));
			_deliveryPointJournalFilterViewModel =
				deliveryPointJournalFilterViewModel ?? throw new ArgumentNullException(nameof(deliveryPointJournalFilterViewModel));
			_selectPaymentTypeViewModel = selectPaymentTypeViewModel ?? throw new ArgumentNullException(nameof(selectPaymentTypeViewModel));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			var uowGeneric = _entityUoWBuilder.CreateUoW<OnlineOrderTemplate>(_unitOfWorkFactory);
			UoW = uowGeneric;
			
			
			//TODO надо переделать инициализацию
			Entity = new OnlineOrderTemplateProxy();
			
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
			ConfigureSelectPaymentTypeViewModel();
		}

		private void ConfigureSelectPaymentTypeViewModel()
		{
			_selectPaymentTypeViewModel.AddExcludedPaymentTypes(
				Domain.Client.PaymentType.Barter,
				Domain.Client.PaymentType.Cashless,
				Domain.Client.PaymentType.ContractDocumentation,
				Domain.Client.PaymentType.SmsQR
				);
		}

		public ILifetimeScope LifetimeScope { get; }
		public IUnitOfWork UoW { get; }
		public OnlineOrderTemplateProxy Entity { get; }
		
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
		public IObservableList<OnlineOrderTemplateSaleItem> SaleItems { get; }
		
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
			var canAddNomenclatureResult = _addNomenclatureToSaleValidator
				.CanAddNomenclature(Entity);
			
			if(canAddNomenclatureResult.IsFailure)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, canAddNomenclatureResult.GetErrorsString());
				return;
			}
			
			var defaultCategory = NomenclatureCategory.water;
			//уточнить по поводу этой настройки
			/*if(CurrentUserSettings.Settings.DefaultSaleCategory.HasValue)
			{
				defaultCategory = CurrentUserSettings.Settings.DefaultSaleCategory.Value;
			}*/

			NavigationManager.OpenViewModel<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
				this,
				f =>
				{
					f.AvailableCategories = NomenclatureEntity.GetCategoriesForSaleToOrder();
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
					vm.SelectionMode = JournalSelectionMode.Multiple;
					vm.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(_commonServices);
					vm.TabName = "Номенклатура на продажу";
					vm.CalculateQuantityOnStock = true;
					vm.OnSelectResult += OnSelectSaleItemsNomenclatures;
				});
		}
		
		private void OnSelectSaleItemsNomenclatures(object sender, JournalSelectedEventArgs args)
		{
			(sender as JournalViewModelBase).OnSelectResult -= OnSelectSaleItemsNomenclatures;
			
			var selectedNodes = args.GetSelectedObjects<NomenclatureJournalNode>();

			if(!selectedNodes.Any())
			{
				return;
			}
			
			var sb = new StringBuilder();

			foreach(var node in selectedNodes)
			{
				var addingResult = _saleHandler.TryAddNomenclature(UoW, UoW.Session.Get<Nomenclature>(node.Id));

				if(addingResult.IsFailure)
				{
					sb.AppendLine(addingResult.GetErrorsString());
				}
			}

			if(sb.Length > 0)
			{
				ShowWarningMessage(sb.ToString(), "Нет возможности добавить все выбранные позиции");
			}
		}

		private void SelectPaymentType()
		{
			
		}
		
		private void YCmbPromoSets_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			if(!(e.SelectedItem is PromotionalSet proSet))
			{
				return;
			}

			var addPromoSetResult = _saleHandler.TryAddPromoSet(UoW, _commonServices.InteractiveService, proSet);

			if(addPromoSetResult.IsFailure)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, addPromoSetResult.GetErrorsString());
			}

			/*if(!yCmbPromoSets.IsSelectedNot)
			{
				yCmbPromoSets.SelectedItem = SpecialComboState.Not;
			}*/
		}
	}
}
