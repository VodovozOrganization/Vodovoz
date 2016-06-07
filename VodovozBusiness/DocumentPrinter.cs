using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
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
			var documentsRDL = documents.Where(doc=>doc.PrintType==PrinterType.RDL);
			BatchRDLRenderer renderer = new BatchRDLRenderer(documentsRDL);

			var result = LongOperationDlg.StartOperation(
				renderer.PrepareDocuments,
				"Подготовка к печати...",
				documentsRDL.Count()
			);
			if (result == LongOperationResult.Canceled)
				return;
			
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