using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QSOrmProject;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "графики работы водителя",
	Nominative = "график работы водителя")]
	[EntityPermission]
	public class DeliveryDaySchedule : PropertyChangedBase, IDomainObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual int Id { get; set; }

		string name;

		[Required(ErrorMessage = "Не заполнено название.")]
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private IList<DeliveryShift> shifts = new List<DeliveryShift>();

		[Display(Name = "Смены доставки")]
		public virtual IList<DeliveryShift> Shifts {
			get { return shifts; }
			set { SetField(ref shifts, value, () => Shifts); }
		}

		GenericObservableList<DeliveryShift> observableShifts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryShift> ObservableShifts {
			get {
				if(observableShifts == null)
					observableShifts = new GenericObservableList<DeliveryShift>(Shifts);
				return observableShifts;
			}
		}

		public DeliveryDaySchedule()
		{
		}

		#region Функции

		public virtual void RemoveShift(DeliveryShift shift)
		{
			ObservableShifts.Remove(shift);
		}

		public virtual void AddShift(DeliveryShift deliveryShift)
		{
			if(Shifts.Any(ds => DomainHelper.EqualDomainObjects(ds, deliveryShift))) {
				logger.Warn("Смена {0} уже добавлена. Пропускаем...", deliveryShift.Name);
				return;
			}
			ObservableShifts.Add(deliveryShift);
		}

		#endregion
	}
}
