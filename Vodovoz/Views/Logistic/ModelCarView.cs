using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ModelCarView : TabViewBase<ModelCarViewModel>
	{
		public ModelCarView(ModelCarViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}
	}
}
