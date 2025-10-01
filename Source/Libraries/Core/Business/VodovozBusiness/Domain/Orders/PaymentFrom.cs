using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Источники оплат",
		GenitivePlural = "Источников оплат",
		Nominative = "Источник оплаты")]
	[HistoryTrace]
	[EntityPermission]
	public class PaymentFrom : PaymentFromEntity, IValidatableObject
	{
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			if(!(validationContext.GetService(
				typeof(IPaymentFromRepository)) is IPaymentFromRepository paymentFromRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(paymentFromRepository)}");
			}
			
			if(!(validationContext.GetService(
				typeof(IOrderSettings)) is IOrderSettings orderSettings))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(orderSettings)}");
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено", new[] { nameof(Name) });
			}

			using(var uow = uowFactory.CreateWithoutRoot())
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
