using QS.Project.Filter;
using Vodovoz.FilterViewModels;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IComplaintsInfoProvider : IInfoProvider
	{
		ComplaintFilterViewModel ComplaintsFilterViewModel { get; }
	}
}
