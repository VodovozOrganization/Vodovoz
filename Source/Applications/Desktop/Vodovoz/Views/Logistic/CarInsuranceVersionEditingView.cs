using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Widgets.Cars.Insurance;
namespace Vodovoz.Views.Logistic
{
	[ToolboxItem(true)]
	public partial class CarInsuranceVersionEditingView : WidgetViewBase<CarInsuranceVersionEditingViewModel>
	{
		public CarInsuranceVersionEditingView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
		}
	}
}
