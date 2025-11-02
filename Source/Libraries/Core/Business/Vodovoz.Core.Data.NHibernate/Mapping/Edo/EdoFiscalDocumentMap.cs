using FluentNHibernate.Mapping;
using NHibernate;
using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoFiscalDocumentMap : ClassMap<EdoFiscalDocument>
	{
		public EdoFiscalDocumentMap()
		{
			Table("edo_fiscal_documents");

			OptimisticLock.Version();

			Version(x => x.Version)
				.Column("version");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.CreationTime)
				.Column("creation_time")
				.ReadOnly();

			References(x => x.ReceiptEdoTask)
				.Column("receipt_edo_task_id");

			Map(x => x.Index)
				.Column("document_index");

			Map(x => x.Stage)
				.Column("stage");

			Map(x => x.Status)
				.Column("status");

			Map(x => x.StatusChangeTime)
				.Column("status_change_time");

			Map(x => x.FiscalTime)
				.Column("fiscal_time");

			Map(x => x.FiscalNumber)
				.Column("fiscal_number");

			Map(x => x.FiscalMark)
				.Column("fiscal_mark");

			Map(x => x.FiscalKktNumber)
				.Column("fiscal_kkt_number");

			Map(x => x.FailureMessage)
				.Column("failure_message");

			Map(x => x.DocumentGuid)
				.Column("document_guid");

			Map(x => x.DocumentNumber)
				.Column("document_number");

			Map(x => x.DocumentType)
				.Column("document_type");

			Map(x => x.CheckoutTime)
				.Column("checkout_time");

			Map(x => x.Contact)
				.Column("contact");

			Map(x => x.ClientInn)
				.Column("client_inn");

			Map(x => x.CashierName)
				.Column("cashier_name");

			Map(x => x.PrintReceipt)
				.Column("print_receipt");

			HasMany(x => x.InventPositions)
				.KeyColumn("edo_fiscal_document_id")
				.Cascade.AllDeleteOrphan();

			HasMany(x => x.MoneyPositions)
				.KeyColumn("edo_fiscal_document_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}
