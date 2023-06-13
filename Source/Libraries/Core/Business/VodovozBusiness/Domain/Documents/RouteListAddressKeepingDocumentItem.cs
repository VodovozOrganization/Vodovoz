using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "Строка документа ведения свободных остатков МЛ для адреса",
		NominativePlural = "Строки документа ведения свободных остатков МЛ для адреса")]

	[HistoryTrace]
	public class RouteListAddressKeepingDocumentItem : PropertyChangedBase, IDomainObject
	{
		private RouteListAddressKeepingDocument _routeListAddressKeepingDocument;
		private DeliveryFreeBalanceOperation _deliveryFreeBalanceOperation;
		private Nomenclature _nomenclature;
		private decimal _amount;

		public virtual int Id { get; set; }

		[Display(Name = "Документ ведения свободных остатков МЛ для адреса")]
		public virtual RouteListAddressKeepingDocument RouteListAddressKeepingDocument
		{
			get => _routeListAddressKeepingDocument;
			set => SetField(ref _routeListAddressKeepingDocument, value);
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
			operation.RouteList = RouteListAddressKeepingDocument.RouteListItem.RouteList;

			DeliveryFreeBalanceOperation = operation;
		}

		public virtual string Title => $"Строка документа ведения свободных остатков МЛ для заказа №{RouteListAddressKeepingDocument.RouteListItem.Order.Id} {Nomenclature.Name} кол-во {Amount}";
	}
}
