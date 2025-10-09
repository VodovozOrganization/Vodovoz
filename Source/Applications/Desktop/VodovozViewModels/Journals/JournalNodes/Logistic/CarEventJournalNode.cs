using System;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarEventJournalNode : JournalEntityNodeBase<CarEvent>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public DateTime CreateDate { get; set; }
		public string AuthorFullName { get; set; }
		public string CarEventTypeName { get; set; }
		public string CarRegistrationNumber { get; set; }
		public int? CarOrderNumber { get; set; }
		public string DriverFullName { get; set; }
		public string GeographicGroups { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public decimal RepairCost { get; set; }
		public decimal RepairPartsCost { get; set; }
		public string Comment { get; set; }
		public CarTypeOfUse CarTypeOfUse { get; set; }
		public CarOwnType CarOwnType { get; set; }
		public decimal RepairAndPartsSummaryCost => RepairCost + RepairPartsCost;

		public string CarTypeOfUseAndOwnTypeString
		{
			get
			{
				string str;
				switch(CarTypeOfUse)
				{
					case CarTypeOfUse.GAZelle:
						str = "Г";
						break;
					case CarTypeOfUse.Minivan:
						str = "Т";
						break;
					case CarTypeOfUse.Largus:
						str = "Л";
						break;
					case CarTypeOfUse.Truck:
						str = "Ф";
						break;
					case CarTypeOfUse.Loader:
						str = "П";
						break;
					default:
						throw new NotSupportedException($"{CarTypeOfUse.GetEnumTitle()} is not supported");
				}

				switch(CarOwnType)
				{
					case CarOwnType.Company:
						str += "К";
						break;
					case CarOwnType.Raskat:
						str += "Р";
						break;
					case CarOwnType.Driver:
						str += "В";
						break;
					default:
						throw new NotSupportedException($"{CarOwnType.GetEnumTitle()} is not supported");
				}

				return str;
			}
		}
	}
}
