using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Vodovoz;
using QSTDI;
using Vodovoz.Domain.Employees;
using System.Collections.Generic;

namespace Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EmployeeWorkChartDlg : TdiTabBase, ITdiDialog
	{
		public bool HasChanges { get;}

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public EmployeeWorkChartDlg()
		{
			this.Build();
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			DateTime now = DateTime.Now;

			yentryEmployee.SubjectType = typeof(Employee);

			yenumcomboMonth.ItemsEnum = typeof(Months);
			yenumcomboMonth.SelectedItem = (Months)now.Month;
			yenumcomboMonth.EnumItemSelected += YenumcomboMonth_EnumItemSelected;

			yspinYear.Value = (double)now.Year;
			yspinYear.ValueChanged += YspinYear_ValueChanged;

			workcharttable.Date = now;
		}

		void YspinYear_ValueChanged (object sender, EventArgs e)
		{
			SetTableDate();
		}

		void YenumcomboMonth_EnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			SetTableDate();
		}

		private void SetTableDate()
		{
			workcharttable.Date = new DateTime(yspinYear.ValueAsInt, (int)yenumcomboMonth.SelectedItem, 1);
			workcharttable.QueueDraw();
		}

		public bool Save()
		{
			return false;
		}

		public void SaveAndClose()
		{
			
		}

		public enum Months
		{
			[Display(Name = "Январь")] 	 Jan = 1,
			[Display(Name = "Февраль")]  Feb,
			[Display(Name = "Март")] 	 Mar,
			[Display(Name = "Апрель")] 	 Apr,
			[Display(Name = "Май")] 	 May,
			[Display(Name = "Июнь")] 	 Jun,
			[Display(Name = "Июль")] 	 Jul,
			[Display(Name = "Август")] 	 Aug,
			[Display(Name = "Сентябрь")] Sep,
			[Display(Name = "Октябрь")]  Oct,
			[Display(Name = "Ноябрь")] 	 Nov,
			[Display(Name = "Декабрь")]  Dec,
		}
	}
}

