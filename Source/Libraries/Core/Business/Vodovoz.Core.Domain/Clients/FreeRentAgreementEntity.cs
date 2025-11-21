using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения бесплатной аренды",
		Nominative = "доп. соглашение бесплатной аренды")]
	[EntityPermission]
	public class FreeRentAgreementEntity : AdditionalAgreementEntity
	{
		IList<FreeRentEquipmentEntity> _equipment = new List<FreeRentEquipmentEntity>();
		private ObservableList<FreeRentEquipmentEntity> _observableEquipment;
		/// <summary>
		/// Список оборудования
		/// </summary>
		[Display(Name = "Список оборудования")]
		public virtual IList<FreeRentEquipmentEntity> Equipment
		{
			get => _equipment;
			set => SetField(ref _equipment, value, () => Equipment);
		}

		/// <summary>
		/// Наблюдаемый список оборудования
		/// </summary>
		public virtual ObservableList<FreeRentEquipmentEntity> ObservableEquipment
		{
			get
			{
				if(_observableEquipment == null)
				{
					_observableEquipment = new ObservableList<FreeRentEquipmentEntity>(Equipment);
				}

				return _observableEquipment;
			}
		}
	}
}
