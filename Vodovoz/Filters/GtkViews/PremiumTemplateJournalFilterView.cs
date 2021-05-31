using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;

namespace Vodovoz.Filters.GtkViews
{
	public partial class PremiumTemplateJournalFilterView : FilterViewBase<PremiumTemplateJournalFilterViewModel>
	{
		public PremiumTemplateJournalFilterView(PremiumTemplateJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
		}
	}
}
