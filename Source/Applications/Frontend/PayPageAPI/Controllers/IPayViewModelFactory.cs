using PayPageAPI.Models;
using Vodovoz.Domain.FastPayments;

namespace PayPageAPI.Controllers
{
	public interface IPayViewModelFactory
	{
		PayViewModel CreateNewPayViewModel(FastPayment fastPayment);
	}
}
