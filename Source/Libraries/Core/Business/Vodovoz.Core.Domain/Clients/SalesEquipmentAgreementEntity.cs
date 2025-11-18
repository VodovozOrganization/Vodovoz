using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения продажи оборудования",
		Nominative = "доп. соглашение продажи оборудования")]
	[HistoryTrace]
	[EntityPermission]
	public class SalesEquipmentAgreementEntity : AdditionalAgreementEntity
	{
		private IList<SalesEquipmentEntity> _salesEqipments = new List<SalesEquipmentEntity>();
		private ObservableList<SalesEquipmentEntity> _observableSalesEqipments;

		/// <summary>
		/// Оборудование на продажу
		/// </summary>
		[Display(Name = "Оборудование на продажу")]
		public virtual IList<SalesEquipmentEntity> SalesEqipments
		{
			get => _salesEqipments;
			set => SetField(ref _salesEqipments, value, () => SalesEqipments);
		}

		/// <summary>
		/// Наблюдаемый список оборудования на продажу
		/// </summary>
		public virtual ObservableList<SalesEquipmentEntity> ObservableSalesEqipments
		{
			get
			{
				if(_observableSalesEqipments == null)
				{
					_observableSalesEqipments = new ObservableList<SalesEquipmentEntity>(SalesEqipments);
				}

				return _observableSalesEqipments;
			}
		}
	}
}
