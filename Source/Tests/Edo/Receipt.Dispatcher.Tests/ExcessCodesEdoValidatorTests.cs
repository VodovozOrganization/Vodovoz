using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using Edo.Problems.Validation.Sources;
using EdoNotifications.Application.Factories;
using EdoNotifications.Application.Providers;
using EdoNotifications.Contracts;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using NSubstitute;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using Receipt.Dispatcher.Tests.Fixtures;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using VodovozInfrastructure.Cryptography;
using Xunit;

namespace Receipt.Dispatcher.Tests
{
	public class ExcessCodesEdoValidatorTests
	{
		private readonly ExcessCodesEdoValidator _validator = new ExcessCodesEdoValidator();
		private readonly IServiceProvider _services = CreateServices();

		private static IServiceProvider CreateServices()
		{
			var services = Substitute.For<IServiceProvider>();
			services.GetService(typeof(IEdoNotificationMessageFactory))
				.Returns(new EdoNotificationMessageFactory(new MD5HexHashFromString()));
			return services;
		}

		[Theory]
		[InlineData(false, 4, true)]
		[InlineData(false, 5, true)]
		[InlineData(false, 6, false)]
		[InlineData(true, 4, true)]
		[InlineData(true, 5, true)]
		[InlineData(true, 6, false)]
		public async Task ChecksTotalCountWithoutChangingCodes(bool receipt, int codes, bool valid)
		{
			var task = CreateTask(receipt, codes);
			var items = task.Items.ToArray();
			var result = await _validator.ValidateAsync(task, _services, default);
			Assert.Equal(valid, result.IsValid);
			Assert.Equal(items, task.Items);
			Assert.All(task.Items, x => Assert.Same(x.ProductCode.SourceCode, x.ProductCode.ResultCode));
		}

		[Fact]
		public async Task UsesActualQuantityAndIgnoresUnmarkedGoodsAndEmptyCodeSlots()
		{
			var task = CreateTask(false, 4);
			((OrderItemEntityFixture)task.FormalEdoRequest.Order.OrderItems.First()).SetActualCount(3);
			task.FormalEdoRequest.Order.OrderItems.Add(CreateOrderItem(100, false));
			task.Items.Add(new EdoTaskItem { ProductCode = new AutoTrueMarkProductCode() });
			Assert.False((await _validator.ValidateAsync(task, _services, default)).IsValid);
			task.Items.RemoveAt(0);
			Assert.True((await _validator.ValidateAsync(task, _services, default)).IsValid);
		}

		[Fact]
		public async Task CountsGroupMembersOnceAndDoesNotCompareEachGtin()
		{
			var task = CreateTask(true, 5);
			foreach(var item in task.Items)
			{
				item.ProductCode.SourceCode.ParentWaterGroupCodeId = 10;
				item.ProductCode.SourceCode.Gtin = "04600000000001";
			}
			((OrderItemEntityFixture)task.FormalEdoRequest.Order.OrderItems.First()).SetCount(2);
			task.FormalEdoRequest.Order.OrderItems.Add(CreateOrderItem(3));
			Assert.True((await _validator.ValidateAsync(task, _services, default)).IsValid);
		}

		[Fact]
		public void AppliesOnlyToNewUpdAndReceiptStages()
		{
			var upd = (DocumentEdoTask)CreateTask(false, 6);
			Assert.True(_validator.IsApplicable(upd));
			upd.Stage = DocumentEdoTaskStage.Sending;
			Assert.False(_validator.IsApplicable(upd));
			upd.Stage = DocumentEdoTaskStage.New;
			upd.DocumentType = EdoDocumentType.Bill;
			Assert.False(_validator.IsApplicable(upd));
			var receipt = (ReceiptEdoTask)CreateTask(true, 6);
			Assert.True(_validator.IsApplicable(receipt));
			receipt.Status = EdoTaskStatus.Completed;
			Assert.False(_validator.IsApplicable(receipt));
			Assert.False(_validator.IsApplicable(new TenderEdoTask()));
		}

		[Fact]
		public void MissingNotificationSettingUsesStandardError()
		{
			var provider = new EdoNotificationsSettingsProvider(Substitute.For<IUnitOfWorkFactory>(),
				Substitute.For<IGenericRepository<EdoNotificationSetting>>());
			var factory = new EdoNotificationMessageFactory(new MD5HexHashFromString());
			var message = factory.Create(EdoNotificationType.ExcessCodes);
			Assert.Throws<InvalidOperationException>(() => provider.GetEdoNotificationSetting(message));
			Assert.Throws<InvalidOperationException>(() =>
				provider.GetEdoNotificationSetting(factory.Create(EdoNotificationType.CodeDuplicated)));
		}

		[Fact]
		public void ExistingNotificationSettingPreservesRecipientsAndTemplate()
		{
			var setting = new EdoNotificationSetting
			{
				EdoNotificationType = EdoNotificationType.ExcessCodes, Template = "{ProblemMessage}",
				Emails = "test@example.com", BitrixDialogs = "123", NotificationDisabled = false
			};
			var repository = Substitute.For<IGenericRepository<EdoNotificationSetting>>();
			repository.Get(Arg.Any<IUnitOfWork>(), Arg.Any<Expression<Func<EdoNotificationSetting, bool>>>(), 0)
				.Returns(new[] { setting });
			var provider = new EdoNotificationsSettingsProvider(Substitute.For<IUnitOfWorkFactory>(), repository);
			var message = new EdoNotificationMessageFactory(new MD5HexHashFromString()).Create(EdoNotificationType.ExcessCodes);
			Assert.Same(setting, provider.GetEdoNotificationSetting(message));
			Assert.False(provider.IsDisabled(message));
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		public async Task RegistersProblemAndNotificationThenResolvesAfterCorrection(bool receipt, bool anotherSource)
		{
			var task = CreateTask(receipt, 6);
			IEdoTaskValidator source = _validator;
			var messageFactory = new EdoNotificationMessageFactory(new MD5HexHashFromString());
			var notificationType = EdoNotificationType.ExcessCodes;
			if(anotherSource)
			{
				source = Substitute.For<IEdoTaskValidator>();
				source.Name.Returns("Test.NotificationSource");
				source.Importance.Returns(EdoProblemImportance.Problem);
				source.IsApplicable(task).Returns(true);
				notificationType = EdoNotificationType.CodeDuplicated;
				source.ValidateAsync(task, Arg.Any<IServiceProvider>(), Arg.Any<CancellationToken>())
					.Returns(_ => Task.FromResult(task.Items.Count > 5
						? EdoValidationResult.InvalidWithNotification(source, messageFactory.Create(notificationType)) : EdoValidationResult.Valid(source)));
			}
			var taskUow = Substitute.For<IUnitOfWork>();
			var problemUow = Substitute.For<IUnitOfWork>();
			var factory = Substitute.For<IUnitOfWorkFactory>();
			factory.CreateWithoutRoot().ReturnsForAnyArgs(problemUow);
			problemUow.Session.GetAsync<EdoTask>(task.Id, Arg.Any<CancellationToken>()).Returns(task);
			var publisher = Substitute.For<IOutboxNotificationPublisher<EdoNotificationMessage>>();
			var registrar = new EdoProblemRegistrar(taskUow, factory,
				new EdoTaskCustomSourcesPersister(factory, Array.Empty<EdoTaskProblemCustomSource>()),
				new EdoTaskExceptionSourcesPersister(factory, Array.Empty<EdoTaskProblemExceptionSource>()),
				publisher);
			var validator = new EdoTaskValidator(Substitute.For<ILogger<EdoTaskValidator>>(),
				new EdoTaskValidatorsProvider(new EdoTaskValidatorsPersister(factory, new[] { source })),
				_services, registrar);
			problemUow.ClearReceivedCalls();

			Assert.False(await validator.Validate(task, default));
			Assert.Equal(EdoTaskStatus.Problem, task.Status);
			Assert.Equal(6, task.Items.Count);
			await publisher.Received(1).TryPublishAsync(problemUow,
				Arg.Is<EdoNotificationMessage>(x => x.EdoNotificationType == notificationType), default);
			await problemUow.Received(1).CommitAsync(default);
			taskUow.Received(1).Dispose();

			var problem = new ValidationEdoTaskProblem { SourceName = source.Name, State = TaskProblemState.Active };
			task.Problems.Add(problem);
			task.Items.RemoveAt(0);
			publisher.ClearReceivedCalls();
			Assert.True(await validator.Validate(task, default));
			Assert.Equal(TaskProblemState.Solved, problem.State);
			Assert.Equal(EdoTaskStatus.InProgress, task.Status);
			Assert.Empty(publisher.ReceivedCalls());
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PoolFailurePersistsOnlySolvedValidationProblems(bool receipt)
		{
			var task = CreateTask(receipt, 5);
			var persistedTask = CreateTask(receipt, 5);
			var solved = new ValidationEdoTaskProblem { SourceName = _validator.Name, State = TaskProblemState.Solved };
			var persisted = new ValidationEdoTaskProblem { SourceName = _validator.Name, State = TaskProblemState.Active };
			var otherProblem = new ValidationEdoTaskProblem { SourceName = "Other", State = TaskProblemState.Active };
			task.Problems.Add(solved);
			persistedTask.Problems.Add(persisted);
			persistedTask.Problems.Add(otherProblem);
			var taskUow = Substitute.For<IUnitOfWork>();
			var problemUow = Substitute.For<IUnitOfWork>();
			var factory = Substitute.For<IUnitOfWorkFactory>();
			factory.CreateWithoutRoot().ReturnsForAnyArgs(problemUow);
			problemUow.GetById<EdoTask>(task.Id).Returns(persistedTask);
			var registrar = new EdoProblemRegistrar(taskUow, factory,
				new EdoTaskCustomSourcesPersister(factory, Array.Empty<EdoTaskProblemCustomSource>()),
				new EdoTaskExceptionSourcesPersister(factory, new[] { new Edo.Problems.Exception.Sources.MissingCodeInPool() }),
				Substitute.For<IOutboxNotificationPublisher<EdoNotificationMessage>>());
			problemUow.ClearReceivedCalls();

			Assert.True(await registrar.TryRegisterExceptionProblem(task,
				new TrueMark.Codes.Pool.EdoCodePoolMissingCodeException(), default));

			Assert.Equal(TaskProblemState.Solved, persisted.State);
			Assert.Equal(TaskProblemState.Active, otherProblem.State);
			Assert.Equal(EdoTaskStatus.Problem, persistedTask.Status);
			await problemUow.Received(1).SaveAsync(persisted, cancellationToken: default);
			await problemUow.Received(1).CommitAsync(default);
			await taskUow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
			taskUow.Received(1).Dispose();
		}

		private static OrderEdoTask CreateTask(bool receipt, int codeCount)
		{
			OrderEdoTask task = receipt
				? (OrderEdoTask)new ReceiptEdoTask { ReceiptStatus = EdoReceiptStatus.New }
				: new DocumentEdoTask { DocumentType = EdoDocumentType.UPD, Stage = DocumentEdoTaskStage.New };
			task.Id = 42;
			task.Problems = new ObservableList<EdoTaskProblem>();
			task.FormalEdoRequest = new ManualEdoRequest { Order = new OrderEntity { Id = 15 } };
			task.FormalEdoRequest.Order.OrderItems.Add(CreateOrderItem(5));
			for(var i = 0; i < codeCount; i++)
			{
				var code = new TrueMarkWaterIdentificationCode { Id = i + 1 };
				task.Items.Add(new EdoTaskItem
				{
					ProductCode = new AutoTrueMarkProductCode { SourceCode = code, ResultCode = code }, CustomerEdoTask = task
				});
			}
			return task;
		}

		private static OrderItemEntityFixture CreateOrderItem(decimal count, bool marked = true)
		{
			var item = new OrderItemEntityFixture
			{
				Nomenclature = new NomenclatureEntity { IsAccountableInTrueMark = marked }
			};
			item.SetCount(count);
			return item;
		}
	}
}
