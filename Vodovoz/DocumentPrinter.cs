using System;
using Vodovoz.Domain.Orders.Documents;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using fyiReporting.RdlGtkViewer;
using fyiReporting.RDL;
using QSProjectsLib;

namespace Vodovoz
{
	public class DocumentPrinter
	{		
		public static void Print(IPrintableDocument document)
		{
			PrintAll(new IPrintableDocument[]{ document });
		}		


		public static void PrintAll(IEnumerable<IPrintableDocument> documents)
		{
			PrintOperation printOp;
			printOp = new PrintOperation();
			printOp.Unit = Unit.Points;
			printOp.UseFullPage = true;
			printOp.ShowProgress = true;

			BatchRDLRenderer renderer = new BatchRDLRenderer(documents.Where(doc=>doc.PrintType==PrinterType.RDL));

			renderer.PrepareDocuments();

			printOp.NPages = renderer.PageCount;

			printOp.DrawPage += renderer.DrawPage;
			printOp.Run(PrintOperationAction.PrintDialog, null);
		}			
			

		public static QSTDI.TdiTabBase GetPreviewTab(IPrintableDocument document)
		{
			return new QSReport.ReportViewDlg (document.GetReportInfoForPreview());				
		}
	}		

	public interface IPrintableDocument
	{
		PrinterType PrintType{ get; }
		DocumentOrientation Orientation{ get; }
		QSReport.ReportInfo GetReportInfo ();
		QSReport.ReportInfo GetReportInfoForPreview();
	}

	public enum PrinterType{
		None, RDL, ODT
	}

	public enum DocumentOrientation{
		Portrait,Landscape
	}


}