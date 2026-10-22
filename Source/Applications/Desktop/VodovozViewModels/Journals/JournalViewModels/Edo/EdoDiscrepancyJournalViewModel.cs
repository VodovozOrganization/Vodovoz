using Core.Infrastructure;
using NHibernate;
using NHibernate.Type;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.TrueMark;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	public class EdoDiscrepancyJournalViewModel : JournalViewModelBase
	{
		private readonly IClipboard _clipboard;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly ICurrentPermissionService _currentPermissionService;

		public EdoDiscrepancyJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IClipboard clipboard,
			IGtkTabsOpener gtkTabsOpener,
			ICurrentPermissionService currentPermissionService,
			INavigationManager navigationManager
			)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			_clipboard = clipboard ?? throw new System.ArgumentNullException(nameof(clipboard));
			_gtkTabsOpener = gtkTabsOpener ?? throw new System.ArgumentNullException(nameof(gtkTabsOpener));
			_currentPermissionService = currentPermissionService ?? throw new System.ArgumentNullException(nameof(currentPermissionService));

			TabName = "Журнал расхождений ЭДО";

			if(!_currentPermissionService.ValidatePresetPermission(EdoPermissions.CanWorkWithEdoDiscrepancyJournal))
			{
				interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Для работы с журналом расхождений ЭДО недостаточно прав.");
				return;
			}

			DataLoader = new ThreadSqlDataLoader<EdoDiscrepancyJournalNode>(
				unitOfWorkFactory,
				ItemsQuery,
				ItemsCountQuery
			);

			SelectionMode = JournalSelectionMode.Single;

			PopupActionsList.Clear();
			NodeActionsList.Clear();
			CreateOpenEdoDiscrepancyDialogAction();
			CreateOpenEdoInOrderDialogAction();
			CreateCopyOrderIdToClipboardAction();
			CreateOpenOrderAction();
			CreateOpenOrderCodesAction();
		}

		private void CreateOpenEdoDiscrepancyDialogAction()
		{
			var action = new JournalAction(
				"Открыть расхождение",
				selected => selected.Count() == 1,
				selected => true,
				selected =>
				{
					var selectedNode = selected.FirstOrDefault() as EdoDiscrepancyJournalNode;
					if(selectedNode == null)
					{
						return;
					}
					NavigationManager.OpenViewModel<EdoDiscrepancyViewModel, int>(null, selectedNode.DocumentId, OpenPageOptions.IgnoreHash);
				}
			);

			RowActivatedAction = action;
			NodeActionsList.Add(action);
			PopupActionsList.Add(action);
		}

		private void CreateOpenEdoInOrderDialogAction()
		{
			var action = new JournalAction(
				"Открыть диалог ЭДО",
				selected => selected.Count() == 1,
				selected => true,
				selected =>
				{
					var selectedNode = selected.FirstOrDefault() as EdoDiscrepancyJournalNode;
					if(selectedNode == null)
					{
						return;
					}
					NavigationManager.OpenViewModel<EdoViewModel, int>(null, selectedNode.OrderId, OpenPageOptions.IgnoreHash);
				}
			);

			NodeActionsList.Add(action);
			PopupActionsList.Add(action);
		}

		private void CreateCopyOrderIdToClipboardAction()
		{
			var action = new JournalAction(
				"Скопировать номер заказа",
				selected => selected.Any(),
				selected => true,
				selected =>
				{
					var selectedNodes = selected.Cast<EdoDiscrepancyJournalNode>().ToList();

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
					var selectedNode = selected.Cast<EdoDiscrepancyJournalNode>().FirstOrDefault();

					_gtkTabsOpener.OpenOrderDlgFromViewModelByNavigator(this, selectedNode.OrderId);
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
					var selectedNode = selected.FirstOrDefault() as EdoDiscrepancyJournalNode;
					if(selectedNode == null)
					{
						return;
					}
					NavigationManager.OpenViewModel<OrderCodesDialogViewModel, int>(null, selectedNode.OrderId, OpenPageOptions.IgnoreHash);
				}
			);

			PopupActionsList.Add(action);
		}

		protected IList<EdoDiscrepancyJournalNode> ItemsQuery(IUnitOfWork uow, int? skip = null, int? take = null)
		{
			var sql = new StringBuilder($@"
select
	o.id as :order_id,
	o.delivery_date as :delivery_date,
	o.order_status as :order_status,
	doc.document_number as :document_number,
	et.id as :order_task_id,
	eod.id as :document_id,
	et.status as :order_task_status,
	eod.status as :document_status,
	tda.state as :taxcom_status
from edo_outgoing_documents eod
left join edo_tasks et on et.id = eod.document_task_id
left join edo_customer_requests ecr on ecr.order_task_id = et.id
left join orders o on o.id = ecr.order_id
left join document_organization_counters doc on doc.order_id = o.id
left join
(
	SELECT
		taxcom_docflows.*,
		(
			SELECT tda.id
			FROM taxcom_docflow_actions tda
			WHERE tda.taxcom_docflow_id = taxcom_docflows.id
			ORDER BY tda.`time` DESC
			LIMIT 1
		) as last_taxcom_action_id
	FROM taxcom_docflows
) td ON td.edo_document_id = eod.id
left join taxcom_docflow_actions tda on tda.id = td.last_taxcom_action_id
where eod.discrepancy_state = 'Active'
group by eod.id
order by eod.id desc
");
			if(skip.HasValue && take.HasValue)
			{
				sql.AppendLine($"limit {skip.Value}, {take.Value}");
			}

			var query = uow.Session.CreateSQLQuery(sql.ToString())
				.MapParametersToNode<EdoDiscrepancyJournalNode>()
				.Map("order_id", x => x.OrderId, NHibernateUtil.Int32)
				.Map("delivery_date", x => x.DeliveryDate, NHibernateUtil.DateTime)
				.Map("order_status", x => x.OrderStatus, new EnumStringType<OrderStatus>())
				.Map("document_number", x => x.DocumentNumber, NHibernateUtil.String)
				.Map("order_task_id", x => x.TaskId, NHibernateUtil.Int32)
				.Map("document_id", x => x.DocumentId, NHibernateUtil.Int32)
				.Map("order_task_status", x => x.EdoTaskStatus, new EnumStringType<EdoTaskStatus>())
				.Map("document_status", x => x.UpdInErpStatus, new EnumStringType<EdoDocumentStatus>())
				.Map("taxcom_status", x => x.UpdInTaxcomStatus, new EnumStringType<EdoDocFlowStatus>())
				.SetResultTransformer();

			var result = query.List<EdoDiscrepancyJournalNode>();
			return result;
		}

		private int ItemsCountQuery(IUnitOfWork uow)
		{
			var sql = @"
select count(*) 
from edo_outgoing_documents eod
where eod.discrepancy_state = 'Active'
";
			var query = uow.Session.CreateSQLQuery(sql);
			return (int)query.UniqueResult<long>();
		}
	}
}
