using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Data.NHibernate.Repositories
{
	public class ReceiptCorrectionRepository : IReceiptCorrectionRepository
	{
		public ReceiptCorrectionProcess GetLatestCompletedProcessForOrder(IUnitOfWork uow, int orderId)
		{
			return uow.Session.Query<ReceiptCorrectionProcess>()
				.Where(x => x.OrderId == orderId && x.Status == ReceiptCorrectionProcessStatus.Completed)
				.OrderByDescending(x => x.CompletedDate)
				.ThenByDescending(x => x.Id)
				.FirstOrDefault();
		}

		public EdoFiscalDocument GetLatestCompletedSaleDocumentForOrder(IUnitOfWork uow, int orderId)
		{
			var tasks = uow.Session.Query<ReceiptEdoTask>()
				.Where(task => task.FormalEdoRequest.Order.Id == orderId)
				.ToList();

			return tasks
				.SelectMany(task => task.FiscalDocuments)
				.Where(document => document.DocumentType == FiscalDocumentType.Sale
					&& document.Stage == FiscalDocumentStage.Completed)
				.OrderByDescending(document => document.FiscalTime)
				.ThenByDescending(document => document.Id)
				.FirstOrDefault();
		}

		public bool ProcessExistsByFingerprint(IUnitOfWork uow, int orderId, string changeFingerprint)
		{
			return uow.Session.Query<ReceiptCorrectionProcess>()
				.Any(x => x.OrderId == orderId && x.ChangeFingerprint == changeFingerprint);
		}

		public IList<int> GetActiveProcessIds(IUnitOfWork uow)
		{
			return uow.Session.Query<ReceiptCorrectionProcess>()
				.Where(x => x.Status == ReceiptCorrectionProcessStatus.Pending
					|| x.Status == ReceiptCorrectionProcessStatus.InProgress)
				.OrderBy(x => x.CreatedDate)
				.Select(x => x.Id)
				.ToList();
		}

		public ReceiptCorrectionProcessDocument GetDocumentByGuid(IUnitOfWork uow, Guid documentGuid)
		{
			return uow.Session.Query<ReceiptCorrectionProcessDocument>()
				.FirstOrDefault(x => x.DocumentGuid == documentGuid);
		}

		public IEnumerable<ReceiptCorrectionProcess> GetProcesses(
			IUnitOfWork uow,
			int? orderId,
			ReceiptCorrectionProcessStatus? status,
			ReceiptCorrectionScenarioType? scenarioType)
		{
			var query = uow.Session.Query<ReceiptCorrectionProcess>().AsQueryable();

			if(orderId.HasValue)
			{
				query = query.Where(x => x.OrderId == orderId.Value);
			}

			if(status.HasValue)
			{
				query = query.Where(x => x.Status == status.Value);
			}

			if(scenarioType.HasValue)
			{
				query = query.Where(x => x.ScenarioType == scenarioType.Value);
			}

			return query
				.OrderByDescending(x => x.CreatedDate)
				.ToList();
		}

		public IEnumerable<ReceiptCorrectionExplanatoryNote> GetExplanatoryNotes(
			IUnitOfWork uow,
			int? orderId,
			int? organizationId,
			ReceiptCorrectionProcessStatus? status = null,
			ReceiptCorrectionScenarioType? scenarioType = null)
		{
			var query = uow.Session.Query<ReceiptCorrectionExplanatoryNote>().AsQueryable();

			if(orderId.HasValue)
			{
				query = query.Where(x => x.Process.OrderId == orderId.Value);
			}

			if(organizationId.HasValue)
			{
				query = query.Where(x => x.OrganizationId == organizationId.Value);
			}

			if(status.HasValue)
			{
				query = query.Where(x => x.Process.Status == status.Value);
			}

			if(scenarioType.HasValue)
			{
				query = query.Where(x => x.Process.ScenarioType == scenarioType.Value);
			}

			return query
				.OrderByDescending(x => x.CreatedDate)
				.ToList();
		}
	}
}
