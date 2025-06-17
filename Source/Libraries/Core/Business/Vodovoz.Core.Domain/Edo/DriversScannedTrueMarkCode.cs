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
		private DriversScannedTrueMarkCodeStatus _driversScannedTrueMarkCodeStatus;
		private DriversScannedTrueMarkCodeError _driversScannedTrueMarkCodeError;

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
		/// Статус обработки отсканированного водителем кода ЧЗ
		/// </summary>
		[Display(Name = "Статус обработки отсканированного водителем кода ЧЗ")]
		public virtual DriversScannedTrueMarkCodeStatus DriversScannedTrueMarkCodeStatus
		{
			get => _driversScannedTrueMarkCodeStatus;
			set => SetField(ref _driversScannedTrueMarkCodeStatus, value);
		}

		/// <summary>
		/// Тип ошибки, возникшей при обработке отсканированных водителем кодов ЧЗ
		/// </summary>
		[Display(Name = "Тип ошибки, возникшей при обработке отсканированных водителем кодов ЧЗ")]
		public virtual DriversScannedTrueMarkCodeError DriversScannedTrueMarkCodeError
		{
			get => _driversScannedTrueMarkCodeError;
			set => SetField(ref _driversScannedTrueMarkCodeError, value);
		}
	}
}
