using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Transport.Reports.IncorrectFuel
{
	public class IncorrectFuelReportRow
	{
		public int RowNumber { get; set; }
		public string CarRegNumber { get; set; }
		public CarOwnType? CarOwnType { get; set; }
		public CarTypeOfUse? CarTypeOfUse { get; set; }
		public string CarModel { get; set; }
		public EmployeeCategory? DriverCategory { get; set; }
		public string DriverName { get; set; }
		public string FuelCardNumber { get; set; }
		public string CarFuelType { get; set; }
		public int TransactionId { get; set; }
		public string TransactionFuelType { get; set; }
		public string TransactionFuelId { get; set; }
		public decimal TransactionLitersAmount { get; set; }
		public DateTime TransactionDateTime { get; set; }

		public string CarOwnTypeString => CarOwnType?.GetEnumDisplayName();
		public string CarTypeOfUseString => CarTypeOfUse?.GetEnumDisplayName();
		public string DriverCategoryString => DriverCategory?.GetEnumDisplayName();
		public string TransactionLitersAmountString => TransactionLitersAmount.ToString("F2");
		public string TransactionDateTimeString => TransactionDateTime.ToString("dd.MM.yyyy\nHH:mm:ss");
	}
}
