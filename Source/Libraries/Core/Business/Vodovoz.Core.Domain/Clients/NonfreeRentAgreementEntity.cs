using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;
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
		IList<PaidRentEquipmentEntity> _equipment = new List<PaidRentEquipmentEntity>();
		ObservableList<PaidRentEquipmentEntity> _observableEquipment;

		[Display(Name = "Количество месяцев аренды для оплаты")]
		public virtual int? RentMonths
		{
			get => _rentMonths;
			set => SetField(ref _rentMonths, value, () => RentMonths);
		}

		/// <summary>
		/// Список оборудования
		/// </summary>
		[Display(Name = "Список оборудования")]
		public virtual IList<PaidRentEquipmentEntity> PaidRentEquipments
		{
			get => _equipment;
			set => SetField(ref _equipment, value, () => PaidRentEquipments);
		}

		/// <summary>
		/// Наблюдаемый список оборудования
		/// </summary>
		public virtual ObservableList<PaidRentEquipmentEntity> ObservableEquipment
		{
			get
			{
				if(_observableEquipment == null)
				{
					_observableEquipment = new ObservableList<PaidRentEquipmentEntity>(PaidRentEquipments);
				}

				return _observableEquipment;
			}
		}
	}
}
