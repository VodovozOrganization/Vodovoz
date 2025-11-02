using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.FastPayments;
using Vodovoz.EntityRepositories.FastPayments;

namespace PayPageAPI.Models
{
	public class AvangardFastPaymentModel : IAvangardFastPaymentModel
	{
		private readonly IUnitOfWork _uow;
		private readonly IFastPaymentRepository _fastPaymentRepository;

		public AvangardFastPaymentModel(
			IUnitOfWork uow,
			IFastPaymentRepository fastPaymentRepository)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_fastPaymentRepository = fastPaymentRepository ?? throw new ArgumentNullException(nameof(fastPaymentRepository));
		}

		public FastPayment GetFastPaymentByGuid(Guid fastPaymentGuid) => _fastPaymentRepository.GetFastPaymentByGuid(_uow, fastPaymentGuid);
	}
}
