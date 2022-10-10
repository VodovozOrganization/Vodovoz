using QS.Project.Filter;
using QS.Services;

namespace Vodovoz.Filters.ViewModels
{
	public class ClientCameFromFilterViewModel : FilterViewModelBase<ClientCameFromFilterViewModel>
	{
		bool restrictArchive;
		public virtual bool RestrictArchive {
			get => restrictArchive;
			set {
				if(SetField(ref restrictArchive, value, () => RestrictArchive)) {
					Update();
					CanChangeShowArchive = false;
				}
			}
		}
		public bool CanChangeShowArchive { get; private set; } = true;
	}
}