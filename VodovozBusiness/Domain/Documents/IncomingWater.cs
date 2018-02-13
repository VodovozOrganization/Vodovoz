using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using DataAnnotationsExtensions;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "документы производства",
		Nominative = "документ производства")]
	public class IncomingWater: Document, IValidatableObject
	{
		Nomenclature product;

		[Required (ErrorMessage = "Продукт должн быть заполнен.")]
		[Display (Name = "Продукт")]
		public virtual Nomenclature Product {
			get { return product; }
			set {
				SetField (ref product, value, () => Product);
				if (ProduceOperation.Nomenclature != product)
					ProduceOperation.Nomenclature = product;
			}
		}

		int amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set {
				SetField (ref amount, value, () => Amount);
				if (ProduceOperation.Amount != Amount)
					ProduceOperation.Amount = Amount;
				if (!NHibernate.NHibernateUtil.IsInitialized(Materials))
					return;
				foreach (var item in Materials) {
					if (item.OneProductAmount.HasValue)
						item.Amount = item.OneProductAmount.Value * Amount;
				}
			}
		}

		Warehouse incomingWarehouse;

		[Required (ErrorMessage = "Склад поступления должен быть указан.")]
		[Display (Name = "Склад поступления")]
		public virtual Warehouse IncomingWarehouse {
			get { return incomingWarehouse; }
			set {
				SetField (ref incomingWarehouse, value, () => IncomingWarehouse);
				if (ProduceOperation.IncomingWarehouse != IncomingWarehouse)
					ProduceOperation.IncomingWarehouse = IncomingWarehouse;
			}
		}

		Warehouse writeOffWarehouse;

		[Required (ErrorMessage = "Склад списания должен быть указан.")]
		[Display (Name = "Склад списания")]
		public virtual Warehouse WriteOffWarehouse {
			get { return writeOffWarehouse; }
			set {
				SetField (ref writeOffWarehouse, value, () => WriteOffWarehouse); 
				foreach (var item in Materials) {
					if (item.ConsumptionMaterialOperation != null && item.ConsumptionMaterialOperation.WriteoffWarehouse != WriteOffWarehouse)
						item.ConsumptionMaterialOperation.WriteoffWarehouse = WriteOffWarehouse;
				}
			}
		}

		public virtual string Title { 
			get { return String.Format ("Документ производства №{0} от {1:d}", Id, TimeStamp); }
		}

		WarehouseMovementOperation produceOperation = new WarehouseMovementOperation() {
			OperationTime = DateTime.Now
		};

		public virtual WarehouseMovementOperation ProduceOperation {
			get { return produceOperation; }
			set { SetField (ref produceOperation, value, () => ProduceOperation); }
		}

		IList<IncomingWaterMaterial> materials = new List<IncomingWaterMaterial> ();

		[Display (Name = "Строки")]
		public virtual IList<IncomingWaterMaterial> Materials {
			get { return materials; }
			set {
				SetField (ref materials, value, () => Materials);
				observableMaterials = null;
			}
		}

		GenericObservableList<IncomingWaterMaterial> observableMaterials;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomingWaterMaterial> ObservableMaterials {
			get {
				if (observableMaterials == null)
					observableMaterials = new GenericObservableList<IncomingWaterMaterial> (Materials);
				return observableMaterials;
			}
		}

		public virtual void AddMaterial (Nomenclature nomenclature, decimal amount, decimal inStock)
		{
			var item = new IncomingWaterMaterial{
				Document = this,
				Nomenclature = nomenclature,
				Amount = amount,
				AmountOnSource = inStock,
			};
			item.CreateOperation(WriteOffWarehouse, TimeStamp);
			ObservableMaterials.Add (item);
		}

		public virtual void AddMaterial (ProductSpecificationMaterial material)
		{
			var item = new IncomingWaterMaterial{
				Document = this,
				Nomenclature = material.Material,
				OneProductAmount = material.Amount,
			};
			item.CreateOperation(WriteOffWarehouse, TimeStamp);
			ObservableMaterials.Add (item);
		}

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Materials.Count == 0)
				yield return new ValidationResult (String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName (o => o.Materials) });

			foreach(var item in Materials)
			{
				if(item.Amount <= 0)
					yield return new ValidationResult (String.Format("Для сырья <{0}> не указано количество.", item.Nomenclature.Name),
						new[] { this.GetPropertyName (o => o.Materials) });
			}
		}

	}
}

