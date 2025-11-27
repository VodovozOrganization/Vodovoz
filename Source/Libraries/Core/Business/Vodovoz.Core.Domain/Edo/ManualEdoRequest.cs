using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Ручная на отправку документов заказа по ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку документов заказа по ЭДО",
		NominativePlural = "заявки на отправку документов заказа по ЭДО"
	)]
	public class ManualEdoRequest : FormalEdoRequest
	{
		private OrderEntity _order;
		private ManualEdoTask _task;

		/// <summary>
		/// Код заказа
		/// </summary>
		[Display(Name = "Код заказа")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		public virtual new ManualEdoTask Task
		{
			get => _task;
			set => SetField(ref _task, value);
		}

		public override EdoRequestType DocumentRequestType => EdoRequestType.Manual;
	}
}
