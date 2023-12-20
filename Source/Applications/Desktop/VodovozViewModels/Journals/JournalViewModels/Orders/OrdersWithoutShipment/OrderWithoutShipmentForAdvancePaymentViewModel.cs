using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Controllers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using EdoDocumentType = Vodovoz.Domain.Orders.Documents.Type;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForAdvancePaymentViewModel : EntityTabViewModelBase<OrderWithoutShipmentForAdvancePayment>, ITdiTabAddedNotifier
	{
		private readonly IUserRepository _userRepository;
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private ILifetimeScope _lifetimeScope;
		private UserSettings _currentUserSettings;
		private IGenericRepository<EdoContainer> _edoContainerRepository;
		private bool _canCreateBillsWithoutShipment;
		private bool _canChoosePremiumDiscount;
		private bool _canAddOnlineStoreNomenclaturesToOrder;
		private bool _userHavePermissionToResendEdoDocuments;

		private object _selectedItem;
		
		public Action<string> OpenCounterpartyJournal;

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
			IRDLPreviewOpener rdlPreviewOpener)
			: base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_edoContainerRepository = edoContainerRepository ?? throw new ArgumentNullException(nameof(edoContainerRepository));
			if(discountReasonRepository == null)
			{
				throw new ArgumentNullException(nameof(discountReasonRepository));
			}

			DiscountsController = discountsController ?? throw new ArgumentNullException(nameof(discountsController));
			CounterpartyAutocompleteSelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);

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
					else
					{
						Entity.Author = currentEmployee;
					}
				}
				else
				{
					AbortOpening("У Вас нет прав на выставление счетов без отгрузки.");
					return;
				}
			}

			TabName = "Счет без отгрузки на предоплату";
			EntityUoWBuilder = uowBuilder;

			var loggerFactory = new LoggerFactory();
			var settingsController = new SettingsController(UnitOfWorkFactory, new Logger<SettingsController>(loggerFactory));
			SendDocViewModel =
				new SendDocumentByEmailViewModel(
					new EmailRepository(),
					new EmailParametersProvider(settingsController),
					currentEmployee,
					commonServices.InteractiveService,
					UoW);

			FillDiscountReasons(discountReasonRepository);

			UpdateEdoContainers();

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
					};
					
					var journalViewModel = _lifetimeScope.Resolve<NomenclaturesJournalViewModel>(
						new TypedParameter(typeof(Action<NomenclatureFilterViewModel>), filterParams));
					
					journalViewModel.SelectionMode = JournalSelectionMode.Single;
					journalViewModel.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
					journalViewModel.TabName = "Номенклатура на продажу";
					journalViewModel.CalculateQuantityOnStock = true;
				
					journalViewModel.OnEntitySelectedResult += (s, ea) =>
					{
						var selectedNode = ea.SelectedNodes.FirstOrDefault();
						
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
					string whatToPrint = "документа \"" + Entity.Type.GetEnumTitle() + "\"";

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

			Entity.PropertyChanged += OnEntityPropertyChanged;
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
					Vodovoz.Permissions.EdoContainer.OrderWithoutShipmentForDebt.CanResendEdoBill, CurrentUser.Id);
		}


		public bool CanSendBillByEdo => Entity.Client?.NeedSendBillByEdo ?? false && !EdoContainers.Any();

		public IEntityUoWBuilder EntityUoWBuilder { get; }

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }

		public object SelectedItem
		{
			get => _selectedItem;
			set => SetField(ref _selectedItem, value);
		}

		private UserSettings CurrentUserSettings =>
			_currentUserSettings ??
			(_currentUserSettings = _userRepository.GetUserSettings(UoW, CommonServices.UserService.CurrentUserId));

		public IList<DiscountReason> DiscountReasons { get; private set; }
		public IOrderDiscountsController DiscountsController { get; }
		public bool CanChangeDiscountValue { get; private set; }

		#region Commands

		public DelegateCommand AddForSaleCommand { get; }

		public DelegateCommand CancelCommand { get; }

		public DelegateCommand DeleteItemCommand { get; }

		public DelegateCommand OpenBillCommand { get; }

		#endregion Commands

		public GenericObservableList<EdoContainer> EdoContainers { get; } = new GenericObservableList<EdoContainer>();

		public bool CanResendEdoBill => _userHavePermissionToResendEdoDocuments && EdoContainers.Any();

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Client))
			{
				OnPropertyChanged(nameof(CanSendBillByEdo));
				OnPropertyChanged(nameof(CanResendEdoBill));
			}
		}
		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
			{
				OpenCounterpartyJournal?.Invoke(string.Empty);
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

		private void SendBillByEdo(IUnitOfWork uow)
		{
			var edoContainer = new EdoContainer
			{
				Type = EdoDocumentType.BillWSForAdvancePayment,
				Created = DateTime.Now,
				Container = new byte[64],
				OrderWithoutShipmentForAdvancePayment = Entity,
				Counterparty = Entity.Counterparty,
				MainDocumentId = string.Empty,
				EdoDocFlowStatus = EdoDocFlowStatus.PreparingToSend
			};

			uow.Save();
			uow.Save(edoContainer);
			uow.Commit();
		}

		public void OnButtonSendDocumentAgainClicked(object sender, EventArgs e)
		{
			if(EdoContainers.Any(x => x.EdoDocFlowStatus == EdoDocFlowStatus.Succeed))
			{
				if(!CommonServices.InteractiveService.Question("Для данного заказа имеется документ со статусом \"Документооборот завершен успешно\".\nВы уверены, что хотите отправить дубль?"))
				{
					return;
				}
			}
			else if(EdoContainers.Any(x => x.EdoDocFlowStatus == EdoDocFlowStatus.InProgress))
			{
				if(!CommonServices.InteractiveService.Question("Для данного заказа имеется документ со статусом \"В процессе\".\nВы уверены, что хотите отправить дубль?"))
				{
					return;
				}
			}

			SendBillByEdo(UoW);
			UpdateEdoContainers();

			OnPropertyChanged(nameof(CanSendBillByEdo));
			OnPropertyChanged(nameof(CanResendEdoBill));
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
			}
		}

		public void OnEntityViewModelEntryChanged(object sender, EventArgs e)
		{
			var email = Entity.GetEmailAddressForBill();

			if(email is null)
			{
				SendDocViewModel.Update(Entity, string.Empty);
			}
			else
			{
				SendDocViewModel.Update(Entity, email.Address);
			}
		}

		public override bool Save(bool close)
		{
			OnPropertyChanged(nameof(CanSendBillByEdo));
			return base.Save(close);
		}

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
