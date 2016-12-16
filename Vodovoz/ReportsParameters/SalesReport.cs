using System;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Goods;
using System.Collections.Generic;

namespace Vodovoz.Reports
{
	public partial class SalesReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		public SalesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			yenumcomboNomenclature.ItemsEnum = typeof(NomenclatureCategory);
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Now;
		}

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get {
				return null;
			}
		}

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title	{ 
			get {
				return "Отчет по продажам";
			}
		}

		#endregion

		private ReportInfo GetReportInfo()
		{			
			return new ReportInfo
			{
				Identifier = "Sales.SalesReport",
				Parameters = new Dictionary<string, object>
				{ 
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull },
					{ "nom_category", yenumcomboNomenclature.SelectedItem.ToString().ToLower() == "all"
							? " " : yenumcomboNomenclature.SelectedItem.ToString()},
				}
			};
		}	
		
		protected void OnButtonCreateReportClicked (object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}
	}
}

