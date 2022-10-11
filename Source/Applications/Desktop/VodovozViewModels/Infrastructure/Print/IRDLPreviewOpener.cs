using System;
using QS.Report;

namespace Vodovoz.Infrastructure.Print
{
	public interface IRDLPreviewOpener
	{
		void OpenRldDocument(Type documentType, IPrintableRDLDocument document);
	}
}
