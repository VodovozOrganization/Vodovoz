using Vodovoz.Domain.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using System;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Navigation;
using NHibernate;
using NHibernate.Transform;
using BaseOrg = Vodovoz.Domain.Organizations.Organization;
using NHibernate.Dialect.Function;
using VodOrder = Vodovoz.Domain.Orders.Order;
using NHibernate.Criterion;
using QS.Project.Domain;
using QS.Project.Journal;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Services;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.JournalViewModels
{
	public class PaymentsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<PaymentJournalNode, PaymentsJournalFilterViewModel>
	{
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationParametersProvider _organizationParametersProvider;
		private readonly IProfitCategoryProvider _profitCategoryProvider;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IDialogsFactory _dialogsFactory;
		private readonly bool _canCreateNewManualPayment;
		
		public PaymentsJournalViewModel(
			PaymentsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IOrderRepository orderRepository,
			IOrganizationParametersProvider organizationParametersProvider,
			IProfitCategoryProvider profitCategoryProvider,
			IPaymentsRepository paymentsRepository,
			IDialogsFactory dialogsFactory) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationParametersProvider = organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
			_profitCategoryProvider = profitCategoryProvider ?? throw new ArgumentNullException(nameof(profitCategoryProvider));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_dialogsFactory = dialogsFactory ?? throw new ArgumentNullException(nameof(dialogsFactory));
			_canCreateNewManualPayment =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_create_new_manual_payment_from_bank_client");
			
			TabName = "Журнал платежей из банк-клиента";

			RegisterPayments();
			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(Payment),
				typeof(PaymentItem),
				typeof(VodOrder)
			);
		}

		private IQueryOver<Payment> GetPaymentQuery(IUnitOfWork uow)
		{
			PaymentJournalNode resultAlias = null;
			Payment paymentAlias = null;
			BaseOrg organizationAlias = null;
			PaymentItem paymentItemAlias = null;
			ProfitCategory profitCategoryAlias = null;

			var paymentQuery = uow.Session.QueryOver(() => paymentAlias)
				.Left.JoinAlias(() => paymentAlias.Organization, () => organizationAlias)
				.Left.JoinAlias(() => paymentAlias.ProfitCategory, () => profitCategoryAlias)
				.Left.JoinAlias(() => paymentAlias.PaymentItems, () => paymentItemAlias);

			#region filter

			if(FilterViewModel != null) 
			{
				if(FilterViewModel.StartDate.HasValue)
				{
					paymentQuery.Where(p => p.Date >= FilterViewModel.StartDate);
				}

				if(FilterViewModel.EndDate.HasValue)
				{
					paymentQuery.Where(p => p.Date <= FilterViewModel.EndDate.Value.AddDays(1).AddMilliseconds(-1));
				}

				if(FilterViewModel.HideCompleted)
				{
					paymentQuery.Where(p => p.Status != PaymentState.completed);
				}

				if(FilterViewModel.PaymentState.HasValue)
				{
					paymentQuery.Where(p => p.Status == FilterViewModel.PaymentState);
				}

				if(FilterViewModel.IsManuallyCreated)
				{
					paymentQuery.Where(p => p.IsManuallyCreated);
				}
			}

			#endregion filter

			var numOrders = Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
							NHibernateUtil.String,
							Projections.Property(() => paymentItemAlias.Order.Id),
							Projections.Constant("\n"));

			paymentQuery.Where(GetSearchCriterion(
				() => paymentAlias.PaymentNum,
				() => paymentAlias.Total,
				() => paymentAlias.CounterpartyName,
				() => paymentItemAlias.Order.Id
			));

			var resultQuery = paymentQuery
				.SelectList(list => list
				   .SelectGroup(() => paymentAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => paymentAlias.PaymentNum).WithAlias(() => resultAlias.PaymentNum)
				   .Select(() => paymentAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => paymentAlias.Total).WithAlias(() => resultAlias.Total)
				   .Select(numOrders).WithAlias(() => resultAlias.Orders)
				   .Select(() => paymentAlias.CounterpartyName).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => organizationAlias.FullName).WithAlias(() => resultAlias.Organization)
				   .Select(() => paymentAlias.PaymentPurpose).WithAlias(() => resultAlias.PaymentPurpose)
				   .Select(() => profitCategoryAlias.Name).WithAlias(() => resultAlias.ProfitCategory)
				   .Select(() => paymentAlias.Status).WithAlias(() => resultAlias.Status)
				   .Select(() => paymentAlias.IsManuallyCreated).WithAlias(() => resultAlias.IsManualCreated)
				)
				.OrderBy(() => paymentAlias.Status).Asc
				.OrderBy(() => paymentAlias.CounterpartyName).Asc
				.OrderBy(() => paymentAlias.Total).Asc
				.TransformUsing(Transformers.AliasToBean<PaymentJournalNode>());
			return resultQuery;
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateAddNewPaymentAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			
			if(_commonServices.UserService.GetCurrentUser(UoW).IsAdmin)
			{
				CreateDefaultDeleteAction();
			}

			NodeActionsList.Add(new JournalAction(
				"Завершить распределение", 
				x => true,
				x => true,
				selectedItems => CompleteAllocation()
				)
			);
		}

		private void CreateAddNewPaymentAction()
		{
			NodeActionsList.Add(new JournalAction(
					"Создать новый платеж", 
					x => true,
					x => _canCreateNewManualPayment,
					selectedItems =>
					{
						_navigationManager.OpenViewModel<CreateManualPaymentFromBankClientViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForCreate());
					}
				)
			);
		}

		private void RegisterPayments()
		{
			var complaintConfig = RegisterEntity<Payment>(GetPaymentQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new PaymentLoaderViewModel(
						UnitOfWorkFactory,
						_commonServices,
						NavigationManager,
						_organizationParametersProvider,
						_profitCategoryProvider,
						_paymentsRepository,
						new CounterpartyRepository(),
						_orderRepository
					),
					//функция диалога открытия документа
					(PaymentJournalNode node) => new ManualPaymentMatchingViewModel(
						EntityUoWBuilder.ForOpen(node.Id),
						UnitOfWorkFactory,
						_commonServices,
						_orderRepository,
						new PaymentItemsRepository(),
						_paymentsRepository,
						_dialogsFactory
					),
					//функция идентификации документа 
					(PaymentJournalNode node) => node.EntityType == typeof(Payment),
					"Платежи",
					new JournalParametersForDocument { HideJournalForCreateDialog = true, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			complaintConfig.FinishConfiguration();
		}

		void CompleteAllocation()
		{
			var distributedPayments = _paymentsRepository.GetAllDistributedPayments(UoW);

			if(distributedPayments.Any())
			{
				foreach(var payment in distributedPayments)
				{
					payment.Status = PaymentState.completed;
					UoW.Save(payment);
				}

				UoW.Commit();
			}
		}
	}
}
