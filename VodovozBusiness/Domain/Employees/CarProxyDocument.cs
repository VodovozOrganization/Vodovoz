using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Employees
{
	public class CarProxyDocument : PropertyChangedBase, IDomainObject, IBusinessObject, IValidatableObject
	{
		public virtual IUnitOfWork UoW { set; get; }

		public virtual int Id { get; set; }

		public virtual string Title {
			get{
				return String.Format("Доверенность на ТС № {0}", Id);
			}
		}

		DateTime date;
		[Display(Name = "Дата доверенности")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField(ref date, value, () => Date); }
		}

		Organization organization;
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField(ref organization, value, () => Organization); }
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

		DocTemplate carProxyDocumentTemplate;
		[Display(Name = "Шаблон доверенности")]
		public virtual DocTemplate CarProxyDocumentTemplate {
			get { return carProxyDocumentTemplate; }
			protected set { SetField(ref carProxyDocumentTemplate, value, () => CarProxyDocumentTemplate); }
		}

		byte[] changedTemplateFile;
		[Display(Name = "Измененная доверенность")]
		public virtual byte[] ChangedTemplateFile {
			get { return changedTemplateFile; }
			set { SetField(ref changedTemplateFile, value, () => ChangedTemplateFile); }
		}

		//Конструкторы
		public static IUnitOfWorkGeneric<CarProxyDocument> Create()
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<CarProxyDocument>();
			return uow;
		}

		[Display(Name = "Дата окончания")]
		public virtual DateTime ExpirationDate {
			get{
				return Date.AddYears(1);
			}
		}

		public virtual void UpdateCarProxyDocumentTemplate(IUnitOfWork uow)
		{
			if(Id > 0 || Organization == null) {
				return;
			}
			carProxyDocumentTemplate = Repository.Client.DocTemplateRepository.GetTemplate(uow, TemplateType.CarProxy, Organization);
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
			
			if(CarProxyDocumentTemplate == null)
				yield return new ValidationResult(String.Format("Не выбран шаблон доверенности"));
		}

		#endregion
	}
}