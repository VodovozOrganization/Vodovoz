using System;
using System.Collections.Generic;

namespace Vodovoz.Domain.WageCalculation
{
	public interface IWageHierarchyNode: ICloneable
	{
		IWageHierarchyNode Parent { get; set; }
		IList<IWageHierarchyNode> Children { get; }
		decimal ForDriverWithForwarder { get; set; }
		decimal ForForwarder { get; set; }
		decimal ForDriverWithoutForwarder { get; set; }

		string Name { get; }

		string GetUnitName { get; }
	}
}
