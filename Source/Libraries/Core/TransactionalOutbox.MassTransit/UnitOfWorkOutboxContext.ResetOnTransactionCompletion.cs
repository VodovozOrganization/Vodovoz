using NHibernate.Transaction;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionalOutbox.MassTransit
{
	public sealed partial class UnitOfWorkOutboxContext
	{
		/// <summary>
		/// Сбрасывает OutboxId контекста после коммита или отката транзакции NHibernate (при откате — с предупреждением в лог)
		/// </summary>
		private sealed class ResetOnTransactionCompletion : ITransactionCompletionSynchronization
		{
			private readonly UnitOfWorkOutboxContext _context;

			public ResetOnTransactionCompletion(UnitOfWorkOutboxContext context)
			{
				_context = context;
			}

			public void ExecuteBeforeTransactionCompletion()
			{
			}

			public Task ExecuteBeforeTransactionCompletionAsync(CancellationToken cancellationToken) => Task.CompletedTask;

			public void ExecuteAfterTransactionCompletion(bool success) => _context.Reset(success);

			public Task ExecuteAfterTransactionCompletionAsync(bool success, CancellationToken cancellationToken)
			{
				_context.Reset(success);
				return Task.CompletedTask;
			}
		}
	}
}
