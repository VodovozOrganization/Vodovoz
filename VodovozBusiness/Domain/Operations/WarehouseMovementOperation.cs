using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Operations
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class WarehouseMovementOperation: OperationBase
	{
		#region Свойства

		Warehouse writeoffWarehouse;

		public virtual Warehouse WriteoffWarehouse {
			get { return writeoffWarehouse; }
			set { SetField (ref writeoffWarehouse, value, () => WriteoffWarehouse); }
		}

		Warehouse incomingWarehouse;

		public virtual Warehouse IncomingWarehouse {
			get { return incomingWarehouse; }
			set { SetField (ref incomingWarehouse, value, () => IncomingWarehouse); }
		}

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		decimal amount;

		public virtual decimal Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		#endregion

		#region Вычисляемые

		public virtual string Title{
			get{
				if (IncomingWarehouse != null && WriteoffWarehouse != null)
					return string.Format("Перемещение из {0} в {1}, {2} - {3}", WriteoffWarehouse.Name, IncomingWarehouse.Name, Nomenclature.Name, Nomenclature.Unit.MakeAmountShortStr(Amount));
				else if (IncomingWarehouse != null)
					return string.Format("Поступление в {0}, {1} - {2}", IncomingWarehouse.Name, Nomenclature.Name, Nomenclature.Unit.MakeAmountShortStr(Amount));
				else if (WriteoffWarehouse != null)
					return string.Format("Выбытие из {0}, {1} - {2}", WriteoffWarehouse.Name, Nomenclature.Name, Nomenclature.Unit.MakeAmountShortStr(Amount));
				else
					return null;
			}
		}

		#endregion

	}
}

