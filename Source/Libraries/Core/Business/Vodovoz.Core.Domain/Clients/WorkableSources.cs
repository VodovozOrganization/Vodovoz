using System.Collections.Generic;

namespace Vodovoz.Core.Domain.Clients
{
	public static class WorkableSources
	{
		public static IEnumerable<Source> SourcesToSendLogoutEvents = new[] { Source.MobileApp, Source.VodovozWebSite };
	}
}
