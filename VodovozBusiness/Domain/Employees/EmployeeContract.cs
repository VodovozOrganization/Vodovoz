using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Print;
using QSDocTemplates;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Domain.Employees
{
	public class EmployeeContract: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Контракт")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		EmployeeDocument document;
		[Display(Name = "Документ")]
		public virtual EmployeeDocument Document {
			get { return document; }
			set { SetField(ref document, value, () => Document); }
		}

		byte[] templateFile;
		[Display(Name = "Файл договора")]
		public virtual byte[] TemplateFile {
			get { return templateFile; }
			set { SetField(ref templateFile, value, () => TemplateFile); }
		}

		DocTemplate employeeContractTemplate;
		[Display(Name = "Шаблон")]
		public virtual DocTemplate EmployeeContractTemplate {
			get { return employeeContractTemplate; }
			set { SetField(ref employeeContractTemplate, value, () => EmployeeContractTemplate); }
		}

		DateTime firstDay;
		[Display(Name = "Дата  начала договора")]
		public virtual DateTime FirstDay {
			get { return firstDay; }
			set {SetField(ref firstDay, value, () => FirstDay);}
		}

		DateTime lastDay;
		[Display(Name = "Дата окончания договора")]
		public virtual DateTime LastDay {
			get { return lastDay; }
			set { SetField(ref lastDay, value, () => LastDay); }

		}

		DateTime contractDate;
		[Display(Name = "Дата окончания договора")]
		public virtual DateTime ContractDate {
			get { return contractDate; }
			set { SetField(ref contractDate, value, () => ContractDate); }

		}

		Organization organization;
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get { return organization; }
			set { SetField(ref organization, value, () => Organization); }
		}

		Employee employee;
		[Display(Name = "Сотрудник")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField(ref employee, value, () => Employee); }
		}

	}
}
