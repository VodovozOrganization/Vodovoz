using System;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointFilter : RepresentationFilterBase<DeliveryPointFilter>
	{
		public DeliveryPointFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public Counterparty Client { get; set; }

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

		public bool RestrictOnlyWithoutStreet {
			get { return checkWithoutStreet.Active; }
			set {
				checkWithoutStreet.Active = value;
				checkWithoutStreet.Sensitive = false;
			}
		}

		protected void OnCheckOnlyNotFoundOsmToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnCheckWithoutStreetToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected override void ConfigureWithUow() { }
	}
}