using System;
using QS.Project.Journal;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CompletedDriversWarehousesEventsJournalNode : JournalEntityNodeBase<CompletedDriverWarehouseEvent>
	{
		public string EventName { get; set; }
		public DriverWarehouseEventType EventType { get; set; }
		public string DriverName { get; set; }
		public string Car { get; set; }
		public DateTime CompletedDate { get; set; }
		public decimal? DistanceMetersFromScanningLocation { get; set; }
		public override string Title => EventName;
		
		public static readonly string IdColumn = "Код";
		public static readonly string EventNameColumn = "Название события";
		public static readonly string EventTypeColumn = "Тип";
		public static readonly string EmployeeColumn = "Сотрудник";
		public static readonly string CarColumn = "Автомобиль";
		public static readonly string CompletedDateColumn = "Время фиксации";
		public static readonly string DistanceColumn = "Расстояние\nот места сканирования";
	}
}
