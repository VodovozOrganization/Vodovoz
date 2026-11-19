namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class VersionedParentChild : SimpleChild
	{
		public virtual VersionedParent Parent { get; set; }
	}
}
