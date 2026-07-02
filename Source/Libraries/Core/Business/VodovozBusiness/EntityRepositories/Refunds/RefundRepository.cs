using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Core.Domain.Refunds;

namespace VodovozBusiness.EntityRepositories.Refunds
{
	public class RefundRepository : IRefundRepository
	{
		public IEnumerable<RefundEntity> GetAllRefunds(IUnitOfWork uow)
		{
			return uow.GetAll<RefundEntity>();
		}
	}
}
