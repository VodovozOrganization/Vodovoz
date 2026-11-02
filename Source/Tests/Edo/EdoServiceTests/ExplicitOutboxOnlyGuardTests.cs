using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using TransactionalOutbox.MassTransit;
using Xunit;

namespace EdoServices.Tests
{
	/// <summary>
	/// В приложениях без штатного outbox MassTransit (десктоп) IPublishEndpoint из DI
	/// не должен отправлять сообщения мимо outbox
	/// </summary>
	public class ExplicitOutboxOnlyGuardTests
	{
		[Fact]
		public async Task ScopedPublishEndpoint_OutsideConsumer_ThrowsWithHint()
		{
			var services = new ServiceCollection();
			services.AddMassTransit(x => x.UsingInMemory());
			services.AddExplicitUnitOfWorkOutbox<IBus>(OutboxBusNames.Pacs);

			await using var provider = services.BuildServiceProvider();
			using var scope = provider.CreateScope();

			var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

			var exception = await Assert.ThrowsAsync<InvalidOperationException>(
				() => publishEndpoint.Publish(new DocumentTaskCreatedEvent { Id = 1 }));

			Assert.Contains("GetPublishEndpoint(uow)", exception.Message);
		}

		[Fact]
		public async Task ScopedSendEndpointProvider_OutsideConsumer_Throws()
		{
			var services = new ServiceCollection();
			services.AddMassTransit(x => x.UsingInMemory());
			services.AddExplicitUnitOfWorkOutbox<IBus>(OutboxBusNames.Pacs);

			await using var provider = services.BuildServiceProvider();
			using var scope = provider.CreateScope();

			var sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();

			await Assert.ThrowsAsync<InvalidOperationException>(
				() => sendEndpointProvider.GetSendEndpoint(new Uri("queue:test")));
		}

		[Fact]
		public void ResolvingPublishEndpoint_DoesNotThrow()
		{
			// Сервисы, которые получают IPublishEndpoint, но не публикуют через него, создаются без ошибки
			var services = new ServiceCollection();
			services.AddMassTransit(x => x.UsingInMemory());
			services.AddExplicitUnitOfWorkOutbox<IBus>(OutboxBusNames.Pacs);

			using var provider = services.BuildServiceProvider();
			using var scope = provider.CreateScope();

			Assert.NotNull(scope.ServiceProvider.GetRequiredService<IPublishEndpoint>());
		}
	}
}
