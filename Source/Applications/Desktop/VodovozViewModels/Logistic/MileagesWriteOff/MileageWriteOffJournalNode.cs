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
		public string DriverLastName { get; set; }
		public string DriverName { get; set; }
		public string DriverPatronymic { get; set; }
		public string AuthorLastName { get; set; }
		public string AuthorName { get; set; }
		public string AuthorPatronymic { get; set; }

		public string DriverFullName =>
			$"{DriverLastName} {DriverName} {DriverPatronymic}";

		public string AuthorFullName =>
			$"{AuthorLastName} {AuthorName} {AuthorPatronymic}";

		public override string Title =>
			$"Списание километража без МЛ на авто \"{CarRegNumber}\" за {WriteOffDate.ToString("dd.MM.yyyy")}";
	}
}
