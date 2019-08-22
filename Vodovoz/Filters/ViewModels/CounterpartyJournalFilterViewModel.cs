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
				x => x.RestrictIncludeCustomer,
				x => x.RestrictIncludeSupplier,
				x => x.RestrictIncludePartner,
				x => x.RestrictIncludeArchive,
				x => x.Tag
			);
		}

		private bool restrictIncludeCustomer;
		public virtual bool RestrictIncludeCustomer {
			get => restrictIncludeCustomer;
			set => SetField(ref restrictIncludeCustomer, value, () => RestrictIncludeCustomer);
		}

		private bool restrictIncludeSupplier;
		public virtual bool RestrictIncludeSupplier {
			get => restrictIncludeSupplier;
			set => SetField(ref restrictIncludeSupplier, value, () => RestrictIncludeSupplier);
		}

		private bool restrictIncludePartner;
		public virtual bool RestrictIncludePartner {
			get => restrictIncludePartner;
			set => SetField(ref restrictIncludePartner, value, () => RestrictIncludePartner);
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
