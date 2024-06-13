using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "входящие накладные",
		Nominative = "входящая накладная")]
	[EntityPermission]
	[HistoryTrace]
	public class IncomingInvoice : Document, IValidatableObject, IWarehouseBoundedDocument
	{
		private IList<IncomingInvoiceItem> _items = new List<IncomingInvoiceItem>();

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

		private GenericObservableList<IncomingInvoiceItem> _observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomingInvoiceItem> ObservableItems
		{
			get
			{
				if(_observableItems == null)
				{
					_observableItems = new GenericObservableList<IncomingInvoiceItem>(Items);
				}

				return _observableItems;
			}
		}

		#region Properties

		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;
				foreach(var item in Items)
				{
					if(item.IncomeGoodsOperation.OperationTime != TimeStamp)
					{
						item.IncomeGoodsOperation.OperationTime = TimeStamp;
					}
				}
			}
		}

		private string _invoiceNumber;

		[Display(Name = "Номер счета-фактуры")]
		public virtual string InvoiceNumber
		{
			get => _invoiceNumber;
			set => SetField(ref _invoiceNumber, value);
		}

		private string _waybillNumber;

		[Display(Name = "Номер входящей накладной")]
		public virtual string WaybillNumber
		{
			get => _waybillNumber;
			set => SetField(ref _waybillNumber, value);
		}

		private Counterparty _contractor;

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

		private Warehouse _warehouse;

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
					if(item.IncomeGoodsOperation.IncomingWarehouse != _warehouse)
					{
						item.IncomeGoodsOperation.IncomingWarehouse = _warehouse;
					}
				}
			}
		}

		private string _comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
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

			item.IncomeGoodsOperation.IncomingWarehouse = _warehouse;
			item.IncomeGoodsOperation.OperationTime = TimeStamp;
			item.Document = this;
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
			int maxCommentLength = 500;
			if(Comment?.Length > maxCommentLength)
			{
				yield return new ValidationResult(
					$"Строка комментария слишком длинная. Максимальное количество символов: {maxCommentLength}",
					new[] { this.GetPropertyName(o => o.Items) }
				);
			}

			if(!Items.Any())
			{
				yield return new ValidationResult(
					"Табличная часть документа пустая.",
					new[] { this.GetPropertyName(o => o.Items) }
				);
			}

			foreach(var item in Items)
			{
				if(item.Amount <= 0)
				{
					yield return new ValidationResult(
						$"Для номенклатуры <{item.Nomenclature.Name}> не указано количество.",
						new[] { this.GetPropertyName(o => o.Items) }
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
				yield return new ValidationResult(
					"\"Номер счета-фактуры\" и \"Номер входящей накладной\" должны быть указаны",
					new[] {
						this.GetPropertyName(o => o.WaybillNumber),
						this.GetPropertyName(o => o.InvoiceNumber)
					}
				);
			}

			if(Contractor == null)
			{
				yield return new ValidationResult(
					"\"Контрагент\" должен быть указан",
					new[] {
						this.GetPropertyName(o => o.Contractor)
					}
				);
			}

			if(string.IsNullOrWhiteSpace(Comment))
			{
				yield return new ValidationResult(
					"Добавьте комментарий",
					new[] {
						this.GetPropertyName(o => o.Comment)
					}
				);
			}
		}

		#endregion
	}
}
