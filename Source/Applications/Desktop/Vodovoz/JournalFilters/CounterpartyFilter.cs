using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;
using Vodovoz.Representations;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyFilter : RepresentationFilterBase<CounterpartyFilter>
	{
		protected override void ConfigureWithUow()
		{
			yentryTag.RepresentationModel = new TagVM(UoW);
		}

		public CounterpartyFilter(IUnitOfWork uow)
		{
			this.Build();
			UoW = uow;
			yenumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
		}

		public CounterpartyType? CounterpartyType {
			get { return (CounterpartyType?)yenumCounterpartyType.SelectedItemOrNull; }
			set {
				yenumCounterpartyType.SelectedItemOrNull = value;
			}
		}

		public bool RestrictIncludeArhive {
			get { return checkIncludeArhive.Active; }
			set {
				checkIncludeArhive.Active = value;
				checkIncludeArhive.Sensitive = false;
			}
		}

		public Tag Tag {
			get { return yentryTag.Subject as Tag; }
			set {
				yentryTag.Subject = value;
				yentryTag.Sensitive = false;
			}
		}

		protected void OnCheckIncludeArhiveToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnYentryTagChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnyenumCounterpartyTypeItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			OnRefiltered();
		}
	}
}