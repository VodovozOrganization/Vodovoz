namespace VodovozInfrastructure.Interfaces
{
    public interface IFileChooserProvider
    {
        string GetExportFilePath(string fileName = null);
        string GetAttachedFileName();
        void CloseWindow();
        void HideWindow();
    }
}
