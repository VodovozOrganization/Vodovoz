using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "Изменение свободных остатков при переносе адреса",
		NominativePlural = "Изменения свободных остатков при переносе адреса")]
	[HistoryTrace]

	public class DeliveryFreeBalanceTransferItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private DeliveryFreeBalanceOperation _deliveryFreeBalanceOperationFrom;
		private DeliveryFreeBalanceOperation _deliveryFreeBalanceOperationTo;
		private AddressTransferDocumentItem _addressTransferDocumentItem;

		[Display(Name = "Строка документа переноса адресов")]
		public virtual AddressTransferDocumentItem AddressTransferDocumentItem
		{
			get => _addressTransferDocumentItem;
			set => SetField(ref _addressTransferDocumentItem, value);
		}

		private RouteList _routeListFrom;
		[Display(Name = "От водителя")]
		public virtual RouteList RouteListFrom
		{
			get => _routeListFrom;
			set => SetField(ref _routeListFrom, value);
		}

		private RouteList _routeListTo;
		[Display(Name = "К водителю")]
		public virtual RouteList RouteListTo
		{
			get => _routeListTo;
			set => SetField(ref _routeListTo, value);
		}

		private Nomenclature _nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		private decimal _amount;
		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Операция списания номенклатуры со свободных остатков МЛ")]
		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperationFrom
		{
			get => _deliveryFreeBalanceOperationFrom;
			set => SetField(ref _deliveryFreeBalanceOperationFrom, value);
		}

		[Display(Name = "Операция зачисления номенклатуры на свободные остатки МЛ")]
		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperationTo
		{
			get => _deliveryFreeBalanceOperationTo;
			set => SetField(ref _deliveryFreeBalanceOperationTo, value);
		}

		public virtual string Title =>
			$"Строка изменения свободных остатков при переносе заказа {AddressTransferDocumentItem.NewAddress.Order.Id}" +
			$" из МЛ {RouteListFrom.Id} в МЛ {RouteListTo.Id}, {Nomenclature.Name} кол-во {Amount}";

	}
}
