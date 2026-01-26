using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
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

		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		public virtual decimal ExpectedAmount
		{
			get => _expectedAmount;
			set => SetField(ref _expectedAmount, value);
		}

		int _equipmentId;
		[PropertyChangedAlso("Serial")]
		public int EquipmentId
		{
			get => _equipmentId;
			set => SetField(ref _equipmentId, value);
		}

		[Display(Name = "№ кулера")]
		public string Redhead
		{
			get => CarUnloadDocumentItem.Redhead;
			set
			{
				if(value != CarUnloadDocumentItem.Redhead)
				{
					CarUnloadDocumentItem.Redhead = value;
				}
			}
		}

		ServiceClaim _serviceClaim;

		public virtual ServiceClaim ServiceClaim
		{
			get => _serviceClaim;
			set => SetField(ref _serviceClaim, value);
		}

		public Equipment NewEquipment { get; set; }
		public bool Returned
		{
			get => Amount > 0;
			set => Amount = value ? 1 : 0;
		}

		GoodsAccountingOperation _movementOperation = new GoodsAccountingOperation();

		public virtual GoodsAccountingOperation MovementOperation
		{
			get => _movementOperation;
			set => SetField(ref _movementOperation, value);
		}

		public ReceptionItemNode(NomenclatureEntity nomenclature, int amount)
		{
			Name = nomenclature.Name;
			NomenclatureId = nomenclature.Id;
			NomenclatureCategory = nomenclature.Category;
			_amount = amount;
		}

		public ReceptionItemNode(GoodsAccountingOperation movementOperation) : this(movementOperation.Nomenclature, (int)movementOperation.Amount)
		{
			_movementOperation = movementOperation;
		}

		CarUnloadDocumentItem _carUnloadDocumentItem = new CarUnloadDocumentItem();

		public virtual CarUnloadDocumentItem CarUnloadDocumentItem
		{
			get => _carUnloadDocumentItem;
			set => SetField(ref _carUnloadDocumentItem, value);
		}

		public ReceptionItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.GoodsAccountingOperation)
		{
			_carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public ReceptionItemNode() { }
	}
}

