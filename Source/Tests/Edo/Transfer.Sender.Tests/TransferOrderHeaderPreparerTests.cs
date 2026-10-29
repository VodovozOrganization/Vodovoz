using Edo.Transfer;
using Edo.Transfer.Sender;
using NSubstitute;
using QS.DomainModel.UoW;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Data.Repositories.Document;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;
using Xunit;

namespace Transfer.Sender.Tests
{
	/// <summary>
	/// Проверяет подготовку шапки УПД трансфера.
	/// </summary>
	public class TransferOrderHeaderPreparerTests
	{
		/// <summary>
		/// Проверяет, что дата и реквизиты документа УПД формируются по минимальной дате доставки.
		/// </summary>
		[Fact]
		public async Task PrepareAsync_UsesMinDeliveryDateForTransferOrderAndDocumentCounter()
		{
			var transferDate = new DateTime(2026, 5, 8);
			var seller = new OrganizationEntity { Id = 10, Prefix = "VV" };
			var customer = new OrganizationEntity { Id = 20 };
			var transferTask = new TransferEdoTask
			{
				Id = 100,
				FromOrganizationId = seller.Id,
				ToOrganizationId = customer.Id,
				StartTime = new DateTime(2026, 7, 29)
			};

			var uow = Substitute.For<IUnitOfWork>();
			uow.SaveAsync(Arg.Any<object>(), cancellationToken: Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

			var dateRepository = Substitute.For<ITransferTaskRepository>();
			dateRepository
				.GetMinOrderDeliveryDateForTransferTaskAsync(uow, transferTask.Id, Arg.Any<CancellationToken>())
				.Returns(transferDate);

			var counterRepository = Substitute.For<IDocumentOrganizationCounterRepository>();
			counterRepository
				.GetMaxDocumentOrganizationCounterOnYearAsync(uow, transferDate, seller, Arg.Any<CancellationToken>())
				.Returns(new DocumentOrganizationCounter { Counter = 7 });

			var organizationRepository = Substitute.For<IOrganizationRepository>();
			organizationRepository.GetOrganizationByIdAsync(seller.Id).Returns(seller);
			organizationRepository.GetOrganizationByIdAsync(customer.Id).Returns(customer);

			var preparer = new TransferOrderHeaderPreparer(
				uow,
				dateRepository,
				counterRepository,
				organizationRepository);

			var result = await preparer.PrepareAsync(transferTask, CancellationToken.None);
			Assert.True(result.IsSuccess);
			var transferOrder = result.Value;

			Assert.NotNull(transferOrder);
			Assert.Equal(transferDate, transferOrder.Date);
			Assert.NotEqual(transferTask.StartTime, transferOrder.Date);
			Assert.Equal(transferDate.Year, transferOrder.TransferDocument.CounterDateYear);
			Assert.Equal("VV26-8", transferOrder.TransferDocument.DocumentNumber);
			Assert.Equal(8, transferOrder.TransferDocument.Counter);
			Assert.Same(seller, transferOrder.TransferDocument.Organization);
			await counterRepository.Received(1).GetMaxDocumentOrganizationCounterOnYearAsync(
				uow,
				transferDate,
				seller,
				Arg.Any<CancellationToken>());
		}
	}
}
