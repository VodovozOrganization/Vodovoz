using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSOrmProject.RepresentationModel;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccountingFilter : RepresentationFilterBase<AccountingFilter>
	{
		protected override void ConfigureFilter()
		{
			comboType.ItemsEnum = typeof(OperationType);
			comboType.Sensitive = true;
			comboType.ShowSpecialStateAll = false;
			comboType.Active = 0;
		}

		public AccountingFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public AccountingFilter()
		{
			this.Build();
		}

		public OperationType RestrictOperationType => (OperationType)comboType.SelectedItem;

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
			[Display(Name = "Все")]
			all,
			[Display(Name = "Доходы")]
			income,
			[Display(Name = "Расходы")]
			expense
		}

		protected void OnPeriodPickerPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnComboTypeChangedByUser(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}

