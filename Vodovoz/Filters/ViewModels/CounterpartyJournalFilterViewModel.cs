using QS.Project.Filter;
using QS.Project.Journal;
using QS.RepresentationModel.GtkUI;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Representations;

namespace Vodovoz.Filters.ViewModels
{
	public class CounterpartyJournalFilterViewModel : FilterViewModelBase<CounterpartyJournalFilterViewModel>, IJournalFilter
	{
		public CounterpartyJournalFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
			UpdateWith(
				x => x.CounterpartyType,
				x => x.RestrictIncludeArchive,
				x => x.Tag
			);
		}

		private CounterpartyType? counterpartyType;
		public virtual CounterpartyType? CounterpartyType {
			get => counterpartyType;
			set => SetField(ref counterpartyType, value, () => CounterpartyType);
		}

		private bool restrictIncludeArchive;
		public virtual bool RestrictIncludeArchive {
			get => restrictIncludeArchive;
			set => SetField(ref restrictIncludeArchive, value, () => RestrictIncludeArchive);
		}

		private Tag tag;
		public virtual Tag Tag {
			get => tag;
			set => SetField(ref tag, value, () => Tag);
		}

		private IRepresentationModel tagVM;
		public virtual IRepresentationModel TagVM {
			get {
				if(tagVM == null) {
					tagVM = new TagVM(UoW);
				}
				return tagVM;
			}
		}
	}
}
