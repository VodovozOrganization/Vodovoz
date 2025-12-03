using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public class DailyRentAgreementEntity : AdditionalAgreementEntity
	{
		private int _rentDays;
		private IObservableList<PaidRentEquipmentEntity> _equipment = new ObservableList<PaidRentEquipmentEntity>();

		/// <summary>
		/// Количество дней аренды
		/// </summary>
		[Display(Name = "Количество дней аренды")]
		public virtual int RentDays
		{
			get => _rentDays;
			set => SetField(ref _rentDays, value);
		}

		/// <summary>
		/// Дата окончания аренды
		/// </summary>
		[Display(Name = "Дата окончания аренды")]
		public virtual DateTime EndDate => base.StartDate.AddDays(RentDays);

		/// <summary>
		/// Список оборудования
		/// </summary>
		[Display(Name = "Список оборудования")]
		public virtual IObservableList<PaidRentEquipmentEntity> Equipment
		{
			get => _equipment;
			set => SetField(ref _equipment, value);
		}
	}
}
