using System;
using System.Collections.Generic;
using Gtk;

namespace Vodovoz.Panel
{
	public static class PanelViewFactory
	{
		public static Widget Create(PanelViewType type)
		{
			switch (type)
			{
				default:
					throw new NotSupportedException();
			}
		}

		public static IEnumerable<Widget> CreateAll(IEnumerable<PanelViewType> types)
		{
			var iterator = types.GetEnumerator();
			while (iterator.MoveNext())
				yield return Create(iterator.Current);
		}
	}

	public enum PanelViewType{
	}
}

