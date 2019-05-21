using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Payments
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "идентификаторы заказов",
		Nominative = "идентификатор заказа"
	)]
	public class OrderIdProviderForMobileApp : BusinessObjectBase<OrderIdProviderForMobileApp>, IDomainObject
	{
		public OrderIdProviderForMobileApp() { }

		public OrderIdProviderForMobileApp(string imei, int sum)
		{
			Imei = imei;
			OrderSum = sum;
		}

		#region свойства для мапинга

		public virtual int Id { get; set; }

		string imei;
		[Display(Name = "IMEI мобильного устройства")]
		public virtual string Imei {
			get => imei;
			set => SetField(ref imei, value, () => Imei);
		}

		decimal orderSum;
		[Display(Name = "Сумма заказа")]
		public virtual decimal OrderSum {
			get => orderSum;
			set => SetField(ref orderSum, value, () => OrderSum);
		}

		DateTime created;
		[Display(Name = "Дата и время создания")]
		public virtual DateTime Created {
			get => created;
			set => SetField(ref created, value, () => Created);
		}

		#endregion свойства для мапинга
	}
}
