using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;

namespace VodovozBusiness.Domain.Documents
{
	/// <summary>
	/// Документ пересортицы товаров
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "пересортицы товаров",
		Nominative = "пересортица товаров")]
	[EntityPermission]
	[HistoryTrace]
	public class RegradingOfGoodsDocument : Document, IValidatableObject, IWarehouseBoundedDocument
	{
		private string _comment;
		private Warehouse _warehouse;
		private IObservableList<RegradingOfGoodsDocumentItem> _items = new ObservableList<RegradingOfGoodsDocumentItem>();

		/// <summary>
		/// Дата пересортицы
		/// </summary>
		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;
				foreach(var item in Items)
				{
					if(item.WarehouseWriteOffOperation != null && item.WarehouseWriteOffOperation.OperationTime != TimeStamp)
					{
						item.WarehouseWriteOffOperation.OperationTime = TimeStamp;
					}

					if(item.WarehouseIncomeOperation != null && item.WarehouseIncomeOperation.OperationTime != TimeStamp)
					{
						item.WarehouseIncomeOperation.OperationTime = TimeStamp;
					}
				}
			}
		}

		/// <summary>
		/// Комментарий
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Склад
		/// </summary>
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
					if(item.WarehouseWriteOffOperation != null && item.WarehouseWriteOffOperation.Warehouse != Warehouse)
					{
						item.WarehouseWriteOffOperation.Warehouse = Warehouse;
					}

					if(item.WarehouseIncomeOperation != null && item.WarehouseIncomeOperation.Warehouse != Warehouse)
					{
						item.WarehouseIncomeOperation.Warehouse = Warehouse;
					}
				}
			}
		}

		/// <summary>
		/// Строки
		/// </summary>
		[Display(Name = "Строки")]
		public virtual IObservableList<RegradingOfGoodsDocumentItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		/// <summary>
		/// Заголовок
		/// </summary>
		public virtual string Title => $"Пересортица товаров №{Id} от {TimeStamp:d}";

		/// <summary>
		/// Добавление строки
		/// </summary>
		/// <param name="item"></param>
		public virtual void AddItem(RegradingOfGoodsDocumentItem item)
		{
			item.Document = this;
			item.WarehouseIncomeOperation.OperationTime = TimeStamp;
			item.WarehouseWriteOffOperation.OperationTime = TimeStamp;
			item.WarehouseIncomeOperation.Warehouse = Warehouse;
			item.WarehouseWriteOffOperation.Warehouse = Warehouse;

			Items.Add(item);
		}

		/// <summary>
		/// Валидация
		/// </summary>
		/// <param name="validationContext">Контекст валидации</param>
		/// <returns></returns>
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Items.Count == 0)
			{
				yield return new ValidationResult(
					"Табличная часть документа пустая.",
					new[] { this.GetPropertyName(o => o.Items) });
			}

			foreach(var item in Items)
			{
				if(item.Amount > item.AmountInStock)
				{
					yield return new ValidationResult(
						$"На складе недостаточное количество <{item.NomenclatureOld.Name}>",
						new[] { this.GetPropertyName(o => o.Items) });
				}

				if(item.NomenclatureOld.Category == NomenclatureCategory.bottle
				   && item.NomenclatureNew.Category == NomenclatureCategory.water
				   && !item.NomenclatureNew.IsDisposableTare
				   && item.Amount > 39)
				{
					yield return new ValidationResult(
						$"Пересортица из {item.Amount} ед. '{item.NomenclatureOld.Name}'" +
						$" в {item.Amount} ед. '{item.NomenclatureNew.Name}' невозможна!",
						new[] { this.GetPropertyName(o => o.Items) });
				}
			}

			if(Items.Any(x => x.IsDefective && x.TypeOfDefect == null))
			{
				yield return new ValidationResult(
					"Необходимо указать вид брака.",
					new[] { this.GetPropertyName(o => o.Items) });
			}

			if(Items.Any(x => x.IsDefective && x.Source == DefectSource.None))
			{
				yield return new ValidationResult(
					"Необходимо указать источник брака.",
					 new[] { this.GetPropertyName(o => o.Items) });
			}

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
				yield return new ValidationResult(
					"Выберите причину пересортицы для всех строк документа",
					new[] { nameof(CarEventType) });
			}

			if(!string.IsNullOrEmpty(Comment) && Comment.Length > 250)
			{
				yield return new ValidationResult(
					"Длина комментария не должна превышать 250 символов",
					new[] { nameof(Comment) });
			}
		}
	}
}
