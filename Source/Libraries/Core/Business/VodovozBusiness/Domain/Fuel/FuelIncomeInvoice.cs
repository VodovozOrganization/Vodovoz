using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Tools;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "входящая накладная по топливу",
		NominativePlural = "входящие накладные по топливу")]
	[EntityPermission]
	[HistoryTrace]
	public class FuelIncomeInvoice : BusinessObjectBase<FuelIncomeInvoice>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private DateTime сreationTime;
		[Display(Name = "Время создания")]
		public virtual DateTime СreationTime {
			get => сreationTime;
			set => SetField(ref сreationTime, value, () => СreationTime);
		}

		private Employee authtor;
		[Display(Name = "Автор")]
		public virtual Employee Author {
			get => authtor;
			set => SetField(ref authtor, value, () => Author);
		}

		private string invoiceDoc;
		[Display(Name = "Накладная")]
		public virtual string InvoiceDoc {
			get => invoiceDoc;
			set => SetField(ref invoiceDoc, value, () => InvoiceDoc);
		}

		private string invoiceBillDoc;
		[Display(Name = "Счет-фактура")]
		public virtual string InvoiceBillDoc {
			get => invoiceBillDoc;
			set => SetField(ref invoiceBillDoc, value, () => InvoiceBillDoc);
		}

		private Counterparty counterparty;
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value, () => Counterparty);
		}

		private Subdivision subdivision;
		[Display(Name = "Касса")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value, () => Subdivision);
		}

		IList<FuelIncomeInvoiceItem> fuelIncomeInvoiceItems = new List<FuelIncomeInvoiceItem>();
		[Display(Name = "Строки накладной")]
		public virtual IList<FuelIncomeInvoiceItem> FuelIncomeInvoiceItems {
			get { return fuelIncomeInvoiceItems; }
			set { SetField(ref fuelIncomeInvoiceItems, value, () => FuelIncomeInvoiceItems); }
		}

		GenericObservableList<FuelIncomeInvoiceItem> observableFuelIncomeInvoiceItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<FuelIncomeInvoiceItem> ObservableFuelIncomeInvoiceItems {
			get {
				if(observableFuelIncomeInvoiceItems == null) {
					observableFuelIncomeInvoiceItems = new GenericObservableList<FuelIncomeInvoiceItem>(FuelIncomeInvoiceItems);
					observableFuelIncomeInvoiceItems.ListContentChanged += (sender, e) => {
						OnPropertyChanged(() => FuelLiters);
						OnPropertyChanged(() => FuelSum);
					};
				}
				return observableFuelIncomeInvoiceItems;
			}
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		[Display(Name = "Объем топлива")]
		public virtual decimal FuelLiters => ObservableFuelIncomeInvoiceItems.Sum(x => x.Liters);

		[Display(Name = "Стоимость топлива")]
		public virtual decimal FuelSum => ObservableFuelIncomeInvoiceItems.Sum(x => x.TotalSum);

		public virtual void UpdateOperations(IFuelRepository fuelRepository)
		{
			if(fuelRepository == null) {
				throw new ArgumentNullException(nameof(fuelRepository));
			}

			var validationContext = new ValidationContext(this);
			validationContext.InitializeServiceProvider(type =>
			{
				if(type == typeof(IFuelRepository))
				{
					return fuelRepository;
				}
				return null;
			});

			string exceptionMessage = this.RaiseValidationAndGetResult(validationContext);
			if(!string.IsNullOrWhiteSpace(exceptionMessage)) {
				throw new ValidationException(exceptionMessage);
			}

			foreach(var item in FuelIncomeInvoiceItems) {
				item.UpdateOperation();
			}
		}

		private decimal GetLitersSumForFuelType(FuelType fuelType)
		{
			if(fuelType == null) {
				return 0m;
			}
			decimal sum = FuelIncomeInvoiceItems
				.Where(x => x.Nomenclature?.FuelType != null)
				.Where(x => x.Nomenclature.FuelType.Id == fuelType.Id)
				.Sum(x => x.Liters);
			return sum;
		}

		private IEnumerable<ValidationResult> ValidateFuelBalance(IUnitOfWorkFactory uowFactory, IFuelRepository fuelRepository)
		{
			if(fuelRepository == null) {
				throw new ArgumentNullException(nameof(fuelRepository));
			}
			if(UoW.IsNew) {
				yield break;
			}
			using(var uow = uowFactory.CreateWithoutRoot())
			{
				var balance = fuelRepository.GetAllFuelsBalanceForSubdivision(uow, Subdivision);
				FuelIncomeInvoice originalInvoice = uow.GetById<FuelIncomeInvoice>(Id);
				var originalBalance = fuelRepository.GetAllFuelsBalanceForSubdivision(UoW, originalInvoice.Subdivision);

				foreach(var originalItem in originalInvoice.FuelIncomeInvoiceItems) {
					var fuelType = originalItem.Nomenclature.FuelType;
					var currentItem = FuelIncomeInvoiceItems.FirstOrDefault(x => x.Id == originalItem.Id);
					//если была удалена строка
					if(currentItem == null) {
						var deletedLiters = originalBalance[fuelType] - originalItem.Liters;
						if(deletedLiters < 0) {
							yield return new ValidationResult($"Невозможно удалить приход для топлива ({fuelType.Name})" +
								" так как на балансе не остается доступного количества топлива");
							continue;
						}
					}
					//если была смена подразделения
					if(originalInvoice.Subdivision.Id != Subdivision.Id) {
						var changedSubdivisionLiters = originalBalance[fuelType] - originalItem.Liters;
						if(changedSubdivisionLiters < 0) {
							yield return new ValidationResult($"Невозможно изменить подразделение, так как на балансе не остается " +
								$"доступного количества топлива ({fuelType.Name}) для подразделения ({originalInvoice.Subdivision.Name})");
							continue;
						}
					}
					//если было изменение значения
					if(currentItem.Liters > originalItem.Liters) {
						continue;
					}
					decimal changedLiters = originalBalance[fuelType] - (originalItem.Liters - currentItem.Liters);
					if(originalItem.Liters < currentItem.Liters) {
						continue;
					}
					if(changedLiters < 0) {
						yield return new ValidationResult($"На балансе нет доступного количества топлива ({fuelType.Name}) для изменения документа");
					}
				}
			}
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			if(!FuelIncomeInvoiceItems.Any())
			{
				yield return new ValidationResult("Необходимо добавить позиции по приходу топлива");
			}

			if(FuelIncomeInvoiceItems.Any(x => x.Liters <= 0)) {
				yield return new ValidationResult("Во всех позициях должен быть указан объем топлива");
			}

			if(Subdivision == null) {
				yield return new ValidationResult("Касса должна быть заполнена");
			}

			if(FuelIncomeInvoiceItems.Any(x => x.Nomenclature == null)) {
				yield return new ValidationResult("Для каждого топлива должна быть выбрана номенклатура");
			}

			if(FuelIncomeInvoiceItems.Any(x => x.Nomenclature != null && x.Nomenclature.Category != NomenclatureCategory.fuel)) {
				yield return new ValidationResult("В документе можно добавлять только топливо");
			}

			if(!(validationContext.GetService(typeof(IFuelRepository)) is IFuelRepository fuelRepository)) {
				throw new ArgumentException($"Для валидации отправки должен быть доступен репозиторий {nameof(IFuelRepository)}");
			}
			foreach(var result in ValidateFuelBalance(uowFactory, fuelRepository)) {
				yield return result;
			}
		}

		public virtual void AddItem(Nomenclature nomenclature)
		{
			if(nomenclature.Category != NomenclatureCategory.fuel) {
				return;
			}
			var newItem = new FuelIncomeInvoiceItem() {
				Nomenclature = nomenclature,
				FuelIncomeInvoice = this
			};
			ObservableFuelIncomeInvoiceItems.Add(newItem);
		}

		public virtual void DeleteItem(FuelIncomeInvoiceItem item)
		{
			if(item == null || !ObservableFuelIncomeInvoiceItems.Contains(item)) {
				return;
			}
			ObservableFuelIncomeInvoiceItems.Remove(item);
		}
	}
}
