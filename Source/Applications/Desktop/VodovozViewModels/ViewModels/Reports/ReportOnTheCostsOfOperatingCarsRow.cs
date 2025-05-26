using Gamma.Utilities;
using System;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class ReportOnTheCostsOfOperatingCarsRow
	{
		public int Index { get; internal set; }
		public int Id { get; private set; }
		public string CarRegistrationNumber { get; private set; }
		public string CarOwnTypeString { get; private set; }
		public string CarTypeOfUseString { get; private set; }
		public DateTime SubjectStart { get; private set; }
		public DateTime SubjectEnd { get; private set; }
		public string SubjectType { get; private set; }
		public string Foundation { get; private set; }
		public string SubjectComment { get; private set; }
		public decimal Price { get; private set; }
		public decimal Refund { get; private set; }
		public decimal CompanyExpenses { get; private set; }

		public ReportOnTheCostsOfOperatingCarsRow(CarEvent carEvent, bool excludeCarPartsCost = false)
		{
			Id = carEvent.Id;
			CarRegistrationNumber = carEvent.Car.RegistrationNumber;
			CarOwnTypeString = carEvent.Car.CarVersions.Last().CarOwnType.GetEnumTitle();
			CarTypeOfUseString = carEvent.Car.CarModel.CarTypeOfUse.GetEnumTitle();
			SubjectStart = carEvent.StartDate;
			SubjectEnd = carEvent.EndDate;
			SubjectType = carEvent.CarEventType.Name;
			Foundation = carEvent.Foundation;
			SubjectComment = carEvent.Comment;
			Price = excludeCarPartsCost ? carEvent.RepairCost : carEvent.RepairAndPartsSummaryCost;
			Refund = carEvent.Fines?.Sum(p => p.TotalMoney) ?? 0;
			CompanyExpenses = Price - Refund;
		}
	}
}
