﻿using NHibernate;
using NHibernate.Type;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using System;
using System.Collections.Generic;
using System.Threading;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	public class EdoProcessJournalViewModel : JournalViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly EdoProcessFilterViewModel _filterViewModel;

		public EdoProcessJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			EdoProcessFilterViewModel filterViewModel,
			IInteractiveService interactiveService,
			INavigationManager navigation = null
			) : base(uowFactory, interactiveService, navigation)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			Title = "ЭДО процессы";

			DataLoader = new AnyDataLoader<EdoProcessJournalNode>(GetNodes);

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
		}

		public override IJournalFilterViewModel JournalFilter 
		{ 
			get => _filterViewModel; 
			protected set => throw new NotSupportedException("Установка фильтра выполняется через конструктор"); 
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
	ecr.`time` as :customer_request_time,
	ecr.source as :customer_request_source,
	et.id as :order_task_id,
	et.`type` as :order_task_type,
	et.status as :order_task_status,
	et.document_task_stage as :order_task_document_stage,
	et.receipt_status as :order_task_receipt_stage,
	Count(distinct if(etri.status = 'Completed', etri.id, null)) as :transfers_completed,
	Count(distinct etri.id) as :transfers_total,
	ttp.id is not null as :transfers_has_problem,
	TIMEDIFF(Max(tt.end_time),Min(tt.start_time)) as :total_transfer_time_by_transfer_tasks,
	TIMEDIFF(et.end_time, et.start_time) as :order_task_time_in_progress
from orders o
left join edo_customer_requests ecr on ecr.order_id = o.id
left join edo_tasks et on et.id = ecr.order_task_id
left join edo_transfer_request_iterations etri on etri.order_edo_task_id = et.id
left join edo_transfer_requests etr on etr.iteration_id = etri.id
left join edo_tasks tt on tt.id = etr.transfer_edo_task_id
left join edo_task_problems ttp on ttp.edo_task_id = tt.id and ttp.state = 'Active'
where et.`type` not in ('BulkAccounting', 'Transfer')
{filterSql}
group by o.id, ecr.id {havingSql}
order by o.delivery_date desc
";

				var query = uow.Session.CreateSQLQuery(sql)
					.MapParametersToNode<EdoProcessJournalNode>()
					.Map("order_id", x => x.OrderId, NHibernateUtil.Int32)
					.Map("delivery_date", x => x.DeliveryDate, NHibernateUtil.DateTime)
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
