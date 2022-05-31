using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class FastDeliveryAvailabilityHistoryJournalViewModel : FilterableSingleEntityJournalViewModelBase<FastDeliveryAvailabilityHistory,
		FastDeliveryAvailabilityHistoryViewModel, FastDeliveryAvailabilityHistoryJournalNode, FastDeliveryAvailabilityFilterViewModel>
	{
		private readonly IEmployeeService _employeeService;

		public FastDeliveryAvailabilityHistoryJournalViewModel(FastDeliveryAvailabilityFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			TabName = "Журнал истории проверок экспресс-доставок";

			UpdateOnChanges(
				typeof(FastDeliveryAvailabilityHistory),
				typeof(FastDeliveryAvailabilityHistoryItem),
				typeof(FastDeliveryNomenclatureDistributionHistory),
				typeof(FastDeliveryOrderItemHistory)
				);
		}

		protected override Func<IUnitOfWork, IQueryOver<FastDeliveryAvailabilityHistory>> ItemsSourceQueryFunction => (uow) =>
		{
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistoryAlias = null;
			FastDeliveryAvailabilityHistoryItem fastDeliveryAvailabilityHistoryItemAlias = null;
			FastDeliveryNomenclatureDistributionHistory fastDeliveryNomenclatureDistributionHistoryAlias = null;
			FastDeliveryOrderItemHistory fastDeliveryOrderItemHistoryAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			Employee logisticianAlias = null;
			Order orderAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			Counterparty counterpartyAlias = null;
			FastDeliveryAvailabilityHistoryJournalNode resultAlias = null;

			var authorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var logisticianProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GET_PERSON_NAME_WITH_INITIALS(?1, ?2, ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => logisticianAlias.LastName),
				Projections.Property(() => logisticianAlias.Name),
				Projections.Property(() => logisticianAlias.Patronymic)
			);

			var itemsQuery = uow.Session.QueryOver(() => fastDeliveryAvailabilityHistoryAlias)
				.JoinEntityAlias(() => fastDeliveryAvailabilityHistoryItemAlias,
					() => fastDeliveryAvailabilityHistoryItemAlias.FastDeliveryAvailabilityHistory.Id ==
						  fastDeliveryAvailabilityHistoryAlias.Id,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => fastDeliveryNomenclatureDistributionHistoryAlias,
					() => fastDeliveryNomenclatureDistributionHistoryAlias.FastDeliveryAvailabilityHistory.Id ==
						  fastDeliveryAvailabilityHistoryAlias.Id,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => fastDeliveryOrderItemHistoryAlias,
					() => fastDeliveryOrderItemHistoryAlias.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id,
					JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryItemAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.District, () => districtAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Logistician, () => logisticianAlias)
			.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Counterparty, () => counterpartyAlias);

			if(FilterViewModel.VerificationDateFrom != null && FilterViewModel.VerificationDateTo != null)
			{
				itemsQuery.Where(x => x.VerificationDate >= FilterViewModel.VerificationDateFrom.Value.Date.Add(new TimeSpan(0, 0, 0, 0))
									  && x.VerificationDate <= FilterViewModel.VerificationDateTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));
			}

			if(FilterViewModel.Counterparty != null)
			{
				itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
			}

			if(FilterViewModel.District != null)
			{
				itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.District.Id == FilterViewModel.District.Id);
			}

			if(FilterViewModel.Logistician != null)
			{
				itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.Logistician.Id == FilterViewModel.Logistician.Id);
			}

			if(FilterViewModel.IsValid != null)
			{
				itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.IsValid == FilterViewModel.IsValid);
			}

			if(FilterViewModel.LogisticianReactionTimeMinutes > 0)
			{
				var timestampDiff = new SQLFunctionTemplate(NHibernateUtil.Int32, "TIMESTAMPDIFF(MINUTE, ?1, ?2)");

				var timestampProjection = Projections.SqlFunction(timestampDiff, NHibernateUtil.Int32,
					Projections.Property(() => fastDeliveryAvailabilityHistoryAlias.VerificationDate),
					Projections.Property(() => fastDeliveryAvailabilityHistoryAlias.LogisticianCommentVersion));

				itemsQuery.Where(Restrictions.Ge(timestampProjection, FilterViewModel.LogisticianReactionTimeMinutes));
			}

			itemsQuery.Where(GetSearchCriterion(
				() => fastDeliveryAvailabilityHistoryAlias.Id,
				() => fastDeliveryAvailabilityHistoryAlias.Order.Id,
				() => authorProjection,
				() => logisticianProjection,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.ShortAddress,
				() => districtAlias.DistrictName,
				() => fastDeliveryAvailabilityHistoryAlias.LogisticianComment,
				() => fastDeliveryAvailabilityHistoryAlias.VerificationDate,
				() => fastDeliveryAvailabilityHistoryAlias.LogisticianCommentVersion
				)
			);

			itemsQuery
				.SelectList(list => list
					.SelectGroup(() => fastDeliveryAvailabilityHistoryAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.VerificationDate).WithAlias(() => resultAlias.VerificationDate)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.Order.Id).WithAlias(() => resultAlias.Order)
					.Select(authorProjection).WithAlias(() => resultAlias.Author)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.Address)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.District)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.IsValid).WithAlias(() => resultAlias.IsValid)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.LogisticianComment).WithAlias(() => resultAlias.LogisticianComment)
					.Select(logisticianProjection).WithAlias(() => resultAlias.Logistician)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.LogisticianCommentVersion).WithAlias(() => resultAlias.LogisticianCommentVersion)
				).OrderBy(() => fastDeliveryAvailabilityHistoryAlias.VerificationDate).Desc
				.TransformUsing(Transformers.AliasToBean<FastDeliveryAvailabilityHistoryJournalNode>());

			return itemsQuery;
		};

		protected override void CreateNodeActions()
		{
			CreateDefaultEditAction();
		}

		protected override Func<FastDeliveryAvailabilityHistoryViewModel> CreateDialogFunction =>
			() => throw new NotSupportedException("Не поддерживается создание из журнала");

		protected override Func<FastDeliveryAvailabilityHistoryJournalNode, FastDeliveryAvailabilityHistoryViewModel> OpenDialogFunction =>
			node => new FastDeliveryAvailabilityHistoryViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				UnitOfWorkFactory,
				commonServices,
				_employeeService);
	}
}
