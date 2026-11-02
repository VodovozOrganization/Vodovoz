using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Services;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.TrueMark;
using VodovozBusiness.Services.TrueMark;
using Xunit;

namespace VodovozBusiness.TrueMark.Tests
{
	public class ExcessTrueMarkCodesDeletionTests
	{
		private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
		private readonly ICurrentPermissionService _permissions = Substitute.For<ICurrentPermissionService>();
		private readonly ITrueMarkRepository _repository = Substitute.For<ITrueMarkRepository>();
		private readonly ITrueMarkCodesPool _pool = Substitute.For<ITrueMarkCodesPool>();
		private readonly ExcessTrueMarkCodesDeletionService _service;

		public ExcessTrueMarkCodesDeletionTests()
		{
			_permissions.ValidatePresetPermission(EdoPermissions.CanDeleteExcessTrueMarkCodes).Returns(true);
			var factory = Substitute.For<TrueMarkCodesPoolFactory>(Substitute.For<IUnitOfWorkFactory>());
			factory.Create(_uow).Returns(_pool);
			_service = new ExcessTrueMarkCodesDeletionService(_permissions, factory, _repository);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ReturnsSelectedCodeAndDeletesOnlyItsTaskItem(bool receipt)
		{
			var task = CreateTask(receipt);
			var removed = task.Items.Last();
			var kept = task.Items.First();

			var result = _service.Delete(_uow, task.Id, new[] { removed.ProductCode.Id });

			Assert.True(result.IsSuccess);
			Assert.Same(kept, Assert.Single(task.Items));
			_pool.Received(1).PutCode(removed.ProductCode.SourceCode.Id);
			_uow.Received(1).Delete(removed);
			Assert.Equal(SourceProductCodeStatus.SavedToPool, removed.ProductCode.SourceCodeStatus);
			Assert.Equal(EdoTaskStatus.Problem, task.Status);
			Assert.DoesNotContain(_uow.ReceivedCalls(), x => x.GetMethodInfo().Name.StartsWith("Commit"));
		}

		[Fact]
		public void RechecksPermissionAtDeletion()
		{
			var task = CreateTask(false);
			Assert.True(_service.CanDelete(task));
			_permissions.ValidatePresetPermission(EdoPermissions.CanDeleteExcessTrueMarkCodes).Returns(false);
			AssertRejected(task, new[] { 2 });
		}

		[Theory]
		[InlineData(EdoTaskStatus.InProgress)]
		[InlineData(EdoTaskStatus.Completed)]
		[InlineData(EdoTaskStatus.Cancelled)]
		[InlineData(EdoTaskStatus.InCancellation)]
		public void RejectsTasksOutsideProblem(EdoTaskStatus status)
		{
			var task = CreateTask(false);
			task.Status = status;
			AssertRejected(task, new[] { 2 });
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void RejectsAlreadyPreparedDocuments(bool receipt)
		{
			var task = CreateTask(receipt);
			if(task is ReceiptEdoTask receiptTask)
			{
				receiptTask.FiscalDocuments.Add(new EdoFiscalDocument());
			}
			else
			{
				((DocumentEdoTask)task).UpdInventPositions.Add(new EdoUpdInventPosition());
			}
			AssertRejected(task, new[] { 2 });
		}

		[Fact]
		public void RejectsMoreThanExcessAndForeignSelection()
		{
			var task = CreateTask(false);
			AssertRejected(task, new[] { 1, 2 });
			AssertRejected(task, new[] { 2, 99 });
			AssertRejected(task, Array.Empty<int>());
		}

		[Fact]
		public void RejectsCodeUsedByAnotherDocument()
		{
			var task = CreateTask(false);
			_repository.AreCodesUsedByOtherEdoTasks(_uow, task.Id, Arg.Any<IEnumerable<int>>()).Returns(true);
			AssertRejected(task, new[] { 2 });
		}

		[Fact]
		public void RejectsCodeStillUsedByUnselectedItem()
		{
			var task = CreateTask(false);
			task.Items.Last().ProductCode.SourceCode = task.Items.First().ProductCode.SourceCode;
			Assert.Same(task.Items.First().ProductCode.SourceCode, task.Items.Last().ProductCode.SourceCode);
			AssertRejected(task, new[] { 2 });
		}

		[Fact]
		public void ReturnsSourceAndReplacementOnceEach()
		{
			var task = CreateTask(false);
			task.Items.Last().ProductCode.ResultCode = new TrueMarkWaterIdentificationCode { Id = 3 };
			Assert.True(_service.Delete(_uow, task.Id, new[] { 2, 2 }).IsSuccess);
			_pool.Received(1).PutCode(2);
			_pool.Received(1).PutCode(3);
		}

		[Fact]
		public void ReturnsPoolCodeWithoutSource()
		{
			var task = CreateTask(false);
			var product = task.Items.Last().ProductCode;
			product.ResultCode = product.SourceCode;
			product.SourceCode = null;
			Assert.True(_service.Delete(_uow, task.Id, new[] { 2 }).IsSuccess);
			_pool.Received(1).PutCode(2);
		}

		[Fact]
		public void PoolDoesNotSuppressDatabaseFailure()
		{
			var query = Substitute.For<ISQLQuery>();
			_uow.Session.CreateSQLQuery(Arg.Any<string>()).Returns(query);
			query.SetParameter("code_id", 2).Returns(query);
			var pool = new TrueMarkCodesPool(_uow);
			query.When(x => x.ExecuteUpdate()).Do(_ => throw new InvalidOperationException("Database failure"));
			Assert.Throws<InvalidOperationException>(() => pool.PutCode(2));
		}

		[Fact]
		public void PoolFailurePreventsItemDeletion()
		{
			var task = CreateTask(false);
			_pool.When(x => x.PutCode(2)).Do(_ => throw new InvalidOperationException("Pool failure"));
			Assert.Throws<InvalidOperationException>(() => _service.Delete(_uow, task.Id, new[] { 2 }));
			Assert.Equal(2, task.Items.Count);
			AssertNoDeletion();
		}

		private OrderEdoTask CreateTask(bool receipt)
		{
			OrderEdoTask task = receipt
				? (OrderEdoTask)new ReceiptEdoTask { ReceiptStatus = EdoReceiptStatus.New }
				: new DocumentEdoTask { DocumentType = EdoDocumentType.UPD, Stage = DocumentEdoTaskStage.New };
			task.Id = 10;
			task.Status = EdoTaskStatus.Problem;
			task.FormalEdoRequest = new PrimaryEdoRequest { Order = new OrderEntity() };
			task.FormalEdoRequest.Order.OrderItems.Add(new MarkedOrderItem());
			foreach(var id in new[] { 1, 2 })
			{
				task.Items.Add(new EdoTaskItem { Id = id, CustomerEdoTask = task,
					ProductCode = new AutoTrueMarkProductCode { Id = id,
						SourceCode = new TrueMarkWaterIdentificationCode { Id = id, RawCode = id.ToString(), Gtin = "04602009723186", SerialNumber = id.ToString() } } });
			}
			_uow.GetById<OrderEdoTask>(task.Id).Returns(task);
			return task;
		}

		private void AssertRejected(OrderEdoTask task, int[] ids)
		{
			Assert.True(_service.Delete(_uow, task.Id, ids).IsFailure);
			Assert.Equal(2, task.Items.Count);
			_pool.DidNotReceive().PutCode(Arg.Any<int>());
			AssertNoDeletion();
		}

		private void AssertNoDeletion() =>
			Assert.DoesNotContain(_uow.ReceivedCalls(), x => x.GetMethodInfo().Name == "Delete");

		private class MarkedOrderItem : OrderItemEntity
		{
			public MarkedOrderItem()
			{
				Count = 1;
				Nomenclature = new NomenclatureEntity { IsAccountableInTrueMark = true };
			}
		}
	}
}
