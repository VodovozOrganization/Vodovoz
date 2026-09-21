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
			HealFinishedCorrectionProcesses(uow, orderId);

			return uow.Session.Query<ReceiptCorrectionProcess>()
				.Where(x => x.OrderId == orderId && x.Status == ReceiptCorrectionProcessStatus.Completed)
				.OrderByDescending(x => x.CompletedDate)
				.ThenByDescending(x => x.Id)
				.FirstOrDefault();
		}

		private static void HealFinishedCorrectionProcesses(IUnitOfWork uow, int orderId)
		{
			var processes = uow.Session.Query<ReceiptCorrectionProcess>()
				.Where(x => x.OrderId == orderId
					&& (x.Status == ReceiptCorrectionProcessStatus.Pending
						|| x.Status == ReceiptCorrectionProcessStatus.InProgress))
				.ToList();

			foreach(var process in processes)
			{
				if(process.Documents == null || !process.Documents.Any())
				{
					continue;
				}

				var changed = false;
				foreach(var processDocument in process.Documents)
				{
					if(processDocument.Status == ReceiptCorrectionProcessStatus.Completed
						|| processDocument.Status == ReceiptCorrectionProcessStatus.Failed)
					{
						continue;
					}

					var edoDocument = ResolveProcessEdoDocument(uow, processDocument);
					if(edoDocument == null || !IsFiscalizationFinished(edoDocument))
					{
						continue;
					}

					processDocument.Status = ReceiptCorrectionProcessStatus.Completed;
					processDocument.EdoFiscalDocumentId = edoDocument.Id;
					processDocument.ErrorDescription = null;
					changed = true;
				}

				if(!process.Documents.All(x => x.Status == ReceiptCorrectionProcessStatus.Completed))
				{
					if(changed)
					{
						uow.Save(process);
					}

					continue;
				}

				process.Status = ReceiptCorrectionProcessStatus.Completed;
				process.CompletedDate = process.CompletedDate ?? DateTime.Now;
				process.ErrorDescription = null;
				uow.Save(process);
			}
		}

		private static EdoFiscalDocument ResolveProcessEdoDocument(
			IUnitOfWork uow,
			ReceiptCorrectionProcessDocument processDocument)
		{
			if(processDocument.EdoFiscalDocumentId.HasValue)
			{
				var byId = uow.GetById<EdoFiscalDocument>(processDocument.EdoFiscalDocumentId.Value);
				if(byId != null)
				{
					return byId;
				}
			}

			return uow.Session.Query<EdoFiscalDocument>()
				.FirstOrDefault(x => x.DocumentGuid == processDocument.DocumentGuid);
		}

		private static bool IsFiscalizationFinished(EdoFiscalDocument document)
		{
			return document.Stage == FiscalDocumentStage.Completed
				|| document.Status == FiscalDocumentStatus.Completed
				|| document.Status == FiscalDocumentStatus.Printed;
		}

		public EdoFiscalDocument GetLatestCompletedSaleDocumentForOrder(IUnitOfWork uow, int orderId)
		{
			var tasks = uow.Session.Query<ReceiptEdoTask>()
				.Where(task => task.FormalEdoRequest.Order.Id == orderId)
				.ToList();

			return tasks
				.SelectMany(task => task.FiscalDocuments)
				.Where(document => document.DocumentType == FiscalDocumentType.Sale
					&& document.Stage == FiscalDocumentStage.Completed
					&& !IsCorrectionSaleDocumentNumber(document.DocumentNumber))
				.OrderByDescending(document => document.FiscalTime)
				.ThenByDescending(document => document.Id)
				.FirstOrDefault();
		}

		private static bool IsCorrectionSaleDocumentNumber(string documentNumber)
		{
			if(string.IsNullOrWhiteSpace(documentNumber))
			{
				return false;
			}

			var value = documentNumber.Trim();
			var markerIndex = value.LastIndexOf("_c", StringComparison.OrdinalIgnoreCase);
			if(markerIndex < 0 || markerIndex + 2 >= value.Length)
			{
				return false;
			}

			return value.EndsWith("_sale", StringComparison.OrdinalIgnoreCase);
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
