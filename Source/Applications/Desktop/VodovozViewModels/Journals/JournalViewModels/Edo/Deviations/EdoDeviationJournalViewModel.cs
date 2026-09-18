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
using Vodovoz.Core.Domain.Goods;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Edo;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.TrueMark;
using Nomenclature = Vodovoz.Domain.Goods.Nomenclature;
using Order = Vodovoz.Domain.Orders.Order;
using OrderItem = Vodovoz.Domain.Orders.OrderItem;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo.Deviations
{
	/// <summary>
	/// Журнал отклонений и проблем документооборота ЭДО.
	/// Первый уровень - заказ, второй - зарегистрированные по нему отклонения, проблемы
	/// и задачи в проблемном статусе без записи проблемы, включая строки задач трансфера,
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

			ExpandAfterReloading = true;

			JournalFilter = _filterViewModel;

			_filterViewModel.IsShow = true;
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;

			var levelQueryLoader =
				new HierarchicalQueryLoader<Order, EdoDeviationJournalNode>(uowFactory, GetOrdersCount);

			levelQueryLoader.SetLevelingModel(GetOrdersQuery)
				.AddNextLevelSource(GetOrderRows);

			RecuresiveConfig = levelQueryLoader.TreeConfig;

			var threadDataLoader = new ThreadDataLoader<EdoDeviationJournalNode>(uowFactory);
			threadDataLoader.DynamicLoadingEnabled = true;
			threadDataLoader.QueryLoaders.Add(levelQueryLoader);
			DataLoader = threadDataLoader;

			CreateNodeActions();
			CreatePopupActions();
		}

		/// <summary>
		/// Настройка построения дерева журнала
		/// </summary>
		public IRecursiveConfig RecuresiveConfig { get; }

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		#region Первый уровень - заказы

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
			ApplyDeliveryDateRestriction(query, orderAlias);
			ApplyOrderIdRestriction(query, orderAlias);
			ApplyHasOrderTrueMarkItemsRestriction(query, orderAlias);
			ApplyMatchingRowsRestriction(query, orderAlias);
			ApplySearchRestriction(query, orderAlias, counterpartyAlias);
		}

		/// <summary>
		/// Оставляет только заказы, по которым есть хотя бы одна строка второго уровня
		/// </summary>
		private void ApplyMatchingRowsRestriction(IQueryOver<Order, Order> query, Order orderAlias)
		{
			var disjunction = Restrictions.Disjunction();

			if(IsOrderTaskDeviationRowsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetOrderTaskDeviationIdsSubquery(orderAlias)));
			}

			if(IsTransferDeviationRowsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetTransferDeviationIdsSubquery(orderAlias)));
			}

			if(IsRequestDeviationRowsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetRequestDeviationIdsSubquery(orderAlias)));
			}

			if(IsOrderTaskProblemRowsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetOrderTaskProblemIdsSubquery(orderAlias)));
			}

			if(IsTransferProblemRowsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetTransferProblemIdsSubquery(orderAlias)));
			}

			if(IsOrderTaskUnknownProblemRowsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetOrderTaskUnknownProblemIdsSubquery(orderAlias)));
			}

			if(IsTransferUnknownProblemRowsRequested())
			{
				disjunction.Add(Subqueries.WhereExists(GetTransferUnknownProblemIdsSubquery(orderAlias)));
			}

			query.Where(disjunction);
		}

		#endregion Первый уровень - заказы

		#region Второй уровень - отклонения и проблемы

		/// <summary>
		/// Собирает строки второго уровня по всем источникам, не отсеченным фильтром
		/// Порядок обхода источников задает порядок строк под заказом
		/// </summary>
		private IList<EdoDeviationJournalNode> GetOrderRows(IEnumerable<EdoDeviationJournalNode> parentNodes)
		{
			var orderIds = parentNodes.Select(x => x.Id).ToArray();

			if(!orderIds.Any())
			{
				return new List<EdoDeviationJournalNode>();
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var nodes = new List<EdoDeviationJournalNode>();

				if(IsOrderTaskDeviationRowsRequested())
				{
					nodes.AddRange(GetOrderTaskDeviations(uow, orderIds));
				}

				if(IsTransferDeviationRowsRequested())
				{
					nodes.AddRange(GetTransferDeviations(uow, orderIds));
				}

				if(IsRequestDeviationRowsRequested())
				{
					nodes.AddRange(GetRequestDeviations(uow, orderIds));
				}

				if(IsOrderTaskProblemRowsRequested())
				{
					nodes.AddRange(GetOrderTaskProblems(uow, orderIds));
				}

				if(IsTransferProblemRowsRequested())
				{
					nodes.AddRange(GetTransferProblems(uow, orderIds));
				}

				if(IsOrderTaskUnknownProblemRowsRequested())
				{
					nodes.AddRange(GetOrderTaskUnknownProblems(uow, orderIds));
				}

				if(IsTransferUnknownProblemRowsRequested())
				{
					nodes.AddRange(GetTransferUnknownProblems(uow, orderIds));
				}

				var distinctNodes = DistinctByRow(nodes);

				FillTaskTypes(uow, distinctNodes);
				FillUnknownProblemTexts(uow, distinctNodes);
				UpdateParentCounters(parentNodes, distinctNodes);

				return distinctNodes;
			}
		}

		/// <summary>
		/// Удаление дубликатов строк второго уровня, которые могут появляться при объединении источников
		/// </summary>
		private static IList<EdoDeviationJournalNode> DistinctByRow(
			IEnumerable<EdoDeviationJournalNode> nodes) =>
			nodes
				.GroupBy(x => new { x.NodeType, x.Id, x.ParentId })
				.Select(x => x.First())
				.ToList();

		/// <summary>
		/// Проставляет строкам тип задачи ЭДО
		/// </summary>
		private static void FillTaskTypes(IUnitOfWork uow, IList<EdoDeviationJournalNode> nodes)
		{
			var taskIds = nodes
				.Where(x => x.EdoTaskId.HasValue)
				.Select(x => x.EdoTaskId.Value)
				.Distinct()
				.ToArray();

			if(!taskIds.Any())
			{
				return;
			}

			var taskTypes = uow.Session.QueryOver<EdoTask>()
				.WhereRestrictionOn(x => x.Id).IsIn(taskIds)
				.List<EdoTask>()
				.ToDictionary(x => x.Id, x => x.TaskType);

			foreach(var node in nodes)
			{
				if(node.EdoTaskId.HasValue && taskTypes.TryGetValue(node.EdoTaskId.Value, out var taskType))
				{
					node.TaskType = taskType;
				}
			}
		}

		/// <summary>
		/// Заполняет строки задач, застрявших в проблемном статусе без зафиксированной причины
		/// </summary>
		private static void FillUnknownProblemTexts(
			IUnitOfWork uow,
			IEnumerable<EdoDeviationJournalNode> nodes)
		{
			var nodesWithoutRecord = nodes
				.Where(x => x.NodeType == EdoDeviationJournalNodeType.UnknownProblem)
				.ToArray();

			if(!nodesWithoutRecord.Any())
			{
				return;
			}

			var taskIds = nodesWithoutRecord
				.Where(x => x.EdoTaskId.HasValue)
				.Select(x => x.EdoTaskId.Value)
				.Distinct()
				.ToArray();

			var taskIdsWithSolvedProblems = GetTaskIdsWithSolvedProblems(uow, taskIds);

			foreach(var node in nodesWithoutRecord)
			{
				var hasSolvedProblems =
					node.EdoTaskId.HasValue && taskIdsWithSolvedProblems.Contains(node.EdoTaskId.Value);

				node.ProblemSourceDescription = hasSolvedProblems
					? EdoDeviationJournalMessages.SolvedProblemResult
					: EdoDeviationJournalMessages.UnknownProblemResult;

				node.Description = hasSolvedProblems
					? EdoDeviationJournalMessages.SolvedProblemDescription
					: EdoDeviationJournalMessages.UnknownProblemDescription;

				node.Recommendation = EdoDeviationJournalMessages.UnknownProblemRecommendation;
			}
		}

		/// <summary>
		/// Задачи, по которым запись проблемы заведена
		/// </summary>
		private static ISet<int> GetTaskIdsWithSolvedProblems(IUnitOfWork uow, int[] taskIds)
		{
			EdoTaskProblem problemAlias = null;

			if(!taskIds.Any())
			{
				return new HashSet<int>();
			}

			var foundTaskIds = uow.Session.QueryOver(() => problemAlias)
				.WhereRestrictionOn(() => problemAlias.EdoTask.Id).IsIn(taskIds)
				.Select(Projections.Distinct(Projections.Property(() => problemAlias.EdoTask.Id)))
				.List<int>();

			return new HashSet<int>(foundTaskIds);
		}

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

		#endregion Второй уровень - отклонения и проблемы

		#region Источник строк - отклонения по задачам заказа

		private QueryOver<EdoTaskDeviation, EdoTaskDeviation> CreateOrderTaskDeviationQuery(
			EdoTaskDeviation deviationAlias,
			EdoTask taskAlias,
			EdoDeviationSource sourceAlias,
			FormalEdoRequest requestAlias)
		{
			var query = QueryOver.Of(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoTask, () => taskAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.JoinEntityAlias(() => requestAlias, () => requestAlias.Task.Id == taskAlias.Id);

			ApplyStateRestriction(query, Projections.Property(() => deviationAlias.State));
			ApplyDeviationTypeRestriction(query, sourceAlias);
			ApplyTaskIdRestriction(query, taskAlias);
			ApplyTaskStatusRestriction(query, taskAlias);
			ApplyOrderTaskTypeRestriction(query, taskAlias);

			return query;
		}

		private QueryOver<EdoTaskDeviation> GetOrderTaskDeviationIdsSubquery(Order orderAlias)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoTask taskAlias = null;
			EdoDeviationSource sourceAlias = null;
			FormalEdoRequest requestAlias = null;

			return CreateOrderTaskDeviationQuery(deviationAlias, taskAlias, sourceAlias, requestAlias)
				.Where(() => requestAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Property(() => deviationAlias.Id));
		}

		private IList<EdoDeviationJournalNode> GetOrderTaskDeviations(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoTask taskAlias = null;
			EdoDeviationSource sourceAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			return CreateOrderTaskDeviationQuery(deviationAlias, taskAlias, sourceAlias, requestAlias)
				.GetExecutableQueryOver(uow.Session)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds))
				.SelectList(list => list
					.Select(() => deviationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Deviation))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => sourceAlias.DeviationType).WithAlias(() => resultAlias.DeviationType)
					.Select(() => sourceAlias.Description).WithAlias(() => resultAlias.Recommendation)
					.Select(() => deviationAlias.Details).WithAlias(() => resultAlias.Description)
					.Select(() => deviationAlias.DetectedTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => deviationAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => deviationAlias.ResolveReason).WithAlias(() => resultAlias.ResolveReason)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		#endregion Источник строк - отклонения по задачам заказа

		#region Источник строк - отклонения по задачам трансфера

		private QueryOver<EdoTaskDeviation, EdoTaskDeviation> CreateTransferDeviationQuery(
			EdoTaskDeviation deviationAlias,
			EdoTask taskAlias,
			EdoDeviationSource sourceAlias,
			TransferEdoRequest transferRequestAlias,
			TransferEdoRequestIteration iterationAlias,
			FormalEdoRequest requestAlias)
		{
			var query = QueryOver.Of(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoTask, () => taskAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.JoinEntityAlias(() => transferRequestAlias, () => transferRequestAlias.TransferEdoTask.Id == taskAlias.Id)
				.JoinAlias(() => transferRequestAlias.Iteration, () => iterationAlias)
				.JoinEntityAlias(() => requestAlias, () => requestAlias.Task.Id == iterationAlias.OrderEdoTask.Id);

			ApplyStateRestriction(query, Projections.Property(() => deviationAlias.State));
			ApplyDeviationTypeRestriction(query, sourceAlias);
			ApplyTaskIdRestriction(query, taskAlias);
			ApplyTaskStatusRestriction(query, taskAlias);

			return query;
		}

		private QueryOver<EdoTaskDeviation> GetTransferDeviationIdsSubquery(Order orderAlias)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoTask taskAlias = null;
			EdoDeviationSource sourceAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;

			return CreateTransferDeviationQuery(
					deviationAlias, taskAlias, sourceAlias, transferRequestAlias, iterationAlias, requestAlias)
				.Where(() => requestAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Property(() => deviationAlias.Id));
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

			return CreateTransferDeviationQuery(
					deviationAlias, taskAlias, sourceAlias, transferRequestAlias, iterationAlias, requestAlias)
				.GetExecutableQueryOver(uow.Session)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds))
				.SelectList(list => list
					.Select(() => deviationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Deviation))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => sourceAlias.DeviationType).WithAlias(() => resultAlias.DeviationType)
					.Select(() => sourceAlias.Description).WithAlias(() => resultAlias.Recommendation)
					.Select(() => deviationAlias.Details).WithAlias(() => resultAlias.Description)
					.Select(() => deviationAlias.DetectedTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => deviationAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => deviationAlias.ResolveReason).WithAlias(() => resultAlias.ResolveReason)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		#endregion Источник строк - отклонения по задачам трансфера

		#region Источник строк - отклонения по заявкам без задачи

		private QueryOver<EdoTaskDeviation, EdoTaskDeviation> CreateRequestDeviationQuery(
			EdoTaskDeviation deviationAlias,
			EdoDeviationSource sourceAlias,
			FormalEdoRequest requestAlias)
		{
			var query = QueryOver.Of(() => deviationAlias)
				.JoinAlias(() => deviationAlias.EdoRequest, () => requestAlias)
				.JoinAlias(() => deviationAlias.DeviationSource, () => sourceAlias)
				.Where(() => deviationAlias.EdoTask == null);

			ApplyStateRestriction(query, Projections.Property(() => deviationAlias.State));
			ApplyDeviationTypeRestriction(query, sourceAlias);

			return query;
		}

		private QueryOver<EdoTaskDeviation> GetRequestDeviationIdsSubquery(Order orderAlias)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoDeviationSource sourceAlias = null;
			FormalEdoRequest requestAlias = null;

			return CreateRequestDeviationQuery(deviationAlias, sourceAlias, requestAlias)
				.Where(() => requestAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Property(() => deviationAlias.Id));
		}

		private IList<EdoDeviationJournalNode> GetRequestDeviations(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskDeviation deviationAlias = null;
			EdoDeviationSource sourceAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			return CreateRequestDeviationQuery(deviationAlias, sourceAlias, requestAlias)
				.GetExecutableQueryOver(uow.Session)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds))
				.SelectList(list => list
					.Select(() => deviationAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Deviation))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => sourceAlias.DeviationType).WithAlias(() => resultAlias.DeviationType)
					.Select(() => sourceAlias.Description).WithAlias(() => resultAlias.Recommendation)
					.Select(() => deviationAlias.Details).WithAlias(() => resultAlias.Description)
					.Select(() => deviationAlias.DetectedTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => deviationAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => deviationAlias.ResolveReason).WithAlias(() => resultAlias.ResolveReason)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		#endregion Источник строк - отклонения по заявкам без задачи

		#region Источник строк - проблемы по задачам заказа

		private QueryOver<EdoTaskProblem, EdoTaskProblem> CreateOrderTaskProblemQuery(
			EdoTaskProblem problemAlias,
			EdoTask taskAlias,
			FormalEdoRequest requestAlias)
		{
			var query = QueryOver.Of(() => problemAlias)
				.JoinAlias(() => problemAlias.EdoTask, () => taskAlias)
				.JoinEntityAlias(() => requestAlias, () => requestAlias.Task.Id == taskAlias.Id);

			ApplyStateRestriction(query, Projections.Property(() => problemAlias.State));
			ApplyProblemSourceNameRestriction(query, problemAlias);
			ApplyTaskIdRestriction(query, taskAlias);
			ApplyTaskStatusRestriction(query, taskAlias);
			ApplyOrderTaskTypeRestriction(query, taskAlias);
			ApplyHasProblemItemGtinsRestriction(query, problemAlias);

			return query;
		}

		private QueryOver<EdoTaskProblem> GetOrderTaskProblemIdsSubquery(Order orderAlias)
		{
			EdoTaskProblem problemAlias = null;
			EdoTask taskAlias = null;
			FormalEdoRequest requestAlias = null;

			return CreateOrderTaskProblemQuery(problemAlias, taskAlias, requestAlias)
				.Where(() => requestAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Property(() => problemAlias.Id));
		}

		private IList<EdoDeviationJournalNode> GetOrderTaskProblems(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskProblem problemAlias = null;
			EdoTask taskAlias = null;
			FormalEdoRequest requestAlias = null;
			ExceptionEdoTaskProblem exceptionProblemAlias = null;
			EdoTaskProblemDescriptionSourceEntity descriptionSourceAlias = null;
			EdoTaskProblemCustomSourceEntity customSourceAlias = null;
			EdoTaskProblemValidatorSourceEntity validatorSourceAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			var query = CreateOrderTaskProblemQuery(problemAlias, taskAlias, requestAlias)
				.GetExecutableQueryOver(uow.Session)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds));

			query
				.JoinEntityAlias(() => exceptionProblemAlias,
					() => exceptionProblemAlias.Id == problemAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => descriptionSourceAlias,
					() => descriptionSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => customSourceAlias,
					() => customSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => validatorSourceAlias,
					() => validatorSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin);

			return query.SelectList(list => list
					.Select(() => problemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Problem))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => problemAlias.SourceName).WithAlias(() => resultAlias.ProblemSourceName)
					.Select(() => descriptionSourceAlias.Description)
						.WithAlias(() => resultAlias.ProblemSourceDescription)
					.Select(() => descriptionSourceAlias.Recommendation)
						.WithAlias(() => resultAlias.Recommendation)
					.Select(GetProblemDescriptionProjection(
						exceptionProblemAlias, customSourceAlias, validatorSourceAlias))
						.WithAlias(() => resultAlias.Description)
					.Select(() => problemAlias.CreationTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => problemAlias.State).WithAlias(() => resultAlias.State)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		#endregion Источник строк - проблемы по задачам заказа

		#region Источник строк - проблемы по задачам трансфера

		private QueryOver<EdoTaskProblem, EdoTaskProblem> CreateTransferProblemQuery(
			EdoTaskProblem problemAlias,
			EdoTask taskAlias,
			TransferEdoRequest transferRequestAlias,
			TransferEdoRequestIteration iterationAlias,
			FormalEdoRequest requestAlias)
		{
			var query = QueryOver.Of(() => problemAlias)
				.JoinAlias(() => problemAlias.EdoTask, () => taskAlias)
				.JoinEntityAlias(() => transferRequestAlias,
					() => transferRequestAlias.TransferEdoTask.Id == taskAlias.Id)
				.JoinAlias(() => transferRequestAlias.Iteration, () => iterationAlias)
				.JoinEntityAlias(() => requestAlias,
					() => requestAlias.Task.Id == iterationAlias.OrderEdoTask.Id);

			ApplyStateRestriction(query, Projections.Property(() => problemAlias.State));
			ApplyProblemSourceNameRestriction(query, problemAlias);
			ApplyTaskIdRestriction(query, taskAlias);
			ApplyTaskStatusRestriction(query, taskAlias);
			ApplyHasProblemItemGtinsRestriction(query, problemAlias);

			return query;
		}

		private QueryOver<EdoTaskProblem> GetTransferProblemIdsSubquery(Order orderAlias)
		{
			EdoTaskProblem problemAlias = null;
			EdoTask taskAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;

			return CreateTransferProblemQuery(
					problemAlias, taskAlias, transferRequestAlias, iterationAlias, requestAlias)
				.Where(() => requestAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Property(() => problemAlias.Id));
		}

		private IList<EdoDeviationJournalNode> GetTransferProblems(IUnitOfWork uow, int[] orderIds)
		{
			EdoTaskProblem problemAlias = null;
			EdoTask taskAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;
			ExceptionEdoTaskProblem exceptionProblemAlias = null;
			EdoTaskProblemDescriptionSourceEntity descriptionSourceAlias = null;
			EdoTaskProblemCustomSourceEntity customSourceAlias = null;
			EdoTaskProblemValidatorSourceEntity validatorSourceAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			var query = CreateTransferProblemQuery(
					problemAlias, taskAlias, transferRequestAlias, iterationAlias, requestAlias)
				.GetExecutableQueryOver(uow.Session)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds));

			query
				.JoinEntityAlias(() => exceptionProblemAlias,
					() => exceptionProblemAlias.Id == problemAlias.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => descriptionSourceAlias,
					() => descriptionSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => customSourceAlias,
					() => customSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => validatorSourceAlias,
					() => validatorSourceAlias.Name == problemAlias.SourceName, JoinType.LeftOuterJoin);

			return query.SelectList(list => list
					.Select(() => problemAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.Problem))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => problemAlias.SourceName).WithAlias(() => resultAlias.ProblemSourceName)
					.Select(() => descriptionSourceAlias.Description)
						.WithAlias(() => resultAlias.ProblemSourceDescription)
					.Select(() => descriptionSourceAlias.Recommendation)
						.WithAlias(() => resultAlias.Recommendation)
					.Select(GetProblemDescriptionProjection(
						exceptionProblemAlias, customSourceAlias, validatorSourceAlias))
						.WithAlias(() => resultAlias.Description)
					.Select(() => problemAlias.CreationTime).WithAlias(() => resultAlias.DetectedTime)
					.Select(() => problemAlias.State).WithAlias(() => resultAlias.State)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		#endregion Источник строк - проблемы по задачам трансфера

		#region Источник строк - задачи заказа в проблемном статусе без записи проблемы

		private QueryOver<OrderEdoTask, OrderEdoTask> CreateOrderTaskUnknownProblemQuery(
			OrderEdoTask taskAlias,
			FormalEdoRequest requestAlias)
		{
			var query = QueryOver.Of(() => taskAlias)
				.Where(() => taskAlias.Status == EdoTaskStatus.Problem)
				.Where(GetNoActiveProblemAndDeviationRestriction(taskAlias));

			query.JoinEntityAlias(() => requestAlias, () => requestAlias.Task.Id == taskAlias.Id);

			ApplyTaskIdRestriction(query, taskAlias);
			ApplyTaskStatusRestriction(query, taskAlias);
			ApplyOrderTaskTypeRestriction(query, taskAlias);

			return query;
		}

		private QueryOver<OrderEdoTask> GetOrderTaskUnknownProblemIdsSubquery(Order orderAlias)
		{
			OrderEdoTask taskAlias = null;
			FormalEdoRequest requestAlias = null;

			return CreateOrderTaskUnknownProblemQuery(taskAlias, requestAlias)
				.Where(() => requestAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Property(() => taskAlias.Id));
		}

		private IList<EdoDeviationJournalNode> GetOrderTaskUnknownProblems(IUnitOfWork uow, int[] orderIds)
		{
			OrderEdoTask taskAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			return CreateOrderTaskUnknownProblemQuery(taskAlias, requestAlias)
				.GetExecutableQueryOver(uow.Session)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds))
				.SelectList(list => list
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.UnknownProblem))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => taskAlias.Version).WithAlias(() => resultAlias.DetectedTime)
					.Select(Projections.Constant(TaskProblemState.Active)).WithAlias(() => resultAlias.State)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		#endregion Источник строк - задачи заказа в проблемном статусе без записи проблемы

		#region Источник строк - задачи трансфера в проблемном статусе без записи проблемы

		private QueryOver<TransferEdoTask, TransferEdoTask> CreateTransferUnknownProblemQuery(
			TransferEdoTask taskAlias,
			TransferEdoRequest transferRequestAlias,
			TransferEdoRequestIteration iterationAlias,
			FormalEdoRequest requestAlias)
		{
			var query = QueryOver.Of(() => taskAlias)
				.Where(() => taskAlias.Status == EdoTaskStatus.Problem)
				.Where(GetNoActiveProblemAndDeviationRestriction(taskAlias));

			query
				.JoinEntityAlias(() => transferRequestAlias,
					() => transferRequestAlias.TransferEdoTask.Id == taskAlias.Id)
				.JoinAlias(() => transferRequestAlias.Iteration, () => iterationAlias)
				.JoinEntityAlias(() => requestAlias,
					() => requestAlias.Task.Id == iterationAlias.OrderEdoTask.Id);

			ApplyTaskIdRestriction(query, taskAlias);
			ApplyTaskStatusRestriction(query, taskAlias);

			return query;
		}

		private QueryOver<TransferEdoTask> GetTransferUnknownProblemIdsSubquery(Order orderAlias)
		{
			TransferEdoTask taskAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;

			return CreateTransferUnknownProblemQuery(
					taskAlias, transferRequestAlias, iterationAlias, requestAlias)
				.Where(() => requestAlias.Order.Id == orderAlias.Id)
				.Select(Projections.Property(() => taskAlias.Id));
		}

		private IList<EdoDeviationJournalNode> GetTransferUnknownProblems(IUnitOfWork uow, int[] orderIds)
		{
			TransferEdoTask taskAlias = null;
			TransferEdoRequest transferRequestAlias = null;
			TransferEdoRequestIteration iterationAlias = null;
			FormalEdoRequest requestAlias = null;
			EdoDeviationJournalNode resultAlias = null;

			return CreateTransferUnknownProblemQuery(
					taskAlias, transferRequestAlias, iterationAlias, requestAlias)
				.GetExecutableQueryOver(uow.Session)
				.Where(Restrictions.In(Projections.Property(() => requestAlias.Order.Id), orderIds))
				.SelectList(list => list
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.ParentId)
					.Select(() => requestAlias.Order.Id).WithAlias(() => resultAlias.OrderId)
					.Select(Projections.Constant(EdoDeviationJournalNodeType.UnknownProblem))
						.WithAlias(() => resultAlias.NodeType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.EdoTaskId)
					.Select(() => taskAlias.Status).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => taskAlias.Version).WithAlias(() => resultAlias.DetectedTime)
					.Select(Projections.Constant(TaskProblemState.Active)).WithAlias(() => resultAlias.State)
				)
				.TransformUsing(Transformers.AliasToBean<EdoDeviationJournalNode>())
				.List<EdoDeviationJournalNode>();
		}

		#endregion Источник строк - задачи трансфера в проблемном статусе без записи проблемы

		#region Общие части запросов источников

		/// <summary>
		/// Сообщение проблемы: у проблемы от исключения - его текст,
		/// у остальных - сообщение из справочника ее источника
		/// </summary>
		private static IProjection GetProblemDescriptionProjection(
			ExceptionEdoTaskProblem exceptionProblemAlias,
			EdoTaskProblemCustomSourceEntity customSourceAlias,
			EdoTaskProblemValidatorSourceEntity validatorSourceAlias) =>
			CustomProjections.Coalesce(
				NHibernateUtil.String,
				Projections.Property(() => exceptionProblemAlias.ExceptionMessage),
				Projections.Property(() => customSourceAlias.Message),
				Projections.Property(() => validatorSourceAlias.Message));

		/// <summary>
		/// Отбирает задачи, по которым нет ни активной проблемы, ни активного отклонения
		/// </summary>
		private static ICriterion GetNoActiveProblemAndDeviationRestriction(EdoTask taskAlias)
		{
			EdoTaskProblem problemAlias = null;
			EdoTaskDeviation deviationAlias = null;

			var activeProblems = QueryOver.Of(() => problemAlias)
				.Where(() => problemAlias.EdoTask.Id == taskAlias.Id)
				.Where(() => problemAlias.State == TaskProblemState.Active)
				.Select(Projections.Property(() => problemAlias.Id));

			var activeDeviations = QueryOver.Of(() => deviationAlias)
				.Where(() => deviationAlias.EdoTask.Id == taskAlias.Id)
				.Where(() => deviationAlias.State == TaskProblemState.Active)
				.Select(Projections.Property(() => deviationAlias.Id));

			return Restrictions.Conjunction()
				.Add(Subqueries.WhereNotExists(activeProblems))
				.Add(Subqueries.WhereNotExists(activeDeviations));
		}

		#endregion Общие части запросов источников

		#region Условия фильтра

		/// <summary>
		/// Отбор по периоду даты доставки заказа
		/// </summary>
		private void ApplyDeliveryDateRestriction(IQueryOver<Order, Order> query, Order orderAlias)
		{
			if(_filterViewModel.DeliveryDateFrom.HasValue)
			{
				query.Where(() => orderAlias.DeliveryDate >= _filterViewModel.DeliveryDateFrom.Value);
			}

			if(_filterViewModel.DeliveryDateTo.HasValue)
			{
				query.Where(() => orderAlias.DeliveryDate <= _filterViewModel.DeliveryDateTo.Value);
			}
		}

		/// <summary>
		/// Отбор по номеру заказа
		/// </summary>
		private void ApplyOrderIdRestriction(IQueryOver<Order, Order> query, Order orderAlias)
		{
			if(_filterViewModel.OrderId.HasValue)
			{
				query.Where(() => orderAlias.Id == _filterViewModel.OrderId.Value);
			}
		}

		/// <summary>
		/// Отбор по строке поиска журнала
		/// </summary>
		private void ApplySearchRestriction(
			IQueryOver<Order, Order> query,
			Order orderAlias,
			CounterpartyEntity counterpartyAlias)
		{
			query.Where(GetSearchCriterion(
				() => orderAlias.Id,
				() => counterpartyAlias.Name
			));
		}

		/// <summary>
		/// Отбор по состоянию отклонения или проблемы
		/// </summary>
		private void ApplyStateRestriction<TRoot>(IQueryOver<TRoot, TRoot> query, IProjection stateProjection)
		{
			if(_filterViewModel.State.HasValue)
			{
				query.Where(Restrictions.Eq(stateProjection, _filterViewModel.State.Value));
			}
		}

		/// <summary>
		/// Отбор по типу отклонения
		/// </summary>
		private void ApplyDeviationTypeRestriction<TRoot>(
			IQueryOver<TRoot, TRoot> query,
			EdoDeviationSource sourceAlias)
		{
			if(_filterViewModel.DeviationType.HasValue)
			{
				query.Where(() => sourceAlias.DeviationType == _filterViewModel.DeviationType.Value);
			}
		}

		/// <summary>
		/// Отбор по идентификатору источника проблемы
		/// </summary>
		private void ApplyProblemSourceNameRestriction<TRoot>(
			IQueryOver<TRoot, TRoot> query,
			EdoTaskProblem problemAlias)
		{
			if(!string.IsNullOrWhiteSpace(_filterViewModel.ProblemSourceName))
			{
				query.Where(Restrictions.Like(
					Projections.Property(() => problemAlias.SourceName),
					_filterViewModel.ProblemSourceName,
					MatchMode.Anywhere));
			}
		}

		/// <summary>
		/// Отбор по номеру задачи ЭДО
		/// </summary>
		private void ApplyTaskIdRestriction<TRoot>(IQueryOver<TRoot, TRoot> query, EdoTask taskAlias)
		{
			if(_filterViewModel.TaskId.HasValue)
			{
				query.Where(() => taskAlias.Id == _filterViewModel.TaskId.Value);
			}
		}

		/// <summary>
		/// Отбор по статусу задачи ЭДО
		/// </summary>
		private void ApplyTaskStatusRestriction<TRoot>(IQueryOver<TRoot, TRoot> query, EdoTask taskAlias)
		{
			if(_filterViewModel.EdoTaskStatus.HasValue)
			{
				query.Where(() => taskAlias.Status == _filterViewModel.EdoTaskStatus.Value);
			}
		}

		/// <summary>
		/// Отбор задач заказа по типу задачи
		/// </summary>
		private void ApplyOrderTaskTypeRestriction<TRoot>(IQueryOver<TRoot, TRoot> query, EdoTask taskAlias)
		{
			switch(_filterViewModel.EdoTaskType)
			{
				case EdoTaskType.Document:
					DocumentEdoTask documentTaskAlias = null;

					query.JoinEntityAlias(() => documentTaskAlias, () => documentTaskAlias.Id == taskAlias.Id);
					break;

				case EdoTaskType.Receipt:
					ReceiptEdoTask receiptTaskAlias = null;

					query.JoinEntityAlias(() => receiptTaskAlias, () => receiptTaskAlias.Id == taskAlias.Id);
					break;

				case EdoTaskType.Tender:
					TenderEdoTask tenderTaskAlias = null;

					query.JoinEntityAlias(() => tenderTaskAlias, () => tenderTaskAlias.Id == taskAlias.Id);
					break;
			}
		}

		/// <summary>
		/// Отбор заказов по наличию в них маркируемой продукции
		/// </summary>
		private void ApplyHasOrderTrueMarkItemsRestriction(
			IQueryOver<Order, Order> query,
			Order orderAlias)
		{
			if(!_filterViewModel.HasProblemTaskItems.HasValue)
			{
				return;
			}

			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			GtinEntity gtinAlias = null;

			var trueMarkOrderItemsCount = QueryOver.Of(() => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Gtins, () => gtinAlias)
				.Where(() => orderItemAlias.Order.Id == orderAlias.Id)
				.And(() => nomenclatureAlias.IsAccountableInTrueMark)
				.And(() => orderItemAlias.Count > 0)
				.Select(Projections.RowCount());

			query.Where(_filterViewModel.HasProblemTaskItems.Value
				? Restrictions.Gt(Projections.SubQuery(trueMarkOrderItemsCount), 0)
				: Restrictions.Eq(Projections.SubQuery(trueMarkOrderItemsCount), 0));
		}

		/// <summary>
		/// Отбор по наличию у проблемы связанных GTIN
		/// </summary>
		private void ApplyHasProblemItemGtinsRestriction<TRoot>(
			IQueryOver<TRoot, TRoot> query,
			EdoTaskProblem problemAlias)
		{
			if(!_filterViewModel.HasProblemItemGtins.HasValue)
			{
				return;
			}

			EdoProblemGtinItem gtinItemAlias = null;

			var gtinItems = QueryOver.Of(() => gtinItemAlias)
				.Where(() => gtinItemAlias.Problem.Id == problemAlias.Id)
				.Select(Projections.Property(() => gtinItemAlias.Id));

			query.Where(_filterViewModel.HasProblemItemGtins.Value
				? Subqueries.WhereExists(gtinItems)
				: Subqueries.WhereNotExists(gtinItems));
		}

		#endregion Условия фильтра

		#region Применимость источников строк

		/// <summary>
		/// Допустимы ли фильтром отклонения, зафиксированные по задачам заказа
		/// </summary>
		private bool IsOrderTaskDeviationRowsRequested() =>
			IsDeviationsRequested() && IsOrderTaskRowsRequested();

		/// <summary>
		/// Допустимы ли фильтром отклонения, зафиксированные по задачам трансфера
		/// </summary>
		private bool IsTransferDeviationRowsRequested() =>
			IsDeviationsRequested() && IsTransferRowsRequested();

		/// <summary>
		///	Допустимы ли фильтром отклонения по заявкам, задача по которым так и не создана
		/// </summary>
		private bool IsRequestDeviationRowsRequested() =>
			IsDeviationsRequested()
			&& !_filterViewModel.TaskId.HasValue
			&& !_filterViewModel.EdoTaskStatus.HasValue
			&& !_filterViewModel.EdoTaskType.HasValue;

		/// <summary>
		/// Допустимы ли фильтром записи проблем, заведенные по задачам заказа
		/// </summary>
		private bool IsOrderTaskProblemRowsRequested() =>
			IsProblemsRequested() && IsOrderTaskRowsRequested();

		/// <summary>
		/// Допустимы ли фильтром записи проблем, заведенные по задачам трансфера
		/// </summary>
		private bool IsTransferProblemRowsRequested() =>
			IsProblemsRequested() && IsTransferRowsRequested();

		/// <summary>
		/// Допустимы ли фильтром задачи заказа, застрявшие в проблемном статусе без действующей записи проблемы
		/// </summary>
		private bool IsOrderTaskUnknownProblemRowsRequested() =>
			IsUnknownProblemsRequested() && IsOrderTaskRowsRequested();

		/// <summary>
		/// Допустимы ли фильтром задачи трансфера, застрявшие в проблемном статусе без действующей записи проблемы
		/// </summary>
		private bool IsTransferUnknownProblemRowsRequested() =>
			IsUnknownProblemsRequested() && IsTransferRowsRequested();

		/// <summary>
		/// Допустимы ли фильтром строки отклонений
		/// </summary>
		private bool IsDeviationsRequested() =>
			(!_filterViewModel.RowType.HasValue
				|| _filterViewModel.RowType == EdoDeviationJournalNodeType.Deviation)
			&& string.IsNullOrWhiteSpace(_filterViewModel.ProblemSourceName)
			&& _filterViewModel.HasProblemItemGtins != true;

		/// <summary>
		/// Допустимы ли фильтром строки проблем
		/// </summary>
		private bool IsProblemsRequested() =>
			(!_filterViewModel.RowType.HasValue
				|| _filterViewModel.RowType == EdoDeviationJournalNodeType.Problem)
			&& !_filterViewModel.DeviationType.HasValue;

		/// <summary>
		/// Допустимы ли фильтром строки задач, застрявших в проблемном статусе без действующей записи проблемы
		/// </summary>
		private bool IsUnknownProblemsRequested() =>
			(!_filterViewModel.RowType.HasValue
				|| _filterViewModel.RowType == EdoDeviationJournalNodeType.UnknownProblem)
			&& !_filterViewModel.DeviationType.HasValue
			&& string.IsNullOrWhiteSpace(_filterViewModel.ProblemSourceName)
			&& (!_filterViewModel.State.HasValue || _filterViewModel.State == TaskProblemState.Active)
			&& (!_filterViewModel.EdoTaskStatus.HasValue
				|| _filterViewModel.EdoTaskStatus == EdoTaskStatus.Problem)
			&& _filterViewModel.HasProblemItemGtins != true;

		/// <summary>
		/// Допустимы ли фильтром задачи заказа
		/// </summary>
		private bool IsOrderTaskRowsRequested() =>
			!_filterViewModel.EdoTaskType.HasValue
			|| _filterViewModel.EdoTaskType == EdoTaskType.Document
			|| _filterViewModel.EdoTaskType == EdoTaskType.Receipt
			|| _filterViewModel.EdoTaskType == EdoTaskType.Tender;

		/// <summary>
		/// Допустимы ли фильтром задачи трансфера
		/// </summary>
		private bool IsTransferRowsRequested() =>
			!_filterViewModel.EdoTaskType.HasValue
			|| _filterViewModel.EdoTaskType == EdoTaskType.Transfer;

		#endregion Применимость источников строк

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
