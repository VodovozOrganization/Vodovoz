using System;
namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IUndeliveredOrdersInfoProvider : IInfoProvider
	{
		DateTime StartDate { get; }
		DateTime EndDate { get; }
	}
}
