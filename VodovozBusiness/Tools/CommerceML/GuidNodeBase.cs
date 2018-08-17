using System;
namespace Vodovoz.Tools.CommerceML
{
	public abstract class GuidNodeBase : IGuidNode
	{
		public GuidNodeBase()
		{
		}

		public abstract Guid Guid { get; }
	}
}
