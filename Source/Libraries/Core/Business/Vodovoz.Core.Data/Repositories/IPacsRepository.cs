namespace Vodovoz.Core.Data.Repositories
{
	public interface IPacsRepository
	{
		bool PacsEnabledFor(int subdivisionId);
	}
}
