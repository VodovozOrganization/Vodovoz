using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Client
{

	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения платной аренды",
		Nominative = "доп. соглашение платной аренды")]
	[EntityPermission]
	public class NonfreeRentAgreement : AdditionalAgreement
	{
		[Display(Name = "Количество месяцев аренды для оплаты")]
		public virtual int? RentMonths { get; set; }

		IList<PaidRentEquipment> equipment = new List<PaidRentEquipment> ();

		[Display (Name = "Список оборудования")]
		public virtual IList<PaidRentEquipment> PaidRentEquipments {
			get { return equipment; }
			set { SetField (ref equipment, value, () => PaidRentEquipments); }
		}

		GenericObservableList<PaidRentEquipment> observableEquipment;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PaidRentEquipment> ObservableEquipment {
			get {
				if (observableEquipment == null)
					observableEquipment = new GenericObservableList<PaidRentEquipment> (PaidRentEquipments);
				return observableEquipment;
			}
		}
	}
	
}
