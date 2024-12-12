using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace Vodovoz.Domain.Orders
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "залоги в заказе",
		Nominative = "залог в заказе"
	)]
	public class OrderDepositItem : OrderDepositItemEntity, IOrderDepositItemWageCalculationSource
	{
		private DepositOperation _depositOperation;
		private Nomenclature _equipmentNomenclature;


		[Display(Name = "Операция залога")]
		public virtual DepositOperation DepositOperation {
			get => _depositOperation;
			set => SetField(ref _depositOperation, value);
		}

		[Display(Name = "Номенклатура оборудования")]
		public virtual Nomenclature EquipmentNomenclature {
			get => _equipmentNomenclature;
			set => SetField(ref _equipmentNomenclature, value);
		}

		#region IOrderDepositItemWageCalculationSource implementation

		public int InitialCount => Count;

		#endregion IOrderDepositItemWageCalculationSource implementation
	}
}

