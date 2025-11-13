using System;
using System.Collections.Generic;
using System.Text;

namespace Vodovoz.Settings.Common
{
	public interface ISlaveDbConnectionSettings
	{
		bool SlaveConnectionEnabled { get; }
		string SlaveConnectionEnabledForThisDatabase { get; }
		string SlaveConnectionHost { get; }
		int SlaveConnectionPort { get; }
	}
}
