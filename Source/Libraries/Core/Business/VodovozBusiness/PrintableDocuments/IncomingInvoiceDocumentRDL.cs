using System.Collections.Generic;
using FluentNHibernate.Data;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.PrintableDocuments
{
    public class IncomingInvoiceDocumentRDL : IPrintableRDLDocument
    {

        public IncomingInvoiceDocumentRDL(IncomingInvoice document, int? currentEmployeeId)
        {
            Document = document;
            CurrentEmployeeId = currentEmployeeId;
        }

        public string Title { get; set; } = "Документ перемещения";
        public PrinterType PrintType { get; }
        public DocumentOrientation Orientation { get; }
        public int CopiesToPrint { get; set; }
        public string Name { get; }
        
        public IncomingInvoice Document { get; }
        public int? CurrentEmployeeId { get; }

        public ReportInfo GetReportInfo(string connectionString = null)
        {
            return new ReportInfo {
                Title = Title,
                Identifier = "Store.IncomingInvoice",
                Parameters = new Dictionary<string, object> {
                    { "document_id",  Document.Id },
                    { "printed_by_id",  CurrentEmployeeId ?? 0 }
                }
            };
        }

        public Dictionary<object, object> Parameters { get; set; }
    }
}
