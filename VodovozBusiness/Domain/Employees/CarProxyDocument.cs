using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Доверенности на ТС",
		Nominative = "Доверенность на ТС")]
	public class CarProxyDocument : ProxyDocument, IValidatableObject
	{
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

		//Конструкторы
		public static IUnitOfWorkGeneric<CarProxyDocument> Create()
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<CarProxyDocument>();
			return uow;
		}

		public virtual void UpdateCarProxyDocumentTemplate(IUnitOfWork uow)
		{
			if(Id > 0 || Organization == null) {
				return;
			}
			DocumentTemplate = Repository.Client.DocTemplateRepository.GetFirstAvailableTemplate(uow, TemplateType.CarProxy, Organization);
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