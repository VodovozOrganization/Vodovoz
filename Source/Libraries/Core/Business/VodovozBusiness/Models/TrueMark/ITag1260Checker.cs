using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Domain.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	public interface ITag1260Checker
	{
		Task UpdateInfoForTag1260Async(CashReceipt cashReceipt, IUnitOfWork _uow, CancellationToken cancellationToken);
		Task UpdateInfoForTag1260Async(IEnumerable<TrueMarkWaterIdentificationCode> sourceCodes, IUnitOfWork unitOfWork, int organizationId, CancellationToken cancellationToken);
	}
}
