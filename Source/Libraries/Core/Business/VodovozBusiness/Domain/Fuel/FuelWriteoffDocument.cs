using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Fuel;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		Nominative = "акт списания топлива",
		NominativePlural = "акты списания топлива")]
	[EntityPermission]
	[HistoryTrace]
	public class FuelWriteoffDocument : BusinessObjectBase<FuelWriteoffDocument>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private DateTime date;
		[Display(Name = "Дата списания")]
		public virtual DateTime Date {
			get => date;
			set => SetField(ref date, value, () => Date);
		}

		private Employee cashier;
		[Display(Name = "Кассир")]
		public virtual Employee Cashier {
			get => cashier;
			set => SetField(ref cashier, value, () => Cashier);
		}

		private Employee employee;
		[Display(Name = "На кого списывают")]
		public virtual Employee Employee {
			get => employee;
			set => SetField(ref employee, value, () => Employee);
		}

		private Subdivision cashSubdivision;
		[Display(Name = "Касса")]
		public virtual Subdivision CashSubdivision {
			get => cashSubdivision;
			set => SetField(ref cashSubdivision, value, () => CashSubdivision);
		}

		private string reason;
		[Display(Name = "Основание")]
		public virtual string Reason {
			get => reason;
			set => SetField(ref reason, value, () => Reason);
		}

		private int? expenseCategoryId;
		[Display(Name = "Статья расхода")]
		[HistoryIdentifier(TargetType = typeof(FinancialExpenseCategory))]
		public virtual int? ExpenseCategoryId {
			get => expenseCategoryId;
			set => SetField(ref expenseCategoryId, value);
		}

		IList<FuelWriteoffDocumentItem> fuelWriteoffDocumentItems = new List<FuelWriteoffDocumentItem>();
		[Display(Name = "Строки акта списания топлива")]
		public virtual IList<FuelWriteoffDocumentItem> FuelWriteoffDocumentItems {
			get { return fuelWriteoffDocumentItems; }
			set { SetField(ref fuelWriteoffDocumentItems, value, () => FuelWriteoffDocumentItems); }
		}

		GenericObservableList<FuelWriteoffDocumentItem> observableFuelWriteoffDocumentItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FuelWriteoffDocumentItem> ObservableFuelWriteoffDocumentItems {
			get {
				if(observableFuelWriteoffDocumentItems == null) {
					observableFuelWriteoffDocumentItems = new GenericObservableList<FuelWriteoffDocumentItem>(FuelWriteoffDocumentItems);
				}
				return observableFuelWriteoffDocumentItems;
			}
		}

		public virtual void AddNewWriteoffItem(FuelType fuelType)
		{
			if(fuelType == null) {
				throw new ArgumentNullException(nameof(fuelType));
			}

			if(ObservableFuelWriteoffDocumentItems.Any(x => x.FuelType.Id == fuelType.Id)) {
				return;
			}
			var newItem = new FuelWriteoffDocumentItem();
			newItem.FuelType = fuelType;
			newItem.FuelWriteoffDocument = this;
			ObservableFuelWriteoffDocumentItems.Add(newItem);
		}

		public virtual void RemoveWriteoffItem(FuelWriteoffDocumentItem item)
		{
			if(ObservableFuelWriteoffDocumentItems.Contains(item)) {
				ObservableFuelWriteoffDocumentItems.Remove(item);
			}
		}

		public virtual void UpdateOperations()
		{
			foreach(FuelWriteoffDocumentItem item in ObservableFuelWriteoffDocumentItems) {
				item.UpdateOperation();
			}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			if(Employee == null)
			{
				yield return new ValidationResult("Необходимо выбрать сотрудника на кого будет списываться топливо");
			}
			if(CashSubdivision == null) {
				yield return new ValidationResult("Необходимо выбрать кассу с которой будет списываться топливо");
			}
			if(string.IsNullOrWhiteSpace(Reason)) {
				yield return new ValidationResult("Необходимо ввести основание");
			}
			if(ExpenseCategoryId == null) {
				yield return new ValidationResult("Необходимо выбрать статью расхода");
			}
			if(ObservableFuelWriteoffDocumentItems == null) {
				yield return new ValidationResult("Необходимо ввести информацию о списываемом топливе");
			}
			if(!ObservableFuelWriteoffDocumentItems.Any()) {
				yield return new ValidationResult("Необходимо добавить топливо");
			}
			if(ObservableFuelWriteoffDocumentItems.Any(x => x.Liters <= 0)) {
				yield return new ValidationResult("Во всех позициях выдаваемого топлива должно быть введено количество");
			}
			if(!(validationContext.GetService(typeof(IFuelRepository)) is IFuelRepository fuelRepository)) {
				throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IFuelRepository)}");
			}
			foreach(var result in ValidateFuelBalance(uowFactory, fuelRepository)) {
				yield return result;
			}
		}

		private IEnumerable<ValidationResult> ValidateFuelBalance(IUnitOfWorkFactory uowFactory, IFuelRepository fuelRepository)
		{
			if(fuelRepository == null) {
				throw new ArgumentNullException(nameof(fuelRepository));
			}

			if(CashSubdivision == null) {
				yield break;
			}
		
			using(var uow = uowFactory.CreateWithoutRoot()) {
				var balance = fuelRepository.GetAllFuelsBalanceForSubdivision(uow, CashSubdivision);
				if(!UoW.IsNew) {
					FuelWriteoffDocument originalDocument = uow.GetById<FuelWriteoffDocument>(Id);
					if(originalDocument.CashSubdivision.Id == CashSubdivision.Id) {
						foreach(FuelWriteoffDocumentItem item in ObservableFuelWriteoffDocumentItems) {
							decimal existedLiters = originalDocument.FuelWriteoffDocumentItems.FirstOrDefault(x => x.Id == item.Id)?.Liters ?? 0;
							if(item.Liters > balance[item.FuelType] + existedLiters) {
								yield return new ValidationResult($"Недостаточно топлива для выдачи ({item.FuelType.Name})");
							}
						}
						yield break;
					}
				} 
				foreach(FuelWriteoffDocumentItem item in ObservableFuelWriteoffDocumentItems) {
					if(item.Liters > balance[item.FuelType]) {
						yield return new ValidationResult($"Недостаточно топлива для выдачи ({item.FuelType.Name})");
					}
				}
			}
		}

		#endregion IValidatableObject implementation
	}
}
