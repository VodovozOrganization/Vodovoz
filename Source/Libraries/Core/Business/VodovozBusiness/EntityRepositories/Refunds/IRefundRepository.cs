using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Refunds;

namespace VodovozBusiness.EntityRepositories.Refunds
{
	public interface IRefundRepository
	{
		IEnumerable<RefundEntity> GetAllRefunds(IUnitOfWork uow);
	}
}
