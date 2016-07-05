using System;
using QSOrmProject.RepresentationModel;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointFilter : RepresentationFilterBase
	{

		public DeliveryPointFilter()
		{
			this.Build();
		}

		public bool RestrictOnlyNotFoundOsm {
			get { return checkOnlyNotFoundOsm.Active; }
			set {
				checkOnlyNotFoundOsm.Active = value;
				checkOnlyNotFoundOsm.Sensitive = false;
			}
		}

		protected void OnCheckOnlyNotFoundOsmToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

