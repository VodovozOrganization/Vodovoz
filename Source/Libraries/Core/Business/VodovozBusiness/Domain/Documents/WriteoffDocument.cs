﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "акты списания ТМЦ",
		Nominative = "акт списания ТМЦ",
		Prepositional = "акте списания"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class WriteoffDocument : Document, IValidatableObject, IWarehouseBoundedDocument
	{
		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;
				foreach(var item in Items)
				{
					if(item.WarehouseWriteoffOperation != null && item.WarehouseWriteoffOperation.OperationTime != TimeStamp)
						item.WarehouseWriteoffOperation.OperationTime = TimeStamp;
					if(item.CounterpartyWriteoffOperation != null && item.CounterpartyWriteoffOperation.OperationTime != TimeStamp)
						item.CounterpartyWriteoffOperation.OperationTime = TimeStamp;
				}
			}
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		Employee responsibleEmployee;

		[Required(ErrorMessage = "Должен быть указан ответственнй за списание.")]
		[Display(Name = "Ответственный")]
		public virtual Employee ResponsibleEmployee
		{
			get => responsibleEmployee;
			set => SetField(ref responsibleEmployee, value, () => ResponsibleEmployee);
		}

		Counterparty client;

		[Display(Name = "Клиент списания")]
		public virtual Counterparty Client
		{
			get => client;
			set
			{
				client = value;
				if(Client != null)
					Warehouse = null;
				if(Client == null || !Client.DeliveryPoints.Contains(DeliveryPoint))
					DeliveryPoint = null;
				foreach(var item in Items)
				{
					if(item.CounterpartyWriteoffOperation != null && item.CounterpartyWriteoffOperation.WriteoffCounterparty != client)
						item.CounterpartyWriteoffOperation.WriteoffCounterparty = client;
				}
			}
		}

		DeliveryPoint deliveryPoint;

		[Display(Name = "Точка доставки списания")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => deliveryPoint;
			set
			{
				deliveryPoint = value;
				foreach(var item in Items)
				{
					if(item.CounterpartyWriteoffOperation != null && item.CounterpartyWriteoffOperation.WriteoffDeliveryPoint != deliveryPoint)
						item.CounterpartyWriteoffOperation.WriteoffDeliveryPoint = deliveryPoint;
				}
			}
		}

		Warehouse _warehouse;

		[Display(Name = "Склад списания")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				_warehouse = value;
				if(Warehouse != null)
					Client = null;

				foreach(var item in Items)
				{
					if(item.WarehouseWriteoffOperation != null && item.WarehouseWriteoffOperation.WriteoffWarehouse != _warehouse)
						item.WarehouseWriteoffOperation.WriteoffWarehouse = _warehouse;
				}
			}
		}

		IList<WriteoffDocumentItem> items = new List<WriteoffDocumentItem>();

		[Display(Name = "Строки")]
		public virtual IList<WriteoffDocumentItem> Items
		{
			get => items;
			set
			{
				SetField(ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<WriteoffDocumentItem> observableItems;

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WriteoffDocumentItem> ObservableItems
		{
			get
			{
				if(observableItems == null)
					observableItems = new GenericObservableList<WriteoffDocumentItem>(Items);
				return observableItems;
			}
		}

		public virtual string Title => String.Format("Акт списания №{0} от {1:d}", Id, TimeStamp);

		public virtual void AddItem(Nomenclature nomenclature, decimal amount, decimal inStock)
		{
			var item = new WriteoffDocumentItem
			{
				Nomenclature = nomenclature,
				AmountOnStock = inStock,
				Amount = amount,
				Document = this
			};
			if(Warehouse != null)
				item.CreateOperation(Warehouse, TimeStamp);
			else
				item.CreateOperation(Client, DeliveryPoint, TimeStamp);
			ObservableItems.Add(item);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Warehouse == null && Client == null)
				yield return new ValidationResult("Склад списания или контрагент должны быть заполнены.");
			if(Client != null && DeliveryPoint == null)
				yield return new ValidationResult("Точка доставки должна быть указана.");

			if(Items.Count == 0)
				yield return new ValidationResult(String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName(o => o.Items) });

			foreach(var item in Items)
			{
				if(item.Amount <= 0)
					yield return new ValidationResult(String.Format("Для номенклатуры <{0}> не указано количество.", item.Nomenclature.Name),
						new[] { this.GetPropertyName(o => o.Items) });
				if(item.Amount > item.AmountOnStock)
					yield return new ValidationResult(String.Format("На складе недостаточное количество <{0}>", item.Nomenclature.Name),
						new[] { this.GetPropertyName(o => o.Items) });
			}
		}

		public WriteoffDocument() => Comment = String.Empty;
	}

	public enum WriteoffType
	{
		[Display(Name = "Со склада")]
		warehouse,
		[Display(Name = "От клиента")]
		counterparty
	}

	public class WriteoffStringType : NHibernate.Type.EnumStringType
	{
		public WriteoffStringType () : base (typeof(WriteoffType))
		{
		}
	}
}

