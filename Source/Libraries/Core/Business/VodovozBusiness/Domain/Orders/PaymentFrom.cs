using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "место, откуда проведены оплаты",
		Nominative = "место, откуда проведена оплата")]
	[HistoryTrace]
	[EntityPermission]
	public class PaymentFrom : PropertyChangedBase, IDomainObject, IValidatableObject, INamed, IArchivable
	{
		private Organization _organizationForOnlinePayments;
		
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual bool IsArchive { get; set; }

		public virtual Organization OrganizationForOnlinePayments
		{
			get => _organizationForOnlinePayments;
			set => SetField(ref _organizationForOnlinePayments, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			if(!(validationContext.ServiceContainer.GetService(
				typeof(IPaymentFromRepository)) is IPaymentFromRepository paymentFromRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(paymentFromRepository) }");
			}
			
			if(!(validationContext.ServiceContainer.GetService(
				typeof(IOrderParametersProvider)) is IOrderParametersProvider orderParametersProvider))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(orderParametersProvider) }");
			}
			
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название должно быть заполнено", new[] { nameof(Name) });
			}

			if(Id > 0
				&& OrganizationForOnlinePayments != null
				&& orderParametersProvider.PaymentsByCardFromAvangard.Contains(Id)
				&& !OrganizationForOnlinePayments.AvangardShopId.HasValue)
			{
				yield return new ValidationResult("Организация присвоена источнику Авангарда, но в базе не заполнено avangard_shop_Id",
					new[] { nameof(OrganizationForOnlinePayments.AvangardShopId) });
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
