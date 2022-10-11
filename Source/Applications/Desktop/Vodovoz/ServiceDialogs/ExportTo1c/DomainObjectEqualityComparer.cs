using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz
{
	public class DomainObjectEqualityComparer<T>:IEqualityComparer<T> where T:IDomainObject
	{
		#region IEqualityComparer implementation
		public bool Equals(T x, T y)
		{			
			if (x == null && y == null)
				return true;
			if (x == null ^ y == null)
				return false;
			if (x.GetType() != y.GetType())
				return false;
			return x.Id == y.Id;
		}

		public int GetHashCode(T obj)
		{
			// возможно плохая реализация хешкода
			var prime = 13;
			var hash = prime * obj.Id.GetHashCode() + obj.GetType().GetHashCode();
			return hash;
		}
		#endregion
	}

}

