using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vodovoz.Services
{
	public interface ISmsSettings
	{
		string MegafonSenderName { get; }
		SmsProvider SmsProvider { get; }
	}

	public enum SmsProvider
	{
		Megafon,
		SmsRu
	}
}
