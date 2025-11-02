using System.Collections.Generic;

namespace Vodovoz.EntityRepositories.Sale
{
	public class DistrictAndDistrictSetComparer : IEqualityComparer<DistrictAndDistrictSet>
	{
		public bool Equals(DistrictAndDistrictSet x, DistrictAndDistrictSet y)
		{
			if(object.ReferenceEquals(x, y))
			{
				return true;
			}

			if(object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
			{
				return false;
			}

			return x.DistrictName == y.DistrictName
					&& x.DistrictSetName == y.DistrictSetName
					&& x.DistrictSetCreationDate == y.DistrictSetCreationDate;
		}

		public int GetHashCode(DistrictAndDistrictSet obj)
		{
			if(obj == null)
			{
				return 0;
			}

			return (obj.DistrictName + obj.DistrictSetName + obj.DistrictSetCreationDate.ToString()).GetHashCode();
		}
	}
}
