using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Отсканированный водителем код ЧЗ
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "отсканированный водителем код ЧЗ",
		NominativePlural = "отсканированные водителями коды ЧЗ",
		Prepositional = "отсканированном водителем коде ЧЗ",
		PrepositionalPlural = "отсканированным водителями кодами ЧЗ",
		Accusative = "отсканированный водителем код ЧЗ",
		AccusativePlural = "отсканированные водителями коды ЧЗ",
		Genitive = "отсканированного водителем кода ЧЗ",
		GenitivePlural = "отсканированных водителями кодов ЧЗ")]
	[HistoryTrace]
	public class DriversScannedTrueMarkCode : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _rawCode;
		private int _orderItemId;
		private int _routeListAddressId;
		private bool _isDefective;
		private bool _isProcessingCompleted;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Отсканированный код
		/// </summary>
		[Display(Name = "Отсканированный код")]
		public virtual string RawCode
		{
			get => _rawCode;
			set => SetField(ref _rawCode, value);
		}

		/// <summary>
		/// Id строки МЛ
		/// </summary>
		[Display(Name = "Id строки заказа")]
		public virtual int OrderItemId
		{
			get => _orderItemId;
			set => SetField(ref _orderItemId, value);
		}

		/// <summary>
		/// Id адреса МЛ
		/// </summary>
		[Display(Name = "Id адреса МЛ")]
		public virtual int RouteListAddressId
		{
			get => _routeListAddressId;
			set => SetField(ref _routeListAddressId, value);
		}

		/// <summary>
		/// Дефект
		/// </summary>
		[Display(Name = "Дефект")]
		public virtual bool IsDefective
		{
			get => _isDefective;
			set => SetField(ref _isDefective, value);
		}

		/// <summary>
		/// Обработка кода завершена
		/// </summary>
		[Display(Name = "Обработка кода завершена")]
		public virtual bool IsProcessingCompleted
		{
			get => _isProcessingCompleted;
			set => SetField(ref _isProcessingCompleted, value);
		}
	}
}
