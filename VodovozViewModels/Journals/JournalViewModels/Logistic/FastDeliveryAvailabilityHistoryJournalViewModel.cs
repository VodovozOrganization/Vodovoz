using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Timers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Sale;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class FastDeliveryAvailabilityHistoryJournalViewModel : FilterableSingleEntityJournalViewModelBase<FastDeliveryAvailabilityHistory,
		FastDeliveryAvailabilityHistoryViewModel, FastDeliveryAvailabilityHistoryJournalNode, FastDeliveryAvailabilityFilterViewModel>
	{
		private readonly Timer _timer;
		private const double _interval = 30 * 1000; //5 минут

		private readonly IEmployeeService _employeeService;
		private readonly IFileDialogService _fileDialogService;

		public FastDeliveryAvailabilityHistoryJournalViewModel(FastDeliveryAvailabilityFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IFileDialogService fileDialogService,
			IFastDeliveryAvailabilityHistoryParameterProvider fastDeliveryAvailabilityHistoryParameterProvider)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			var availabilityHistoryParameterProvider = fastDeliveryAvailabilityHistoryParameterProvider
			                                           ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistoryParameterProvider));

			TabName = "Журнал истории проверок экспресс-доставок";

			UpdateOnChanges(
				typeof(FastDeliveryAvailabilityHistory),
				typeof(FastDeliveryAvailabilityHistoryItem),
				typeof(FastDeliveryNomenclatureDistributionHistory),
				typeof(FastDeliveryOrderItemHistory)
				);

			
			var fastDeliveryAvailabilityHistoryModel = new FastDeliveryAvailabilityHistoryModel(unitOfWorkFactory);
			fastDeliveryAvailabilityHistoryModel.ClearFastDeliveryAvailabilityHistory(availabilityHistoryParameterProvider);

			_timer = new Timer(_interval);
			_timer.Elapsed += TimerOnElapsed;
			_timer.Start();
		}

		private void TimerOnElapsed(object sender, ElapsedEventArgs e)
		{
			_timer.Interval = _interval;
			Refresh();
		}

		protected override Func<IUnitOfWork, IQueryOver<FastDeliveryAvailabilityHistory>> ItemsSourceQueryFunction => (uow) =>
		{
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistoryAlias = null;
			FastDeliveryAvailabilityHistoryItem fastDeliveryAvailabilityHistoryItemAlias = null;
			FastDeliveryNomenclatureDistributionHistory fastDeliveryNomenclatureDistributionHistoryAlias = null;
			FastDeliveryOrderItemHistory fastDeliveryOrderItemHistoryAlias = null;
			Employee authorAlias = null;
			Employee logisticianAlias = null;
			Order orderAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			Counterparty counterpartyAlias = null;
			FastDeliveryAvailabilityHistoryJournalNode resultAlias = null;

			var authorProjection = CustomProjections.Concat_WS(
				"",
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var logisticianProjection = CustomProjections.Concat_WS(
				"",
				Projections.Property(() => logisticianAlias.LastName),
				Projections.Property(() => logisticianAlias.Name),
				Projections.Property(() => logisticianAlias.Patronymic)
			);

			var isValidSubquery = QueryOver.Of(() => fastDeliveryAvailabilityHistoryItemAlias)
				.Where(() => fastDeliveryAvailabilityHistoryItemAlias.FastDeliveryAvailabilityHistory.Id ==
							 fastDeliveryAvailabilityHistoryAlias.Id)
				.And(() => fastDeliveryAvailabilityHistoryItemAlias.IsValidToFastDelivery)
				.Select(Projections.Conditional(Restrictions.Gt(Projections.RowCount(), 0),
					Projections.Constant(true),
					Projections.Constant(false)));


			var itemsQuery = uow.Session.QueryOver(() => fastDeliveryAvailabilityHistoryAlias)
				.JoinEntityAlias(() => fastDeliveryNomenclatureDistributionHistoryAlias,
					() => fastDeliveryNomenclatureDistributionHistoryAlias.FastDeliveryAvailabilityHistory.Id ==fastDeliveryAvailabilityHistoryAlias.Id,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => fastDeliveryOrderItemHistoryAlias,
					() => fastDeliveryOrderItemHistoryAlias.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id,
					JoinType.LeftOuterJoin)
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
				itemsQuery.Where(Restrictions.Eq(Projections.SubQuery(isValidSubquery), FilterViewModel.IsValid));
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
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
					.Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorLastName)
					.Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
					.Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
					.Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.Address)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.District)
					.SelectSubQuery(isValidSubquery).WithAlias(() => resultAlias.IsValid)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.LogisticianComment).WithAlias(() => resultAlias.LogisticianComment)
					.Select(() => logisticianAlias.LastName).WithAlias(() => resultAlias.LogisticianLastName)
					.Select(() => logisticianAlias.Name).WithAlias(() => resultAlias.LogisticianName)
					.Select(() => logisticianAlias.Patronymic).WithAlias(() => resultAlias.LogisticianPatronymic)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.LogisticianCommentVersion).WithAlias(() => resultAlias.LogisticianCommentVersion)
				).OrderBy(() => fastDeliveryAvailabilityHistoryAlias.VerificationDate).Desc
				.TransformUsing(Transformers.AliasToBean<FastDeliveryAvailabilityHistoryJournalNode>());

			return itemsQuery;
		};

		protected override void CreateNodeActions()
		{
			CreateDefaultEditAction();
			CreateXLExportAction();
		}

		private void CreateXLExportAction()
		{
			var xlExportAction = new JournalAction("Экспорт в Excel",
				(selected) => true,
				(selected) => true,
				(selected) =>
				{
					var rows = ItemsSourceQueryFunction.Invoke(UoW)
						.List<FastDeliveryAvailabilityHistoryJournalNode>();

					var report = new FastDeliveryAvailabilityHistoryReport(rows, _fileDialogService);
					report.Export();
				}
			);

			NodeActionsList.Add(xlExportAction);
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
