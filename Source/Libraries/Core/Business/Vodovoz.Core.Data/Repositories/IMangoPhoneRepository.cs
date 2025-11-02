using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Mango;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IMangoPhoneRepository
    {
		Task<IEnumerable<MangoPhone>> GetMangoPhones();
    }
}
