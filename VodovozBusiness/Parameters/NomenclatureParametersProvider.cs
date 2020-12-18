using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class NomenclatureParametersProvider : INomenclatureParametersProvider
    {
        private ParametersProvider parametersProvider;
        
        public NomenclatureParametersProvider()
        {
            parametersProvider = ParametersProvider.Instance;
        }
        
        #region INomenclatureParametersProvider implementation

        public int Folder1cForOnlineStoreNomenclatures => parametersProvider.GetIntValue("folder_1c_for_online_store_nomenclatures");

        public int MeasurementUnitForOnlineStoreNomenclatures => parametersProvider.GetIntValue("measurement_unit_for_online_store_nomenclatures");

        public int RootProductGroupForOnlineStoreNomenclatures => parametersProvider.GetIntValue("root_product_group_for_online_store_nomenclatures");

        public int CurrentOnlineStoreId => parametersProvider.GetIntValue("current_online_store_id");

        public string OnlineStoreExportFileUrl => parametersProvider.GetStringValue("online_store_export_file_url");

        #region Получение номенклатур воды

		public Nomenclature GetWaterSemiozerie(IUnitOfWork uow)
		{
			int id = parametersProvider.GetIntValue("nomenclature_semiozerie_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterKislorodnaya(IUnitOfWork uow)
		{
			int id = parametersProvider.GetIntValue("nomenclature_kislorodnaya_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow)
		{
			int id = parametersProvider.GetIntValue("nomenclature_snyatogorskaya_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow)
		{
			int id = parametersProvider.GetIntValue("nomenclature_kislorodnaya_deluxe_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterStroika(IUnitOfWork uow)
		{
			int id = parametersProvider.GetIntValue("nomenclature_stroika_id");
			return uow.GetById<Nomenclature>(id);
		}

		public Nomenclature GetWaterRuchki(IUnitOfWork uow)
		{
			int id = parametersProvider.GetIntValue("nomenclature_ruchki_id");
			return uow.GetById<Nomenclature>(id);
		}
		
		public decimal GetWaterPriceIncrement => parametersProvider.GetDecimalValue("water_price_increment");

		#endregion

        #endregion INomenclatureParametersProvider implementation
    }
}