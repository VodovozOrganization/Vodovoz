using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "изменение свободных остатков при переносе адреса",
		NominativePlural = "изменения свободных остатков при переносе адреса")]
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

		[Display(Name = "Операция списания номенклатуры со свободных остатков сотрудника")]
		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperationFrom
		{
			get => _deliveryFreeBalanceOperationFrom;
			set => SetField(ref _deliveryFreeBalanceOperationFrom, value);
		}

		[Display(Name = "Операция зачисления номенклатуры на свободные остатки сотрудника")]
		public virtual DeliveryFreeBalanceOperation DeliveryFreeBalanceOperationTo
		{
			get => _deliveryFreeBalanceOperationTo;
			set => SetField(ref _deliveryFreeBalanceOperationTo, value);
		}

		public virtual void CreateOrUpdateOperations()
		{
			if(AddressTransferDocumentItem.AddressTransferType == AddressTransferType.FromDriverToDriver)
			{
				return;
			}

			var freeBalanceOperationFrom = DeliveryFreeBalanceOperationFrom ?? new DeliveryFreeBalanceOperation();
			freeBalanceOperationFrom.Amount = Amount;
			freeBalanceOperationFrom.Nomenclature = Nomenclature;
			freeBalanceOperationFrom.OperationTime = DateTime.Now;
			freeBalanceOperationFrom.RouteList = AddressTransferDocumentItem.OldAddress.RouteList;

			DeliveryFreeBalanceOperationFrom = freeBalanceOperationFrom;
			RouteListFrom.ObservableDeliveryFreeBalanceOperations.Add(DeliveryFreeBalanceOperationFrom);

			if(AddressTransferDocumentItem.AddressTransferType == AddressTransferType.NeedToReload)
			{
				return;
			}

			var freeBalanceOperationTo = DeliveryFreeBalanceOperationTo ?? new DeliveryFreeBalanceOperation();
			freeBalanceOperationTo.Amount = -Amount;
			freeBalanceOperationTo.Nomenclature = Nomenclature;
			freeBalanceOperationTo.OperationTime = DateTime.Now;
			freeBalanceOperationTo.RouteList = AddressTransferDocumentItem.NewAddress.RouteList;

			DeliveryFreeBalanceOperationTo = freeBalanceOperationTo;
			RouteListTo.ObservableDeliveryFreeBalanceOperations.Add(DeliveryFreeBalanceOperationTo);
		}
	}
}
