using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "штрафы сотрудникам",
		Nominative = "штраф сотрудникам")]
	public class Fine: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public DateTime Date {
			get { return date; }
			set {
				SetField (ref date, value, () => Date);

			}
		}

		decimal totalMoney;

		[Display (Name = "Всего денег")]
		public virtual decimal TotalMoney {
			get { return totalMoney; }
			set { SetField (ref totalMoney, value, () => TotalMoney); }
		}

		IList<FineItem> items = new List<FineItem> ();

		[Display (Name = "Строки")]
		public virtual IList<FineItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<FineItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FineItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<FineItem> (Items);
				return observableItems;
			}
		}

		public virtual string Title { 
			get { return String.Format ("Штраф №{0} от {1:d}", Id, Date); }
		}

/*		public virtual void AddItem (IncomingInvoiceItem item)
		{
			item.IncomeGoodsOperation.IncomingWarehouse = warehouse;
			item.IncomeGoodsOperation.OperationTime = TimeStamp;
			item.Document = this;
			ObservableItems.Add (item);
		}
*/
		public Fine ()
		{
		}
	}
}

