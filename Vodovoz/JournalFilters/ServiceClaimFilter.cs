using System;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Service;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ServiceClaimFilter : RepresentationFilterBase<ServiceClaimFilter>
	{
		public ServiceClaimFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public ServiceClaimFilter()
		{
			this.Build();
		}

		protected override void ConfigureFilter()
		{
			comboStatus.ItemsEnum = typeof(ServiceClaimStatus);
			comboType.ItemsEnum = typeof(ServiceClaimType);
		}

		public ServiceClaimStatus? RestrictServiceClaimStatus {
			get { return comboStatus.SelectedItem as ServiceClaimStatus?; }
			set {
				comboStatus.SelectedItem = value;
				comboStatus.Sensitive = false;
			}
		}

		public ServiceClaimType? RestrictServiceClaimType {
			get { return comboType.SelectedItem as ServiceClaimType?; }
			set {
				comboType.SelectedItem = value;
				comboType.Sensitive = false;
			}
		}

		protected void OnComboTypeChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnComboStatusChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

