using System;

namespace Vodovoz.Core.Domain.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public class OnlineStoreGuidAttribute : Attribute
	{
		public Guid Guid;

		public OnlineStoreGuidAttribute(Guid guid)
		{
			Guid = guid;
		}

		public OnlineStoreGuidAttribute(string guid)
		{
			Guid = Guid.Parse(guid);
		}
	}
}
