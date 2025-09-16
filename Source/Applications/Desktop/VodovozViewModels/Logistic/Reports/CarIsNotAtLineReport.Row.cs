using System;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public partial class CarIsNotAtLineReport
	{
		/// <summary>
		/// Строка отчета - основная секция
		/// </summary>
		public class Row
		{
			/// <summary>
			/// № п/п
			/// </summary>
			public int Id { get; set; }
			public string IdString => Id.ToString();

			/// <summary>
			/// дата начала простоя
			/// </summary>
			public DateTime? DowntimeStartedAt { get; set; }

			public string DowntimeStartedAtString => DowntimeStartedAt is null ? "" : DowntimeStartedAt.Value.ToString(_defaultDateTimeFormat);

			/// <summary>
			/// Тип авто
			/// </summary>
			public string CarType { get; set; }

			/// <summary>
			/// Тип авто с географической группой
			/// </summary>
			public string CarTypeWithGeographicalGroup { get; set; }

			/// <summary>
			/// Госномер
			/// </summary>
			public string RegistationNumber { get; set; }

			/// <summary>
			/// время / описание поломки
			/// </summary>
			public string TimeAndBreakdownReason { get; set; }

			/// <summary>
			/// Зона ответственности
			/// </summary>
			public string AreaOfResponsibility { get; set; }

			/// <summary>
			/// Планируемая дата выпуска автомобиля на линию
			/// </summary>
			public DateTime? PlannedReturnToLineDate { get; set; }

			public string PlannedReturnToLineDateString => PlannedReturnToLineDate is null ? "" : PlannedReturnToLineDate.Value.ToString(_defaultDateTimeFormat);

			/// <summary>
			/// планируемая дата выпуска автомобиля на линию/ основания переноса даты
			/// </summary>
			public string PlannedReturnToLineDateAndReschedulingReason { get; set; }

			/// <summary>
			/// Название события (для группировки)
			/// </summary>
			public string CarEventTypes { get; set; }
		}
	}
}
