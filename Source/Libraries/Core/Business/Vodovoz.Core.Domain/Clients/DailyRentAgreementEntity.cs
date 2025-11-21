using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public class DailyRentAgreementEntity : AdditionalAgreementEntity
	{
		private int _rentDays;
		private IList<PaidRentEquipmentEntity> _equipment = new List<PaidRentEquipmentEntity>();
		private ObservableList<PaidRentEquipmentEntity> _observableEquipment;

		/// <summary>
		/// Количество дней аренды
		/// </summary>
		[Display(Name = "Количество дней аренды")]
		public virtual int RentDays
		{
			get => _rentDays;
			set => SetField(ref _rentDays, value, () => RentDays);
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
		public virtual IList<PaidRentEquipmentEntity> Equipment
		{
			get => _equipment;
			set => SetField(ref _equipment, value, () => Equipment);
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
					_observableEquipment = new ObservableList<PaidRentEquipmentEntity>(Equipment);
				}

				return _observableEquipment;
			}
		}
	}
}
