using System;
using QSOrmProject;
using QSReport;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ShortfallBattlesReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public ShortfallBattlesReport()
		{
			this.Build();
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get	{
				return null;
			}
		}

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get	{
				return "Отчет о несданных бутылях";
			}
		}

		#endregion
	}
}

