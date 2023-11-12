using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	public class SettingsEvent
	{
		public IPacsDomainSettings Settings { get; set; }
	}
}
