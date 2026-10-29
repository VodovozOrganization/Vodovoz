using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using System;
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

		public string CarTypeOfUseAndOwnTypeString => CarTypeOfUse.GetEnumShortTitle() + CarOwnType.GetEnumShortTitle();
	}
}
