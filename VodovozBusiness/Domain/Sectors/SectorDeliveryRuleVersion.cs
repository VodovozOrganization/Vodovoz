using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Sectors
{
	public class SectorDeliveryRuleVersion : PropertyChangedBase, IDomainObject, ICloneable, IValidatableObject
	{
		public int Id { get; set; }

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
		
		private IList<CommonDistrictRuleItem> _commonDistrictRuleItems = new List<CommonDistrictRuleItem>();
		[Display(Name = "Правила и цены доставки района")]
		public virtual IList<CommonDistrictRuleItem> CommonDistrictRuleItems {
			get => _commonDistrictRuleItems;
			set => SetField(ref _commonDistrictRuleItems, value, () => CommonDistrictRuleItems);
		}

		private GenericObservableList<CommonDistrictRuleItem> _observableCommonDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CommonDistrictRuleItem> ObservableCommonDistrictRuleItems =>
			_observableCommonDistrictRuleItems ?? (_observableCommonDistrictRuleItems =
				new GenericObservableList<CommonDistrictRuleItem>(CommonDistrictRuleItems));
		
		private SectorsSetStatus _status;
		public virtual SectorsSetStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		public SectorDeliveryRuleVersion()
		{
			Status = SectorsSetStatus.Draft;
		}

		public object Clone()
		{
			var commonDistrictRuleItemsClone = new List<CommonDistrictRuleItem>();
			CommonDistrictRuleItems.ForEach(x => commonDistrictRuleItemsClone.Add(x.Clone() as CommonDistrictRuleItem));
			return new SectorDeliveryRuleVersion
			{
				StartDate = StartDate,
				EndDate = EndDate,
				Sector = Sector,
				Status = Status,
				CommonDistrictRuleItems = commonDistrictRuleItemsClone
			};
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate.HasValue == false)
			{
				yield return new ValidationResult($"Необходимо поставить дату активации", new[] {nameof(StartDate)});
			}
			if(ObservableCommonDistrictRuleItems.Any(i => i.Price <= 0))
			{
				yield return new ValidationResult(
					$"Для всех правил доставки для района \"{Sector.Id}\" должны быть указаны цены",
					new[] {nameof(CommonDistrictRuleItems)});
			}
		}
	}
}