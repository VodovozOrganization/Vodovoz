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
using Vodovoz.Settings.Nomenclature;
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
		private readonly ICounterpartyJournalFactory _counterpartySelectorFactory;
		private readonly IUserRepository _userRepository;
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private UserSettings _currentUserSettings;
		private IGenericRepository<EdoContainer> _edoContainerRepository;

		private object _selectedItem;
		
		public Action<string> OpenCounterpartyJournal;
		private bool _isSendBillByEdo;
		private bool _canSendBillByEdo;

		public OrderWithoutShipmentForAdvancePaymentViewModel(
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
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_edoContainerRepository = edoContainerRepository ?? throw new ArgumentNullException(nameof(edoContainerRepository));
			if(discountReasonRepository == null)
			{
				throw new ArgumentNullException(nameof(discountReasonRepository));
			}

			DiscountsController = discountsController ?? throw new ArgumentNullException(nameof(discountsController));
			_counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));

			bool canCreateBillsWithoutShipment =
				CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_bills_without_shipment");

			CanResendEdoBill = CommonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Permissions.EdoContainer.OrderWithoutShipmentForDebt.CanResendEdoBill, CurrentUser.Id);

			CanChangeDiscountValue = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_direct_discount_value");

			var currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);

			if(uowBuilder.IsNewEntity)
			{
				if(canCreateBillsWithoutShipment)
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

					var journalViewModel = NavigationManager.OpenViewModel<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
						this,
						f =>
						{
							f.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder();
							f.SelectCategory = defaultCategory;
							f.SelectSaleCategory = SaleCategory.forSale;
							f.RestrictArchive = false;
						},
						OpenPageOptions.AsSlave,
						vm =>
						{
							vm.SelectionMode = JournalSelectionMode.Single;
							vm.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
							vm.TabName = "Номенклатура на продажу";
							vm.CalculateQuantityOnStock = true;
						})
					.ViewModel;
				
					journalViewModel.OnEntitySelectedResult += (s, ea) =>
					{
						var selectedNode = ea.SelectedNodes.FirstOrDefault();
						
						if(selectedNode == null)
						{
							return;
						}

						TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id));
					};
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

			if(!UoWGeneric.IsNew)
			{
				CanSendBillByEdo = Entity.Client.NeedSendBillByEdo;
			}
		}

		public bool IsSendBillByEdo
		{
			get => _isSendBillByEdo;
			set => SetField(ref _isSendBillByEdo, value);
		}

		public bool CanSendBillByEdo
		{
			get => _canSendBillByEdo;
			set
			{
				if(SetField(ref _canSendBillByEdo, value) && !value)
				{
					IsSendBillByEdo = false;
				}
			}
		}

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

		public bool CanChangeDiscountValue { get; }

		#region Commands

		public DelegateCommand AddForSaleCommand { get; }

		public DelegateCommand CancelCommand { get; }

		public DelegateCommand DeleteItemCommand { get; }

		public DelegateCommand OpenBillCommand { get; }

		public ICounterpartyJournalFactory CounterpartySelectorFactory => _counterpartySelectorFactory;

		#endregion Commands

		public GenericObservableList<EdoContainer> EdoContainers { get; } =
			new GenericObservableList<EdoContainer>();

		public bool CanResendEdoBill { get; }

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Client))
			{
				CanSendBillByEdo = Entity.Client?.NeedSendBillByEdo ?? false;
			}
		}

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
			{
				OpenCounterpartyJournal?.Invoke(string.Empty);
			}
		}

		private void FillDiscountReasons(IDiscountReasonRepository discountReasonRepository)
		{
			var canChoosePremiumDiscount = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_choose_premium_discount");
			DiscountReasons = canChoosePremiumDiscount
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
			if(nomenclature.OnlineStore != null
				&& !ServicesConfig.CommonServices.CurrentPermissionService
					.ValidatePresetPermission("can_add_online_store_nomenclatures_to_order"))
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
			if(!Entity.IsBillWithoutShipmentSent && !EdoContainers.Any() && Entity.Id == 0 && IsSendBillByEdo)
			{
				SendBillByEdo(UoW);
			}

			return base.Save(close);
		}
	}
}
