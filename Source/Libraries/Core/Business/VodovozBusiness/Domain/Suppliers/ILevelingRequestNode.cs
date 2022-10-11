using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Suppliers
{
	public interface ILevelingRequestNode
	{

		#region With Nhibernate mapping

		int Id { get; set; }
		Nomenclature Nomenclature { get; set; }
		decimal Quantity { get; set; }
		RequestToSupplier RequestToSupplier { get; set; }

		#endregion With Nhibernate mapping

		#region Without Nhibernate mapping

		SupplierPriceItem SupplierPriceItem { get; set; }

		ILevelingRequestNode Parent { get; set; }
		IList<ILevelingRequestNode> Children { get; set; }

		#endregion Without Nhibernate mapping
	}
}