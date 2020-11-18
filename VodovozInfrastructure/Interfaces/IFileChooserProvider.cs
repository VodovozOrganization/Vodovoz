namespace VodovozInfrastructure.Interfaces
{
    public interface IFileChooserProvider
    {
        string GetExportFilePath();
        void CloseWindow();
    }
}