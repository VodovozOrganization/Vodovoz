using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Infrastructure;
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
using Vodovoz.Core.Domain.Edo;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using ExceptionEdoTaskProblem = Vodovoz.Core.Domain.Edo.ExceptionEdoTaskProblem;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	public class EdoProblemJournalViewModel : JournalViewModelBase
	{
		private readonly EdoProblemFilterViewModel _filterViewModel;
		private readonly IClipboard _clipboard;
		private readonly IGtkTabsOpener _gtkTabsOpener;

		public EdoProblemJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			EdoProblemFilterViewModel filterViewModel,
			IInteractiveService interactiveService,
			IClipboard clipboard,
			IGtkTabsOpener gtkTabsOpener,
			INavigationManager navigation
		) : base(uowFactory, interactiveService, navigation)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));

			Title = "Журнал проблем документооборота с клиентами";

			_filterViewModel.IsShow = true;
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;

			CreateNodeActions();
			CreatePopupActions();

			DataLoader = new AnyDataLoader<EdoProblemJournalNode>(GetNodes);
		}

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();
			CreateCopyOrderIdToClipboardAction();
			CreateOpenOrderAction();
			CreateCopyTaskIdToClipboardAction();
		}

		private void CreateCopyOrderIdToClipboardAction()
		{
			var action = new JournalAction(
				"Скопировать номер заказа",
				selected => selected.Any(),
				selected => true,
				selected =>
				{
					var selectedNodes = selected.Cast<EdoProblemJournalNode>().ToList();

					var orderIds = string.Join(", ", selectedNodes.Select(x => x.OrderId));

					_clipboard.SetText(orderIds);
				}
			);

			PopupActionsList.Add(action);
		}

		private void CreateOpenOrderAction()
		{
			var action = new JournalAction(
				"Открыть заказ",
				selected => selected.Count() == 1,
				selected => true,
				selected =>
				{
					var selectedNode = selected.Cast<EdoProblemJournalNode>().First();

					_gtkTabsOpener.OpenOrderDlgFromViewModelByNavigator(this, selectedNode.OrderId);
				}
			);

			PopupActionsList.Add(action);
		}

		private void CreateCopyTaskIdToClipboardAction()
		{
			var action = new JournalAction(
				"Скопировать номер задачи",
				selected => selected.Any(),
				selected => true,
				selected =>
				{
					var selectedNodes = selected.Cast<EdoProblemJournalNode>().ToList();

					var orderIds = string.Join(", ", selectedNodes.Select(x => x.OrderTaskId));

					_clipboard.SetText(orderIds);
				}
			);

			PopupActionsList.Add(action);
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		private IList<EdoProblemJournalNode> GetNodes(CancellationToken token)
		{
			EdoTaskProblem problemAlias = null;
			ExceptionEdoTaskProblem exceptionEdoTaskProblemAlias = null;
			FormalEdoRequest edoRequestAlias = null;
			Order orderAlias = null;
			EdoTask edoTaskAlias = null;
			EdoTaskProblemDescriptionSourceEntity problemDescriptionSourceAlias = null;
			EdoTaskProblemCustomSourceEntity customProblemDescriptionSourceAlias = null;
			EdoTaskProblemValidatorSourceEntity validatorProblemDescriptionSourceAlias = null;
			EdoProblemJournalNode resultNode = null;

			var query = UoW.Session.QueryOver(() => problemAlias)
				.JoinEntityAlias(() => exceptionEdoTaskProblemAlias, () => exceptionEdoTaskProblemAlias.Id == problemAlias.Id,
					JoinType.LeftOuterJoin)
				.JoinAlias(() => problemAlias.EdoTask, () => edoTaskAlias)
				.JoinEntityAlias(() => edoRequestAlias, () => edoRequestAlias.Task.Id == edoTaskAlias.Id)
				.JoinAlias(() => edoRequestAlias.Order, () => orderAlias)
				.JoinEntityAlias(() => problemDescriptionSourceAlias,
					() => problemDescriptionSourceAlias.Name == problemAlias.SourceName)
				.JoinEntityAlias(() => customProblemDescriptionSourceAlias,
					() => customProblemDescriptionSourceAlias.Name == problemDescriptionSourceAlias.Name, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => validatorProblemDescriptionSourceAlias,
					() => validatorProblemDescriptionSourceAlias.Name == problemDescriptionSourceAlias.Name, JoinType.LeftOuterJoin);

			if(_filterViewModel.DeliveryDateFrom != null)
			{
				query.Where(() => orderAlias.DeliveryDate >= _filterViewModel.DeliveryDateFrom.Value);
			}

			if(_filterViewModel.DeliveryDateTo != null)
			{
				query.Where(() => orderAlias.DeliveryDate <= _filterViewModel.DeliveryDateTo.Value);
			}

			if(_filterViewModel.OrderId != null)
			{
				query.Where(() => orderAlias.Id == _filterViewModel.OrderId);
			}

			if(_filterViewModel.TaskId != null)
			{
				query.Where(() => edoTaskAlias.Id == _filterViewModel.TaskId);
			}

			if(_filterViewModel.ProblemSourceName != null)
			{
				query.Where(Restrictions.Like(
					Projections.Property(() => problemDescriptionSourceAlias.Name),
					_filterViewModel.ProblemSourceName, MatchMode.Anywhere));
			}

			if(_filterViewModel.EdoTaskStatus != null)
			{
				query.Where(() => edoTaskAlias.Status == _filterViewModel.EdoTaskStatus);
			}

			if(_filterViewModel.TaskProblemState != null)
			{
				query.Where(() => problemAlias.State == _filterViewModel.TaskProblemState);
			}

			if(_filterViewModel.HasProblemTaskItems != null)
			{
				var taskItemsSubquery = QueryOver.Of<EdoTaskItem>()
					.Where(x => x.CustomerEdoTask.Id == edoTaskAlias.Id)
					.Select(x => x.Id)
					.Take(1);

				if(_filterViewModel.HasProblemTaskItems.Value)
				{
					query.WithSubquery.WhereExists(taskItemsSubquery);
				}
				else
				{
					query.WithSubquery.WhereNotExists(taskItemsSubquery);
				}
			}

			if(_filterViewModel.HasProblemItemGtins != null)
			{
				var problemCustomItemsSubquery = QueryOver.Of<EdoProblemCustomItem>()
					.Where(x => x.Problem.Id == problemAlias.Id)
					.Select(x => x.Id)
					.Take(1);

				if(_filterViewModel.HasProblemItemGtins.Value)
				{
					query.WithSubquery.WhereExists(problemCustomItemsSubquery);
				}
				else
				{
					query.WithSubquery.WhereNotExists(problemCustomItemsSubquery);
				}
			}

			query.Where(
				GetSearchCriterion(
					() => exceptionEdoTaskProblemAlias.ExceptionMessage,
					() => customProblemDescriptionSourceAlias.Message,
					() => validatorProblemDescriptionSourceAlias.Message,
					() => problemDescriptionSourceAlias.Description
				));

			var result = query.SelectList(list => list
					.Select(() => orderAlias.Id).WithAlias(() => resultNode.OrderId)
					.Select(() => edoTaskAlias.Id).WithAlias(() => resultNode.OrderTaskId)
					.Select(() => edoTaskAlias.Status).WithAlias(() => resultNode.OrderTaskStatus)
					.Select(() => problemAlias.State).WithAlias(() => resultNode.TaskProblemState)
					.Select(() => problemAlias.SourceName).WithAlias(() => resultNode.ProblemSourceName)
					.Select(CustomProjections.Coalesce(NHibernateUtil.String,
						Projections.Property(() => exceptionEdoTaskProblemAlias.ExceptionMessage),
						Projections.Property(() => customProblemDescriptionSourceAlias.Message),
						Projections.Property(() => validatorProblemDescriptionSourceAlias.Message))
					).WithAlias(() => resultNode.Message)
					.Select(() => problemDescriptionSourceAlias.Description).WithAlias(() => resultNode.Description)
					.Select(() => problemDescriptionSourceAlias.Recommendation).WithAlias(() => resultNode.Recomendation)
					.Select(() => orderAlias.DeliveryDate.Value).WithAlias(() => resultNode.DeliveryDate)
				)
				.OrderBy(() => orderAlias.Id).Desc
				.TransformUsing(Transformers.AliasToBean<EdoProblemJournalNode>())
				.ListAsync<EdoProblemJournalNode>(token).Result;

			return result;
		}

		public override IJournalFilterViewModel JournalFilter
		{
			get => _filterViewModel;
			protected set => throw new NotSupportedException("Установка фильтра выполняется через конструктор");
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
