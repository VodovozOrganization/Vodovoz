using System;
namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IComplaintsInfoProvider : IInfoProvider
	{
		DateTime StartDate { get; }
		DateTime EndDate { get; }
	}
}
