using System.Threading.Tasks;

namespace EdoService.Services
{
	public interface ITrueApiService
	{
		Task<bool> ParticipantsAsync(string inn, string productGroup);
	}
}
