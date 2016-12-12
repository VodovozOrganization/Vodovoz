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
			yentryEmployee.SubjectType = typeof(Employee);
			workcharttable.Date = DateTime.Now;
		}

		void YenumcomboMonth_EnumItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			workcharttable.Month = (int)e.SelectedItem;
			workcharttable.QueueDraw();
		}

		public bool Save()
		{
			return false;
		}

		public void SaveAndClose()
		{
			
		}

		protected void OnYdatepickerDateChanged (object sender, EventArgs e)
		{
			workcharttable.Date = ydatepicker.DateOrNull ?? default(DateTime);
			workcharttable.QueueDraw();
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

