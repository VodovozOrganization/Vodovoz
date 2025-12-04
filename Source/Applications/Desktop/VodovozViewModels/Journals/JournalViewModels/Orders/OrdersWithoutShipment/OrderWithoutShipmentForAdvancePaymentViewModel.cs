using Autofac;
using EdoService.Library;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Organizations;
using EdoDocumentType = Vodovoz.Core.Domain.Documents.DocumentContainerType;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForAdvancePaymentViewModel : EntityTabViewModelBase<OrderWithoutShipmentForAdvancePayment>, ITdiTabAddedNotifier
	{
		private readonly IUserRepository _userRepository;
		private readonly IEmailRepository _emailRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IOrderSettings _orderSettings;
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private readonly IEmailSettings _emailSettings;
		private readonly IEdoService _edoService;
		private ILifetimeScope _lifetimeScope;
		private UserSettings _currentUserSettings;
		private IGenericRepository<EdoContainer> _edoContainerRepository;
		private IGenericRepository<OrderEdoTrueMarkDocumentsActions> _orderEdoTrueMarkDocumentsActionsRepository;
		private bool _canCreateBillsWithoutShipment;
		private bool _canChoosePremiumDiscount;
		private bool _canAddOnlineStoreNomenclaturesToOrder;
		private bool _userHavePermissionToResendEdoDocuments;

		private object _selectedItem;
		
		public Action<string> OpenCounterpartyJournal;
		private bool _canSetOrganization = true;

		public OrderWithoutShipmentForAdvancePaymentViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			IUserRepository userRepository,
			IDiscountReasonRepository discountReasonRepository,
			IOrderDiscountsController discountsController,
			CommonMessages commonMessages,
			IGenericRepository<EdoContainer> edoContainerRepository,
			IGenericRepository<OrderEdoTrueMarkDocumentsActions> orderEdoTrueMarkDocumentsActionsRepository,
			IRDLPreviewOpener rdlPreviewOpener,
			IEmailSettings emailSettings,
			IEmailRepository emailRepository,
			IEdoService edoService,
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			ViewModelEEVMBuilder<Organization> organizationViewModelEEVMBuilder)
			: base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_emailRepository = emailRepository;
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
			_edoContainerRepository = edoContainerRepository ?? throw new ArgumentNullException(nameof(edoContainerRepository));
			_orderEdoTrueMarkDocumentsActionsRepository = orderEdoTrueMarkDocumentsActionsRepository ?? throw new ArgumentNullException(nameof(orderEdoTrueMarkDocumentsActionsRepository));
			if(discountReasonRepository == null)
			{
				throw new ArgumentNullException(nameof(discountReasonRepository));
			}

			DiscountsController = discountsController ?? throw new ArgumentNullException(nameof(discountsController));
			CounterpartyAutocompleteSelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);

			OrganizationViewModel = organizationViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();
			
			OrganizationViewModel.PropertyChanged += OnOrganizationViewModelPropertyChanged;
			
			SetPermissions();
			
			var currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);

			if(uowBuilder.IsNewEntity)
			{
				if (_canCreateBillsWithoutShipment)
				{
					if(!AskQuestion("Вы действительно хотите создать счет без отгрузки на предоплату?"))
					{
						AbortOpening();
						return;
					}
					Entity.Author = currentEmployee;
				}
				else
				{
					AbortOpening("У Вас нет прав на выставление счетов без отгрузки.");
					return;
				}
			}

			TabName = "Счет без отгрузки на предоплату";
			EntityUoWBuilder = uowBuilder;

			SendDocViewModel =
				new SendDocumentByEmailViewModel(
					uowFactory,
					_emailRepository,
					_emailSettings,
					currentEmployee,
					commonServices,
					UoW);

			FillDiscountReasons(discountReasonRepository);

			UpdateEdoContainers();
			InitializeCommands();

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		private void OnOrganizationViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			UpdateEmails();
		}
		
		private void UpdateEmails()
		{
			var email = Entity.GetEmailAddressForBill();

			SendDocViewModel.Update(Entity, email is null ? string.Empty : email.Address);
		}

		public bool CanSendBillByEdo => Entity.Client?.NeedSendBillByEdo ?? false && !EdoContainers.Any();

		public bool CanSetOrganization
		{
			get => _canSetOrganization;
			set => SetField(ref _canSetOrganization, value);
		}

		public IEntityUoWBuilder EntityUoWBuilder { get; }
		
		public IEntityEntryViewModel OrganizationViewModel { get; }

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }

		public object SelectedItem
		{
			get => _selectedItem;
			set => SetField(ref _selectedItem, value);
		}
		
		public IList<DiscountReason> DiscountReasons { get; private set; }
		public IOrderDiscountsController DiscountsController { get; }
		public bool CanChangeDiscountValue { get; private set; }
		public GenericObservableList<EdoContainer> EdoContainers { get; } = new GenericObservableList<EdoContainer>();
		public bool CanResendEdoBill => _userHavePermissionToResendEdoDocuments && EdoContainers.Any();
		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }
		
		public DelegateCommand AddForSaleCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }
		public DelegateCommand DeleteItemCommand { get; private set; }
		public DelegateCommand OpenBillCommand { get; private set; }
		
		private UserSettings CurrentUserSettings =>
			_currentUserSettings ??
			(_currentUserSettings = _userRepository.GetUserSettings(UoW, CommonServices.UserService.CurrentUserId));
		
		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
			{
				OpenCounterpartyJournal?.Invoke(string.Empty);
			}
		}
		
		public void OnButtonSendDocumentAgainClicked(object sender, EventArgs e)
		{
			var edoValidateResult = _edoService.ValidateEdoContainers(EdoContainers);

			var errorMessages = edoValidateResult.Errors.Select(x => x.Message).ToArray();

			if(edoValidateResult.IsFailure)
			{
				if(edoValidateResult.Errors.Any(error => error.Code == Errors.Edo.EdoErrors.AlreadySuccefullSended)
					&& !CommonServices.InteractiveService.Question(
						"Вы уверены, что хотите отправить дубль?\n" +
						string.Join("\n", errorMessages),
						"Требуется подтверждение!"))
				{
					return;
				}
			}

			if(UoW.IsNew)
			{
				if(CommonServices.InteractiveService.Question("Перед отправкой необходимо сохранить счёт, продолжить?"))
				{
					UoW.Save();
				}
				else
				{
					return;
				}
			}

			_edoService.SetNeedToResendEdoDocumentForOrder(Entity, EdoDocumentType.BillWSForAdvancePayment);

			UpdateEdoContainers();

			OnPropertyChanged(nameof(CanSendBillByEdo));
			OnPropertyChanged(nameof(CanResendEdoBill));

			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Отправлено");
		}

		public void UpdateEdoContainers()
		{
			EdoContainers.Clear();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				foreach(var item in _edoContainerRepository.Get(uow, EdoContainerSpecification.CreateForOrderWithoutShipmentForAdvancePaymentId(Entity.Id)))
				{
					EdoContainers.Add(item);
				}

				var action = _orderEdoTrueMarkDocumentsActionsRepository.Get(uow,x => x.OrderWithoutShipmentForAdvancePayment.Id == Entity.Id && x.IsNeedToResendEdoBill == true)
					.FirstOrDefault();

				if(action != null)
				{
					var tempContainer = new EdoContainer { Type = EdoDocumentType.BillWSForAdvancePayment, EdoDocFlowStatus = EdoDocFlowStatus.PreparingToSend };
					EdoContainers.Add(tempContainer);
				}
			}
		}

		public void OnCounterpartyEntityViewModelEntryChanged(object sender, EventArgs e)
		{
			UpdateEmails();
		}

		public override bool Save(bool close)
		{
			OnPropertyChanged(nameof(CanSendBillByEdo));
			return base.Save(close);
		}

		private void InitializeCommands()
		{
			AddForSaleCommand = new DelegateCommand(
				() =>
				{
					if(!CanAddNomenclaturesToOrder())
					{
						return;
					}

					var defaultCategory = NomenclatureCategory.water;
					if(CurrentUserSettings.DefaultSaleCategory.HasValue)
					{
						defaultCategory = CurrentUserSettings.DefaultSaleCategory.Value;
					}

					Action<NomenclatureFilterViewModel> filterParams = f =>
					{
						f.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder();
						f.SelectCategory = defaultCategory;
						f.SelectSaleCategory = SaleCategory.forSale;
						f.RestrictArchive = false;
						f.CanChangeOnlyOnlineNomenclatures = false;
					};
					
					var journalViewModel = _lifetimeScope.Resolve<NomenclaturesJournalViewModel>(
						new TypedParameter(typeof(Action<NomenclatureFilterViewModel>), filterParams));
					
					journalViewModel.SelectionMode = JournalSelectionMode.Single;
					journalViewModel.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
					journalViewModel.TabName = "Номенклатура на продажу";
					journalViewModel.CalculateQuantityOnStock = true;
				
					journalViewModel.OnSelectResult += (s, ea) =>
					{
						var selectedNode = ea.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
						
						if(selectedNode == null)
						{
							return;
						}

						TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id));
					};
					
					TabParent.AddSlaveTab(this, journalViewModel);
				},
				() => true);

			CancelCommand = new DelegateCommand(
				() => Close(true, CloseSource.Cancel),
				() => true);

			DeleteItemCommand = new DelegateCommand(
				() =>
				{
					var item = SelectedItem as OrderWithoutShipmentForAdvancePaymentItem;
					Entity.RemoveItem(item);
				},
				() => SelectedItem != null);

			OpenBillCommand = new DelegateCommand(
				() =>
				{
					var whatToPrint = "документа \"" + Entity.Type.GetEnumTitle() + "\"";
					
					if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(OrderWithoutShipmentForAdvancePayment), whatToPrint))
					{
						if(Save(false))
						{
							_rdlPreviewOpener.OpenRldDocument(typeof(OrderWithoutShipmentForAdvancePayment), Entity);
						}
					}

					if(!UoWGeneric.HasChanges && Entity.Id > 0)
					{
						_rdlPreviewOpener.OpenRldDocument(typeof(OrderWithoutShipmentForAdvancePayment), Entity);
					}
				},
				() => true);
		}

		private void SetPermissions()
		{
			var permissionService = CommonServices.CurrentPermissionService;
			
			_canCreateBillsWithoutShipment = permissionService.ValidatePresetPermission("can_create_bills_without_shipment");
			CanChangeDiscountValue = permissionService.ValidatePresetPermission("can_set_direct_discount_value");
			_canChoosePremiumDiscount = permissionService.ValidatePresetPermission("can_choose_premium_discount");
			_canAddOnlineStoreNomenclaturesToOrder =
				permissionService.ValidatePresetPermission("can_add_online_store_nomenclatures_to_order");
			_userHavePermissionToResendEdoDocuments =
				CommonServices.PermissionService.ValidateUserPresetPermission(
					Vodovoz.Core.Domain.Permissions.EdoContainerPermissions.OrderWithoutShipmentForDebt.CanResendEdoBill, CurrentUser.Id);
		}

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Client))
			{
				OnPropertyChanged(nameof(CanSendBillByEdo));
				OnPropertyChanged(nameof(CanResendEdoBill));
			}
		}

		private void FillDiscountReasons(IDiscountReasonRepository discountReasonRepository)
		{
			DiscountReasons = _canChoosePremiumDiscount
				? discountReasonRepository.GetActiveDiscountReasons(UoW)
				: discountReasonRepository.GetActiveDiscountReasonsWithoutPremiums(UoW);
		}

		private bool CanAddNomenclaturesToOrder()
		{
			if(Entity.Client is null) {
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,"Для добавления товара на продажу должен быть выбран клиент.");
				return false;
			}

			return true;
		}

		private void TryAddNomenclature(Nomenclature nomenclature, int count = 0, decimal discount = 0, DiscountReason discountReason = null)
		{
			if(nomenclature.OnlineStore != null && !_canAddOnlineStoreNomenclaturesToOrder)
			{
				ShowWarningMessage("У вас недостаточно прав для добавления на продажу номенклатуры интернет магазина");
				return;
			}

			Entity.AddNomenclature(nomenclature, count, discount, false, discountReason);
		}
		
		private bool IsOnlineStoreOrderWithoutShipment(OrderWithoutShipmentForAdvancePayment order)
		{
			return order.OrderWithoutDeliveryForAdvancePaymentItems.Any(x =>
				x.Nomenclature.OnlineStore != null && x.Nomenclature.OnlineStore.Id != _orderSettings.OldInternalOnlineStoreId);
		}

		public override void Dispose()
		{
			OrganizationViewModel.PropertyChanged -= OnOrganizationViewModelPropertyChanged;
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
