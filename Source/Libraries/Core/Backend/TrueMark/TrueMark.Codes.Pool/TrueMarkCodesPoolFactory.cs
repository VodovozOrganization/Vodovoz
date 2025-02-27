using QS.DomainModel.UoW;
using System;

namespace TrueMark.Codes.Pool
{
	public class TrueMarkCodesPoolFactory
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public TrueMarkCodesPoolFactory(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public TrueMarkCodesPool Create()
		{
			return new TrueMarkCodesPool(_uowFactory.CreateWithoutRoot());
		}

		public TrueMarkCodesPool Create(IUnitOfWork uow)
		{
			return new TrueMarkCodesPool(uow);
		}
	}
}
