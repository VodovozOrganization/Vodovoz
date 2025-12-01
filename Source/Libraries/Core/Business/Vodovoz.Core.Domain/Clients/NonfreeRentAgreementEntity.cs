using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "доп. соглашения платной аренды",
		Nominative = "доп. соглашение платной аренды")]
	[EntityPermission]
	public class NonfreeRentAgreementEntity : AdditionalAgreementEntity
	{
		private int? _rentMonths;
		private IObservableList<PaidRentEquipmentEntity> _equipment = new ObservableList<PaidRentEquipmentEntity>();

		[Display(Name = "Количество месяцев аренды для оплаты")]
		public virtual int? RentMonths
		{
			get => _rentMonths;
			set => SetField(ref _rentMonths, value);
		}

		/// <summary>
		/// Список оборудования
		/// </summary>
		[Display(Name = "Список оборудования")]
		public virtual IObservableList<PaidRentEquipmentEntity> PaidRentEquipments
		{
			get => _equipment;
			set => SetField(ref _equipment, value);
		}
	}
}
