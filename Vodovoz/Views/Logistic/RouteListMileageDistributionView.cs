using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListMileageDistributionView : TabViewBase<RouteListMileageDistributionViewModel>
	{
		public RouteListMileageDistributionView(RouteListMileageDistributionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{

		}


		/// <summary>
		/// ----------------------------------------------------------------
		/// </summary>
		/// 





		//public bool AskSaveOnClose => _canEdit;

		

	}
}
