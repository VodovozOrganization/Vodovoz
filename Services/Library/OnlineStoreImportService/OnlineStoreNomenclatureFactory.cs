using System;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;

namespace OnlineStoreImportService
{
    public class OnlineStoreNomenclatureFactory
    {
        private readonly INomenclatureRepository nomenclatureRepository;
        private readonly int measurementUnitForOnlineStoreNomenclatures;
        private readonly int folder1cForOnlineStoreNomenclatures;
        
        public OnlineStoreNomenclatureFactory(INomenclatureParametersProvider nomenclatureParametersProvider, INomenclatureRepository nomenclatureRepository)
        {
            if (nomenclatureParametersProvider == null) {
                throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
            }
            this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));

            measurementUnitForOnlineStoreNomenclatures = nomenclatureParametersProvider.MeasurementUnitForOnlineStoreNomenclatures;
            folder1cForOnlineStoreNomenclatures = nomenclatureParametersProvider.Folder1cForOnlineStoreNomenclatures;
        }
        
        public Nomenclature CreateNewNomenclature(IUnitOfWork uow)
        {
            var nomenclature = new Nomenclature {
                Category = NomenclatureCategory.additional,
                Unit = uow.GetById<MeasurementUnits>(measurementUnitForOnlineStoreNomenclatures),
                Code1c = nomenclatureRepository.GetNextCode1c(uow),
                VAT = VAT.Vat20,
                Folder1C = uow.GetById<Folder1c>(folder1cForOnlineStoreNomenclatures),
                SaleCategory = SaleCategory.forSale
            };
            return nomenclature;
        }
    }
}