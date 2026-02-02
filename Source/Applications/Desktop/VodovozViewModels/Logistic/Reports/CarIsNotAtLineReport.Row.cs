using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Domain.Logistics.Cars;

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
			/// Принадлежность авто
			/// </summary>
			public string CarOwnType{ get; set; }

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
			public List<AreaOfResponsibility?> AreasOfResponsibility { get; set; } = new List<AreaOfResponsibility?>();

			/// <summary>
			/// Короткое имя зоны ответственности для отчёта/UI
			/// </summary>
			public string AreasOfResponsibilityShortNames
			{
				get
				{
					return AreasOfResponsibility == null || !AreasOfResponsibility.Any()
						? "Простой"
						: string.Join(", ",
						AreasOfResponsibility
							.Where(a => a.HasValue)
							.Select(a =>
							{
								var member = typeof(AreaOfResponsibility).GetMember(a.Value.ToString()).FirstOrDefault();
								var displayAttr = member?.GetCustomAttribute<DisplayAttribute>();
								return !string.IsNullOrWhiteSpace(displayAttr?.ShortName)
									? displayAttr.ShortName
									: a.Value.ToString();
							})
					);
				}
			}

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
