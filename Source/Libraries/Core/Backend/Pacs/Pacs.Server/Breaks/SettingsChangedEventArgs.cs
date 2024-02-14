using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Breaks
{
	internal class SettingsChangedEventArgs : EventArgs
	{
		public IPacsDomainSettings Settings { get; set; }
		public IEnumerable<OperatorState> AllOperatorsBreakStates { get; set; }
	}
}
