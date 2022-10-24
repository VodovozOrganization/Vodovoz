using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vodovoz.RDL.Facades.Utils
{
	public static class Utils
	{
		public static IEnumerable<TElement> GetElements<TElement, TElementType>(
			ref IEnumerable<TElement> elementsDest,
			TElementType[] elementTypes,
			TElementType elementType,
			Func<int, IEnumerable<TElement>> elementsSelector)
		{
			if(elementsDest == null)
			{
				var elementIndex = Array.IndexOf(elementTypes, elementType);
				if(elementIndex == -1)
				{
					elementsDest = Enumerable.Empty<TElement>();
					return elementsDest;
				}
				elementsDest = elementsSelector(elementIndex);
			}
			return elementsDest;
		}

		public static T GetProperty<T>(ref bool propInit, ref T property, object[] source)
		{
			if(!propInit)
			{
				var result = source.FirstOrDefault(x => x is T);
				property = result == null ? default(T) : (T)result;
				propInit = true;
			}
			return property;
		}
	}
}
