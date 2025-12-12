using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку документов ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку документов ЭДО",
		NominativePlural = "заявки на отправку документов ЭДО"
	)]
	public abstract class FormalEdoRequest : EdoRequest
	{
		private OrderEntity _order;
		private IObservableList<TrueMarkProductCode> _productCodes = new ObservableList<TrueMarkProductCode>();

		/// <summary>
		/// Тип заявки
		/// </summary>
		public abstract EdoRequestType DocumentRequestType { get; }

		/// <summary>
		/// Коды маркировки
		/// </summary>
		[Display(Name = "Коды маркировки")]
		public virtual IObservableList<TrueMarkProductCode> ProductCodes
		{
			get => _productCodes;
			set => SetField(ref _productCodes, value);
		}

		/// <summary>
		/// Код заказа
		/// </summary>
		[Display(Name = "Код заказа")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}
	}
}
