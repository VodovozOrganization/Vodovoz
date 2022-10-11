using QS.Project.Journal;
using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class TrackPointJournalNode : JournalEntityNodeBase<TrackPoint>
	{
		public DateTime Time { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public DateTime ReceiveTime { get; set; }
		public int RouteListId { get; set; }
		public override string Title => $"Точка маршрута { Time } МЛ №{ RouteListId }";
	}
}
