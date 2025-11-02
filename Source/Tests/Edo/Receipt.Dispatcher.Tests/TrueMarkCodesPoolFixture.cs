using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;

namespace Receipt.Dispatcher.Tests
{
	public class TrueMarkCodesPoolFixture : TrueMarkCodesPool
	{
		private int _autoIncrementId = 10000;

		public TrueMarkCodesPoolFixture(IUnitOfWork unitOfWork) : base (unitOfWork)
		{
		}

		public override void PutCode(int codeId)
		{
		}

		public override int TakeCode(string gtin)
		{
			return _autoIncrementId++;
		}

		public override Task<int> TakeCode(string gtin, CancellationToken cancellationToken)
		{
			return Task.FromResult(_autoIncrementId++);
		}

		public override Task PutCodeAsync(int codeId, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
