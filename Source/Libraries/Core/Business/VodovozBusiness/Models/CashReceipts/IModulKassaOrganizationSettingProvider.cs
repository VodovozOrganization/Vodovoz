using System.Collections.Generic;
using VodovozBusiness.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public interface IModulKassaOrganizationSettingProvider
	{
		IEnumerable<ModulKassaOrganizationSetting> GetModulKassaOrganizationSettings();
	}
}
