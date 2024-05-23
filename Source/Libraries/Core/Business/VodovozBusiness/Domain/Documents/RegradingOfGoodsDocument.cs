﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "пересортицы товаров",
		Nominative = "пересортица товаров")]
	[EntityPermission]
	[HistoryTrace]
	public class RegradingOfGoodsDocument: Document, IValidatableObject, IWarehouseBoundedDocument
	{
		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.WarehouseWriteOffOperation != null && item.WarehouseWriteOffOperation.OperationTime != TimeStamp)
						item.WarehouseWriteOffOperation.OperationTime = TimeStamp;
					if (item.WarehouseIncomeOperation != null && item.WarehouseIncomeOperation.OperationTime != TimeStamp)
						item.WarehouseIncomeOperation.OperationTime = TimeStamp;
				}
			}
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		Warehouse warehouse;

		[Display (Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set {
				SetField (ref warehouse, value, () => Warehouse);

				foreach (var item in Items) {
					if (item.WarehouseWriteOffOperation != null && item.WarehouseWriteOffOperation.Warehouse != Warehouse)
						item.WarehouseWriteOffOperation.Warehouse = Warehouse;
					if (item.WarehouseIncomeOperation != null && item.WarehouseIncomeOperation.Warehouse != Warehouse)
						item.WarehouseIncomeOperation.Warehouse = Warehouse;
				}
			}
		}

		IList<RegradingOfGoodsDocumentItem> items = new List<RegradingOfGoodsDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<RegradingOfGoodsDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<RegradingOfGoodsDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RegradingOfGoodsDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<RegradingOfGoodsDocumentItem> (Items);
				return observableItems;
			}
		}

		#region Функции

		public virtual string Title => String.Format("Пересортица товаров №{0} от {1:d}", Id, TimeStamp);

		public virtual void AddItem(RegradingOfGoodsDocumentItem item)
		{
			item.Document = this;
			item.WarehouseIncomeOperation.OperationTime = TimeStamp;
			item.WarehouseWriteOffOperation.OperationTime = TimeStamp;
			item.WarehouseIncomeOperation.Warehouse = Warehouse;
			item.WarehouseWriteOffOperation.Warehouse = Warehouse;
				
			ObservableItems.Add (item);
		}

		#endregion

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Items.Count == 0)
				yield return new ValidationResult (String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName (o => o.Items) });

			foreach(var item in Items)
			{
				if(item.Amount > item.AmountInStock)
					yield return new ValidationResult (String.Format("На складе недостаточное количество <{0}>", item.NomenclatureOld.Name),
						new[] { this.GetPropertyName (o => o.Items) });

				if(item.NomenclatureOld.Category == NomenclatureCategory.bottle
				   && item.NomenclatureNew.Category == NomenclatureCategory.water && !item.NomenclatureNew.IsDisposableTare
				   && item.Amount > 39)
					yield return new ValidationResult(String.Format("Пересортица из {0} ед. '{1}' в {0} ед. '{2}' невозможна!", item.Amount, item.NomenclatureOld.Name, item.NomenclatureNew.Name), 
					                                  new[] { this.GetPropertyName(o => o.Items) });
			}
			if(ObservableItems.Any(x => x.NomenclatureNew.IsDefectiveBottle && x.TypeOfDefect == null))
				yield return new ValidationResult("Необходимо указать вид брака.",
				                                  new[] { this.GetPropertyName(o => o.ObservableItems) });
			if(ObservableItems.Any(x => x.NomenclatureNew.IsDefectiveBottle && x.Source == DefectSource.None))
				yield return new ValidationResult("Необходимо указать источник брака.",
												  new[] { this.GetPropertyName(o => o.ObservableItems) });

			var needWeightOrVolume = Items
				.Select(item => item.NomenclatureNew)
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
					"Для всех новых добавленных номенклатур должны быть заполнены вес и объём.\n" +
					"Список номенклатур, в которых не заполнен вес или объём:\n" +
					$"{string.Join("\n", needWeightOrVolume.Select(x => $"({x.Id}) {x.Name}"))}",
					new[] { nameof(Items) });
			}

			if(Items.Any(x => x.RegradingOfGoodsReason == null))
			{
				yield return new ValidationResult("Выберите причину пересортицы для всех строк документа",
					new[] { nameof(CarEventType) });
			}
		}
	}
}

