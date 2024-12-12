using QS.Print;
using System;

namespace Vodovoz.ViewModels.Print
{
	public class PrintEventArgs : EventArgs
	{
		public PrintEventArgs(IPrintableDocument document)
		{
			Document = document;
		}

		public IPrintableDocument Document { get; }
	}
}
