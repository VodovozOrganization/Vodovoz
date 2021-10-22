using System;
using Vodovoz.ViewModels.ViewModels.Logistic;
using QS.Views.GtkUI;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ManufacturerCarsView : TabViewBase<ManufacturerCarsViewModel>
	{
		public ManufacturerCarsView(ManufacturerCarsViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
