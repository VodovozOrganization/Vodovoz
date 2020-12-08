using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Services
{
    public interface INomenclatureParametersProvider
    {
        int Folder1cForOnlineStoreNomenclatures { get; }
        int MeasurementUnitForOnlineStoreNomenclatures { get; }
        
        int RootProductGroupForOnlineStoreNomenclatures { get; }
        
        int CurrentOnlineStoreId { get; }
        
        string OnlineStoreExportFileUrl { get; }

        Nomenclature GetWaterSemiozerie(IUnitOfWork uow);
        Nomenclature GetWaterKislorodnaya(IUnitOfWork uow);
        Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow);
        Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow);
        Nomenclature GetWaterStroika(IUnitOfWork uow);
        Nomenclature GetWaterRuchki(IUnitOfWork uow);
        decimal GetWaterPriceIncrement { get; }
    }
}