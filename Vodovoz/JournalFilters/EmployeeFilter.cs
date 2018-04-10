using System;
using NHibernate;
using NHibernate.Criterion;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	public partial class EmployeeFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
				enumcomboCategory.ItemsEnum = typeof(EmployeeCategory);
			}
		}

		public EmployeeFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public EmployeeFilter()
		{
			this.Build();
		}

		public EmployeeFilter(bool showFired) : this()
		{
			RestrictFired = showFired;
		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered()
		{
			if(Refiltered != null)
				Refiltered(this, new EventArgs());
		}

		#endregion

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

