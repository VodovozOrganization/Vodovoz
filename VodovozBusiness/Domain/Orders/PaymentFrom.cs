using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "место, откуда проведены оплаты",
		Nominative = "место, откуда проведена оплата")]
	[HistoryTrace]
	[EntityPermission]
	public class PaymentFrom : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Organization _organizationForAvangardPayments;
		
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }

		public virtual Organization OrganizationForAvangardPayments
		{
			get => _organizationForAvangardPayments;
			set => SetField(ref _organizationForAvangardPayments, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.ServiceContainer.GetService(
				typeof(IPaymentFromRepository)) is IPaymentFromRepository paymentFromRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(paymentFromRepository) }");
			}
			
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено", new[] { nameof(Name) });
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var duplicate = paymentFromRepository.GetDuplicatePaymentFromByName(uow, Id, Name);
				if(duplicate != null)
				{
					yield return new ValidationResult("Источник оплаты с таким названием уже существует\n" +
						$"Id {duplicate.Id} Название {duplicate.Name}", new[] { nameof(Name) });
				}
			}
		}
	}
}
