using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public partial class EmployeeFilter : RepresentationFilterBase<EmployeeFilter>
	{
		protected override void ConfigureWithUow()
		{
			enumcomboCategory.ItemsEnum = typeof(EmployeeCategory);
		}

		public EmployeeFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public EmployeeFilter()
		{
			this.Build();
		}

		public EmployeeFilter(IUnitOfWork uow, bool showFired) : this()
		{
			UoW = uow;
			RestrictFired = showFired;
		}

		public EmployeeCategory? RestrictCategory {
			get { return enumcomboCategory.SelectedItem as EmployeeCategory?; }
			set {
				enumcomboCategory.SelectedItem = value;
				enumcomboCategory.Sensitive = false;
			}
		}

		public bool RestrictFired {
			get { return checkFired.Active; }
			set {
				checkFired.Active = value;
				checkFired.Sensitive = false;
			}
		}

		protected void OnCheckFiredToggled(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnEnumcomboCategoryChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

