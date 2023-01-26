using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "строка документа ведения свободных остатков МЛ для адреса",
		NominativePlural = "строки документа ведения свободных остатков МЛ для адреса")]

	[HistoryTrace]
	public class RouteListKeepingDocumentItem : PropertyChangedBase, IDomainObject
	{
		private RouteListKeepintDocument _routeListKeepintDocument;
		private DeliveryFreeBalanceOperation _deliveryFreeBalanceOperation;
		private Nomenclature _nomenclature;
		private decimal _amount;

		public virtual int Id { get; set; }

		[Display(Name = "Документ ведения свободных остатков МЛ для адреса")]
		public virtual RouteListKeepintDocument RouteListKeepintDocument
		{
			get => _routeListKeepintDocument;
			set => SetField(ref _routeListKeepintDocument, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Кол-во")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Операция со свободными остатками МЛ")]
		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperation
		{
			get => _deliveryFreeBalanceOperation;
			set => SetField(ref _deliveryFreeBalanceOperation, value);
		}

		public virtual void CreateOrUpdateOperation()
		{
			var operation = DeliveryFreeBalanceOperation ?? new DeliveryFreeBalanceOperation();
			operation.Amount = Amount;
			operation.Nomenclature = Nomenclature;
			operation.RouteList = RouteListKeepintDocument.RouteListItem.RouteList;

			DeliveryFreeBalanceOperation = operation;
		}

		public virtual string Title => $"Строка документа ведения свободных остатков МЛ для адреса №{RouteListKeepintDocument.RouteListItem.Id} {Nomenclature.Name} кол-во {Amount}";
	}
}
