using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Vodovoz;
using QSTDI;
using Vodovoz.Domain.Employees;

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
			yenumcomboMonth.ItemsEnum = typeof(Months);
			yentryEmployee.SubjectType = typeof(Employee);
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

