using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;

namespace Vodovoz.ViewWidgets.Store
{
	public class DefectiveItemNode : PropertyChangedBase
	{
		public DefectiveItemNode(NomenclatureEntity nomenclature, int amount)
		{
			Name = nomenclature.Name;
			NomenclatureId = nomenclature.Id;
			NomenclatureCategory = nomenclature.Category;
			this.amount = amount;
		}

		public DefectiveItemNode(GoodsAccountingOperation movementOperation) : this(movementOperation.Nomenclature, (int)movementOperation.Amount)
		{
			this.movementOperation = movementOperation;
		}

		public DefectiveItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.GoodsAccountingOperation)
		{
			this.carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public DefectiveItemNode() { }

		CarUnloadDocumentItem carUnloadDocumentItem = new CarUnloadDocumentItem();
		public virtual CarUnloadDocumentItem CarUnloadDocumentItem
		{
			get => carUnloadDocumentItem;
			set => SetField(ref carUnloadDocumentItem, value);
		}

		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		decimal amount;
		public virtual decimal Amount
		{
			get => amount;
			set => SetField(ref amount, value);
		}

		GoodsAccountingOperation movementOperation;
		public virtual GoodsAccountingOperation MovementOperation
		{
			get => movementOperation;
			set => SetField(ref movementOperation, value);
		}

		CullingCategory typeOfDefect;
		[Display(Name = "Тип брака")]
		public virtual CullingCategory TypeOfDefect
		{
			get => typeOfDefect;
			set => SetField(ref typeOfDefect, value);
		}

		DefectSource source = DefectSource.Driver;
		[Display(Name = "Источник брака")]
		public virtual DefectSource Source
		{
			get => source;
			set => SetField(ref source, value);
		}
	}
}
