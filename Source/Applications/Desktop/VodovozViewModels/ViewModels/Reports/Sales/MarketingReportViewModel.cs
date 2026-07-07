using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Presentation.ViewModels.Common;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public class MarketingReportViewModel : DialogTabViewModelBase
	{
		private readonly IIncludeExcludeSalesFilterFactory _includeExcludeSalesFilterFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly bool _userIsSalesRepresentative;

		private IncludeExludeFiltersViewModel _filterViewModel;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private MarketingReportGroupingType _groupingType = MarketingReportGroupingType.All;
		private MarketingReportDateType _dateType = MarketingReportDateType.DeliveryDate;
		private MarketingReport _report;
		private bool _isGenerating;
		private bool _canSave;
		private bool _isSaving;

		public MarketingReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IIncludeExcludeSalesFilterFactory includeExcludeSalesFilterFactory,
			ICurrentPermissionService currentPermissionService,
			IUserService userService,
			IEmployeeRepository employeeRepository)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			if(!currentPermissionService.ValidatePresetPermission(ReportPermissions.Sales.CanAccessSalesReports))
			{
				throw new AbortCreatingPageException("У вас нет разрешения на доступ в этот отчет", "Доступ запрещен");
			}

			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_includeExcludeSalesFilterFactory = includeExcludeSalesFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeSalesFilterFactory));

			Title = "Маркетинговый отчет";
			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();

			_userIsSalesRepresentative = currentPermissionService.ValidatePresetPermission(UserPermissions.IsSalesRepresentative)
				&& !userService.GetCurrentUser().IsAdmin;

			StartDate = DateTime.Now.Date.AddMonths(-3);
			EndDate = DateTime.Now.Date;

			ShowInfoCommand = new DelegateCommand(ShowInfo);
			_filterViewModel = _includeExcludeSalesFilterFactory.CreateMarketingReportIncludeExcludeFilter(
				_unitOfWork,
				_userIsSalesRepresentative ? (int?)employeeRepository.GetEmployeeForCurrentUser(_unitOfWork).Id : null);
		}

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public IncludeExludeFiltersViewModel FilterViewModel => _filterViewModel;

		public DelegateCommand ShowInfoCommand { get; }

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public MarketingReportGroupingType GroupingType
		{
			get => _groupingType;
			set => SetField(ref _groupingType, value);
		}

		public MarketingReportDateType DateType
		{
			get => _dateType;
			set => SetField(ref _dateType, value);
		}

		public MarketingReport Report
		{
			get => _report;
			set
			{
				SetField(ref _report, value);
				CanSave = _report != null;
			}
		}

		public bool CanSave
		{
			get => _canSave;
			set => SetField(ref _canSave, value);
		}

		public bool IsSaving
		{
			get => _isSaving;
			set
			{
				SetField(ref _isSaving, value);
				CanSave = !IsSaving && Report != null;
			}
		}

		public bool CanGenerate => !IsGenerating;

		public bool IsGenerating
		{
			get => _isGenerating;
			set
			{
				SetField(ref _isGenerating, value);
				OnPropertyChanged(nameof(CanGenerate));
			}
		}

		public async Task<MarketingReport> ActionGenerateReport(CancellationToken cancellationToken)
		{
			try
			{
				return await Task.Run(() => Generate(cancellationToken), cancellationToken);
			}
			finally
			{
				_unitOfWork.Session.Clear();
			}
		}

		public void ExportReport(string path) => Report.Export(path);

		private MarketingReport Generate(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if(StartDate == null || EndDate == null)
			{
				throw new InvalidOperationException("Заполните период отчета.");
			}

			_filterViewModel.GetReportParametersSet(out var filtersDescriptionBuilder, withCounts: false);

			var includedOrderStatuses = _filterViewModel
				.GetFilter<IncludeExcludeEnumFilter<OrderStatus>>()
				.GetIncluded()
				.ToArray();

			var excludedOrderStatuses = _filterViewModel
				.GetFilter<IncludeExcludeEnumFilter<OrderStatus>>()
				.GetExcluded()
				.ToArray();

			var totalCounterparties = GetTotalCounterparties();
			var orders = GetOrders(StartDate.Value, EndDate.Value, includedOrderStatuses, excludedOrderStatuses, cancellationToken);
			var clientHistories = GetClientHistories(includedOrderStatuses, excludedOrderStatuses, cancellationToken);

			return MarketingReport.Create(
				StartDate.Value,
				EndDate.Value,
				filtersDescriptionBuilder.ToString(),
				GroupingType,
				DateType,
				totalCounterparties,
				orders,
				clientHistories);
		}

		private int GetTotalCounterparties()
		{
			Counterparty counterpartyAlias = null;
			return _unitOfWork.Session.QueryOver(() => counterpartyAlias)
				.RowCount();
		}

		private IList<MarketingReportOrderNode> GetOrders(
			DateTime startDate,
			DateTime endDate,
			OrderStatus[] includedOrderStatuses,
			OrderStatus[] excludedOrderStatuses,
			CancellationToken cancellationToken)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			Employee authorAlias = null;
			Counterparty counterpartyAlias = null;
			CounterpartyClassification counterpartyClassificationAlias = null;
			OrderRating orderRatingAlias = null;
			MarketingReportOrderNode resultAlias = null;

			var lastCalculationSettingsId = _unitOfWork.GetAll<CounterpartyClassification>()
				.Select(c => c.ClassificationCalculationSettingsId)
				.OrderByDescending(d => d)
				.FirstOrDefault();

			var orderSumSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.Select(OrderProjections.GetOrderSumProjection());

			var bottles19LSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water
					&& nomenclatureAlias.TareVolume == Core.Domain.Goods.TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.ActualCount));

			var additionalServicesCategories = new object[]
			{
				NomenclatureCategory.equipment,
				NomenclatureCategory.service,
				NomenclatureCategory.master,
				NomenclatureCategory.additional
			};

			var additionalServicesCountSubquery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(Restrictions.In(Projections.Property(() => nomenclatureAlias.Category), additionalServicesCategories))
				.Select(Projections.Count(() => orderItemAlias.Id));

			var classificationProjection = BuildCounterpartyClassificationProjection(counterpartyClassificationAlias);

			var ratingSubquery = QueryOver.Of(() => orderRatingAlias)
				.Where(() => orderAlias.Id == orderRatingAlias.Order.Id)
				.Select(Projections.Property(() => orderRatingAlias.Rating))
				.Take(1);

			var fullNameOrderAuthorProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT(?1, ' ', ?2, ' ', ?3)"),
				NHibernateUtil.String,
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic));

			var orderAuthorNameProjection = Projections.Conditional(
				Restrictions.IsNotNull(Projections.Property(() => authorAlias.LastName)),
				fullNameOrderAuthorProjection,
				Projections.Constant("Без автора заказа"));

			var orderAuthorIdProjection = Projections.Conditional(
				Restrictions.IsNull(Projections.Property(() => authorAlias.Id)),
				Projections.Constant(0),
				Projections.Property(() => authorAlias.Id));

			var query = _unitOfWork.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinEntityAlias(
					() => counterpartyClassificationAlias,
					() => counterpartyAlias.Id == counterpartyClassificationAlias.CounterpartyId
						&& counterpartyClassificationAlias.ClassificationCalculationSettingsId == lastCalculationSettingsId,
					JoinType.LeftOuterJoin)
				.Where(() => !orderAlias.IsContractCloser);

			if(DateType == MarketingReportDateType.DeliveryDate)
			{
				query.Where(Restrictions.Between(Projections.Property(() => orderAlias.DeliveryDate), startDate, endDate));
			}
			else
			{
				query.Where(Restrictions.Between(Projections.Property(() => orderAlias.CreateDate), startDate, endDate));
			}

			if(includedOrderStatuses.Any())
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(includedOrderStatuses);
			}

			if(excludedOrderStatuses.Any())
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(excludedOrderStatuses);
			}

			cancellationToken.ThrowIfCancellationRequested();

			return query
				.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.ClientId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(() => orderAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.SelectSubQuery(orderSumSubquery).WithAlias(() => resultAlias.OrderSum)
					.SelectSubQuery(bottles19LSubquery).WithAlias(() => resultAlias.Bottles19LCount)
					.Select(Projections.Conditional(
						Restrictions.Gt(Projections.SubQuery(additionalServicesCountSubquery), 0),
						Projections.Constant(true),
						Projections.Constant(false))).WithAlias(() => resultAlias.HasAdditionalServices)
					.SelectSubQuery(ratingSubquery).WithAlias(() => resultAlias.Rating)
					.Select(orderAuthorIdProjection).WithAlias(() => resultAlias.AuthorId)
					.Select(orderAuthorNameProjection).WithAlias(() => resultAlias.AuthorName)
					.Select(classificationProjection).WithAlias(() => resultAlias.AbcClassification))
				.SetTimeout(120)
				.TransformUsing(Transformers.AliasToBean<MarketingReportOrderNode>())
				.List<MarketingReportOrderNode>();
		}

		private IList<MarketingReportClientHistoryNode> GetClientHistories(
			OrderStatus[] includedOrderStatuses,
			OrderStatus[] excludedOrderStatuses,
			CancellationToken cancellationToken)
		{
			Order orderAlias = null;
			MarketingReportClientHistoryNode resultAlias = null;

			var query = _unitOfWork.Session.QueryOver(() => orderAlias)
				.Where(() => !orderAlias.IsContractCloser);

			if(includedOrderStatuses.Any())
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(includedOrderStatuses);
			}

			if(excludedOrderStatuses.Any())
			{
				query.WhereRestrictionOn(() => orderAlias.OrderStatus).Not.IsIn(excludedOrderStatuses);
			}

			cancellationToken.ThrowIfCancellationRequested();

			var dateProjection = DateType == MarketingReportDateType.DeliveryDate
				? Projections.Property(() => orderAlias.DeliveryDate)
				: Projections.Property(() => orderAlias.CreateDate);

			return query
				.SelectList(list => list
					.SelectGroup(() => orderAlias.Client.Id)
					.Select(() => orderAlias.Client.Id).WithAlias(() => resultAlias.ClientId)
					.Select(Projections.Min(dateProjection)).WithAlias(() => resultAlias.FirstOrderDate)
					.Select(Projections.Max(dateProjection)).WithAlias(() => resultAlias.LastOrderDate)
					.Select(Projections.Count(() => orderAlias.Id)).WithAlias(() => resultAlias.OrdersCount))
				.SetTimeout(120)
				.TransformUsing(Transformers.AliasToBean<MarketingReportClientHistoryNode>())
				.List<MarketingReportClientHistoryNode>();
		}

		private IProjection BuildCounterpartyClassificationProjection(CounterpartyClassification counterpartyClassificationAlias)
		{
			var classificationByBottlesCountProjection =
				Projections.Property(() => counterpartyClassificationAlias.ClassificationByBottlesCount);

			var classificationByOrdersCountProjection =
				Projections.Property(() => counterpartyClassificationAlias.ClassificationByOrdersCount);

			var classificationIsAXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsAYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsAZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.A),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));

			var classificationIsBXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsBYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsBZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.B),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));

			var classificationIsCXRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.X));

			var classificationIsCYRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Y));

			var classificationIsCZRestriction =
				Restrictions.And(
					Restrictions.Eq(classificationByBottlesCountProjection, CounterpartyClassificationByBottlesCount.C),
					Restrictions.Eq(classificationByOrdersCountProjection, CounterpartyClassificationByOrdersCount.Z));

			return Projections.Conditional(
				classificationIsAXRestriction, Projections.Constant(CounterpartyCompositeClassification.AX),
				Projections.Conditional(
					classificationIsAYRestriction, Projections.Constant(CounterpartyCompositeClassification.AY),
					Projections.Conditional(
						classificationIsAZRestriction, Projections.Constant(CounterpartyCompositeClassification.AZ),
						Projections.Conditional(
							classificationIsBXRestriction, Projections.Constant(CounterpartyCompositeClassification.BX),
							Projections.Conditional(
								classificationIsBYRestriction, Projections.Constant(CounterpartyCompositeClassification.BY),
								Projections.Conditional(
									classificationIsBZRestriction, Projections.Constant(CounterpartyCompositeClassification.BZ),
									Projections.Conditional(
										classificationIsCXRestriction, Projections.Constant(CounterpartyCompositeClassification.CX),
										Projections.Conditional(
											classificationIsCYRestriction, Projections.Constant(CounterpartyCompositeClassification.CY),
											Projections.Conditional(
												classificationIsCZRestriction, Projections.Constant(CounterpartyCompositeClassification.CZ),
												Projections.Constant(CounterpartyCompositeClassification.New))))))))));
		}

		private void ShowInfo()
		{
			var info = "Маркетинговый отчет показывает динамику активной клиентской базы (АКБ) и ключевые маркетинговые метрики.\r\n\r\n" +
				"Период по умолчанию — последние 3 месяца.\r\n" +
				"Группировка: все данные, категория ABC_XYZ или автор заказа.\r\n" +
				"Статусы заказов по умолчанию совпадают с отчетом по оборачиваемости с динамикой.\r\n\r\n" +
				"Результат можно выгрузить в Excel.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, info, "Информация");
		}

		public override void Dispose()
		{
			ReportGenerationCancelationTokenSource?.Dispose();
			base.Dispose();
		}
	}
}
