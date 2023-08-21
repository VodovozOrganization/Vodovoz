﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class DocumentPrintHistoryMap : ClassMap<DocumentPrintHistory>
	{
		public DocumentPrintHistoryMap()
		{
			Table("documents_print_history");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.PrintingTime).Column("printing_time");
			Map(x => x.DocumentType).Column("document_type").CustomType<PrintedDocumentTypeStringType>();

			References(x => x.RouteList).Column("route_list_id");
		}
	}
}
