using QS.Project.Journal;
using System;

namespace Vodovoz.ViewModels.Logistic.MileagesWriteOff
{
	public class MileageWriteOffJournalNode : JournalEntityNodeBase
	{
		public int Id { get; set; }
		public DateTime WriteOffDate { get; set; }
		public DateTime CreateDate { get; set; }
		public decimal DistanceKm { get; set; }
		public string CarRegNumber { get; set; }
		public string DriverName { get; set; }
		public string AuthorName { get; set; }

		public override string Title =>
			$"Списание километража без МЛ на авто \"{CarRegNumber}\" за {WriteOffDate.ToString("dd.MM.yyyy")}";
	}
}
