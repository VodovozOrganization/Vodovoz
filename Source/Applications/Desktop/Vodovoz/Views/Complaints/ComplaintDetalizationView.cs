using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	public partial class ComplaintDetalizationView : TabViewBase<ComplaintDetalizationViewModel>
	{
		public ComplaintDetalizationView(ComplaintDetalizationViewModel viewModel)
			: base(viewModel)
		{
			Build();
		}
	}
}
