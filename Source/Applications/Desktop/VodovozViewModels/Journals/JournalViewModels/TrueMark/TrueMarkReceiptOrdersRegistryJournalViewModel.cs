using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.DataLoader.Hierarchy;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Models.TrueMark;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class TrueMarkReceiptOrdersRegistryJournalViewModel : JournalViewModelBase
	{
		private readonly TrueMarkReceiptOrderJournalFilterViewModel _filter;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval;

		public TrueMarkReceiptOrdersRegistryJournalViewModel(
			TrueMarkReceiptOrderJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			TrueMarkCodesPool trueMarkCodesPool,
			ITrueMarkRepository trueMarkRepository,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_autoRefreshInterval = 30;

			Title = "Журнал кодов честного знака";
			Filter = filter;

			var levelDataLoader = new HierarchicalQueryLoader<TrueMarkCashReceiptOrder, TrueMarkReceiptOrderNode>(unitOfWorkFactory);

			levelDataLoader.SetLevelingModel(GetQuery)
				.AddNextLevelSource(GetDetails);
			levelDataLoader.SetCountFunction(GetCount);

			RecuresiveConfig = levelDataLoader.TreeConfig;

			var threadDataLoader = new ThreadDataLoader<TrueMarkReceiptOrderNode>(unitOfWorkFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.QueryLoaders.Add(levelDataLoader);
			DataLoader = threadDataLoader;

			CreateNodeActions();
			StartAutoRefresh();
		}

		private IJournalFilter filter;
		public IJournalFilter Filter
		{
			get => filter;
			protected set
			{
				if(filter != null)
					filter.OnFiltered -= FilterViewModel_OnFiltered;
				filter = value;
				if(filter != null)
					filter.OnFiltered += FilterViewModel_OnFiltered;
			}
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public override string FooterInfo
		{
			get
			{
				var poolCount = _trueMarkCodesPool.GetTotalCount();
				var defectivePoolCount = _trueMarkCodesPool.GetDefectiveTotalCount();
				var autorefreshInfo = GetAutoRefreshInfo();
				var codeErrorsOrdersCount = _trueMarkRepository.GetCodeErrorsOrdersCount(UoW);
				return $"Заказов с ошибками кодов: {codeErrorsOrdersCount} | Кодов в пуле: {poolCount}, бракованных: {defectivePoolCount} | {autorefreshInfo} | {base.FooterInfo}";
			}
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateAutorefreshAction();
		}

		#region Queries

		private IQueryOver<TrueMarkCashReceiptOrder> GetQuery(IUnitOfWork uow)
		{
			TrueMarkReceiptOrderNode resultAlias = null;
			TrueMarkCashReceiptOrder trueMarkOrderAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Employee driverAlias = null;

			var query = uow.Session.QueryOver(() => trueMarkOrderAlias);
			query.JoinEntityQueryOver(() => routeListItemAlias, Restrictions.Where(() => trueMarkOrderAlias.Order.Id == routeListItemAlias.Order.Id), JoinType.LeftOuterJoin);
			query.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias);
			query.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias);

			if(_filter.Status.HasValue)
			{
				query.Where(Restrictions.Eq(Projections.Property(() => trueMarkOrderAlias.Status), _filter.Status.Value));
			}

			if(_filter.StartDate.HasValue)
			{
				var startDate = _filter.StartDate.Value;
				query.Where(
					Restrictions.Ge(
						Projections.SqlFunction("DATE",
							NHibernateUtil.Date,
							Projections.Property(() => trueMarkOrderAlias.Date)),
						startDate
					)
				);
			}

			if(_filter.EndDate.HasValue)
			{
				var endDate = _filter.EndDate.Value;
				query.Where(
					Restrictions.Le(
						Projections.SqlFunction("DATE",
							NHibernateUtil.Date,
							Projections.Property(() => trueMarkOrderAlias.Date)),
						endDate
					)
				);
			}

			if(_filter.HasUnscannedReason)
			{
				query.Where(Restrictions.Eq(Projections.SqlFunction("IS_NULL_OR_WHITESPACE", NHibernateUtil.Boolean, Projections.Property(() => trueMarkOrderAlias.UnscannedCodesReason)), false));
			}

			query.Where(
				GetSearchCriterion(
					() => trueMarkOrderAlias.Id,
					() => trueMarkOrderAlias.Order.Id,
					() => trueMarkOrderAlias.UnscannedCodesReason
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => trueMarkOrderAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(Projections.Constant(TrueMarkOrderNodeType.Order)).WithAlias(() => resultAlias.NodeType)
				.Select(Projections.Property(() => trueMarkOrderAlias.Date)).WithAlias(() => resultAlias.Time)
				.Select(Projections.Property(() => trueMarkOrderAlias.Order.Id)).WithAlias(() => resultAlias.OrderAndItemId)
				.Select(Projections.Property(() => routeListAlias.Id)).WithAlias(() => resultAlias.RouteListId)
				.Select(Projections.Property(() => driverAlias.Name)).WithAlias(() => resultAlias.DriverName)
				.Select(Projections.Property(() => driverAlias.LastName)).WithAlias(() => resultAlias.DriverLastName)
				.Select(Projections.Property(() => driverAlias.Patronymic)).WithAlias(() => resultAlias.DriverPatronimyc)
				.Select(Projections.Property(() => trueMarkOrderAlias.Status)).WithAlias(() => resultAlias.OrderStatus)
				.Select(Projections.Property(() => trueMarkOrderAlias.UnscannedCodesReason)).WithAlias(() => resultAlias.UnscannedReason)
				.Select(Projections.Property(() => trueMarkOrderAlias.ErrorDescription)).WithAlias(() => resultAlias.ErrorDescription)
				.Select(Projections.Property(() => trueMarkOrderAlias.CashReceipt.Id)).WithAlias(() => resultAlias.ReceiptId)
			)
			.OrderByAlias(() => trueMarkOrderAlias.Id).Desc()
			.TransformUsing(Transformers.AliasToBean<TrueMarkReceiptOrderNode>());

			return query;
		}

		private IList<TrueMarkReceiptOrderNode> GetDetails(IEnumerable<TrueMarkReceiptOrderNode> parentNodes)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				TrueMarkCashReceiptProductCode trueMarkReceiptCodeAlias = null;
				TrueMarkWaterIdentificationCode trueMarkSourceCodeAlias = null;
				TrueMarkWaterIdentificationCode trueMarkResultCodeAlias = null;
				TrueMarkReceiptOrderNode resultAlias = null;

				var query = uow.Session.QueryOver(() => trueMarkReceiptCodeAlias)
					.Left.JoinAlias(() => trueMarkReceiptCodeAlias.SourceCode, () => trueMarkSourceCodeAlias)
					.Left.JoinAlias(() => trueMarkReceiptCodeAlias.ResultCode, () => trueMarkResultCodeAlias)
					.Where(Restrictions.In(Projections.Property(() => trueMarkReceiptCodeAlias.TrueMarkCashReceiptOrder.Id), parentNodes.Select(x => x.Id).ToArray()));

				query.SelectList(list => list
					.SelectGroup(() => trueMarkReceiptCodeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(Projections.Constant(TrueMarkOrderNodeType.Code)).WithAlias(() => resultAlias.NodeType)
					.Select(() => trueMarkReceiptCodeAlias.TrueMarkCashReceiptOrder.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => trueMarkReceiptCodeAlias.OrderItem.Id).WithAlias(() => resultAlias.OrderAndItemId)
					.Select(() => trueMarkReceiptCodeAlias.IsDefectiveSourceCode).WithAlias(() => resultAlias.IsDefectiveCode)
					.Select(() => trueMarkReceiptCodeAlias.IsDuplicateSourceCode).WithAlias(() => resultAlias.IsDuplicateCode)
					.Select(() => trueMarkSourceCodeAlias.GTIN).WithAlias(() => resultAlias.SourceGtin)
					.Select(() => trueMarkSourceCodeAlias.SerialNumber).WithAlias(() => resultAlias.SourceSerialnumber)
					.Select(() => trueMarkResultCodeAlias.GTIN).WithAlias(() => resultAlias.ResultGtin)
					.Select(() => trueMarkResultCodeAlias.SerialNumber).WithAlias(() => resultAlias.ResultSerialnumber)
				)
				.TransformUsing(Transformers.AliasToBean<TrueMarkReceiptOrderNode>());

				return query.List<TrueMarkReceiptOrderNode>();
			}
		}

		#endregion Queries

		private int GetCount(IUnitOfWork uow)
		{
			var query = GetQuery(uow);
			var count = query.List<TrueMarkReceiptOrderNode>().Count();
			return count;
		}

		#region Autorefresh

		private bool autoRefreshEnabled => _autoRefreshTimer != null && _autoRefreshTimer.Enabled;

		private void StartAutoRefresh()
		{
			if(autoRefreshEnabled)
			{
				return;
			}
			_autoRefreshTimer = new Timer(_autoRefreshInterval * 1000);
			_autoRefreshTimer.Elapsed += (s, e) => Refresh();
			_autoRefreshTimer.Start();
		}

		private void StopAutoRefresh()
		{
			_autoRefreshTimer?.Stop();
			_autoRefreshTimer = null;
		}

		private void SwitchAutoRefresh()
		{
			if(autoRefreshEnabled)
			{
				StopAutoRefresh();
			}
			else
			{
				StartAutoRefresh();
			}
			OnPropertyChanged(nameof(FooterInfo));
		}

		private string GetAutoRefreshInfo()
		{
			if(autoRefreshEnabled)
			{
				return $"Автообновление каждые {_autoRefreshInterval} сек.";
			}
			else
			{
				return $"Автообновление выключено";
			}
		}

		private void CreateAutorefreshAction()
		{
			var switchAutorefreshAction = new JournalAction("Вкл/Выкл автообновление",
				(selected) => true,
				(selected) => true,
				(selected) => SwitchAutoRefresh()
			);
			NodeActionsList.Add(switchAutorefreshAction);
		}

		#endregion Autorefresh
	}
}
