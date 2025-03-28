using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using VodovozBusiness.EntityRepositories.Edo;
using VodovozBusiness.Nodes;

namespace Vodovoz.Infrastructure.Persistance.Edo
{
	public class EdoDocflowRepository : IEdoDocflowRepository
	{
		public IList<EdoDockflowData> GetEdoDocflowDataByOrderId(IUnitOfWork uow, int orderId)
		{
			var data =
				from orderEdoRequest in uow.Session.Query<OrderEdoRequest>()
				join oet in uow.Session.Query<OrderEdoTask>() on orderEdoRequest.Task.Id equals oet.Id into orderEdoTasks
				from orderEdoTask in orderEdoTasks.DefaultIfEmpty()
				join oed in uow.Session.Query<OrderEdoDocument>() on orderEdoTask.Id equals oed.DocumentTaskId into orderEdoDocuments
				from orderEdoDocument in orderEdoDocuments.DefaultIfEmpty()
				join td in uow.Session.Query<TaxcomDocflow>() on orderEdoDocument.Id equals td.EdoDocumentId into taxcomDocflows
				from taxcomDocflow in taxcomDocflows.DefaultIfEmpty()
				join tda in uow.Session.Query<TaxcomDocflowAction>() on taxcomDocflow.Id equals tda.TaxcomDocflowId into taxcomDocflowActions
				from taxcomDocflowAction in taxcomDocflowActions.DefaultIfEmpty()

				let lastTaxcomDocflowActionTime = (DateTime?)uow.Session.Query<TaxcomDocflowAction>()
					.Where(x => x.TaxcomDocflowId == taxcomDocflow.Id)
					.OrderByDescending(x => x.Id)
					.Select(x => x.Time)
					.FirstOrDefault()

				where
					orderEdoRequest.Order.Id == orderId
					&& (taxcomDocflowAction.Id == null || taxcomDocflowAction.Time == lastTaxcomDocflowActionTime)

				select new EdoDockflowData
				{
					DocFlowId = taxcomDocflow.DocflowId,
					EdoDocFlowStatus = taxcomDocflowAction == null ? default(EdoDocFlowStatus?) : taxcomDocflowAction.State,
					IsReceived = default,// taxcomDocflowActions == null ? default : taxcomDocflowActions.IsReceived,
					ErrorDescription = taxcomDocflowAction == null ? default : taxcomDocflowAction.ErrorMessage,
					IsNewDockflow = true,
					EdoDocumentType = orderEdoDocument.DocumentType,
					EdoTaskStatus = orderEdoTask.Status,
					EdoDocumentStatus = orderEdoDocument.Status
				};

			return data.ToList();
		}
	}
}
