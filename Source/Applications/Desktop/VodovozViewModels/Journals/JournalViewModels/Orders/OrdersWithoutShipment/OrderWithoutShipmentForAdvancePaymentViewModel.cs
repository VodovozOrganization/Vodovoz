using Gamma.Utilities;
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
using System.Linq;
using Microsoft.Extensions.Logging;
using Vodovoz.Controllers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForAdvancePaymentViewModel : EntityTabViewModelBase<OrderWithoutShipmentForAdvancePayment>, ITdiTabAddedNotifier
	{
		private readonly IEmployeeService _employeeService;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
		private readonly ICounterpartyJournalFactory _counterpartySelectorFactory;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IUserRepository _userRepository;
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private UserSettings _currentUserSettings;

		private object selectedItem;
		public object SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}
		
		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		
		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;
		
		public Action<string> OpenCounterpartyJournal;
		public IEntityUoWBuilder EntityUoWBuilder { get; }

		public OrderWithoutShipmentForAdvancePaymentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			IDiscountReasonRepository discountReasonRepository,
			IParametersProvider parametersProvider,
			IOrderDiscountsController discountsController,
			CommonMessages commonMessages,
			IRDLPreviewOpener rdlPreviewOpener) : base(uowBuilder, uowFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			if(parametersProvider == null)
			{
				throw new ArgumentNullException(nameof(parametersProvider));
			}
			if(discountReasonRepository == null)
			{
				throw new ArgumentNullException(nameof(discountReasonRepository));
			}
			DiscountsController = discountsController ?? throw new ArgumentNullException(nameof(discountsController));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			
			bool canCreateBillsWithoutShipment = 
				CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_bills_without_shipment");
			CanChangeDiscountValue = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_direct_discount_value");
			
			var currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			
			if (uowBuilder.IsNewEntity)
			{
				if (canCreateBillsWithoutShipment)
				{
					if (!AskQuestion("Вы действительно хотите создать счет без отгрузки на предоплату?"))
					{
						AbortOpening();
					}
					else
					{
						Entity.Author = currentEmployee;
					}
				}
				else
				{
					AbortOpening("У Вас нет прав на выставление счетов без отгрузки.");
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
		}
		
		private UserSettings CurrentUserSettings =>
			_currentUserSettings ??
			(_currentUserSettings = _userRepository.GetUserSettings(UoW, CommonServices.UserService.CurrentUserId));

		public IList<DiscountReason> DiscountReasons { get; private set; }
		public IOrderDiscountsController DiscountsController { get; }
		public bool CanChangeDiscountValue { get; }

		#region Commands

		private DelegateCommand addForSaleCommand;
		public DelegateCommand AddForSaleCommand => addForSaleCommand ?? (addForSaleCommand = new DelegateCommand(
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

				var nomenclatureFilter = new NomenclatureFilterViewModel();
				nomenclatureFilter.SetAndRefilterAtOnce(
					x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
					x => x.SelectCategory = defaultCategory,
					x => x.SelectSaleCategory = SaleCategory.forSale,
					x => x.RestrictArchive = false
				);

				NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
					nomenclatureFilter,
					UnitOfWorkFactory,
					ServicesConfig.CommonServices,
					_employeeService,
					_nomenclatureSelectorFactory,
					CounterpartySelectorFactory,
					_nomenclatureRepository,
					_userRepository
				) {
					SelectionMode = JournalSelectionMode.Single,
				};
				journalViewModel.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
				journalViewModel.TabName = "Номенклатура на продажу";
				journalViewModel.CalculateQuantityOnStock = true;
				journalViewModel.OnEntitySelectedResult += (s, ea) => {
					var selectedNode = ea.SelectedNodes.FirstOrDefault();
					if(selectedNode == null)
						return;
					TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id));
				};
				TabParent.AddSlaveTab(this, journalViewModel);
			},
			() => true
		));

		private DelegateCommand cancelCommand;
		public DelegateCommand CancelCommand => cancelCommand ?? (cancelCommand = new DelegateCommand(
			() =>Close(true, CloseSource.Cancel),
			() => true
		));

		private DelegateCommand deleteItemCommand;
		public DelegateCommand DeleteItemCommand => deleteItemCommand ?? (deleteItemCommand = new DelegateCommand(
			() => {
				var item = SelectedItem as OrderWithoutShipmentForAdvancePaymentItem;
				Entity.RemoveItem(item);
			},
			() => SelectedItem != null
		));

		private DelegateCommand openBillCommand;
		public DelegateCommand OpenBillCommand => openBillCommand ?? (openBillCommand = new DelegateCommand(
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
			() => true
		));

		public ICounterpartyJournalFactory CounterpartySelectorFactory => _counterpartySelectorFactory;

		#endregion Commands

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
				OpenCounterpartyJournal?.Invoke(string.Empty);
		}

		private void FillDiscountReasons(IDiscountReasonRepository discountReasonRepository)
		{
			var canChoosePremiumDiscount = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_choose_premium_discount");
			DiscountReasons = canChoosePremiumDiscount
				? discountReasonRepository.GetActiveDiscountReasons(UoW)
				: discountReasonRepository.GetActiveDiscountReasonsWithoutPremiums(UoW);
		}
		
		bool CanAddNomenclaturesToOrder()
		{
			if(Entity.Client == null) {
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,"Для добавления товара на продажу должен быть выбран клиент.");
				return false;
			}

			return true;
		}

		void TryAddNomenclature(Nomenclature nomenclature, int count = 0, decimal discount = 0, DiscountReason discountReason = null)
		{
			if(nomenclature.OnlineStore != null && !ServicesConfig.CommonServices.CurrentPermissionService
				.ValidatePresetPermission("can_add_online_store_nomenclatures_to_order"))
			{
				ShowWarningMessage("У вас недостаточно прав для добавления на продажу номенклатуры интернет магазина");
				return;
			}

			Entity.AddNomenclature(nomenclature, count, discount, false, discountReason);
		}
		
		public void OnEntityViewModelEntryChanged(object sender, EventArgs e)
		{
			var email = Entity.GetEmailAddressForBill();

			if (email != null)
				SendDocViewModel.Update(Entity, email.Address);
			else
				SendDocViewModel.Update(Entity, string.Empty);
		}
	}
}
