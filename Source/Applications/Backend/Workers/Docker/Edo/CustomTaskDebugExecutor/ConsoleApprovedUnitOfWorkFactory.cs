using QS.DomainModel.UoW;
using System;
using System.Runtime.CompilerServices;

namespace CustomTaskDebugExecutor
{
	public class ConsoleApprovedUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly TrackedUnitOfWorkFactory _trackedUnitOfWorkFactory;

		public ConsoleApprovedUnitOfWorkFactory(TrackedUnitOfWorkFactory trackedUnitOfWorkFactory)
		{
			_trackedUnitOfWorkFactory = trackedUnitOfWorkFactory ?? throw new ArgumentNullException(nameof(trackedUnitOfWorkFactory));
		}

		public IUnitOfWork CreateWithoutRoot(string userActionTitle = null, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
		{
			var uow = ((IUnitOfWorkFactory)_trackedUnitOfWorkFactory).CreateWithoutRoot(userActionTitle, callerMemberName, callerFilePath, callerLineNumber);
			return new ConsoleApprovedUnitOfWork(uow);
		}

		IUnitOfWorkGeneric<TEntity> IUnitOfWorkFactory.CreateForRoot<TEntity>(int id, string userActionTitle, string callerMemberName, string callerFilePath, int callerLineNumber)
		{
			throw new NotSupportedException();
		}

		IUnitOfWorkGeneric<TEntity> IUnitOfWorkFactory.CreateWithNewRoot<TEntity>(string userActionTitle, string callerMemberName, string callerFilePath, int callerLineNumber)
		{
			throw new NotSupportedException();
		}

		IUnitOfWorkGeneric<TEntity> IUnitOfWorkFactory.CreateWithNewRoot<TEntity>(TEntity entity, string userActionTitle, string callerMemberName, string callerFilePath, int callerLineNumber)
		{
			throw new NotSupportedException();
		}
	}
}
