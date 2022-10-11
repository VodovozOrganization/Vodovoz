using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	public partial class DistrictView : TabViewBase<DistrictViewModel>
	{
		public DistrictView(DistrictViewModel setViewModel) : base(setViewModel)
		{
			this.Build();
		}
	}
}
