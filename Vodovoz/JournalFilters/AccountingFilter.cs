using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSOrmProject.RepresentationModel;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class AccountingFilter : Gtk.Bin, IRepresentationFilter
	{
		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				uow = value;
			}
		}

		public AccountingFilter (IUnitOfWork iuow) : this ()
		{
			UoW = iuow;
		}

		public AccountingFilter ()
		{
			this.Build ();
			comboType.ItemsEnum = typeof(OperationType);
			comboType.Sensitive = true;
			comboType.ShowSpecialStateAll = false;
			comboType.Active = 0;

		}

		#region IReferenceFilter implementation

		public event EventHandler Refiltered;

		void OnRefiltered ()
		{
			if (Refiltered != null)
				Refiltered (this, new EventArgs ());
		}

		#endregion

		void UpdateCreteria ()
		{
			OnRefiltered ();
		}

		public OperationType RestrictOperationType {
			get { return (OperationType)comboType.SelectedItem; }
		}

		public DateTime? RestrictStartDate {
			get { return periodPicker.StartDateOrNull; }
			set {
				periodPicker.StartDateOrNull = value;
				periodPicker.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate {
			get { return periodPicker.EndDateOrNull; }
			set {
				periodPicker.EndDateOrNull = value;
				periodPicker.Sensitive = false;
			}
		}

		public enum OperationType
		{
			[Display (Name = "Все")]
			all,
			[Display(Name = "Доходы")]
			income,
			[Display(Name = "Расходы")]
			expense
		}

		protected void OnPeriodPickerPeriodChanged (object sender, EventArgs e)
		{
			OnRefiltered ();
		}

		protected void OnComboTypeChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

