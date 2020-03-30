using Vodovoz.Domain.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels;
using System;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Navigation;
using NHibernate;
using NHibernate.Transform;
using Vodovoz.Domain.Client;
using BaseOrg = Vodovoz.Domain.Organization;
using NHibernate.Dialect.Function;
using VodOrder = Vodovoz.Domain.Orders.Order;
using NHibernate.Criterion;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;

namespace Vodovoz.JournalViewModels
{
	public class PaymentsJournalViewModel : FilterableMultipleEntityJournalViewModelBase<PaymentJournalNode, PaymentsJournalFilterViewModel>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly INavigationManager navigationManager;
		private readonly ICommonServices commonServices;

		public PaymentsJournalViewModel(
			PaymentsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager
			//IEntityAutocompleteSelectorFactory employeeSelectorFactory
		) : base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал платежей из банк-клиента";
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.navigationManager = navigationManager;

			RegisterPayments();

			var threadLoader = DataLoader as ThreadDataLoader<PaymentJournalNode>;
			//threadLoader.MergeInOrderBy(x => x.Id, false);

			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(Payment),
				typeof(VodOrder)
			);
		}

		private IQueryOver<Payment> GetPaymentQuery(IUnitOfWork uow)
		{
			PaymentJournalNode resultAlias = null;
			Payment paymentAlias = null;
			Counterparty counterpartyAlias = null;
			BaseOrg organizationAlias = null;
			VodOrder orderAlias = null;
			CategoryProfit categoryProfitAlias = null;

			var paymentQuery = uow.Session.QueryOver(() => paymentAlias)
				.Left.JoinAlias(() => paymentAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => paymentAlias.Organization, () => organizationAlias)
				.Left.JoinAlias(() => paymentAlias.ProfitCategory, () => categoryProfitAlias)
				.Left.JoinAlias(() => paymentAlias.Orders, () => orderAlias);

			if(FilterViewModel != null) 
			{
				if(FilterViewModel.StartDate.HasValue)
					paymentQuery.Where(x => x.Date >= FilterViewModel.StartDate);

				if(FilterViewModel.EndDate.HasValue)
					paymentQuery.Where(x => x.Date <= FilterViewModel.EndDate);

				if(FilterViewModel.HideCompleted)
					paymentQuery.Where(x => x.Status != PaymentState.completed);

				if(FilterViewModel.PaymentState.HasValue)
					paymentQuery.Where(x => x.Status == FilterViewModel.PaymentState);
			}

			paymentQuery.Where(GetSearchCriterion(
				() => paymentAlias.PaymentNum,
				() => paymentAlias.Total,
				() => paymentAlias.CounterpartyName
				//() => orderAlias.Id
			));

			var numOrders = Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
							NHibernateUtil.String,
							Projections.Property(() => orderAlias.Id),
							Projections.Constant("\n"));

			var resultQuery = paymentQuery
				.SelectList(list => list
				   .SelectGroup(() => paymentAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => paymentAlias.PaymentNum).WithAlias(() => resultAlias.PaymentNum)
				   .Select(() => paymentAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => paymentAlias.Total).WithAlias(() => resultAlias.Total)
				   .Select(numOrders).WithAlias(() => resultAlias.Orders)
				   .Select(() => counterpartyAlias.FullName).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => organizationAlias.FullName).WithAlias(() => resultAlias.Organization)
				   .Select(() => paymentAlias.PaymentPurpose).WithAlias(() => resultAlias.PaymentPurpose)
				   .Select(() => categoryProfitAlias.Name).WithAlias(() => resultAlias.ProfitCategory)
				   .Select(() => paymentAlias.Status).WithAlias(() => resultAlias.Status)
				)
				.OrderBy(() => paymentAlias.Status).Asc
				.OrderBy(() => counterpartyAlias.FullName).Asc
				.OrderBy(() => paymentAlias.Total).Asc
				.TransformUsing(Transformers.AliasToBean<PaymentJournalNode>());
			return resultQuery;
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
		}

		private void RegisterPayments()
		{
			var complaintConfig = RegisterEntity<Payment>(GetPaymentQuery)
				.AddDocumentConfiguration(
					//функция диалога создания документа
					() => new PaymentLoaderVM(
						//EntityUoWBuilder.ForCreate(),
						unitOfWorkFactory,
						commonServices,
						navigationManager
					),
					//функция диалога открытия документа
					(PaymentJournalNode node) => new ManualPaymentMatchingVM(
						EntityUoWBuilder.ForOpen(node.Id),
						unitOfWorkFactory,
						commonServices
					),
					//функция идентификации документа 
					(PaymentJournalNode node) => {
						return node.EntityType == typeof(Payment);
					},
					"Платежи",
					new JournalParametersForDocument { HideJournalForCreateDialog = true, HideJournalForOpenDialog = true }
				);

			//завершение конфигурации
			complaintConfig.FinishConfiguration();
		}
	}
}
