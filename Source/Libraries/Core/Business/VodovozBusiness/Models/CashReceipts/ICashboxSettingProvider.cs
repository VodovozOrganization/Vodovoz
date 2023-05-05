using System.Collections.Generic;
using Vodovoz.Models.CashReceipts.DTO;

namespace Vodovoz.Models.CashReceipts
{
	public interface ICashboxSettingProvider
	{
		IEnumerable<CashboxSetting> GetCashBoxSettings();
	}
}
