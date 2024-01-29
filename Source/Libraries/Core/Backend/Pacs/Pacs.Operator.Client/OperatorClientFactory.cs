using Autofac;
using System;

namespace Pacs.Operators.Client
{
	public class OperatorClientFactory : IOperatorClientFactory
	{
		private readonly ILifetimeScope _scope;

		public OperatorClientFactory(ILifetimeScope scope)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		public IOperatorClient CreateOperatorClient(int operatorId)
		{
			return _scope.Resolve<IOperatorClient>(new TypedParameter(typeof(int), operatorId));
		}

		public OperatorKeepAliveController CreateOperatorKeepAliveController(int operatorId)
		{
			var client = CreateOperatorClient(operatorId);
			return _scope.Resolve<OperatorKeepAliveController>(new TypedParameter(typeof(IOperatorClient), client));
		}
	}
}
