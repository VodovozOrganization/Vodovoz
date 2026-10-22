using System;
using System.Linq;
using Edo.Common;
using Edo.Documents.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Payments;

namespace Receipt.Dispatcher.Tests.Fixtures
{
	/// <summary>
	/// Подготовка данных и зависимостей для полной сборки документа УПД в тестах распределения кодов
	/// </summary>
	public static class UpdDocumentBuilderFixture
	{
		public static UpdDocumentBuilder CreateBuilder(
			IUnitOfWork uow,
			ITrueMarkCodesPool pool,
			ITrueMarkCodeRepository codeRepository,
			ITrueMarkCodesPoolCodeProvider codesPoolCodeProvider,
			IUpdTransportCodeService updTransportCodeService)
		{
			var counterpartyEdoAccountController = Substitute.For<ICounterpartyEdoAccountEntityController>();
			counterpartyEdoAccountController
				.GetDefaultCounterpartyEdoAccountByOrganizationId(Arg.Any<CounterpartyEntity>(), Arg.Any<int?>())
				.Returns(new CounterpartyEdoAccountEntity());

			var paymentRepository = Substitute.For<IPaymentRepository>();
			paymentRepository.GetOrderPayments(Arg.Any<IUnitOfWork>(), Arg.Any<int>())
				.Returns(Enumerable.Empty<PaymentEntity>());

			return new UpdDocumentBuilder(
				uow,
				pool,
				codeRepository,
				codesPoolCodeProvider,
				updTransportCodeService,
				counterpartyEdoAccountController,
				paymentRepository,
				Substitute.For<ILogger<UpdDocumentBuilder>>());
		}

		/// <summary>
		/// Заполняет реквизиты заказа, которые нужны для создания документа УПД
		/// </summary>
		public static void PrepareForUpd(DocumentEdoTask task)
		{
			var order = (OrderEntityFixture)task.FormalEdoRequest.Order;

			order.SetDeliveryDate(new DateTime(2025, 1, 1));
			order.SetClient(new CounterpartyEntity { PersonType = PersonType.legal });

			var sourceOrganization = order.Contract.Organization;
			var organization = Substitute.For<OrganizationEntity>();
			organization.INN = sourceOrganization.INN;
			organization.TaxcomEdoSettings.Returns(new TaxcomEdoSettings());
			organization.ActiveOrganizationVersion.Returns(new OrganizationVersionEntity());
			order.Contract.Organization = organization;

			foreach(var orderItem in order.OrderItems)
			{
				orderItem.Nomenclature.Unit = orderItem.Nomenclature.Unit ?? new MeasurementUnits();
			}
		}
	}
}
