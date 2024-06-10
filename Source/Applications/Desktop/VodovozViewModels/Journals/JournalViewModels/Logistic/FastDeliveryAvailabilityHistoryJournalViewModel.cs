using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Sale;
using Vodovoz.Services;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class FastDeliveryAvailabilityHistoryJournalViewModel : FilterableSingleEntityJournalViewModelBase<FastDeliveryAvailabilityHistory,
		FastDeliveryAvailabilityHistoryViewModel, FastDeliveryAvailabilityHistoryJournalNode, FastDeliveryAvailabilityFilterViewModel>
	{
		private readonly Timer _timer;
		private const double _interval = 5 * 60000; //5 минут

		private readonly IEmployeeService _employeeService;
		private readonly IFileDialogService _fileDialogService;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private IList<FastDeliveryAvailabilityHistoryJournalNode> _sequenceNodes;

		public FastDeliveryAvailabilityHistoryJournalViewModel(
			FastDeliveryAvailabilityFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IFileDialogService fileDialogService,
			IFastDeliveryAvailabilityHistorySettings fastDeliveryAvailabilityHistorySettings,
			INomenclatureSettings nomenclatureSettings,
			Action<FastDeliveryAvailabilityFilterViewModel> filterParams = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			var availabilityHistorySettings = fastDeliveryAvailabilityHistorySettings
													   ?? throw new ArgumentNullException(nameof(fastDeliveryAvailabilityHistorySettings));

			TabName = "Журнал истории проверок экспресс-доставок";

			UpdateOnChanges(
				typeof(FastDeliveryAvailabilityHistory),
				typeof(FastDeliveryAvailabilityHistoryItem),
				typeof(FastDeliveryNomenclatureDistributionHistory),
				typeof(FastDeliveryOrderItemHistory)
				);

			_timer = new Timer(_interval);
			_timer.Elapsed += TimerOnElapsed;
			_timer.Start();

			if(filterParams != null)
			{
				FilterViewModel.ConfigureWithoutFiltering(filterParams);
			}

			DataLoader.PostLoadProcessingFunc = BeforeItemsUpdated;

			FilterViewModel.PropertyChanged += OnFilterViewModelPropertyChanged;
			FilterViewModel.InitFailsReport();
		}

		private void OnFilterViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(FilterViewModel.IsNomenclatureNotInStock))
			{
				var isNomenclatureNotInStock = FilterViewModel.IsNomenclatureNotInStock;
				var reportName = FastDeliveryFailsReport.GetReportName(isNomenclatureNotInStock);

				FilterViewModel.FailsReportName = reportName;
				FilterViewModel.FailsReportAction = () =>
				{
					var report = new FastDeliveryFailsReport(UnitOfWorkFactory, FilterViewModel, Search, _nomenclatureSettings, _fileDialogService);
					report.Export();
				};
			}
		}

		protected void BeforeItemsUpdated(IList items, uint start)
		{
			_sequenceNodes = SequenceItemsSourceQueryFunction(UoW).List<FastDeliveryAvailabilityHistoryJournalNode>();

			var grouppedByDateNodes = _sequenceNodes.GroupBy(x => x.VerificationDate.Date)
				  .Select(group =>
						new
						{
							Date = group.Key,
							Nodes = group
						});

			foreach(var grouppedNode in grouppedByDateNodes)
			{
				foreach(var node in grouppedNode.Nodes)
				{
					var sequenceNum = grouppedNode.Nodes.ToList().IndexOf(node) + 1;
					var journalNode = items.OfType<FastDeliveryAvailabilityHistoryJournalNode>().FirstOrDefault(x => x.Id == node.Id);
					if(journalNode != null)
					{
						journalNode.SequenceNumber = sequenceNum;
					}
				}
			}
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
			Nomenclature nomenclatureAlias = null;
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

			var nomenclatureDistributionSubquery = QueryOver.Of(() => fastDeliveryNomenclatureDistributionHistoryAlias)
			.Where(() => fastDeliveryNomenclatureDistributionHistoryAlias.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id)
			.Where(()=> fastDeliveryOrderItemHistoryAlias.Nomenclature.Id == fastDeliveryNomenclatureDistributionHistoryAlias.Nomenclature.Id)
			.Select(Projections.Property(() => fastDeliveryNomenclatureDistributionHistoryAlias.Nomenclature.Id));

			var nomenclatureNotInStockSubquery = QueryOver.Of(() => fastDeliveryOrderItemHistoryAlias)
			.JoinAlias(() => fastDeliveryOrderItemHistoryAlias.Nomenclature, () => nomenclatureAlias)
			.Where(() => fastDeliveryOrderItemHistoryAlias.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id)
			.Where(() => nomenclatureAlias.ProductGroup.Id != _nomenclatureSettings.PromotionalNomenclatureGroupId)
			.WithSubquery.WhereNotExists(nomenclatureDistributionSubquery)
			.Select(Projections.Conditional(
				Restrictions.Gt(
					Projections.Max(Projections.Property(() => fastDeliveryOrderItemHistoryAlias.Nomenclature.Id)), 0),
					Projections.Constant(true),
					Projections.Constant(false))
			);

			var itemsQuery = uow.Session.QueryOver(() => fastDeliveryAvailabilityHistoryAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Author, () => authorAlias)
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

			if(FilterViewModel.IsVerificationFromSite != null)
			{
				if(FilterViewModel.IsVerificationFromSite.Value)
				{
					itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.Author == null);
				}
				else
				{
					itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.Author != null);
				}
			}

			if(FilterViewModel.IsNomenclatureNotInStock != null)
			{
				itemsQuery.Where(Restrictions.Eq(Projections.SubQuery(nomenclatureNotInStockSubquery), FilterViewModel.IsNomenclatureNotInStock));
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
					.Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.AddressFromDeliveryPoint)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.AddressWithoutDeliveryPoint).WithAlias(() => resultAlias.AddressWithoutDeliveryPoint)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.District)
					.SelectSubQuery(isValidSubquery).WithAlias(() => resultAlias.IsValid)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.LogisticianComment).WithAlias(() => resultAlias.LogisticianComment)
					.Select(() => logisticianAlias.LastName).WithAlias(() => resultAlias.LogisticianLastName)
					.Select(() => logisticianAlias.Name).WithAlias(() => resultAlias.LogisticianName)
					.Select(() => logisticianAlias.Patronymic).WithAlias(() => resultAlias.LogisticianPatronymic)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.LogisticianCommentVersion).WithAlias(() => resultAlias.LogisticianCommentVersion)
					.SelectSubQuery(nomenclatureNotInStockSubquery).WithAlias(() => resultAlias.IsNomenclatureNotInStockSubquery)
				).OrderBy(() => fastDeliveryAvailabilityHistoryAlias.VerificationDate).Desc
				.TransformUsing(Transformers.AliasToBean<FastDeliveryAvailabilityHistoryJournalNode>());

			return itemsQuery;
		};

		private Func<IUnitOfWork, IQueryOver<FastDeliveryAvailabilityHistory>> SequenceItemsSourceQueryFunction => (uow) =>
		{
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistoryAlias = null;
			FastDeliveryAvailabilityHistoryItem fastDeliveryAvailabilityHistoryItemAlias = null;
			FastDeliveryNomenclatureDistributionHistory fastDeliveryNomenclatureDistributionHistoryAlias = null;
			FastDeliveryOrderItemHistory fastDeliveryOrderItemHistoryAlias = null;
			Employee authorAlias = null;
			Employee logisticianAlias = null;
			Nomenclature nomenclatureAlias = null;
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

			var nomenclatureDistributionSubquery = QueryOver.Of(() => fastDeliveryNomenclatureDistributionHistoryAlias)
			.Where(() => fastDeliveryNomenclatureDistributionHistoryAlias.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id)
			.Where(() => fastDeliveryOrderItemHistoryAlias.Nomenclature.Id == fastDeliveryNomenclatureDistributionHistoryAlias.Nomenclature.Id)
			.Select(Projections.Property(() => fastDeliveryNomenclatureDistributionHistoryAlias.Nomenclature.Id));

			var nomenclatureNotInStockSubquery = QueryOver.Of(() => fastDeliveryOrderItemHistoryAlias)
			.JoinAlias(() => fastDeliveryOrderItemHistoryAlias.Nomenclature, () => nomenclatureAlias)
			.Where(() => fastDeliveryOrderItemHistoryAlias.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id)
			.Where(() => nomenclatureAlias.ProductGroup.Id != _nomenclatureSettings.PromotionalNomenclatureGroupId)
			.WithSubquery.WhereNotExists(nomenclatureDistributionSubquery)
			.Select(Projections.Conditional(
				Restrictions.Gt(
					Projections.Max(Projections.Property(() => fastDeliveryOrderItemHistoryAlias.Nomenclature.Id)), 0),
					Projections.Constant(true),
					Projections.Constant(false))
			);

			var itemsQuery = uow.Session.QueryOver(() => fastDeliveryAvailabilityHistoryAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Author, () => authorAlias)
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

			if(FilterViewModel.IsVerificationFromSite != null)
			{
				if(FilterViewModel.IsVerificationFromSite.Value)
				{
					itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.Author == null);
				}
				else
				{
					itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.Author != null);
				}
			}

			if(FilterViewModel.IsNomenclatureNotInStock != null)
			{
				itemsQuery.Where(Restrictions.Eq(Projections.SubQuery(nomenclatureNotInStockSubquery), FilterViewModel.IsNomenclatureNotInStock));
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
				)
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

					var grouppedByDateNodes = rows.OrderBy(x => x.Id).GroupBy(x => x.VerificationDate.Date)
					.Select(group =>
							new
							{
								Date = group.Key,
								Nodes = group
							});

					foreach(var grouppedNode in grouppedByDateNodes)
					{
						foreach(var node in grouppedNode.Nodes)
						{
							node.SequenceNumber = grouppedNode.Nodes.ToList().IndexOf(node) + 1;
						}
					}

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

		public override void Dispose()
		{
			FilterViewModel.PropertyChanged -= OnFilterViewModelPropertyChanged;
			_timer?.Dispose();
			base.Dispose();
		}
	}
}
