using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using NHibernate.Util;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Sectors
{
	public class SectorDeliveryRuleVersion : PropertyChangedBase, IDomainObject, ICloneable
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

		private DateTime _startDate;

		[Display(Name = "Время создания")]
		public virtual DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		private DateTime _endDate;

		[Display(Name = "Время закрытия")]
		public virtual DateTime EndDate
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
			var sectorClone = Sector.Clone() as Sector;

			var commonDistrictRuleItemsClone = new List<CommonDistrictRuleItem>();
			CommonDistrictRuleItems.ForEach(x => commonDistrictRuleItemsClone.Add(x.Clone() as CommonDistrictRuleItem));
			return new SectorDeliveryRuleVersion
			{
				StartDate = StartDate,
				EndDate = EndDate,
				Sector = sectorClone,
				Status = Status,
				CommonDistrictRuleItems = commonDistrictRuleItemsClone
			};
		}
	}
}