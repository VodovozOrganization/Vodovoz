using System;
using QSOrmProject;
using System.Data.Bindings;
using QSContacts;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject("Доверенности")]
	public class Proxy : BaseNotifyPropertyChanged, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Number { get; set; }
		public virtual DateTime IssueDate { get; set; }
		public virtual DateTime StartDate { get; set; }
		public virtual DateTime ExpirationDate { get; set; }
		public virtual IList<Person> Persons { get; set; }
		public virtual DeliveryPoint DeliveryPoint { get; set; }
		#endregion

		public Proxy ()
		{
			Number = String.Empty;
		}
		public string Issue { get { return IssueDate.ToShortDateString(); } } 
		public string Start { get { return StartDate.ToShortDateString(); } } 
		public string Expiration { get { return ExpirationDate.ToShortDateString(); } }  
	}

	public interface IProxyOwner
	{
		IList<Proxy> Proxies { get; set;}
	}
}

