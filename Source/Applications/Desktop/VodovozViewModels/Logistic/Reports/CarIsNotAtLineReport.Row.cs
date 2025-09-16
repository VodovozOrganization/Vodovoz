using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Vodovoz.Domain.Logistic;

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
			private AreaOfResponsibility? _areaOfResponsibility;

			public string AreaOfResponsibility
			{
				get
				{
					if(!_areaOfResponsibility.HasValue)
					{
						return "Простой1";
					}

					var member = typeof(AreaOfResponsibility).GetMember(_areaOfResponsibility.Value.ToString()).FirstOrDefault();
					if(member != null)
					{
						var displayAttr = member.GetCustomAttribute<DisplayAttribute>();
						if(displayAttr != null && !string.IsNullOrWhiteSpace(displayAttr.ShortName))
						{
							return displayAttr.ShortName;
						}
					}
					return _areaOfResponsibility.Value.ToString();
				}
				set
				{
					if(string.IsNullOrWhiteSpace(value))
					{
						_areaOfResponsibility = null;
					}
					else
					{
						if(Enum.TryParse<AreaOfResponsibility>(value, out var parsed))
						{
							_areaOfResponsibility = parsed;
						}
						else
						{
							_areaOfResponsibility = GetAreaOfResponsibilityByShortName(value);
						}
					}
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

			private static AreaOfResponsibility? GetAreaOfResponsibilityByShortName(string shortName)
			{
				foreach(var value in Enum.GetValues(typeof(AreaOfResponsibility)).Cast<AreaOfResponsibility>())
				{
					var member = typeof(AreaOfResponsibility).GetMember(value.ToString()).FirstOrDefault();
					if(member != null)
					{
						var displayAttr = member.GetCustomAttribute<DisplayAttribute>();
						if(displayAttr != null && displayAttr.ShortName == shortName)
						{
							return value;
						}
					}
				}
				return null;
			}
		}
	}
}
