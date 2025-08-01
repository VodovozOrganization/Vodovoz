using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class ReportOnTheCostsOfOperatingCars
	{
		public ReportOnTheCostsOfOperatingCars()
		{
			Rows = new GenericObservableList<ReportOnTheCostsOfOperatingCarsRow>();
		}

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string SelectCar { get; set; }
		public string SelectedSubject { get; set; }
		public string CarOwnType { get; set; }
		public string CarTypeOfUse { get; set; }
		public string SumPrice
		{
			get
			{
				decimal sum = Rows?.Sum(x => x.Price) ?? 0;

				return Math.Round(sum, 2).ToString("N");
			}
		}
		public string SumRefund
		{
			get
			{
				decimal sum = Rows?.Sum(x => x.Refund) ?? 0;

				return Math.Round(sum, 2).ToString("N");
			}
		}

		public string SumCompanyExpenses
		{
			get
			{
				decimal sum = Rows?.Sum(x => x.CompanyExpenses) ?? 0;

				return Math.Round(sum, 2).ToString("N");
			}
		}
		public GenericObservableList<ReportOnTheCostsOfOperatingCarsRow> Rows { get; set; }

		public void UpdateIndex()
		{
			for(int i = 0; i < Rows.Count; i++)
			{
				Rows[i].Index = i + 1;
			}
		}
	}
}
