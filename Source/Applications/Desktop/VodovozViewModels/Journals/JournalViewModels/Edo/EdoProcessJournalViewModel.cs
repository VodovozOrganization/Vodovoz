using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Infrastructure;
using Edo.Transport;
using NHibernate;
using NHibernate.Type;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.ViewModels.Edo;
using Core.Infrastructure;
using Vodovoz.ViewModels.TrueMark;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	public class EdoProcessJournalViewModel : JournalViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly EdoProcessFilterViewModel _filterViewModel;
		private readonly IInteractiveService _interactiveService;
		private readonly IGenericRepository<ReceiptEdoTask> _receiptRepository;
		private readonly IGenericRepository<DocumentEdoTask> _documentRepository;
		private readonly MessageService _messageService;
		private readonly IUserService _userService;
		private readonly IClipboard _clipboard;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly bool _userCanSentReceiptWasSaveCodes;

		public EdoProcessJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			EdoProcessFilterViewModel filterViewModel,
			IInteractiveService interactiveService,
			IGenericRepository<ReceiptEdoTask> receiptRepository,
			IGenericRepository<DocumentEdoTask> documentRepository,
			MessageService messageService,
			IUserService userService,
			IClipboard clipboard,
			IGtkTabsOpener gtkTabsOpener,
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigation = null
			) : base(uowFactory, interactiveService, navigation)
		{
			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}
			
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_receiptRepository = receiptRepository ?? throw new ArgumentNullException(nameof(receiptRepository));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
			_interactiveService = interactiveService;

			Title = "Документооброт с клиентами";

			DataLoader = new AnyDataLoader<EdoProcessJournalNode>(GetNodes);

			SearchEnabled = false;
			_filterViewModel.IsShow = true;
			SelectionMode = JournalSelectionMode.Multiple;

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			CreateNodeActions();
			CreatePopupActions();
			
			_userCanSentReceiptWasSaveCodes =
				_userService.GetCurrentUser().IsAdmin
				|| currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CashReceipt.CanResendDuplicateReceipts);
		}

		public override IJournalFilterViewModel JournalFilter 
		{ 
			get => _filterViewModel; 
			protected set => throw new NotSupportedException("Установка фильтра выполняется через конструктор"); 
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateResendReceiptFromSaveCodesTaskAction();
			CreateResendDocumentFromSaveCodesTaskAction();
		}

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();
			CreateCopyOrderIdToClipboardAction();
			CreateOpenOrderAction();
			CreateCopyTaskIdToClipboardAction();
			CreateOpenTenderPopupAction();
			CreateOpenOrderCodesAction();
		}

		private void CreateOpenTenderPopupAction()
		{
			var action = new JournalAction(
				"Открыть задачу по госзаказу",
				selectedItems => true,
				selectedItems => selectedItems.FirstOrDefault() is EdoProcessJournalNode selectedNode && selectedNode.OrderTaskType == EdoTaskType.Tender,
				selectedItems =>
				{
					if(selectedItems.FirstOrDefault() is EdoProcessJournalNode selectedNode
					   && selectedNode.OrderTaskId != null)
					{
						NavigationManager.OpenViewModel<TenderEdoViewModel, IEntityUoWBuilder>(this,
							EntityUoWBuilder.ForOpen(selectedNode.OrderTaskId.Value));
					}
				}
			);

			PopupActionsList.Add(action);
		}


		private void CreateResendReceiptFromSaveCodesTaskAction()
		{
			var action = new JournalAction(
				"Отправить чек, ушедший в сохранение кодов",
				sensitive => sensitive.Any() && sensitive.All(x =>
					x is EdoProcessJournalNode edoTask
					&& edoTask.OrderTaskType == EdoTaskType.Receipt
					&& edoTask.OrderTaskReceiptStage == EdoReceiptStatus.SavedToPool
					&& edoTask.OrderTaskStatus == EdoTaskStatus.Completed),
				visible => _userCanSentReceiptWasSaveCodes,
				async selected =>
				{
					var selectedNodes = selected.Cast<EdoProcessJournalNode>().ToList();
					
					using(var uow = _uowFactory.CreateWithoutRoot("Обработка переотправки чеков с кодами, сохраненными в пул"))
					{
						foreach(var selectedNode in selectedNodes)
						{
							if(selectedNode.OrderTaskType != EdoTaskType.Receipt
								|| selectedNode.OrderTaskReceiptStage != EdoReceiptStatus.SavedToPool
								|| selectedNode.OrderTaskStatus != EdoTaskStatus.Completed)
							{
								continue;
							}

							var orderId = selectedNode.OrderId;

							var tasks = _receiptRepository.Get(
									uow,
									f => f.FormalEdoRequest.Order.Id == orderId && f.Id != selectedNode.OrderTaskId)
								.ToList();

							if(tasks.Any(x => x.ReceiptStatus != EdoReceiptStatus.SavedToPool))
							{
								_interactiveService.ShowMessage(
									ImportanceLevel.Warning,
									$"Переотправка чека невозможна, т.к. помимо задачи на сохранение кодов по заказу {orderId}, есть другая задача");
								continue;
							}

							var newRequest = new PrimaryEdoRequest
							{
								Order = new Order
								{
									Id = orderId
								},
								Time = DateTime.Now,
								Source = CustomerEdoRequestSource.Manual,
								DocumentType = EdoDocumentType.UPD
							};

							await uow.SaveAsync(newRequest);
							await uow.CommitAsync();

							await _messageService.PublishEdoRequestCreatedEvent(newRequest.Id);
						}
					}
				}
			);
			
			NodeActionsList.Add(action);
		}

		private void CreateResendDocumentFromSaveCodesTaskAction()
		{
			var action = new JournalAction(
				"Отправить документ, ушедший в сохранение кодов",
				sensitive => sensitive.Any(),
				visible => _userService.GetCurrentUser().IsAdmin,
				async selected =>
				{
					var selectedNodes = selected.Cast<EdoProcessJournalNode>().ToList();

					using(var uow = _uowFactory.CreateWithoutRoot("Обработка переотправки документов с кодами, сохраненными в пул"))
					{
						foreach(var selectedNode in selectedNodes)
						{
							if(selectedNode.OrderTaskType != EdoTaskType.SaveCode
							   || selectedNode.OrderTaskStatus != EdoTaskStatus.Completed)
							{
								continue;
							}

							var orderId = selectedNode.OrderId;

							var tasks = _documentRepository.Get(
									uow,
									t => t.FormalEdoRequest.Order.Id == orderId && t.Id != selectedNode.OrderTaskId)
								.ToList();

							if(tasks.Any(x => x.TaskType != EdoTaskType.SaveCode))
							{
								_interactiveService.ShowMessage(
									ImportanceLevel.Warning,
									$"Переотправка документа невозможна, т.к. помимо задачи на сохранение кодов по заказу {orderId}, есть другая задача");
								continue;
							}

							var newRequest = new PrimaryEdoRequest
							{
								Order = new Order
								{
									Id = orderId
								},
								Time = DateTime.Now,
								Source = CustomerEdoRequestSource.Manual,
								DocumentType = EdoDocumentType.UPD
							};

							await uow.SaveAsync(newRequest);
							await uow.CommitAsync();

							await _messageService.PublishEdoRequestCreatedEvent(newRequest.Id);
						}
					}
				}
			);

			NodeActionsList.Add(action);
		}

		private void CreateCopyOrderIdToClipboardAction()
		{
			var action = new JournalAction(
				"Скопировать номер заказа",
				selected => selected.Any(),
				selected => true,
				selected =>
				{
					var selectedNodes = selected.Cast<EdoProcessJournalNode>().ToList();

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
					var selectedNode = selected.Cast<EdoProcessJournalNode>().FirstOrDefault();

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
					var selectedNodes = selected.Cast<EdoProcessJournalNode>().ToList();

					var orderIds = string.Join(", ", selectedNodes.Select(x => x.OrderTaskId));
					_clipboard.SetText(orderIds);
				}
			);

			PopupActionsList.Add(action);
		}

		private void CreateOpenOrderCodesAction()
		{
			var action = new JournalAction(
				"Просмотр кодов по заказу",
				selected => selected.Count() == 1,
				selected => true,
				selected =>
				{
					var selectedNode = selected.FirstOrDefault() as EdoProcessJournalNode;
					if(selectedNode == null)
					{
						return;
					}
					NavigationManager.OpenViewModel<OrderCodesViewModel, int>(null, selectedNode.OrderId, OpenPageOptions.IgnoreHash);
				}
			);

			PopupActionsList.Add(action);
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		private IList<EdoProcessJournalNode> GetNodes(CancellationToken token)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var filterSql = "";
				var havingSql = "";

				if(_filterViewModel.OrderId.HasValue)
				{
					filterSql += " and o.id = :filter_order_id";
				}

				if(_filterViewModel.RequestSource.HasValue)
				{
					filterSql += " and ecr.source = :filter_request_source";
				}

				if(_filterViewModel.EdoTaskType.HasValue)
				{
					filterSql += " and et.type = :filter_task_type";
				}

				if(_filterViewModel.EdoTaskStatus.HasValue)
				{
					filterSql += " and et.status = :filter_task_status";
				}

				if(_filterViewModel.DocumentTaskStage.HasValue)
				{
					filterSql += " and et.document_task_stage = :filter_document_stage";
				}

				if(_filterViewModel.ReceiptTaskStage.HasValue)
				{
					filterSql += " and et.receipt_status = :filter_receipt_stage";
				}

				if(_filterViewModel.AllTransfersComplete.HasValue)
				{
					if(_filterViewModel.AllTransfersComplete.Value)
					{
						havingSql += " HAVING Count(distinct if(etri.status = 'Completed', etri.id, null)) = Count(distinct etri.id) and Count(distinct if(etri.status = 'Completed', etri.id, null)) > 0";
					}
					else
					{
						havingSql += " HAVING Count(distinct if(etri.status = 'Completed', etri.id, null)) != Count(distinct etri.id) and Count(distinct etri.id) > 0";
					}
				}

				if(_filterViewModel.HasTransferProblem.HasValue)
				{
					if(_filterViewModel.HasTransferProblem.Value)
					{
						filterSql += " and ttp.id is not null";
					}
					else
					{
						filterSql += " and ttp.id is null";
					}
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
					.MapParametersToNode<EdoProcessJournalNode>()
					.Map("order_id", x => x.OrderId, NHibernateUtil.Int32)
					.Map("delivery_date", x => x.DeliveryDate, NHibernateUtil.DateTime)
					.Map("order_status", x => x.OrderStatus, new EnumStringType<OrderStatus>())
					.Map("customer_request_time", x => x.CustomerRequestTime, NHibernateUtil.DateTime)
					.Map("customer_request_source", x => x.CustomerRequestSource, new EnumStringType<CustomerEdoRequestSource>())
					.Map("order_task_id", x => x.OrderTaskId, NHibernateUtil.Int32)
					.Map("order_task_type", x => x.OrderTaskType, new EnumStringType<EdoTaskType>())
					.Map("order_task_status", x => x.OrderTaskStatus, new EnumStringType<EdoTaskStatus>())
					.Map("order_task_document_stage", x => x.OrderTaskDocumentStage, new EnumStringType<DocumentEdoTaskStage>())
					.Map("order_task_receipt_stage", x => x.OrderTaskReceiptStage, new EnumStringType<EdoReceiptStatus>())
					.Map("transfers_completed", x => x.TransfersCompleted, NHibernateUtil.Int32)
					.Map("transfers_total", x => x.TransfersTotal, NHibernateUtil.Int32)
					.Map("transfers_has_problem", x => x.TransfersHasProblems, NHibernateUtil.Boolean)
					.Map("total_transfer_time_by_transfer_tasks", x => x.TotalTransferTimeByTransferTasks, NHibernateUtil.TimeAsTimeSpan)
					.Map("order_task_time_in_progress", x => x.OrderTaskTimeInProgress, NHibernateUtil.TimeAsTimeSpan)
					.SetResultTransformer();

				query.SetParameter("delivery_date_from", _filterViewModel.DeliveryDateFrom);
				query.SetParameter("delivery_date_to", _filterViewModel.DeliveryDateTo);

				if(_filterViewModel.OrderId.HasValue)
				{
					query.SetParameter("filter_order_id", _filterViewModel.OrderId.Value);
				}

				if(_filterViewModel.RequestSource.HasValue)
				{
					query.SetParameter(
						"filter_request_source", 
						_filterViewModel.RequestSource.Value, 
						new EnumStringType<CustomerEdoRequestSource>()
					);
				}

				if(_filterViewModel.EdoTaskType.HasValue)
				{
					query.SetParameter(
						"filter_task_type", 
						_filterViewModel.EdoTaskType.Value, 
						new EnumStringType<EdoTaskType>()
					);
				}

				if(_filterViewModel.EdoTaskStatus.HasValue)
				{
					query.SetParameter(
						"filter_task_status", 
						_filterViewModel.EdoTaskStatus.Value, 
						new EnumStringType<EdoTaskStatus>()
					);
				}

				if(_filterViewModel.DocumentTaskStage.HasValue)
				{
					query.SetParameter(
						"filter_document_stage",
						_filterViewModel.DocumentTaskStage.Value,
						new EnumStringType<DocumentEdoTaskStage>()
					);
				}

				if(_filterViewModel.ReceiptTaskStage.HasValue)
				{
					query.SetParameter(
						"filter_receipt_stage",
						_filterViewModel.ReceiptTaskStage.Value,
						new EnumStringType<EdoReceiptStatus>()
					);
				}

				var result = query.ListAsync<EdoProcessJournalNode>(token).Result;
				return result;
			}
		}
	}
}
