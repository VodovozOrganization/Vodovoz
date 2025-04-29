using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "истории печати документов",
		Nominative = "история печати документов")]
	public class DocumentPrintHistory : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; }

		private DateTime _printingTime;
		[Display(Name = "Время печати")]
		public virtual DateTime PrintingTime
		{
			get => _printingTime;
			set => SetField(ref _printingTime, value);
		}

		private PrintedDocumentType _documentType;
		[Display(Name = "Тип документа")]
		public virtual PrintedDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		private RouteList _routeList;
		[Display(Name = "Маршрутный лист")]
		public virtual RouteList RouteList
		{
			get => _routeList;
			set => SetField(ref _routeList, value);
		}
	}

	public enum PrintedDocumentType
	{
		[Display(Name = "Маршрутный лист")]
		RouteList,
		[Display(Name = "Закрытый маршрутный лист")]
		ClosedRouteList
	}
}
