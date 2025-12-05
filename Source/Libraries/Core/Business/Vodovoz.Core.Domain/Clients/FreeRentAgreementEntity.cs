using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения бесплатной аренды",
		Nominative = "доп. соглашение бесплатной аренды")]
	[EntityPermission]
	public class FreeRentAgreementEntity : AdditionalAgreementEntity
	{
		private IObservableList<FreeRentEquipmentEntity> _equipment = new ObservableList<FreeRentEquipmentEntity>();

		/// <summary>
		/// Список оборудования
		/// </summary>
		[Display(Name = "Список оборудования")]
		public virtual IObservableList<FreeRentEquipmentEntity> Equipments
		{
			get => _equipment;
			set => SetField(ref _equipment, value);
		}
	}
}
