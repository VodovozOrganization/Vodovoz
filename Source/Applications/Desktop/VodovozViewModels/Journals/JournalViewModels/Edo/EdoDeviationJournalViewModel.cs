using Core.Infrastructure;
using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.DataLoader.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Edo;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.TrueMark;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	/// <summary>
	/// Журнал отклонений и проблем документооборота ЭДО.
	/// Первый уровень — заказ, второй — зарегистрированные по нему отклонения
	/// и проблемы, включая отклонения и проблемы задач трансфера,
	/// которые переносят коды этого заказа
	/// </summary>
	public class EdoDeviationJournalViewModel : JournalViewModelBase
	{
		private readonly EdoDeviationFilterViewModel _filterViewModel;
		private readonly IClipboard _clipboard;
		private readonly IGtkTabsOpener _gtkTabsOpener;

		public EdoDeviationJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			EdoDeviationFilterViewModel filterViewModel,
			IInteractiveService interactiveService,
			IClipboard clipboard,
			IGtkTabsOpener gtkTabsOpener,
			INavigationManager navigation
		) : base(uowFactory, interactiveService, navigation)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			Title = "Журнал отклонений документооборота ЭДО";

			_filterViewModel.IsShow = true;
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;

			var levelQueryLoader =
				new HierarchicalQueryLoader<Order, EdoDeviationJournalNode>(uowFactory, GetOrdersCount);

			levelQueryLoader.SetLevelingModel(GetOrdersQuery)
				.AddNextLevelSource(GetDeviations)
				.AddNextLevelSource(GetProblems);

			RecuresiveConfig = levelQueryLoader.TreeConfig;

			var threadDataLoader = new ThreadDataLoader<EdoDeviationJournalNode>(uowFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.QueryLoaders.Add(levelQueryLoader);
			DataLoader = threadDataLoader;

			CreateNodeActions();
			CreatePopupActions();
		}

		public IRecursiveConfig RecuresiveConfig { get; }

		public override IJournalFilterViewModel JournalFilter
		{
			get => _filterViewModel;
			protected set => throw new NotSupportedException("Установка фильтра выполняется через конструктор");
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		#region Первый уровень — заказы

		private IQueryOver<Order> GetOrdersQuery(IUnitOfWork uow)
		{
			Order orderAlias = null;
			CounterpartyEntity counterpartyAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias);

			ApplyOrderRestrictions(query, orderAlias, counterpartyAlias);

			query.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Order))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.DeliveryDate)
					.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
				)
				.OrderBy(() => orderAlias.Id).Desc
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>());

			return query;
		}

		private int GetOrdersCount(IUnitOfWork uow)
		{
			Order orderAlias = null;
			CounterpartyEntity counterpartyAlias = null;

			var query = uow.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias);

			ApplyOrderRestrictions(query, orderAlias, counterpartyAlias);

			return query.Select(Projections.RowCount()).SingleOrDefault<int>();
		}

		private void ApplyOrderRestrictions(
			IQueryOver<Order, Order> query,
			Order orderAlias,
			CounterpartyEntity counterpartyAlias)
		{
			if(_filterViewModel.DeliveryDateFrom.HasValue)
			{
				query.Where(() => orderAlias.DeliveryDate >= _filterViewModel.DeliveryDateFrom.Value);
			}

			if(_filterViewModel.DeliveryDateTo.HasValue)
			{
				query.Where(() => orderAlias.DeliveryDate <= _filterViewModel.DeliveryDateTo.Value);
			}

			if(_filterViewModel.OrderId.HasValue)
			{
				query.Where(() => orderAlias.Id == _filterViewModel.OrderId.Value);
			}

			query.Where(GetHasMatchingRowsRestriction(orderAlias));

			query.Where(GetSearchCriterion(
				() => orderAlias.Id,
				() => counterpartyAlias.Name
			));
		}

		private ICriterion GetHasMatchingRowsRestriction(Order orderAlias)
		{
			var disjunction = Restrictions.Disjunction();

			if(AreDeviationsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetOrderTaskDeviationIdsSubquery(orderAlias)));
				disjunction.Add(Subqueries.WhereExists(GetTransferDeviationIdsSubquery(orderAlias)));

				if(AreRequestDeviationsRequested())
				{
					disjunction.Add(Subqueries.WhereExists(GetRequestDeviationIdsSubquery(orderAlias)));
				}
			}

			if(AreProblemsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetOrderTaskProblemIdsSubquery(orderAlias)));
				disjunction.Add(Subqueries.WhereExists(GetTransferProblemIdsSubquery(orderAlias)));
			}

			return disjunction;
		}

		private bool AreDeviationsRequested() =>
			_filterViewModel.RowType != EdoDeviationJournalNodeType.Problem
			&& string.IsNullOrWhiteSpace(_filterViewModel.ProblemSourceName);

		private bool AreProblemsRequested() =>
			_filterViewModel.RowType != EdoDeviationJournalNodeType.Deviation
			&& !_filterViewModel.DeviationType.HasValue;

		private bool AreRequestDeviationsRequested() =>
			!_filterViewModel.TaskId.HasValue && !_filterViewModel.EdoTaskStatus.HasValue;

		#endregion Первый уровень — заказы

		#region Подзапросы отбора заказов

		private QueryOver<EdoTaskDeviation> GetOrderTaskDeviationIdsSubquery(Order orderAlias)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoTask taskAlias = null;
			EdoDeviationSource sourceAlias = null;
			FormalEdoRequest requestAlias = null;

			var subquery = QueryOver.Of(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoTask, () => taskAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.JoinEntityAlias(() => requestAlias, () => requestAlias.Task.Id == taskAlias.Id)
				.Where(() => requestAlias.Order.Id == orderAlias.Id);

			ApplyDeviationRestrictions(subquery, deviationAlias, sourceAlias);
			ApplyTaskRestrictions(subquery, taskAlias);

			return subquery.Select(Projections.Property(() => deviationAlias.Id));
		}

		private QueryOver<EdoTaskDeviation> GetRequestDeviationIdsSubquery(Order orderAlias)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoDeviationSource sourceAlias = null;
			FormalEdoRequest requestAlias = null;

			var subquery = QueryOver.Of(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoRequest, () => requestAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.Where(() => deviationAlias.EdoTask == null)
				.Where(() => requestAlias.Order.Id == orderAlias.Id);

			ApplyDeviationRestrictions(subquery, deviationAlias, sourceAlias);

			return subquery.Select(Projections.Property(() => deviationAlias.Id));
		}

		private QueryOver<EdoTaskDeviation> GetTransferDeviationIdsSubquery(Order orderAlias)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoTask taskAlias = null;
			EdoDeviationSource sourceAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;

			var subquery = QueryOver.Of(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoTask, () => taskAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.JoinEntityAlias(() => transferRequestAlias,
					() => transferRequestAlias.TransferEdoTask.Id == taskAlias.Id)
				.JoinAlias(() => transferRequestAlias.Iteration, () => iterationAlias)
				.JoinEntityAlias(() => requestAlias,
					() => requestAlias.Task.Id == iterationAlias.OrderEdoTask.Id)
				.Where(() => requestAlias.Order.Id == orderAlias.Id);

			ApplyDeviationRestrictions(subquery, deviationAlias, sourceAlias);
			ApplyTaskRestrictions(subquery, taskAlias);

			return subquery.Select(Projections.Property(() => deviationAlias.Id));
		}

		private QueryOver<EdoTaskProblem> GetOrderTaskProblemIdsSubquery(Order orderAlias)
		{
			EdoTaskProblem problemAlias = null;
			EdoTask taskAlias = null;
			FormalEdoRequest requestAlias = null;

			var subquery = QueryOver.Of(() => problemAlias)
				.JoinAlias(() => problemAlias.EdoTask, () => taskAlias)
				.JoinEntityAlias(() => requestAlias, () => requestAlias.Task.Id == taskAlias.Id)
				.Where(() => requestAlias.Order.Id == orderAlias.Id);

			ApplyProblemRestrictions(subquery, problemAlias);
			ApplyTaskRestrictions(subquery, taskAlias);

			return subquery.Select(Projections.Property(() => problemAlias.Id));
		}

		private QueryOver<EdoTaskProblem> GetTransferProblemIdsSubquery(Order orderAlias)
		{
			EdoTaskProblem problemAlias = null;
			EdoTask taskAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;

			var subquery = QueryOver.Of(() => problemAlias)
				.JoinAlias(() => problemAlias.EdoTask, () => taskAlias)
				.JoinEntityAlias(() => transferRequestAlias,
					() => transferRequestAlias.TransferEdoTask.Id == taskAlias.Id)
				.JoinAlias(() => transferRequestAlias.Iteration, () => iterationAlias)
				.JoinEntityAlias(() => requestAlias,
					() => requestAlias.Task.Id == iterationAlias.OrderEdoTask.Id)
				.Where(() => requestAlias.Order.Id == orderAlias.Id);

			ApplyProblemRestrictions(subquery, problemAlias);
			ApplyTaskRestrictions(subquery, taskAlias);

			return subquery.Select(Projections.Property(() => problemAlias.Id));
		}

		#endregion Подзапросы отбора заказов

		#region Общие условия фильтра

		private void ApplyDeviationRestrictions<TRoot>(
			IQueryOver<TRoot, TRoot> query,
			EdoTaskDeviation deviationAlias,
			EdoDeviationSource sourceAlias)
		{
			if(_filterViewModel.State.HasValue)
			{
				query.Where(() => deviationAlias.State == _filterViewModel.State.Value);
			}

			if(_filterViewModel.DeviationType.HasValue)
			{
				query.Where(() => sourceAlias.DeviationType == _filterViewModel.DeviationType.Value);
			}
		}

		private void ApplyProblemRestrictions<TRoot>(
			IQueryOver<TRoot, TRoot> query,
			EdoTaskProblem problemAlias)
		{
			if(_filterViewModel.State.HasValue)
			{
				query.Where(() => problemAlias.State == _filterViewModel.State.Value);
			}

			if(!string.IsNullOrWhiteSpace(_filterViewModel.ProblemSourceName))
			{
				query.Where(Restrictions.Like(
					Projections.Property(() => problemAlias.SourceName),
					_filterViewModel.ProblemSourceName,
					MatchMode.Anywhere));
			}
		}

		private void ApplyTaskRestrictions<TRoot>(IQueryOver<TRoot, TRoot> query, EdoTask taskAlias)
		{
			if(_filterViewModel.TaskId.HasValue)
			{
				query.Where(() => taskAlias.Id == _filterViewModel.TaskId.Value);
			}

			if(_filterViewModel.EdoTaskStatus.HasValue)
			{
				query.Where(() => taskAlias.Status == _filterViewModel.EdoTaskStatus.Value);
			}
		}

		#endregion Общие условия фильтра

		#region Второй уровень — отклонения и проблемы

		private IList<EdoDeviationJournalNode> GetDeviations(IEnumerable<EdoDeviationJournalNode> parentNodes)
		{
			var orderIds = parentNodes.Select(x => x.Id).ToArray();

			if(!orderIds.Any() || !AreDeviationsRequested())
			{
				return new List<EdoDeviationJournalNode>();
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var nodes = new List<EdoDeviationJournalNode>();

				nodes.AddRange(GetOrderTaskDeviations(uow, orderIds));
				nodes.AddRange(GetTransferDeviations(uow, orderIds));

				if(AreRequestDeviationsRequested())
				{
					nodes.AddRange(GetRequestDeviations(uow, orderIds));
				}

				var distinctNodes = DistinctByRow(nodes);

				UpdateParentCounters(parentNodes, distinctNodes);

				return distinctNodes;
			}
		}

		private IList<EdoDeviationJournalNode> GetProblems(IEnumerable<EdoDeviationJournalNode> parentNodes)
		{
			var orderIds = parentNodes.Select(x => x.Id).ToArray();

			if(!orderIds.Any() || !AreProblemsRequested())
			{
				return new List<EdoDeviationJournalNode>();
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var nodes = new List<EdoDeviationJournalNode>();

				nodes.AddRange(GetOrderTaskProblems(uow, orderIds));
				nodes.AddRange(GetTransferProblems(uow, orderIds));

				var distinctNodes = DistinctByRow(nodes);

				UpdateParentCounters(parentNodes, distinctNodes);

				return distinctNodes;
			}
		}

		private IList<EdoDeviationJournalNode> GetOrderTaskDeviations(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoTask taskAlias = null;
			DocumentEdoTask documentTaskAlias = null;
			ReceiptEdoTask receiptTaskAlias = null;
			EdoDeviationSource sourceAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoTask, () => taskAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.JoinEntityAlias(() => requestAlias, () => requestAlias.Task.Id == taskAlias.Id)
				.JoinEntityAlias(() => documentTaskAlias,
					() => documentTaskAlias.Id == taskAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => receiptTaskAlias,
					() => receiptTaskAlias.Id == taskAlias.Id, JoinType.LeftOuterJoin)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds));

			ApplyDeviationRestrictions(query, deviationAlias, sourceAlias);
			ApplyTaskRestrictions(query, taskAlias);

			var nodes = query.SelectList(list => list
					.Select(() => deviationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Deviation))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => documentTaskAlias.Id).WithAlias(() => resultAlias.DocumentTaskId)
					.Select(() => receiptTaskAlias.Id).WithAlias(() => resultAlias.ReceiptTaskId)
					.Select(() => sourceAlias.DeviationType).WithAlias(() => resultAlias.DeviationType)
					.Select(() => sourceAlias.ErrorMessage).WithAlias(() => resultAlias.Recommendation)
					.Select(() => deviationAlias.Details).WithAlias(() => resultAlias.Description)
					.Select(() => deviationAlias.DetectedTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => deviationAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => deviationAlias.ResolveReason).WithAlias(() => resultAlias.ResolveReason)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();

			FillOrderTaskType(nodes);

			return nodes;
		}

		private IList<EdoDeviationJournalNode> GetRequestDeviations(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoDeviationSource sourceAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoRequest, () => requestAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.Where(() => deviationAlias.EdoTask == null)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds));

			ApplyDeviationRestrictions(query, deviationAlias, sourceAlias);

			return query.SelectList(list => list
					.Select(() => deviationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Deviation))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => sourceAlias.DeviationType).WithAlias(() => resultAlias.DeviationType)
					.Select(() => sourceAlias.ErrorMessage).WithAlias(() => resultAlias.Recommendation)
					.Select(() => deviationAlias.Details).WithAlias(() => resultAlias.Description)
					.Select(() => deviationAlias.DetectedTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => deviationAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => deviationAlias.ResolveReason).WithAlias(() => resultAlias.ResolveReason)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		private IList<EdoDeviationJournalNode> GetTransferDeviations(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoTask taskAlias = null;
			EdoDeviationSource sourceAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoTask, () => taskAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.JoinEntityAlias(() => transferRequestAlias,
					() => transferRequestAlias.TransferEdoTask.Id == taskAlias.Id)
				.JoinAlias(() => transferRequestAlias.Iteration, () => iterationAlias)
				.JoinEntityAlias(() => requestAlias,
					() => requestAlias.Task.Id == iterationAlias.OrderEdoTask.Id)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds));

			ApplyDeviationRestrictions(query, deviationAlias, sourceAlias);
			ApplyTaskRestrictions(query, taskAlias);

			return query.SelectList(list => list
					.Select(() => deviationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Deviation))
						.WithAlias(() => resultAlias.NodeType)
					.Select(Projections.Constant(EdoTaskType.Transfer)).WithAlias(() => resultAlias.TaskType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => sourceAlias.DeviationType).WithAlias(() => resultAlias.DeviationType)
					.Select(() => sourceAlias.ErrorMessage).WithAlias(() => resultAlias.Recommendation)
					.Select(() => deviationAlias.Details).WithAlias(() => resultAlias.Description)
					.Select(() => deviationAlias.DetectedTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => deviationAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => deviationAlias.ResolveReason).WithAlias(() => resultAlias.ResolveReason)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		private IList<EdoDeviationJournalNode> GetOrderTaskProblems(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskProblem problemAlias = null;
			ExceptionEdoTaskProblem exceptionProblemAlias = null;
			EdoTask taskAlias = null;
			DocumentEdoTask documentTaskAlias = null;
			ReceiptEdoTask receiptTaskAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoTaskProblemDescriptionSourceEntity descriptionSourceAlias = null;
			EdoTaskProblemCustomSourceEntity customSourceAlias = null;
			EdoTaskProblemValidatorSourceEntity validatorSourceAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => problemAlias)
				.JoinAlias(() => problemAlias.EdoTask, () => taskAlias)
				.JoinEntityAlias(() => requestAlias, () => requestAlias.Task.Id == taskAlias.Id)
				.JoinEntityAlias(() => documentTaskAlias,
					() => documentTaskAlias.Id == taskAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => receiptTaskAlias,
					() => receiptTaskAlias.Id == taskAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => exceptionProblemAlias,
					() => exceptionProblemAlias.Id == problemAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => descriptionSourceAlias,
					() => descriptionSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => customSourceAlias,
					() => customSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => validatorSourceAlias,
					() => validatorSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds));

			ApplyProblemRestrictions(query, problemAlias);
			ApplyTaskRestrictions(query, taskAlias);

			var nodes = query.SelectList(list => list
					.Select(() => problemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Problem))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => documentTaskAlias.Id).WithAlias(() => resultAlias.DocumentTaskId)
					.Select(() => receiptTaskAlias.Id).WithAlias(() => resultAlias.ReceiptTaskId)
					.Select(() => descriptionSourceAlias.Description)
						.WithAlias(() => resultAlias.ProblemSourceDescription)
					.Select(() => descriptionSourceAlias.Recommendation)
						.WithAlias(() => resultAlias.Recommendation)
					.Select(CustomProjections.Coalesce(
						NHibernateUtil.String,
						Projections.Property(() => exceptionProblemAlias.ExceptionMessage),
						Projections.Property(() => customSourceAlias.Message),
						Projections.Property(() => validatorSourceAlias.Message)))
						.WithAlias(() => resultAlias.Description)
					.Select(() => problemAlias.CreationTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => problemAlias.State).WithAlias(() => resultAlias.State)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();

			FillOrderTaskType(nodes);

			return nodes;
		}

		private IList<EdoDeviationJournalNode> GetTransferProblems(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskProblem problemAlias = null;
			ExceptionEdoTaskProblem exceptionProblemAlias = null;
			EdoTask taskAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoTaskProblemDescriptionSourceEntity descriptionSourceAlias = null;
			EdoTaskProblemCustomSourceEntity customSourceAlias = null;
			EdoTaskProblemValidatorSourceEntity validatorSourceAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => problemAlias)
				.JoinAlias(() => problemAlias.EdoTask, () => taskAlias)
				.JoinEntityAlias(() => transferRequestAlias,
					() => transferRequestAlias.TransferEdoTask.Id == taskAlias.Id)
				.JoinAlias(() => transferRequestAlias.Iteration, () => iterationAlias)
				.JoinEntityAlias(() => requestAlias,
					() => requestAlias.Task.Id == iterationAlias.OrderEdoTask.Id)
				.JoinEntityAlias(() => exceptionProblemAlias,
					() => exceptionProblemAlias.Id == problemAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => descriptionSourceAlias,
					() => descriptionSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => customSourceAlias,
					() => customSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => validatorSourceAlias,
					() => validatorSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds));

			ApplyProblemRestrictions(query, problemAlias);
			ApplyTaskRestrictions(query, taskAlias);

			return query.SelectList(list => list
					.Select(() => problemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Problem))
						.WithAlias(() => resultAlias.NodeType)
					.Select(Projections.Constant(EdoTaskType.Transfer)).WithAlias(() => resultAlias.TaskType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => descriptionSourceAlias.Description)
						.WithAlias(() => resultAlias.ProblemSourceDescription)
					.Select(() => descriptionSourceAlias.Recommendation)
						.WithAlias(() => resultAlias.Recommendation)
					.Select(CustomProjections.Coalesce(
						NHibernateUtil.String,
						Projections.Property(() => exceptionProblemAlias.ExceptionMessage),
						Projections.Property(() => customSourceAlias.Message),
						Projections.Property(() => validatorSourceAlias.Message)))
						.WithAlias(() => resultAlias.Description)
					.Select(() => problemAlias.CreationTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => problemAlias.State).WithAlias(() => resultAlias.State)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		private static void FillOrderTaskType(IEnumerable<EdoDeviationJournalNode> nodes)
		{
			foreach(var node in nodes)
			{
				if(node.DocumentTaskId.HasValue)
				{
					node.TaskType = EdoTaskType.Document;
				}
				else if(node.ReceiptTaskId.HasValue)
				{
					node.TaskType = EdoTaskType.Receipt;
				}
			}
		}

		private static IList<EdoDeviationJournalNode> DistinctByRow(
			IEnumerable<EdoDeviationJournalNode> nodes) =>
			nodes
				.GroupBy(x => new { x.NodeType, x.Id, x.ParentId })
				.Select(x => x.First())
				.ToList();

		private static void UpdateParentCounters(
			IEnumerable<EdoDeviationJournalNode> parentNodes,
			IEnumerable<EdoDeviationJournalNode> childNodes)
		{
			var parentsById = parentNodes.ToDictionary(x => x.Id);

			foreach(var child in childNodes)
			{
				if(!child.ParentId.HasValue || !parentsById.TryGetValue(child.ParentId.Value, out var parent))
				{
					continue;
				}

				if(child.NodeType == EdoDeviationJournalNodeType.Deviation)
				{
					parent.DeviationsCount++;
				}
				else
				{
					parent.ProblemsCount++;
				}
			}
		}

		#endregion Второй уровень — отклонения и проблемы

		#region Действия по правой кнопке

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();

			CreateOpenOrderAction();
			CreateOpenEdoDialogAction();
			CreateOpenOrderCodesAction();
			CreateOpenCounterpartyAction();
			CreateCopyOrderIdToClipboardAction();
			CreateCopyTaskIdToClipboardAction();
		}

		private void CreateOpenOrderAction()
		{
			PopupActionsList.Add(new JournalAction(
				"Открыть заказ",
				selected => selected.Count() == 1,
				selected => true,
				selected =>
				{
					var node = selected.Cast<EdoDeviationJournalNode>().First();
					_gtkTabsOpener.OpenOrderDlgFromViewModelByNavigator(this, node.OrderId);
				}
			));
		}

		private void CreateOpenEdoDialogAction()
		{
			PopupActionsList.Add(new JournalAction(
				"Открыть диалог ЭДО",
				selected => selected.Count() == 1,
				selected => true,
				selected =>
				{
					var node = selected.Cast<EdoDeviationJournalNode>().First();
					NavigationManager.OpenViewModel<EdoViewModel, int>(
						null, node.OrderId, OpenPageOptions.IgnoreHash);
				}
			));
		}

		private void CreateOpenOrderCodesAction()
		{
			PopupActionsList.Add(new JournalAction(
				"Просмотр кодов по заказу",
				selected => selected.Count() == 1,
				selected => true,
				selected =>
				{
					var node = selected.Cast<EdoDeviationJournalNode>().First();
					NavigationManager.OpenViewModel<OrderCodesDialogViewModel, int>(
						null, node.OrderId, OpenPageOptions.IgnoreHash);
				}
			));
		}

		private void CreateOpenCounterpartyAction()
		{
			PopupActionsList.Add(new JournalAction(
				"Открыть контрагента",
				selected => selected.Count() == 1
					&& GetCounterpartyId(selected.Cast<EdoDeviationJournalNode>().First()).HasValue,
				selected => true,
				selected =>
				{
					var node = selected.Cast<EdoDeviationJournalNode>().First();
					var counterpartyId = GetCounterpartyId(node);

					if(counterpartyId.HasValue)
					{
						_gtkTabsOpener.OpenCounterpartyDlg(this, counterpartyId.Value);
					}
				}
			));
		}

		private void CreateCopyOrderIdToClipboardAction()
		{
			PopupActionsList.Add(new JournalAction(
				"Скопировать номер заказа",
				selected => selected.Any(),
				selected => true,
				selected =>
				{
					var orderIds = selected.Cast<EdoDeviationJournalNode>()
						.Select(x => x.OrderId)
						.Distinct();

					_clipboard.SetText(string.Join(", ", orderIds));
				}
			));
		}

		private void CreateCopyTaskIdToClipboardAction()
		{
			PopupActionsList.Add(new JournalAction(
				"Скопировать номер задачи",
				selected => selected.Cast<EdoDeviationJournalNode>().Any(x => x.EdoTaskId.HasValue),
				selected => true,
				selected =>
				{
					var taskIds = selected.Cast<EdoDeviationJournalNode>()
						.Where(x => x.EdoTaskId.HasValue)
						.Select(x => x.EdoTaskId.Value)
						.Distinct();

					_clipboard.SetText(string.Join(", ", taskIds));
				}
			));
		}

		private static int? GetCounterpartyId(EdoDeviationJournalNode node) =>
			node.CounterpartyId ?? node.Parent?.CounterpartyId;

		#endregion Действия по правой кнопке

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
