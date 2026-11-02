using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using QS.DomainModel.UoW;
using TransactionalOutbox.MassTransit;
using Xunit;

namespace EdoServices.Tests
{
	/// <summary>
	/// На бэкенде IPublishEndpoint из DI пишет сообщение в транзакцию IUnitOfWork своего DI-scope
	/// </summary>
	public class UnitOfWorkScopedPublishTests
	{
		[Fact]
		public void ScopedPublishEndpoint_WritesToScopeUnitOfWork()
		{
			var publishEndpoint = Substitute.For<IPublishEndpoint>();
			var outbox = Substitute.For<IUnitOfWorkOutbox<IBus>>();
			outbox.GetPublishEndpoint(Arg.Any<IUnitOfWork>()).Returns(publishEndpoint);

			var services = new ServiceCollection();
			services.AddMassTransit(x => x.UsingInMemory());
			services.Replace(ServiceDescriptor.Scoped<IScopedBusContextProvider<IBus>, UnitOfWorkScopedBusContextProvider>());
			services.AddExplicitUnitOfWorkOutbox<IBus>(OutboxBusNames.Pacs);
			services.Replace(ServiceDescriptor.Singleton(outbox));
			services.AddScoped(_ => Substitute.For<IUnitOfWork>());

			using var provider = services.BuildServiceProvider();
			using var scope = provider.CreateScope();

			Assert.Same(publishEndpoint, scope.ServiceProvider.GetRequiredService<IPublishEndpoint>());
			outbox.Received(1).GetPublishEndpoint(scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
		}
	}
}
