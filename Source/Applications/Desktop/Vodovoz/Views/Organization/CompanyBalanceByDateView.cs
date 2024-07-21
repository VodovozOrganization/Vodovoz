using QS.Views.Dialog;
using Vodovoz.Presentation.ViewModels.Organisations;

namespace Vodovoz.Views.Organization
{
	public partial class CompanyBalanceByDateView : DialogViewBase<CompanyBalanceByDateViewModel>
	{
		public CompanyBalanceByDateView(CompanyBalanceByDateViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{

		}
	}
}
