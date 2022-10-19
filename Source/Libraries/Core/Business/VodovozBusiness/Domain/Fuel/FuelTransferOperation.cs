using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "операция перемещения топлива",
		NominativePlural = "операции перемещения топлива")]
	public class FuelTransferOperation : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Subdivision subdivisionFrom;
		[Display(Name = "Касса отправитель")]
		public virtual Subdivision SubdivisionFrom {
			get => subdivisionFrom;
			set => SetField(ref subdivisionFrom, value, () => SubdivisionFrom);
		}

		private Subdivision subdivisionTo;
		[Display(Name = "Касса получатель")]
		public virtual Subdivision SubdivisionTo {
			get => subdivisionTo;
			set => SetField(ref subdivisionTo, value, () => SubdivisionTo);
		}

		private FuelType fuelType;
		[Display(Name = "Тип топлива")]
		public virtual FuelType FuelType {
			get => fuelType;
			set => SetField(ref fuelType, value, () => FuelType);
		}

		private decimal transferedLiters;
		[Display(Name = "Транспортируемое топливо")]
		public virtual decimal TransferedLiters {
			get => transferedLiters;
			set => SetField(ref transferedLiters, value, () => TransferedLiters);
		}

		private DateTime sendTime;
		[Display(Name = "Дата отправки")]
		public virtual DateTime SendTime {
			get => sendTime;
			set => SetField(ref sendTime, value, () => SendTime);
		}

		private DateTime? receiveTime;
		[Display(Name = "Дата получения")]
		public virtual DateTime? ReceiveTime {
			get => receiveTime;
			set => SetField(ref receiveTime, value, () => ReceiveTime);
		}
	}
}
