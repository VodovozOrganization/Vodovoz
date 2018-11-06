using System;

namespace Vodovoz.Domain.Goods
{
	[System.AttributeUsage(System.AttributeTargets.Field)]
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
