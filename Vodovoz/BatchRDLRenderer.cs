using System;
using fyiReporting.RDL;
using QSProjectsLib;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders.Documents;
using Gtk;
using fyiReporting.RdlGtkViewer;
using NLog;

namespace Vodovoz
{
	public class BatchRDLRenderer
	{		
		IList<Pages> reportPages;
		IList<IPrintableDocument> documents;
		private Logger log = LogManager.GetCurrentClassLogger ();

		public int PageCount
		{
			get
			{
				return reportPages.Sum(pages => pages.PageCount);
			}
		}

		public BatchRDLRenderer(IEnumerable<IPrintableDocument> documents)
		{							
			this.documents = documents.ToList();
			reportPages = new List<Pages>();
		}

		public void PrepareDocuments(IWorker worker)
		{
			int step = 0;
			foreach (var document in documents)
			{
				worker.ReportProgress(step, (document as OrderDocument).Name);
				Prepare(document);
				step++;
			}
		}

		protected void Prepare(IPrintableDocument document)
		{
			if (document.PrintType == PrinterType.RDL)
			{
				Report report = GetReportFromFile(document.GetReportInfo().GetReportUri().LocalPath);
				report.RunGetData(document.GetReportInfo().Parameters);
				reportPages.Add(report.BuildPages());
			}
		}

		private Report GetReportFromFile(string path)
		{
			RDLParser rdlp;
			Report r;
			string source = System.IO.File.ReadAllText(path);

			rdlp = new RDLParser(source);
			rdlp.Folder = System.IO.Path.GetDirectoryName (path);
			rdlp.OverwriteConnectionString = QSMain.ConnectionString;
			rdlp.OverwriteInSubreport = true;

			r = rdlp.Parse();

			return r;
		}

		public void DrawPage(object o,DrawPageArgs args){
			var g = args.Context.CairoContext;
			var printContext = args.Context;
			int pageNumber = args.PageNr;
			int reportNumber = 0;
			while (pageNumber >= reportPages[reportNumber].PageCount)
			{
				pageNumber -= reportPages[reportNumber].PageCount;
				reportNumber++;
			}

			g.Save();
			if (documents[reportNumber].Orientation == DocumentOrientation.Landscape)
			{
				g.Translate(printContext.Width, 0);
				g.Rotate(Math.PI / 2);
			}
			RenderCairo render = new RenderCairo(g);
			render.RunPage(reportPages[reportNumber][pageNumber]);		
			g.Restore();
		}
	}
}

