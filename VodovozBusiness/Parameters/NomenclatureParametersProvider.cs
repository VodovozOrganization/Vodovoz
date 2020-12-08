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

        private int GetIntValue(string parameterId)
        {
            if(!parametersProvider.ContainsParameter(parameterId)) {
                throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})" );
            }
                
            string value = parametersProvider.GetParameterValue(parameterId);

            if(string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int result))
            {
                throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
            }

            return result;
        }
        
        private decimal GetDecimalValue(string parameterId)
        {
	        if(!parametersProvider.ContainsParameter(parameterId)) {
		        throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})" );
	        }
                
	        string value = parametersProvider.GetParameterValue(parameterId);

	        if(string.IsNullOrWhiteSpace(value) || !decimal.TryParse(value, out decimal result))
	        {
		        throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
	        }

	        return result;
        }
        
        private string GetStringValue(string parameterId)
        {
            if(!parametersProvider.ContainsParameter(parameterId)) {
                throw new InvalidProgramException($"В параметрах базы не настроен параметр ({parameterId})" );
            }
                
            string value = parametersProvider.GetParameterValue(parameterId);

            if(string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidProgramException($"В параметрах базы неверно заполнено значение параметра ({parameterId})");
            }

            return value;
        }

        #region INomenclatureParametersProvider implementation

        public int Folder1cForOnlineStoreNomenclatures {
            get {
                string parameterId = "folder_1c_for_online_store_nomenclatures";
                return GetIntValue(parameterId);
            }
        }
        
        public int MeasurementUnitForOnlineStoreNomenclatures {
            get {
                string parameterId = "measurement_unit_for_online_store_nomenclatures";
                return GetIntValue(parameterId);
            }
        }

        public int RootProductGroupForOnlineStoreNomenclatures {
            get {
                string parameterId = "root_product_group_for_online_store_nomenclatures";
                return GetIntValue(parameterId);
            }
        }

        public int CurrentOnlineStoreId  {
            get {
                string parameterId = "current_online_store_id";
                return GetIntValue(parameterId);
            }
        }

        public string OnlineStoreExportFileUrl  {
            get {
                string parameterId = "online_store_export_file_url";
                return GetStringValue(parameterId);
            }
        }
        
        #region Получение номенклатур воды

		public Nomenclature GetWaterSemiozerie(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_semiozerie_id";
			if(!ParametersProvider.Instance.ContainsParameter(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Семиозерье");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(bottleDepositParameter)));
		}

		public Nomenclature GetWaterKislorodnaya(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_kislorodnaya_id";
			if(!ParametersProvider.Instance.ContainsParameter(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Кислородная");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(bottleDepositParameter)));
		}

		public Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_snyatogorskaya_id";
			if(!ParametersProvider.Instance.ContainsParameter(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Снятогорская");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(bottleDepositParameter)));
		}

		public Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_kislorodnaya_deluxe_id";
			if(!ParametersProvider.Instance.ContainsParameter(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Кислородная Deluxe");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(bottleDepositParameter)));
		}

		public Nomenclature GetWaterStroika(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_stroika_id";
			if(!ParametersProvider.Instance.ContainsParameter(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Стройка");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(bottleDepositParameter)));
		}

		public Nomenclature GetWaterRuchki(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_ruchki_id";
			if(!ParametersProvider.Instance.ContainsParameter(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды С ручками");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(bottleDepositParameter)));
		}
		
		public decimal GetWaterPriceIncrement {
			get {
				var waterPriceParam = "water_price_increment";
				return GetDecimalValue(waterPriceParam);
			}
		}

		#endregion

        #endregion INomenclatureParametersProvider implementation
    }
}