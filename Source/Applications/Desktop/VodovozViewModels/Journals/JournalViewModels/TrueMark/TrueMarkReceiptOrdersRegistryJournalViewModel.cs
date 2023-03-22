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
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.TrueMark;
using Vodovoz.ViewModels.Journals.FilterViewModels.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class TrueMarkReceiptOrdersRegistryJournalViewModel : JournalViewModelBase
	{
		private readonly TrueMarkReceiptOrderJournalFilterViewModel _filter;
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval;

		public TrueMarkReceiptOrdersRegistryJournalViewModel(
			TrueMarkReceiptOrderJournalFilterViewModel filter,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			TrueMarkCodesPool trueMarkCodesPool,
			ICashReceiptRepository cashReceiptRepository,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_autoRefreshInterval = 30;

			Title = "Журнал кодов честного знака";
			Filter = filter;

			var levelDataLoader = new HierarchicalQueryLoader<CashReceipt, CashReceiptJournalNode>(unitOfWorkFactory);

			levelDataLoader.SetLevelingModel(GetQuery)
				.AddNextLevelSource(GetDetails);
			levelDataLoader.SetCountFunction(GetCount);

			RecuresiveConfig = levelDataLoader.TreeConfig;

			var threadDataLoader = new ThreadDataLoader<CashReceiptJournalNode>(unitOfWorkFactory);
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
				var codeErrorsReceiptCount = _cashReceiptRepository.GetCodeErrorsReceiptCount(UoW);
				return $"Заказов с ошибками кодов: {codeErrorsReceiptCount} | Кодов в пуле: {poolCount}, бракованных: {defectivePoolCount} | {autorefreshInfo} | {base.FooterInfo}";
			}
		}

		public override JournalSelectionMode SelectionMode => JournalSelectionMode.Single;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateAutorefreshAction();
		}

		#region Queries

		private IQueryOver<CashReceipt> GetQuery(IUnitOfWork uow)
		{
			CashReceiptJournalNode resultAlias = null;
			CashReceipt cashReceiptAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Employee driverAlias = null;

			var query = uow.Session.QueryOver(() => cashReceiptAlias);
			query.JoinEntityQueryOver(() => routeListItemAlias, Restrictions.Where(() => cashReceiptAlias.Order.Id == routeListItemAlias.Order.Id), JoinType.LeftOuterJoin);
			query.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias);
			query.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias);

			if(_filter.Status.HasValue)
			{
				query.Where(Restrictions.Eq(Projections.Property(() => cashReceiptAlias.Status), _filter.Status.Value));
			}

			if(_filter.StartDate.HasValue)
			{
				var startDate = _filter.StartDate.Value;
				query.Where(
					Restrictions.Ge(
						Projections.SqlFunction("DATE",
							NHibernateUtil.Date,
							Projections.Property(() => cashReceiptAlias.CreateDate)),
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
							Projections.Property(() => cashReceiptAlias.CreateDate)),
						endDate
					)
				);
			}

			if(_filter.HasUnscannedReason)
			{
				query.Where(Restrictions.Eq(Projections.SqlFunction("IS_NULL_OR_WHITESPACE", NHibernateUtil.Boolean, Projections.Property(() => cashReceiptAlias.UnscannedCodesReason)), false));
			}

			query.Where(
				GetSearchCriterion(
					() => cashReceiptAlias.Id,
					() => cashReceiptAlias.Order.Id,
					() => cashReceiptAlias.UnscannedCodesReason
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => cashReceiptAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(Projections.Constant(CashReceiptNodeType.Receipt)).WithAlias(() => resultAlias.NodeType)
				.Select(Projections.Property(() => cashReceiptAlias.CreateDate)).WithAlias(() => resultAlias.Time)
				.Select(Projections.Property(() => cashReceiptAlias.Order.Id)).WithAlias(() => resultAlias.OrderAndItemId)
				.Select(Projections.Property(() => routeListAlias.Id)).WithAlias(() => resultAlias.RouteListId)
				.Select(Projections.Property(() => driverAlias.Name)).WithAlias(() => resultAlias.DriverName)
				.Select(Projections.Property(() => driverAlias.LastName)).WithAlias(() => resultAlias.DriverLastName)
				.Select(Projections.Property(() => driverAlias.Patronymic)).WithAlias(() => resultAlias.DriverPatronimyc)
				.Select(Projections.Property(() => cashReceiptAlias.Status)).WithAlias(() => resultAlias.ReceiptStatus)
				.Select(Projections.Property(() => cashReceiptAlias.UnscannedCodesReason)).WithAlias(() => resultAlias.UnscannedReason)
				.Select(Projections.Property(() => cashReceiptAlias.ErrorDescription)).WithAlias(() => resultAlias.ErrorDescription)
			)
			.OrderByAlias(() => cashReceiptAlias.Id).Desc()
			.TransformUsing(Transformers.AliasToBean<CashReceiptJournalNode>());

			return query;
		}

		private IList<CashReceiptJournalNode> GetDetails(IEnumerable<CashReceiptJournalNode> parentNodes)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				CashReceiptProductCode productCodeAlias = null;
				TrueMarkWaterIdentificationCode sourceCodeAlias = null;
				TrueMarkWaterIdentificationCode resultCodeAlias = null;
				CashReceiptJournalNode resultAlias = null;

				var query = uow.Session.QueryOver(() => productCodeAlias)
					.Left.JoinAlias(() => productCodeAlias.SourceCode, () => sourceCodeAlias)
					.Left.JoinAlias(() => productCodeAlias.ResultCode, () => resultCodeAlias)
					.Where(Restrictions.In(Projections.Property(() => productCodeAlias.CashReceipt.Id), parentNodes.Select(x => x.Id).ToArray()));

				query.SelectList(list => list
					.SelectGroup(() => productCodeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(Projections.Constant(CashReceiptNodeType.Code)).WithAlias(() => resultAlias.NodeType)
					.Select(() => productCodeAlias.CashReceipt.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => productCodeAlias.OrderItem.Id).WithAlias(() => resultAlias.OrderAndItemId)
					.Select(() => productCodeAlias.IsDefectiveSourceCode).WithAlias(() => resultAlias.IsDefectiveCode)
					.Select(() => productCodeAlias.IsDuplicateSourceCode).WithAlias(() => resultAlias.IsDuplicateCode)
					.Select(() => sourceCodeAlias.GTIN).WithAlias(() => resultAlias.SourceGtin)
					.Select(() => sourceCodeAlias.SerialNumber).WithAlias(() => resultAlias.SourceSerialnumber)
					.Select(() => resultCodeAlias.GTIN).WithAlias(() => resultAlias.ResultGtin)
					.Select(() => resultCodeAlias.SerialNumber).WithAlias(() => resultAlias.ResultSerialnumber)
				)
				.TransformUsing(Transformers.AliasToBean<CashReceiptJournalNode>());

				return query.List<CashReceiptJournalNode>();
			}
		}

		#endregion Queries

		private int GetCount(IUnitOfWork uow)
		{
			var query = GetQuery(uow);
			var count = query.List<CashReceiptJournalNode>().Count();
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
