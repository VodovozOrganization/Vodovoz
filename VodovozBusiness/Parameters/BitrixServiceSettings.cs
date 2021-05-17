using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class BitrixServiceSettings : IBitrixServiceSettings
    {
        private readonly IParametersProvider parametersProvider;
        public int MaxStatusesInQueueForWorkingService => parametersProvider.GetIntValue("MaxStatusesInQueueForWorkingService");
        
        public int EmployeeForOrderCreate => parametersProvider.GetIntValue("сотрудник_по_умолчанию_для_службы_Bitrix");
        
        public int ActiveOnlineStoreId => parametersProvider.GetIntValue("active_online_store_id");

        public string OsrmServiceURL => parametersProvider.GetStringValue("osrm_service_url");

        public BitrixServiceSettings(IParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }
    }
}