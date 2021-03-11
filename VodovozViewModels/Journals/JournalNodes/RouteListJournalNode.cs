using QS.Project.Journal;
using QS.Utilities.Text;
using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
    public class RouteListJournalNode : JournalEntityNodeBase<RouteList>
    {
        public int Id { get; set; }

        public RouteListStatus StatusEnum { get; set; }

        public string ShiftName { get; set; }

        public DateTime Date { get; set; }

        public string DriverSurname { get; set; }
        public string DriverName { get; set; }
        public string DriverPatronymic { get; set; }

        public string Driver => PersonHelper.PersonFullName(DriverSurname, DriverName, DriverPatronymic);

        public string CarModel { get; set; }

        public string CarNumber { get; set; }

        public string DriverAndCar => string.Format("{0} - {1} ({2})", Driver, CarModel, CarNumber);
        public string LogisticiansComment { get; set; }
        public string ClosinComments { get; set; }
        public string ClosingSubdivision { get; set; }
        public bool NotFullyLoaded { get; set; }
        public CarTypeOfUse CarTypeOfUse { get; set; }
        public bool UsesCompanyCar => !CarTypeOfUse.Equals(CarTypeOfUse.DriverCar);
    }
}
