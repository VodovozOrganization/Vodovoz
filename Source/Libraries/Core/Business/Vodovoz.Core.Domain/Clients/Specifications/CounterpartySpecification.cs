using Core.Infrastructure.Specifications;
using System;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Client;

namespace Vodovoz.Core.Domain.Clients.Specifications
{
	public class CounterpartySpecification : ExpressionSpecification<CounterpartyEntity>
	{
		public CounterpartySpecification(Expression<Func<CounterpartyEntity, bool>> expression) : base(expression)
		{
		}

		public static CounterpartySpecification ById(int id)
			=> new CounterpartySpecification(x => x.Id == id);

		public static CounterpartySpecification NonEmptyInn()
			=> new CounterpartySpecification(x => !string.IsNullOrWhiteSpace(x.INN));

		public static CounterpartySpecification ReasonForLeavingIs(ReasonForLeaving reasonForLeaving)
			=> new CounterpartySpecification(x => reasonForLeaving == x.ReasonForLeaving);

		public static CounterpartySpecification ReasonForLeavingIsIn(params ReasonForLeaving[] reasonsForLeaving)
			=> new CounterpartySpecification(x => reasonsForLeaving.Contains(x.ReasonForLeaving));

		public static CounterpartySpecification IsLegalPerson()
			=> new CounterpartySpecification(x => x.PersonType == PersonType.legal);

		public static CounterpartySpecification IsNaturalPerson()
			=> new CounterpartySpecification(x => x.PersonType == PersonType.natural);

		public static CounterpartySpecification TruemarkRegisatrationStatusIsIn(params RegistrationInChestnyZnakStatus[] registrationInChestnyZnakStatuses)
			=> new CounterpartySpecification(x => registrationInChestnyZnakStatuses.Contains(x.RegistrationInChestnyZnakStatus));

		public static CounterpartySpecification ConsentForEdoStatusIs(ConsentForEdoStatus consentForEdoStatus)
			=> new CounterpartySpecification(x => x.ConsentForEdoStatus == consentForEdoStatus);

		public static ExpressionSpecification<CounterpartyEntity> IsEdoPaymentTypeCashlessAllowed(PaymentType paymentType)
			=> CounterpartySpecification
				.ReasonForLeavingIs(ReasonForLeaving.Other)
				.And(CounterpartySpecification
					.IsLegalPerson())
				.And(CounterpartySpecification
					.NonEmptyInn()
					.Or(CounterpartySpecification
						.ReasonForLeavingIs(ReasonForLeaving.ForOwnNeeds)))
				.Or(CounterpartySpecification
					.ReasonForLeavingIs(ReasonForLeaving.Resale)
					);

		public static ExpressionSpecification<CounterpartyEntity> IsEdoPaymentTypeReceiptAllowed(PaymentType paymentType)
			=> 
	}
}
