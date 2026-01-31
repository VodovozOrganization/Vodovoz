using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class RouteListJournalNode : JournalEntityNodeBase<RouteList>
	{
		public RouteListStatus StatusEnum { get; set; }
		public string ShiftName { get; set; }
		public DateTime Date { get; set; }
		public string DriverSurname { get; set; }
		public string DriverName { get; set; }
		public string DriverPatronymic { get; set; }
		public string DriverComment { get; set; }
		public string Driver => PersonHelper.PersonFullName(DriverSurname, DriverName, DriverPatronymic);
		public string CarModelName { get; set; }
		public string CarNumber { get; set; }
		public string DriverAndCar => string.Format("{0} - {1} ({2})", Driver, CarModelName, CarNumber);
		public string LogisticiansComment { get; set; }
		public string ClosinComments { get; set; }
		public string ClosingSubdivision { get; set; }
		public bool NotFullyLoaded { get; set; }
		public CarTypeOfUse CarTypeOfUse { get; set; }
		public CarOwnType CarOwnType { get; set; }
		public decimal? GrossMarginPercents { get; set; }
		public decimal RouteListProfitabilityIndicator { get; set; }
		public decimal RouteListDebt { get; set; }
		public bool HasAddresses { get; set; }
		public bool HasAdditionalLoading { get; set; }
		public bool HasAddressesOrAdditionalLoading => HasAddresses || HasAdditionalLoading;

		public override string Title => $"Маршрутный лист №{Id}";
	}
}
