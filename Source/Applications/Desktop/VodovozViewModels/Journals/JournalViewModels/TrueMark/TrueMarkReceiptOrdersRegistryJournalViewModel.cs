using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Criterion;
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
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Models.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class TrueMarkReceiptOrdersRegistryJournalViewModel : JournalViewModelBase
	{
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval;

		public TrueMarkReceiptOrdersRegistryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			TrueMarkCodesPool trueMarkCodesPool,
			ITrueMarkRepository trueMarkRepository,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_autoRefreshInterval = 30;

			Title = "Реестр заказов для чека";

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
			TrueMarkCashReceiptOrder trueMarkOrderAlias = null;
			TrueMarkReceiptOrderNode resultAlias = null;

			var query = uow.Session.QueryOver(() => trueMarkOrderAlias);

			query.Where(
				GetSearchCriterion(
					() => trueMarkOrderAlias.Id,
					() => trueMarkOrderAlias.Order.Id
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => trueMarkOrderAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(Projections.Constant(TrueMarkOrderNodeType.Order)).WithAlias(() => resultAlias.NodeType)
				.Select(Projections.Property(() => trueMarkOrderAlias.Date)).WithAlias(() => resultAlias.Time)
				.Select(Projections.Property(() => trueMarkOrderAlias.Order.Id)).WithAlias(() => resultAlias.OrderAndItemId)
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
