using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы производства",
		Nominative = "документ производства")]
	[EntityPermission]
	[HistoryTrace]
	public class IncomingWater : Document, IValidatableObject, ITwoWarhousesBindedDocument
	{
		Nomenclature product;

		[Required(ErrorMessage = "Продукт должн быть заполнен.")]
		[Display(Name = "Продукт")]
		public virtual Nomenclature Product {
			get { return product; }
			set {
				SetField(ref product, value, () => Product);
				if(ProduceOperation.Nomenclature != product)
				{
					ProduceOperation.Nomenclature = product;
				}
			}
		}

		int amount;

		[Display(Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set {
				SetField(ref amount, value, () => Amount);
				if(ProduceOperation.Amount != Amount)
				{
					ProduceOperation.Amount = Amount;
				}

				if(!NHibernate.NHibernateUtil.IsInitialized(Materials))
				{
					return;
				}

				foreach(var item in Materials) {
					if(item.OneProductAmount.HasValue)
					{
						item.Amount = item.OneProductAmount.Value * Amount;
					}
				}
			}
		}

		private Warehouse _incomingWarehouse;

		[Required(ErrorMessage = "Склад поступления должен быть указан.")]
		[Display(Name = "Склад поступления")]
		public virtual Warehouse IncomingWarehouse {
			get => _incomingWarehouse;
			set {
				SetField(ref _incomingWarehouse, value);
				if(ProduceOperation.Warehouse != IncomingWarehouse)
				{
					ProduceOperation.Warehouse = IncomingWarehouse;
				}
			}
		}

		private Warehouse _writeOffWarehouse;

		[Required(ErrorMessage = "Склад списания должен быть указан.")]
		[Display(Name = "Склад списания")]
		public virtual Warehouse WriteOffWarehouse {
			get => _writeOffWarehouse;
			set {
				SetField(ref _writeOffWarehouse, value);
				foreach(var item in Materials) {
					if(item.ConsumptionMaterialOperation != null && item.ConsumptionMaterialOperation.Warehouse != WriteOffWarehouse)
					{
						item.ConsumptionMaterialOperation.Warehouse = WriteOffWarehouse;
					}
				}
			}
		}

		public virtual string Title => string.Format("Документ производства №{0} от {1:d}", Id, TimeStamp);

		WarehouseBulkGoodsAccountingOperation produceOperation = new WarehouseBulkGoodsAccountingOperation {
			OperationTime = DateTime.Now
		};

		public virtual WarehouseBulkGoodsAccountingOperation ProduceOperation {
			get => produceOperation;
			set => SetField(ref produceOperation, value);
		}

		IList<IncomingWaterMaterial> materials = new List<IncomingWaterMaterial>();

		[Display(Name = "Строки")]
		public virtual IList<IncomingWaterMaterial> Materials {
			get => materials;
			set {
				SetField(ref materials, value, () => Materials);
				observableMaterials = null;
			}
		}

		GenericObservableList<IncomingWaterMaterial> observableMaterials;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomingWaterMaterial> ObservableMaterials {
			get {
				if(observableMaterials == null)
				{
					observableMaterials = new GenericObservableList<IncomingWaterMaterial>(Materials);
				}

				return observableMaterials;
			}
		}

		public virtual void AddMaterial(Nomenclature nomenclature, decimal amount, decimal inStock)
		{
			var item = new IncomingWaterMaterial {
				Document = this,
				Nomenclature = nomenclature,
				Amount = amount,
				AmountOnSource = inStock,
			};
			item.CreateOperation(WriteOffWarehouse, TimeStamp);
			ObservableMaterials.Add(item);
		}

		public virtual void AddMaterial(ProductSpecificationMaterial material)
		{
			var item = new IncomingWaterMaterial {
				Document = this,
				Nomenclature = material.Material,
				OneProductAmount = material.Amount,
			};
			item.CreateOperation(WriteOffWarehouse, TimeStamp);
			ObservableMaterials.Add(item);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Materials.Count == 0)
			{
				yield return new ValidationResult(string.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName(o => o.Materials) });
			}

			foreach(var item in Materials) {
				if(item.Amount <= 0)
				{
					yield return new ValidationResult($"Для сырья <{item.Nomenclature.Name}> не указано количество.",
						new[] { this.GetPropertyName(o => o.Materials) });
				}
			}

			if(Nomenclature.CategoriesWithWeightAndVolume.Contains(Product.Category)
			   && (Product.Weight == default || Product.Length == default || Product.Width == default || Product.Height == default))
			{
				yield return new ValidationResult(
					"В продукте обязательно должны быть заполнены вес и объём",
					new[] { nameof(Product) });
			}

			if(Amount < 1)
			{
				yield return new ValidationResult("Количество должно быть больше 1", new[] { nameof(Amount) });
			}
		}
	}
}
