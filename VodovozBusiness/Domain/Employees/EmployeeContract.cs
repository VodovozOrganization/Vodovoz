using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

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

		DateTime firstDay;
		[Display(Name = "Дата начала договора")]
		public virtual DateTime FirstDay {
			get { return firstDay; }
			set {SetField(ref firstDay, value, () => FirstDay);}
		}

		DateTime? lastDay;
		[Display(Name = "Дата окончания договора")]
		public virtual DateTime? LastDay {
			get { return lastDay; }
			set { SetField(ref lastDay, value, () => LastDay); }

		}
	}
}
