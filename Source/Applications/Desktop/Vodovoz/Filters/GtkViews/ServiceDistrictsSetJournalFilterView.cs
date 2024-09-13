using QS.Views.GtkUI;
using Vodovoz.Journals.FilterViewModels;
namespace Vodovoz.Filters.GtkViews
{
	public partial class ServiceDistrictsSetJournalFilterView : FilterViewBase<ServiceDistrictsSetJournalFilterViewModel>
	{
		public ServiceDistrictsSetJournalFilterView(ServiceDistrictsSetJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
		}
	}
}
