using System.Collections.Generic;
using VodovozBusiness.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public interface ITrueMarkOrganizationClientSettingProvider
	{
		IEnumerable<TrueMarkOrganizationClientSetting> GetModulKassaOrganizationSettings();
	}
}
