using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Client
{

	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения посуточной аренды",
		Nominative = "доп. соглашение посуточной аренды")]
	[EntityPermission]
	public class DailyRentAgreement : AdditionalAgreement
	{
		[Display (Name = "Количество дней аренды")]
		public virtual int RentDays { get; set; }
		
		public virtual DateTime EndDate => base.StartDate.AddDays(RentDays);

		IList<PaidRentEquipment> equipment = new List<PaidRentEquipment> ();

		[Display (Name = "Список оборудования")]
		public virtual IList<PaidRentEquipment> Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		GenericObservableList<PaidRentEquipment> observableEquipment;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PaidRentEquipment> ObservableEquipment {
			get {
				if (observableEquipment == null)
					observableEquipment = new GenericObservableList<PaidRentEquipment> (Equipment);
				return observableEquipment;
			}
		}
	}
}
