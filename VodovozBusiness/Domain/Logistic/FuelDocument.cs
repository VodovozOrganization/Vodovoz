using System;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Logistic
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "документы выдачи топлива",
		Nominative = "документ выдачи топлива")]
	public class FuelDocument: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set {SetField(ref date, value, () => Date);}
		}

		private Employee driver;

		[Display (Name = "Водитель")]
		public virtual Employee Driver {
			get { return driver; }
			set {SetField(ref driver, value, () => Driver);}
		}

		private Car car;

		[Display(Name = "Транспортное средство")]
		public virtual Car Car
		{
			get { return car; }
			set { SetField(ref car, value, () => Car); }
		}

		private FuelOperation operation;

		[Display (Name = "Операции выдачи")]
		public virtual FuelOperation Operation {
			get { return operation; }
			set {SetField(ref operation, value, () => Operation);}
		}

		private decimal? payedForLiter;


		[Display (Name = "Топливо, оплаченное деньгами")]
		public virtual decimal? PayedForFuel {
			get { return payedForLiter; }
			set {SetField(ref payedForLiter, value, () => PayedForFuel);}
		}

		private decimal literCost;


		[Display (Name = "Стоимость литра топлива")]
		public virtual decimal LiterCost {
			get { return literCost; }
			set { SetField(ref literCost, value, () => LiterCost);}
		}

		private FuelType fuel;

		[Display (Name = "Вид топлива")]
		public virtual FuelType Fuel {
			get { return fuel; }
			set { SetField(ref fuel, value, () => Fuel);}
		}
			
		IList<FuelDocumentItem> fuelTickets = new List<FuelDocumentItem> ();

		[Display (Name = "Адреса в маршрутном листе")]
		public virtual IList<FuelDocumentItem> FuelTickets {
			get { return fuelTickets; }
			set { 
				SetField (ref fuelTickets, value, () => FuelTickets); 
			}
		}

		private Expense fuelCashExpense;

		[Display(Name = "Оплата топлива")]
		public virtual Expense FuelCashExpense {
			get { return fuelCashExpense; }
			set { SetField(ref fuelCashExpense, value, () => FuelCashExpense); }
		}

		public FuelDocument()
		{
		}

		public virtual void UpdateOperation(Dictionary<GazTicket, int> TicketsList) 
		{
			int litersByTickets = TicketsList.Sum(x => x.Value * x.Key.Liters);

			decimal litersByMoney = 0;
			if(Fuel.Cost >0 && PayedForFuel.HasValue)
				litersByMoney = PayedForFuel.Value / Fuel.Cost;
			if (Operation == null)
				Operation = new FuelOperation();

			Car car = Car;
			Employee driver = Driver;
			if (car.IsCompanyHavings)
				driver = null;
			else
				car = null;
			
			Operation.Driver 		 = driver;
			Operation.Car			 = car;
			Operation.Fuel 			 = Fuel;
			Operation.LitersGived 	 = litersByTickets + litersByMoney;
			Operation.LitersOutlayed = 0;
			Operation.OperationTime  = Date;
		}

		public virtual void UpdateFuelCashExpense(IUnitOfWork uow, Employee cashier, int routeListId)
		{
			if (PayedForFuel.HasValue) {
				if (FuelCashExpense == null) {
					FuelCashExpense = new Expense
					{
						ExpenseCategory = Repository.Cash.CategoryRepository.FuelDocumentExpenseCategory(uow),
						TypeOperation 	= ExpenseType.Expense,
						Date 			= DateTime.Now,
						Casher 			= cashier,
						Employee 		= Driver,
						Description 	=$"Оплата топлива по МЛ №{routeListId}",
					};
				}
				FuelCashExpense.Money = PayedForFuel.Value;
			}
			else
				FuelCashExpense = null;
		}

		public virtual void UpdateRowList(Dictionary<GazTicket, int> ticketsList)
		{
			FuelTickets
				.Where(x => !ticketsList.Any(y => x.GasTicket.Id == y.Key.Id && y.Value > 0))
				.ToList().ForEach(x => FuelTickets.Remove(x));

			foreach (var ticket in ticketsList.Where(y => y.Value > 0))
			{
				var item = FuelTickets.FirstOrDefault(x => ticket.Key.Id == x.Id);
				if (item != null)
				{
					item.TicketsCount = ticket.Value;
				}
				else
				{
					item = new FuelDocumentItem();
					item.Document = this;
					item.GasTicket = ticket.Key;
					item.TicketsCount = ticket.Value;

					FuelTickets.Add(item);
				}
			}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Operation == null || operation.LitersGived == 0)
			{
				yield return new ValidationResult("Необходимо заполнить талон или заплатить",
					new[] {Gamma.Utilities.PropertyUtil.GetPropertyName(this, o=>o.Operation)});
			}
		}

		#endregion
	}
}

