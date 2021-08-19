using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Sectors
{
	[HistoryTrace]
	public class SectorWeekDayScheduleVersion : PropertyChangedBase, IDomainObject, ICloneable, IValidatableObject
	{
		public virtual int Id { get; set; }

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

		public virtual Sector Sector
		{
			get => _sector;
			set => SetField(ref _sector, value);
		}

		private IList<DeliveryScheduleRestriction> _sectorSchedules = new List<DeliveryScheduleRestriction>();

		public virtual IList<DeliveryScheduleRestriction> SectorSchedules
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

		public virtual object Clone()
		{
			var sectorSchedulesClone = new List<DeliveryScheduleRestriction>();
			foreach(var schedule in SectorSchedules)
				sectorSchedulesClone.Add(schedule.Clone() as DeliveryScheduleRestriction);
			
			return new SectorWeekDayScheduleVersion
			{
				Sector = Sector,
				SectorSchedules = sectorSchedulesClone,
				StartDate = StartDate,
				Status = Status,
				EndDate = EndDate
			};
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate.HasValue == false)
			{
				yield return new ValidationResult($"Необходимо поставить дату активации", new[] {nameof(StartDate)});
			}
			if(ObservableSectorSchedules.Any(i => i.AcceptBefore == null))
			{
				yield return new ValidationResult(
					$"Для графиков доставки для района \"{Sector.Id}\" должно быть указано время приема до");
			}
		}
	}
}