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
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.TrueMark;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Roboats
{
	public class TrueMarkReceiptOrdersRegistryJournalViewModel : JournalViewModelBase
	{
		private readonly TrueMarkCodesPool _trueMarkCodesPool;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private Timer _autoRefreshTimer;
		private int _autoRefreshInterval;

		public TrueMarkReceiptOrdersRegistryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			TrueMarkCodesPool trueMarkCodesPool,
			ICashReceiptRepository cashReceiptRepository,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_autoRefreshInterval = 30;

			Title = "Реестр чеков";

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
				var codeErrorsOrdersCount = _cashReceiptRepository.GetCodeErrorsReceiptCount(UoW);
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

		private IQueryOver<CashReceipt> GetQuery(IUnitOfWork uow)
		{
			CashReceipt cashReceiptAlias = null;
			CashReceiptJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => cashReceiptAlias);

			query.Where(
				GetSearchCriterion(
					() => cashReceiptAlias.Id,
					() => cashReceiptAlias.Order.Id
				)
			);

			query.SelectList(list => list
				.SelectGroup(() => cashReceiptAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(Projections.Constant(CashReceiptNodeType.Order)).WithAlias(() => resultAlias.NodeType)
				.Select(Projections.Property(() => cashReceiptAlias.CreateDate)).WithAlias(() => resultAlias.Time)
				.Select(Projections.Property(() => cashReceiptAlias.Order.Id)).WithAlias(() => resultAlias.OrderAndItemId)
				.Select(Projections.Property(() => cashReceiptAlias.Status)).WithAlias(() => resultAlias.OrderStatus)
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
				CashReceiptProductCode trueMarkReceiptCodeAlias = null;
				TrueMarkWaterIdentificationCode trueMarkSourceCodeAlias = null;
				TrueMarkWaterIdentificationCode trueMarkResultCodeAlias = null;
				CashReceiptJournalNode resultAlias = null;

				var query = uow.Session.QueryOver(() => trueMarkReceiptCodeAlias)
					.Left.JoinAlias(() => trueMarkReceiptCodeAlias.SourceCode, () => trueMarkSourceCodeAlias)
					.Left.JoinAlias(() => trueMarkReceiptCodeAlias.ResultCode, () => trueMarkResultCodeAlias)
					.Where(Restrictions.In(Projections.Property(() => trueMarkReceiptCodeAlias.CashReceipt.Id), parentNodes.Select(x => x.Id).ToArray()));

				query.SelectList(list => list
					.SelectGroup(() => trueMarkReceiptCodeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(Projections.Constant(CashReceiptNodeType.Code)).WithAlias(() => resultAlias.NodeType)
					.Select(() => trueMarkReceiptCodeAlias.CashReceipt.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => trueMarkReceiptCodeAlias.OrderItem.Id).WithAlias(() => resultAlias.OrderAndItemId)
					.Select(() => trueMarkReceiptCodeAlias.IsDefectiveSourceCode).WithAlias(() => resultAlias.IsDefectiveCode)
					.Select(() => trueMarkReceiptCodeAlias.IsDuplicateSourceCode).WithAlias(() => resultAlias.IsDuplicateCode)
					.Select(() => trueMarkSourceCodeAlias.GTIN).WithAlias(() => resultAlias.SourceGtin)
					.Select(() => trueMarkSourceCodeAlias.SerialNumber).WithAlias(() => resultAlias.SourceSerialnumber)
					.Select(() => trueMarkResultCodeAlias.GTIN).WithAlias(() => resultAlias.ResultGtin)
					.Select(() => trueMarkResultCodeAlias.SerialNumber).WithAlias(() => resultAlias.ResultSerialnumber)
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
