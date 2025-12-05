using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Print;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Доверенности на ТС",
		Nominative = "Доверенность на ТС")]
	[EntityPermission]
	public class CarProxyDocument : ProxyDocument, IValidatableObject
	{
		ICounterpartyRepository counterpartyRepository => ScopeProvider.Scope
			.Resolve<ICounterpartyRepository>();

		public virtual string Title {
			get {
				return String.Format("Доверенность на ТС № {0}", Id);
			}
		}

		public override ProxyDocumentType Type {
			get {
				return ProxyDocumentType.CarProxy;
			}
		}

		public override PrinterType PrintType {
			get {
				return PrinterType.ODT;
			}
		}

		DateTime date;
		[Display(Name = "Дата доверенности")]
		public override DateTime Date {
			get => date; 
			set {
				SetField(ref date, value, () => Date);
				ExpirationDate = date.AddYears(1);
			}
		}

		Employee driver;
		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set { SetField(ref driver, value, () => Driver); }
		}

		Car car;
		[Display(Name = "Автомобиль")]
		public virtual Car Car {
			get { return car; }
			set { SetField(ref car, value, () => Car); }
		}


		DateTime expirationDate;
		public override DateTime ExpirationDate {
			get => expirationDate;
			set { SetField(ref expirationDate, value, () => ExpirationDate); }
		}

		public virtual void UpdateCarProxyDocumentTemplate(IUnitOfWork uow, IDocTemplateRepository docTemplateRepository)
		{
			if(Id > 0 || Organization == null) {
				return;
			}
			DocumentTemplate = docTemplateRepository.GetFirstAvailableTemplate(uow, TemplateType.CarProxy, Organization);
		}

		public virtual string GetDealers()
		{
			List<string> dealers = new List<string>();
			foreach(var dealer in counterpartyRepository.GetDealers())
			{
				dealers.Add($"{dealer.FullName} (ОГРН {dealer.OGRN})");
			}

			return string.Join(", ", dealers);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Organization == null)
				yield return new ValidationResult(String.Format("Не выбрана организация"));

			if(Driver == null)
				yield return new ValidationResult(String.Format("Не выбран водитель"));

			if(Car == null)
				yield return new ValidationResult(String.Format("Не выбран автомобиль"));

			if(DocumentTemplate == null)
				yield return new ValidationResult(String.Format("Не выбран шаблон доверенности"));
		}

		#endregion
	}
}
