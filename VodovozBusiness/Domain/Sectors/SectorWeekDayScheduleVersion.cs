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
	public class SectorWeekDayScheduleVersion : PropertyChangedBase, IDomainObject, ICloneable
	{
		public int Id { get; set; }

		private DateTime? _startDate;
		
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

		private List<DeliveryScheduleRestriction> _sectorSchedules = new List<DeliveryScheduleRestriction>();

		public virtual List<DeliveryScheduleRestriction> SectorSchedules
		{
			get => _sectorSchedules;
			set => SetField(ref _sectorSchedules, value);
		}

		private GenericObservableList<DeliveryScheduleRestriction> _observableSectorSchedules;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableSectorSchedules =>
			_observableSectorSchedules ?? (_observableSectorSchedules =
				new GenericObservableList<DeliveryScheduleRestriction>(SectorSchedules));

		private SectorsSetStatus _status;
		public virtual SectorsSetStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		public SectorWeekDayScheduleVersion()
		{
			Status = SectorsSetStatus.Draft;
		}

		public object Clone()
		{
			var sectorClone = Sector.Clone() as Sector;
			var sectorSchedulesClone = new List<DeliveryScheduleRestriction>();
			SectorSchedules.ForEach(a => sectorSchedulesClone.Add(a.Clone() as DeliveryScheduleRestriction));

			return new SectorWeekDayScheduleVersion
			{
				Sector = sectorClone,
				SectorSchedules = sectorSchedulesClone,
				StartDate = StartDate,
				Status = Status,
				EndDate = EndDate
			};
		}
	}
}