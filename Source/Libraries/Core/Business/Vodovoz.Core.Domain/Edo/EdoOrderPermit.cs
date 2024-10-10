using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	///	Разрешение для отправки документов по заказу<br/>
	///	В данном случае под документами подразумеваются ЭДО документы и чеки<br/>
	///	В штатном режиме разрешение выдается автоматически при отправке или доставке заказа, 
	///	в зависимости от источника
	/// </summary>

	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "разрешения отправки документов",
		Nominative = "разрешение отправки документов"
	)]
	public class EdoOrderPermit : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _orderId;
		private DateTime _time;
		private OrderPermitSource _source;
		private string _reason;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время")]
		public virtual DateTime Time
		{
			get => _time;
			set => SetField(ref _time, value);
		}

		[Display(Name = "Заказ")]
		public virtual int OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		[Display(Name = "Источник")]
		public virtual OrderPermitSource Source
		{
			get => _source;
			set => SetField(ref _source, value);
		}

		[Display(Name = "Причина")]
		public virtual string Reason
		{
			get => _reason;
			set => SetField(ref _reason, value);
		}
	}
}
