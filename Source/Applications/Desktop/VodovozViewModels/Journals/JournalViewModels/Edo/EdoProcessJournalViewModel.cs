using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VodovozOrder = Vodovoz.Domain.Orders.Order;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Documents;
using System.Threading;
using NHibernate.Transform;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Employees.Journals;
using NLog;
using Gamma.Utilities;
using NHibernate.Type;
using System.Linq.Expressions;
using NPOI.SS.Formula.Functions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	public class EdoProcessJournalNode
	{
		private CustomerEdoRequestSource? _customerRequestSource;
		private EdoTaskType? _orderTaskType;
		private EdoTaskStatus? _orderTaskStatus;
		private DocumentEdoTaskStage? _orderTaskDocumentStage;
		private EdoReceiptStatus? _edoReceiptStatus;
		private TimeSpan? _totalTransferTimeByTransferTasks;
		private TimeSpan? _orderTaskTimeInProgress;


		public int OrderId { get; set; }

		public DateTime DeliveryDate { get; set; }

		public DateTime CustomerRequestTime { get; internal set; }

		public string CustomerRequestSourceTitle { get; set; }
		public CustomerEdoRequestSource? CustomerRequestSource
		{
			get => _customerRequestSource;
			set
			{
				_customerRequestSource = value;
				CustomerRequestSourceTitle = value.GetEnumTitle();
			}
		}

		public int OrderTaskId { get; set; }

		public string OrderTaskTypeTitle { get; set; }
		public EdoTaskType? OrderTaskType
		{
			get => _orderTaskType;
			set
			{
				_orderTaskType = value;
				OrderTaskTypeTitle = value.GetEnumTitle();
			}
		}

		public string OrderTaskStatusTitle { get; set; }
		public EdoTaskStatus? OrderTaskStatus
		{
			get => _orderTaskStatus;
			set
			{
				_orderTaskStatus = value;
				OrderTaskStatusTitle = value.GetEnumTitle();
			}
		}

		public string OrderTaskDocumentStageTitle { get; set; }
		public DocumentEdoTaskStage? OrderTaskDocumentStage
		{
			get => _orderTaskDocumentStage;
			set
			{
				_orderTaskDocumentStage = value;
				if(_orderTaskDocumentStage != null)
				{
					OrderTaskDocumentStageTitle = value.GetEnumTitle();
				}
			}
		}

		public string OrderTaskReceiptStageTitle { get; set; }
		public EdoReceiptStatus? OrderTaskReceiptStage
		{
			get => _edoReceiptStatus;
			set
			{
				_edoReceiptStatus = value;
				if(_edoReceiptStatus != null)
				{
					OrderTaskReceiptStageTitle = value.GetEnumTitle();
				}
			}
		}

		public string TaskStage
		{
			get
			{
				if(OrderTaskDocumentStage != null)
				{
					return OrderTaskDocumentStageTitle;
				}

				if(OrderTaskReceiptStage != null)
				{
					return OrderTaskReceiptStageTitle;
				}

				return "";
			}
		}

		public int TransfersCompleted { get; set; }
		public int TransfersTotal { get; set; }
		public string TransfersCompletedTitle => $"{TransfersCompleted}/{TransfersTotal}";

		public bool TransfersHasProblems { get; set; }

		public string TotalTransferTimeByTransferTasksTitle { get; set; }
        public TimeSpan? TotalTransferTimeByTransferTasks
		{
			get => _totalTransferTimeByTransferTasks;
			set
			{
				_totalTransferTimeByTransferTasks = value;
				if(TotalTransferTimeByTransferTasks != null)
				{
					TotalTransferTimeByTransferTasksTitle = _totalTransferTimeByTransferTasks.Value.ToString(@"hh\:mm\:ss");
				}
			}
		}

		public string OrderTaskTimeInProgressTitle { get; set; }
		public TimeSpan? OrderTaskTimeInProgress
		{
			get => _orderTaskTimeInProgress;
			set
			{
				_orderTaskTimeInProgress = value;
				if(OrderTaskTimeInProgress != null)
				{
					OrderTaskTimeInProgressTitle = OrderTaskTimeInProgressTitle = _orderTaskTimeInProgress.Value.ToString(@"hh\:mm\:ss");
				}
			}
		}
	}

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
				var filterSql = @"";
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
						filterSql += " and Count(distinct if(etri.status = 'Completed', etri.id, null)) = Count(distinct etri.id)";
					}
					else
					{
						filterSql += " and Count(distinct if(etri.status = 'Completed', etri.id, null)) != Count(distinct etri.id)";
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
group by o.id, ecr.id
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


	public static class NhibernateSqlQueryMapExtenstions
	{
		/// <summary>
		/// Добавляет параметр в запрос и мапит его на свойство объекта
		/// </summary>
		/// <typeparam name="Node">Тип ноды используемый в трансформере AliasToBean</typeparam>
		/// <param name="parameter">Имя парметра. В тексте SQL запроса должен быть записан как :parameter_name</param>
		/// <param name="propertySelection">Выражение для выбра свойства ноды на которе будет маппиться параметр</param>
		/// <param name="type">Тип данных параметра</param>
		public static ISQLQuery MapScalarParameter<Node>(
			this ISQLQuery query, 
			string parameter,
			Expression<Func<Node, object>> propertySelection, 
			IType type
			)
		{
			var propertyName = PropertyUtil.GetName(propertySelection);

			query.SetParameter(parameter, propertyName);
			query.AddScalar(propertyName, type);
			return query;
		}

		/// <summary>
		/// Настраивает маппинг параметров на свойства ноды
		/// </summary>
		/// <typeparam name="Node">Тип ноды</typeparam>
		public static ISQLQueryParameterMapping<Node> MapParametersToNode<Node>(this ISQLQuery query)
			where Node : class
		{
			return new SQLQueryParameterMapping<Node>(query);
		}

		public interface ISQLQueryParameterMapping<Node>
			where Node : class
		{
			/// <summary>
			/// Добавляет параметр в запрос и мапит его на свойство объекта
			/// </summary>
			/// <typeparam name="Node">Тип ноды используемый в трансформере AliasToBean</typeparam>
			/// <param name="parameter">Имя парметра. В тексте SQL запроса должен быть записан как :parameter_name</param>
			/// <param name="propertySelection">Выражение для выбра свойства ноды на которе будет маппиться параметр</param>
			/// <param name="type">Тип данных параметра</param>
			ISQLQueryParameterMapping<Node> Map(
				string parameter,
				Expression<Func<Node, object>> propertySelection,
				IType type
			);

			/// <summary>
			/// Устанавливает AliasToBean трансформер
			/// </summary>
			/// <returns></returns>
			IQuery SetResultTransformer();
		}

		private class SQLQueryParameterMapping<Node> : ISQLQueryParameterMapping<Node>
			where Node : class
		{
			private readonly ISQLQuery _query;

			public SQLQueryParameterMapping(ISQLQuery query)
			{
				_query = query ?? throw new ArgumentNullException(nameof(query));
			}

			public ISQLQueryParameterMapping<Node> Map(
				string parameter,
				Expression<Func<Node, object>> propertySelection,
				IType type
				)
			{
				var propertyName = PropertyUtil.GetName(propertySelection);

				_query.SetParameter(parameter, propertyName);
				_query.AddScalar(propertyName, type);

				return this;
			}

			public IQuery SetResultTransformer()
			{
				return _query.SetResultTransformer(Transformers.AliasToBean<Node>());
			}
		}
	}
}
