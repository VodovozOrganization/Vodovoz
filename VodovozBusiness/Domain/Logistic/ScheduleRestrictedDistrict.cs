﻿using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Gamma.Utilities;
using System.Data.Bindings.Collections.Generic;
using GeoAPI.Geometries;

namespace Vodovoz.Domain.Logistic
{
	public class ScheduleRestrictedDistrict : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string districtName;

		public virtual string DistrictName {
			get { return districtName; }
			set { SetField(ref districtName, value, () => DistrictName); }
		}

		int minBottles;

		public virtual int MinBottles {
			get { return minBottles; }
			set { SetField(ref minBottles, value, () => MinBottles); }
		}

		IList<ScheduleRestriction> scheduleRestrictions = new List<ScheduleRestriction>();

		public virtual IList<ScheduleRestriction> ScheduleRestrictions {
			get { return scheduleRestrictions; }
			set { SetField(ref scheduleRestrictions, value, () => ScheduleRestrictions); }
		}

		GenericObservableList<ScheduleRestriction> observableScheduleRestrictions;

		public virtual GenericObservableList<ScheduleRestriction> ObservableScheduleRestrictions {
			get {
				if(observableScheduleRestrictions == null) {
					observableScheduleRestrictions = new GenericObservableList<ScheduleRestriction>(ScheduleRestrictions);
				}
				return observableScheduleRestrictions;
			}
		}

		private IGeometry districtBorder;

		public virtual IGeometry DistrictBorder {
			get { return districtBorder; }
			set { SetField(ref districtBorder, value, () => DistrictBorder); }
		}

		public virtual void AddSchedule(IUnitOfWork UoW)
		{
			var schedule = new ScheduleRestriction() {
				District = this
			};
			observableScheduleRestrictions.Add(schedule);
		}

		public virtual void Save(IUnitOfWork UoW)
		{
			UoW.Save(this);
			foreach(ScheduleRestriction restriction in ObservableScheduleRestrictions) {
				restriction.Save(UoW);
			}
		}

		public virtual void Remove(IUnitOfWork UoW)
		{
			foreach(ScheduleRestriction restriction in ObservableScheduleRestrictions) {
				restriction.Remove(UoW);
			}
			UoW.Delete(this);
		}
	}
}