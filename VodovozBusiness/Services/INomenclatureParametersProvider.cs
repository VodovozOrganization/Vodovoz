namespace Vodovoz.Services
{
    public interface INomenclatureParametersProvider
    {
        int Folder1cForOnlineStoreNomenclatures { get; }
        int MeasurementUnitForOnlineStoreNomenclatures { get; }
        int RootProductGroupForOnlineStoreNomenclatures { get; }
        int CurrentOnlineStoreId { get; }
        string OnlineStoreExportFileUrl { get; }
    }
}