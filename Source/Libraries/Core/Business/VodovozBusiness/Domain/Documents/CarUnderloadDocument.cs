using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "документ недопогрузки МЛ",
		NominativePlural = "документы недопогрузки МЛ")]
	[HistoryTrace]
	public class CarUnderloadDocument : Document
	{
		private RouteList _routeList;
		private IList<CarUnderloadDocumentItem> _items = new List<CarUnderloadDocumentItem>();

		[Display(Name = "МЛ")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}

		[Display(Name = "Строки документа ведения свободных остатков МЛ")]
		public virtual IList<CarUnderloadDocumentItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		public virtual string Title => $"Документ недопогрузки МЛ № {RouteList.Id}";
	}
}
