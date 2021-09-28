using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class BitrixServiceSettings : IBitrixServiceSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public BitrixServiceSettings(IParametersProvider parametersProvider)
		{
			this._parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int MaxStatusesInQueueForWorkingService => _parametersProvider.GetIntValue("MaxStatusesInQueueForWorkingService");

		public int EmployeeForOrderCreate => _parametersProvider.GetIntValue("сотрудник_по_умолчанию_для_службы_Bitrix");

		public int ActiveOnlineStoreId => _parametersProvider.GetIntValue("active_online_store_id");

		public string OsrmServiceURL => _parametersProvider.GetStringValue("osrm_service_url");
	}
}