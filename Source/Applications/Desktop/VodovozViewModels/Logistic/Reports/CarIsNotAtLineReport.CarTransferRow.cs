using System;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public partial class CarIsNotAtLineReport
	{
		/// <summary>
		/// Строка отчета - передача автомобиля
		/// </summary>
		public class CarTransferRow
		{
			/// <summary>
			/// № п/п
			/// </summary>
			public int Id { get; set; }

			/// <summary>
			/// Дата
			/// </summary>
			public DateTime TransferedAt { get; set; }

			/// <summary>
			/// Тип авто
			/// </summary>
			public string CarTypeWithGeographicalGroup { get; set; }
			
			/// <summary>
			/// Принадлежность авто
			/// </summary>
			public string CarOwnType { get; set; }

			/// <summary>
			/// Госномер
			/// </summary>
			public string RegistationNumber { get; set; }

			/// <summary>
			/// Комментарий
			/// </summary>
			public string Comment { get; set; }
		}
	}
}
