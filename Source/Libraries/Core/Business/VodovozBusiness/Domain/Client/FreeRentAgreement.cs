using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения бесплатной аренды",
		Nominative = "доп. соглашение бесплатной аренды")]
	[EntityPermission]
	public class FreeRentAgreement : AdditionalAgreement
	{
		IList<FreeRentEquipment> equipment = new List<FreeRentEquipment> ();

		[Display (Name = "Список оборудования")]
		public virtual IList<FreeRentEquipment> Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		GenericObservableList<FreeRentEquipment> observableEquipment;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FreeRentEquipment> ObservableEquipment {
			get {
				if (observableEquipment == null)
					observableEquipment = new GenericObservableList<FreeRentEquipment> (Equipment);
				return observableEquipment;
			}
		}
	}
}
