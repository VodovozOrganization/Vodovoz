using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	internal class ReferenceComparer : IEqualityComparer<object>
	{
		public static readonly ReferenceComparer Instance = new ReferenceComparer();

		public new bool Equals(object x, object y) => ReferenceEquals(x, y);

		public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
	}
}
