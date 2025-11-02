using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewWidgets.Store
{
	public class ReceptionNonSerialEquipmentItemNode : PropertyChangedBase
	{
		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		public int NeedReceptionCount { get; set; }

		private int _amount;
		public virtual int Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		private int _returned;
		public int Returned
		{
			get => _returned;
			set => SetField(ref _returned, value);
		}
	}
}
