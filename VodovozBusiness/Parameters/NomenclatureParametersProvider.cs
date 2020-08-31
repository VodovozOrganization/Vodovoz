using System;
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

        #endregion INomenclatureParametersProvider implementation
    }
}