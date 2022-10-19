using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи оборудования",
		Nominative = "доп. соглашение продажи оборудования")]
	[HistoryTrace]
	[EntityPermission]
	public class SalesEquipmentAgreement : AdditionalAgreement
	{
		IList<SalesEquipment> salesEqipments = new List<SalesEquipment>();

		[Display(Name = "Оборудование на продажу")]
		public virtual IList<SalesEquipment> SalesEqipments {
			get { return salesEqipments; }
			set { SetField(ref salesEqipments, value, () => SalesEqipments); }
		}

		GenericObservableList<SalesEquipment> observableSalesEqipments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<SalesEquipment> ObservableSalesEqipments {
			get {
				if(observableSalesEqipments == null)
					observableSalesEqipments = new GenericObservableList<SalesEquipment>(SalesEqipments);
				return observableSalesEqipments;
			}
		}
	}
}