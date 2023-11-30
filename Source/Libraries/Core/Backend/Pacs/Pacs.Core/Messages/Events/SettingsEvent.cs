using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Events
{
	public class SettingsEvent
	{
		public DomainSettings Settings { get; set; }
	}
}
