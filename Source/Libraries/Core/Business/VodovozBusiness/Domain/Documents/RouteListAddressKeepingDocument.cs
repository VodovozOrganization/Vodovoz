using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "Документ ведения свободных остатков МЛ для адреса",
		NominativePlural = "Документы ведения свободных остатков МЛ для адреса")]
	[HistoryTrace]

	public class RouteListAddressKeepingDocument : Document
	{
		private RouteListItem _routeListItem;
		private IList<RouteListAddressKeepingDocumentItem> _items = new List<RouteListAddressKeepingDocumentItem>();

		[Display(Name = "Адрес")]
		public virtual RouteListItem RouteListItem
		{
			get => _routeListItem;
			set => SetField(ref _routeListItem, value);
		}

		[Display(Name = "Строки документа ведения свободных остатков МЛ")]
		public virtual IList<RouteListAddressKeepingDocumentItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		public virtual string Title => $"Документ ведения свободных остатков МЛ для адреса {RouteListItem.Id}";
	}
}
