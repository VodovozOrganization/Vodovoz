using System;
using QS.Report;
using QS.Tdi;
using Vodovoz.Infrastructure.Print;

namespace Vodovoz.Core
{
	public class RdlPreviewOpener : IRDLPreviewOpener
	{
		public void OpenRldDocument(Type documentType, IPrintableRDLDocument document)
		{
			var rdlTab = QSReport.DocumentPrinter.GetPreviewTab(document);
			TDIMain.MainNotebook.OpenTab(documentType.Name + "_rdl", () => rdlTab);
		}
	}
}
