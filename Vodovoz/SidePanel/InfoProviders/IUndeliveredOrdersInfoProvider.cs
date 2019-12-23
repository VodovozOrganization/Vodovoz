using System;
using Vodovoz.JournalFilters;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IUndeliveredOrdersInfoProvider : IInfoProvider
	{
		UndeliveredOrdersFilter UndeliveredOrdersFilter { get; }
	}
}
