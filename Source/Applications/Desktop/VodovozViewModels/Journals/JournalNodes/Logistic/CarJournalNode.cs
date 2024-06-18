using QS.Project.Journal;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarJournalNode : JournalEntityNodeBase<Car>
	{
		public override string Title => $"{ManufacturerName} {ModelName} ({RegistrationNumber}) {DriverName}";

		public string CarOwner { get; set; }
		public string ModelName { get; set; }
		public string ManufacturerName { get; set; }
		public string RegistrationNumber { get; set; }
		public string DriverName { get; set; }
		public string OsagoInsurer { get; set; }
		public string KaskoInsurer { get; set; }
		public bool IsArchive { get; set; }
		public bool IsShowBackgroundColorNotification { get; set; }
		public string InsurersNames
		{
			get
			{
				var insurersNames = new List<string>();

				if(!string.IsNullOrEmpty(OsagoInsurer))
				{
					insurersNames.Add(OsagoInsurer);
				}

				if(!string.IsNullOrEmpty(KaskoInsurer))
				{
					insurersNames.Add(KaskoInsurer);
				}

				return string.Join(", ", insurersNames);
			}
		}
	}
}
