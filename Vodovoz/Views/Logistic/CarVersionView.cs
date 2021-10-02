using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarVersionView : TabViewBase<CarVersionViewModel>
	{
		public CarVersionView(CarVersionViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
