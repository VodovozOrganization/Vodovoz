using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ ведения свободных остатков МЛ для адреса",
		NominativePlural = "документы ведения свободных остатков МЛ для адреса")]
	[HistoryTrace]

	public class RouteListKeepintDocument : Document
	{
		private RouteListItem _routeListItem;
		private IList<RouteListKeepingDocumentItem> _items = new List<RouteListKeepingDocumentItem>();

		[Display(Name = "Адрес")]
		public virtual RouteListItem RouteListItem
		{
			get => _routeListItem;
			set => SetField(ref _routeListItem, value);
		}

		[Display(Name = "Строки документа ведения свободных остатков МЛ")]
		public virtual IList<RouteListKeepingDocumentItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		public virtual string Title => $"Документ ведения свободных остатков МЛ для адреса {RouteListItem.Id}";
	}
}
