using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Logistics
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "адреса маршрутного листа",
		Nominative = "адрес маршрутного листа")]
	[HistoryTrace]
	public class RouteListItemEntity : PropertyChangedBase, IDomainObject
	{
		private DateTime _version;
		private string _unscannedCodesReason;
		private IObservableList<RouteListItemTrueMarkProductCode> _trueMarkCodes = new ObservableList<RouteListItemTrueMarkProductCode>();

		public virtual int Id { get; set; }

		/// <summary>
		/// Версия
		/// </summary>
		[Display(Name = "Версия")]
		public virtual DateTime Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		/// <summary>
		/// Причина не отсканированных кодов
		/// </summary>
		[Display(Name = "Причина не отсканированных кодов")]
		public virtual string UnscannedCodesReason
		{
			get => _unscannedCodesReason;
			set => SetField(ref _unscannedCodesReason, value);
		}

		/// <summary>
		/// Коды ЧЗ товаров
		/// </summary>
		[Display(Name = "Коды ЧЗ товаров")]
		public virtual IObservableList<RouteListItemTrueMarkProductCode> TrueMarkCodes
		{
			get => _trueMarkCodes;
			set => SetField(ref _trueMarkCodes, value);
		}
	}
}
