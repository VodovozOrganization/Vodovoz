using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class WarehouseMovementOperation : OperationBase
	{
		#region Свойства

		private Warehouse writeoffWarehouse;
		public virtual Warehouse WriteoffWarehouse {
			get => writeoffWarehouse;
			set { SetField (ref writeoffWarehouse, value, () => WriteoffWarehouse); }
		}

		private Warehouse incomingWarehouse;
		public virtual Warehouse IncomingWarehouse {
			get => incomingWarehouse;
			set { SetField (ref incomingWarehouse, value, () => IncomingWarehouse); }
		}

		private Nomenclature nomenclature;
		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		private Equipment equipment;
		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get => equipment;
			set { SetField (ref equipment, value, () => Equipment); }
		}

		private decimal primeCost;
		public virtual decimal PrimeCost {
			get => primeCost;
			set { SetField(ref primeCost, value, () => PrimeCost); }
		}

		private decimal amount;
		public virtual decimal Amount {
			get => amount;
			set { SetField (ref amount, value, () => Amount); }
		}

		#endregion

		#region Вычисляемые

		public virtual string Title {
			get {
				if (IncomingWarehouse != null && WriteoffWarehouse != null)
					return $"Перемещение из {WriteoffWarehouse.Name} в {IncomingWarehouse.Name}, {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";
				if (IncomingWarehouse != null)
					return $"Поступление в {IncomingWarehouse.Name}, {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";
				if (WriteoffWarehouse != null)
					return $"Выбытие из {WriteoffWarehouse.Name}, {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";
				return null;
			}
		}

		#endregion
	}
}

