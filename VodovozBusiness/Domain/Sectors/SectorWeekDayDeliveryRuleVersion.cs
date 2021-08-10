using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Sectors
{
	[HistoryTrace]
	public class SectorWeekDayDeliveryRuleVersion: PropertyChangedBase, IDomainObject, ICloneable
	{
		public int Id { get; set; }

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
		
		private DateTime? _startDate;
		
		[Display(Name = "Время создания")]
		public virtual DateTime? StartDate
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

		private List<WeekDayDistrictRuleItem> _weekDayDistrictRules = new List<WeekDayDistrictRuleItem>();

		public virtual List<WeekDayDistrictRuleItem> WeekDayDistrictRules
		{
			get => _weekDayDistrictRules;
			set => SetField(ref _weekDayDistrictRules, value);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> _observableWeekDayDistrictRules;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableWeekDayDistrictRules =>
			_observableWeekDayDistrictRules ?? (_observableWeekDayDistrictRules =
				new GenericObservableList<WeekDayDistrictRuleItem>(WeekDayDistrictRules));
		
		private SectorsSetStatus _status;
		public virtual SectorsSetStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		public SectorWeekDayDeliveryRuleVersion()
		{
			Status = SectorsSetStatus.Draft;
		}

		public object Clone()
		{
			var sectorClone = Sector.Clone() as Sector;
			
			var weekDayDeliveryRuleClone = new List<WeekDayDistrictRuleItem>();
			WeekDayDistrictRules.ForEach(a => weekDayDeliveryRuleClone.Add(a.Clone() as WeekDayDistrictRuleItem));

			return new SectorWeekDayDeliveryRuleVersion
			{
				Sector = sectorClone,
				WeekDayDistrictRules = weekDayDeliveryRuleClone,
				StartDate = StartDate,
				Status = Status,
				EndDate = EndDate
			};
		}
	}
}