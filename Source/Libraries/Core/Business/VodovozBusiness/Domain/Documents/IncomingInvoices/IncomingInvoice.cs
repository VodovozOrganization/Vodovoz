using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.IncomingInvoices
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "входящие накладные",
		Nominative = "входящая накладная")]
	[EntityPermission]
	[HistoryTrace]
	public class IncomingInvoice : Document, IValidatableObject
	{
		private const int _maxCommentLength = 500;
		private string _invoiceNumber;
		private string _waybillNumber;
		private string _comment;
		private Counterparty _contractor;
		private Warehouse _warehouse;

		private IList<IncomingInvoiceItem> _items = new List<IncomingInvoiceItem>();
		private GenericObservableList<IncomingInvoiceItem> _observableItems;

		[Display(Name = "Строки")]
		public virtual IList<IncomingInvoiceItem> Items
		{
			get => _items;
			set
			{
				SetField(ref _items, value);
				_observableItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomingInvoiceItem> ObservableItems =>
			_observableItems ?? (_observableItems = new GenericObservableList<IncomingInvoiceItem>(Items));

		#region Properties

		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;
				foreach(var item in Items)
				{
					item.GoodsAccountingOperation.OperationTime = value;
				}
			}
		}

		[Display(Name = "Номер счета-фактуры")]
		public virtual string InvoiceNumber
		{
			get => _invoiceNumber;
			set => SetField(ref _invoiceNumber, value);
		}

		[Display(Name = "Номер входящей накладной")]
		public virtual string WaybillNumber
		{
			get => _waybillNumber;
			set => SetField(ref _waybillNumber, value);
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Contractor
		{
			get => _contractor;
			set => SetField(ref _contractor, value);
		}
		
		public virtual decimal TotalSum
		{
			get
			{
				decimal total = 0;
				foreach(var item in Items)
				{
					total += item.Sum;
				}
				return total;
			}
		}

		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set
			{
				SetField(ref _warehouse, value);
				foreach(var item in Items)
				{
					item.UpdateWarehouseOperation();
				}
			}
		}
		
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => _comment;
			set => SetField(ref _comment, value);
		}

		public virtual string Title => $"Поступление №{Id} от {TimeStamp:d}";
		
		public virtual bool CanAddItem => true;
		public virtual bool CanDeleteItems => true;

		#endregion
		
		#region Functions
		
		public virtual void AddItem(IncomingInvoiceItem item)
		{
			if(!CanAddItem)
			{
				return;
			}

			item.Document = this;
			item.UpdateWarehouseOperation();
			item.GoodsAccountingOperation.OperationTime = TimeStamp;
			
			ObservableItems.Add(item);
		}
		
		public virtual void DeleteItem(IncomingInvoiceItem item)
		{
			if(item == null || !CanDeleteItems || !ObservableItems.Contains(item))
			{
				return;
			}

			ObservableItems.Remove(item);
		}
		
		public IncomingInvoice()
		{
			WaybillNumber = string.Empty;
			InvoiceNumber = string.Empty;
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Comment?.Length > _maxCommentLength)
			{
				yield return new ValidationResult(
					$"Строка комментария слишком длинная. Максимальное количество символов: {_maxCommentLength}",
					new[] { nameof(Items) }
				);
			}

			if(!Items.Any())
			{
				yield return new ValidationResult("Табличная часть документа пустая", new[] { nameof(Items) });
			}

			foreach(var item in Items) {
				if(item.Amount <= 0)
				{
					yield return new ValidationResult(
						$"Для номенклатуры <{item.Nomenclature.Name}> не указано количество.",
						new[] { nameof(Items) }
					);
				}
			}

			var needWeightOrVolume = Items
				.Select(item => item.Nomenclature)
				.Where(nomenclature =>
					Nomenclature.CategoriesWithWeightAndVolume.Contains(nomenclature.Category)
					&& (nomenclature.Weight == default
						|| nomenclature.Length == default
						|| nomenclature.Width == default
						|| nomenclature.Height == default))
				.ToList();
			if(needWeightOrVolume.Any())
			{
				yield return new ValidationResult(
					"Для всех добавленных номенклатур должны быть заполнены вес и объём.\n" +
					"Список номенклатур, в которых не заполнен вес или объём:\n" +
					$"{string.Join("\n", needWeightOrVolume.Select(x => $"({x.Id}) {x.Name}"))}",
					new[] { nameof(Items) });
			}

			if(string.IsNullOrWhiteSpace(WaybillNumber) || string.IsNullOrWhiteSpace(InvoiceNumber))
			{
				yield return new ValidationResult("\"Номер счета-фактуры\" и \"Номер входящей накладной\" должны быть указаны",
					new[] { nameof(WaybillNumber), nameof(InvoiceNumber) }
				);
			}

			if(Contractor == null)
			{
				yield return new ValidationResult("\"Контрагент\" должен быть указан", new[] { nameof(Contractor) });
			}

			if(string.IsNullOrWhiteSpace(Comment))
			{
				yield return new ValidationResult("Добавьте комментарий", new[] { nameof(Comment) });
			}
		}

		#endregion
	}
}

