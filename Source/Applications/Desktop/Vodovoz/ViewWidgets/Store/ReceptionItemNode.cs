using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Service;

namespace Vodovoz
{
	public class ReceptionItemNode : PropertyChangedBase
	{
		private decimal _amount;
		private decimal _expectedAmount;

		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		public virtual decimal Amount {
			get => _amount;
			set => SetField(ref _amount, value, () => Amount);
		}

		public virtual decimal ExpectedAmount {
			get => _expectedAmount;
			set => SetField(ref _expectedAmount, value, () => ExpectedAmount);
		}

		int equipmentId;
		[PropertyChangedAlso("Serial")]
		public int EquipmentId {
			get => equipmentId;
			set => SetField(ref equipmentId, value, () => EquipmentId);
		}

		[Display(Name = "№ кулера")]
		public string Redhead {
			get => CarUnloadDocumentItem.Redhead;
			set {
				if(value != CarUnloadDocumentItem.Redhead)
					CarUnloadDocumentItem.Redhead = value;
			}
		}

		ServiceClaim serviceClaim;

		public virtual ServiceClaim ServiceClaim {
			get => serviceClaim;
			set => SetField(ref serviceClaim, value, () => ServiceClaim);
		}

		public Equipment NewEquipment { get; set; }
		public bool Returned {
			get => Amount > 0;
			set => Amount = value ? 1 : 0;
		}

		GoodsAccountingOperation movementOperation = new GoodsAccountingOperation();

		public virtual GoodsAccountingOperation MovementOperation {
			get => movementOperation;
			set => SetField(ref movementOperation, value, () => MovementOperation);
		}

		public ReceptionItemNode(Nomenclature nomenclature, int amount)
		{
			Name = nomenclature.Name;
			NomenclatureId = nomenclature.Id;
			NomenclatureCategory = nomenclature.Category;
			_amount = amount;
		}

		public ReceptionItemNode(GoodsAccountingOperation movementOperation) : this(movementOperation.Nomenclature, (int)movementOperation.Amount)
		{
			this.movementOperation = movementOperation;
		}

		CarUnloadDocumentItem carUnloadDocumentItem = new CarUnloadDocumentItem();

		public virtual CarUnloadDocumentItem CarUnloadDocumentItem {
			get => carUnloadDocumentItem;
			set => SetField(ref carUnloadDocumentItem, value, () => CarUnloadDocumentItem);
		}

		public ReceptionItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.GoodsAccountingOperation)
		{
			this.carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public ReceptionItemNode() { }
	}
}

