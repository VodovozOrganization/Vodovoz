using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Sectors
{
	[HistoryTrace]
	public class SectorWeekDayRulesVersion : PropertyChangedBase, IDomainObject, ICloneable
	{
		public int Id { get; set; }

		private DateTime _startDate;
		
		private Employee _author;
		[Display(Name = "Автор")]
		public virtual Employee Author {
			get => _author;
			set => SetField(ref _author, value);
		}

		private Employee _lastEditor;
		public virtual Employee LastEditor {
			get => _lastEditor;
			set => SetField(ref _lastEditor, value);
		}
		
		[Display(Name = "Время создания")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		private DateTime? _endDate;

		[Display(Name = "Время закрытия")]
		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		private Sector _sector;

		public Sector Sector
		{
			get => _sector;
			set => SetField(ref _sector, value);
		}

		private List<SectorWeekDaySchedule> _sectorSchedules;

		public List<SectorWeekDaySchedule> SectorSchedules
		{
			get => _sectorSchedules;
			set => SetField(ref _sectorSchedules, value);
		}
		
		private List<SectorWeekDayDeliveryRule> _sectorDeliveryRules;

		public List<SectorWeekDayDeliveryRule> SectorDeliveryRules
		{
			get => _sectorDeliveryRules;
			set => SetField(ref _sectorDeliveryRules, value);
		}
		
		private SectorsSetStatus _status;
		public virtual SectorsSetStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		public SectorWeekDayRulesVersion()
		{
			Status = SectorsSetStatus.Draft;
		}

		public object Clone()
		{
			var sectorClone = Sector.Clone() as Sector;
			var sectorSchedulesClone = new List<SectorWeekDaySchedule>();
			SectorSchedules.ForEach(a => sectorSchedulesClone.Add(a.Clone() as SectorWeekDaySchedule));
			
			var sectorWeekDayDeliveryRuleClone = new List<SectorWeekDayDeliveryRule>();
			SectorDeliveryRules.ForEach(a => sectorWeekDayDeliveryRuleClone.Add(a.Clone() as SectorWeekDayDeliveryRule));

			return new SectorWeekDayRulesVersion
			{
				Sector = sectorClone,
				SectorSchedules = sectorSchedulesClone,
				SectorDeliveryRules = sectorWeekDayDeliveryRuleClone,
				StartDate = StartDate,
				Status = Status,
				EndDate = EndDate
			};
		}
	}
}