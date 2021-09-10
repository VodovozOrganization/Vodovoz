namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IScanDialog
	{
		void GetFileFromDialog(out string fileName, out byte[] file);
	}
}
