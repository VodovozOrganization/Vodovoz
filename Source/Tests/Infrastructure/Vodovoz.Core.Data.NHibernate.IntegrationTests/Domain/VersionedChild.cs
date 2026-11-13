namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class VersionedChild : SimpleChild
	{
		public virtual int Version { get; set; }
	}
}
