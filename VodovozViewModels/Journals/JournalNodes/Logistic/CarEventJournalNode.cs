using System;
using Gamma.Utilities;
using QS.Project.Journal;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarEventJournalNode : JournalEntityNodeBase<CarEvent>
	{
		public DateTime CreateDate { get; set; }
		public string AuthorFullName { get; set; }
		public string CarEventTypeShortName { get; set; }
		public string CarRegistrationNumber { get; set; }
		public int? CarOrderNumber { get; set; }
		public string DriverFullName { get; set; }
		public string Subdivision { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Comment { get; set; }

		public CarTypeOfUse CarTypeOfUse { get; set; }
		public string CarTypeOfUseString
		{
			get
			{
				switch(CarTypeOfUse)
				{
					case CarTypeOfUse.CompanyGAZelle:
						return "ГК";
					case CarTypeOfUse.CompanyLargus:
						return "ЛК";
				}

				return CarTypeOfUse.GetEnumTitle();
			}
		}
	}
}
