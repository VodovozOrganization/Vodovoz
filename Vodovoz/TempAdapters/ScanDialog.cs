using Gtk;
using QSAttachment;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class ScanDialog : IScanDialog
	{
		public void GetFileFromDialog(out string fileName, out byte[] file)
		{
			fileName = string.Empty;
			file = new byte[0];

			var scanDialog = new GetFromScanner();

			scanDialog.Show();

			if(scanDialog.Run() == (int)ResponseType.Ok)
			{
				fileName = scanDialog.FileName;
				file = scanDialog.File;
			}

			scanDialog.Destroy();
		}
	}
}
