using NHibernate;
using NHibernate.Type;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Edo.Transport;
using QS.Services;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Core.Infrastructure;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	public class EdoProblemJournalViewModel : JournalViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly EdoProblemFilterViewModel _filterViewModel;
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<ReceiptEdoTask> _receiptRepository;
		private readonly MessageService _messageService;
		private readonly IUserService _userService;
		private readonly IClipboard _clipboard;
		private readonly IGtkTabsOpener _gtkTabsOpener;

		public EdoProblemJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			EdoProblemFilterViewModel filterViewModel,
			IInteractiveService interactiveService,
			IGenericRepository<ReceiptEdoTask> receiptRepository,
			MessageService messageService,
			IUserService userService,
			IClipboard clipboard,
			IGtkTabsOpener gtkTabsOpener,
			INavigationManager navigation = null
			) : base(uowFactory, interactiveService, navigation)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_receiptRepository = receiptRepository ?? throw new ArgumentNullException(nameof(receiptRepository));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_interactiveService = interactiveService;

			Title = "Журнал проблем";

			DataLoader = new AnyDataLoader<EdoProblemJournalNode>(GetNodes);

			SearchEnabled = false;
			_filterViewModel.IsShow = true;
			SelectionMode = JournalSelectionMode.Multiple;

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			CreateNodeActions();
			CreatePopupActions();
		}

		public override IJournalFilterViewModel JournalFilter 
		{ 
			get => _filterViewModel; 
			protected set => throw new NotSupportedException("Установка фильтра выполняется через конструктор"); 
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
					var selectedNode = selected.Cast<EdoProblemJournalNode>().FirstOrDefault();

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

					var orderIds = string.Join(", ", selectedNodes.Select(x => x.OrderId));
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
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var filterSql = "";
				var havingSql = "";

				if(_filterViewModel.OrderId.HasValue)
				{
					filterSql += " and o.id = :filter_order_id";
				}
				
				if(_filterViewModel.TaskId.HasValue)
				{
					filterSql += " and et.id = :filter_et_id";
				}
				
				if(_filterViewModel.EdoTaskStatus.HasValue)
				{
					filterSql += " and et.status = :filter_task_status";
				}
				
				if(_filterViewModel.TaskProblemState.HasValue)
				{
					filterSql += " and p.state = :filter_problem_state";
				}

				if(!_filterViewModel.SourceId.IsNullOrWhiteSpace() )
				{
					filterSql += " and p.source_name = :filter_source_name";
				}

				if(_filterViewModel.HasProblemItems.HasValue)
				{
					filterSql += " and pi.id is not null";
				}
				
				if(_filterViewModel.HasProblemItemGtins.HasValue)
				{
					filterSql += " and pci.id is not null";
				}

				var sql = $@"
select 
	o.id as :order_id,
	o.delivery_date as :delivery_date,
	o.order_status as :order_status,
	ecr.`time` as :customer_request_time,
	ecr.source as :customer_request_source,
	et.id as :order_task_id,
	et.`type` as :order_task_type,
	et.status as :order_task_status,
	et.document_task_stage as :order_task_document_stage,
	et.receipt_status as :order_task_receipt_stage,
	Count(distinct if(etri.status = 'Completed', etri.id, null)) as :transfers_completed,
	Count(distinct etri.id) as :transfers_total,
	IF(tt.id is not null, ttp.id is not null, null) as :transfers_has_problem,
	TIMEDIFF(Max(tt.end_time),Min(tt.start_time)) as :total_transfer_time_by_transfer_tasks,
	TIMEDIFF(et.end_time, et.start_time) as :order_task_time_in_progress
from orders o
left join edo_customer_requests ecr on ecr.order_id = o.id
left join edo_tasks et on et.id = ecr.order_task_id
left join edo_transfer_request_iterations etri on etri.order_edo_task_id = et.id
left join edo_transfer_requests etr on etr.iteration_id = etri.id
left join edo_tasks tt on tt.id = etr.transfer_edo_task_id
left join edo_task_problems ttp on ttp.edo_task_id = tt.id and ttp.state = 'Active'
where (et.`type` not in ('Transfer') or et.id IS NULL)
and o.delivery_date >= :delivery_date_from and o.delivery_date <= :delivery_date_to
{filterSql}
group by o.id, ecr.id
{havingSql}
order by o.delivery_date desc
";

				var query = uow.Session.CreateSQLQuery(sql)
					.MapParametersToNode<EdoProblemJournalNode>()
					// .Map("order_id", x => x.OrderId, NHibernateUtil.Int32)
					// .Map("delivery_date", x => x.DeliveryDate, NHibernateUtil.DateTime)
					// .Map("order_status", x => x.OrderStatus, new EnumStringType<OrderStatus>())
					// .Map("customer_request_time", x => x.CustomerRequestTime, NHibernateUtil.DateTime)
					// .Map("customer_request_source", x => x.CustomerRequestSource, new EnumStringType<CustomerEdoRequestSource>())
					// .Map("order_task_id", x => x.OrderTaskId, NHibernateUtil.Int32)
					// .Map("order_task_type", x => x.OrderTaskType, new EnumStringType<EdoTaskType>())
					// .Map("order_task_status", x => x.OrderTaskStatus, new EnumStringType<EdoTaskStatus>())
					// .Map("order_task_document_stage", x => x.OrderTaskDocumentStage, new EnumStringType<DocumentEdoTaskStage>())
					// .Map("order_task_receipt_stage", x => x.OrderTaskReceiptStage, new EnumStringType<EdoReceiptStatus>())
					// .Map("transfers_completed", x => x.TransfersCompleted, NHibernateUtil.Int32)
					// .Map("transfers_total", x => x.TransfersTotal, NHibernateUtil.Int32)
					// .Map("transfers_has_problem", x => x.TransfersHasProblems, NHibernateUtil.Boolean)
					// .Map("total_transfer_time_by_transfer_tasks", x => x.TotalTransferTimeByTransferTasks, NHibernateUtil.TimeAsTimeSpan)
					// .Map("order_task_time_in_progress", x => x.OrderTaskTimeInProgress, NHibernateUtil.TimeAsTimeSpan)
					.SetResultTransformer();

				query.SetParameter("delivery_date_from", _filterViewModel.DeliveryDateFrom);
				query.SetParameter("delivery_date_to", _filterViewModel.DeliveryDateTo);

				if(_filterViewModel.OrderId.HasValue)
				{
					query.SetParameter("filter_order_id", _filterViewModel.OrderId.Value);
				}

				if(_filterViewModel.EdoTaskStatus.HasValue)
				{
					query.SetParameter(
						"filter_task_status", 
						_filterViewModel.EdoTaskStatus.Value, 
						new EnumStringType<EdoTaskStatus>()
					);
				}

				if(_filterViewModel.TaskProblemState.HasValue)
				{
					query.SetParameter(
						"filter_document_stage",
						_filterViewModel.TaskProblemState.Value,
						new EnumStringType<TaskProblemState>()
					);
				}

				var result = query.ListAsync<EdoProblemJournalNode>(token).Result;
				return result;
			}
		}
	}
}
