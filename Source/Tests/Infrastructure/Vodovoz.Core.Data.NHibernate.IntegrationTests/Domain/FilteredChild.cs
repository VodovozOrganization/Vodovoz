namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class FilteredChild : SimpleChild
	{
		public virtual bool IsArchived { get; set; }
	}
}
