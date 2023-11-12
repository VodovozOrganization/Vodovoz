using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacs.Admin.Server
{
	public class SettingsRequest
	{
		public int AdministratorId { get; set; }
		public int MaxOperatorsOnBreak { get; set; }
		public TimeSpan MaxBreakTime { get; set; }
		public TimeSpan OperatorInactivityTimeout { get; set; }
		public TimeSpan OperatorKeepAliveInterval { get; set; }
	}
}
