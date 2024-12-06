using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Domain
{
	public class NomenclatureFixedPriceController : INomenclatureFixedPriceController 
	{
		private readonly INomenclatureFixedPriceFactory _nomenclatureFixedPriceFactory;
		private readonly INomenclatureFixedPriceRepository _nomenclatureFixedPriceRepository;

		public NomenclatureFixedPriceController(
			INomenclatureFixedPriceFactory nomenclatureFixedPriceFactory,
			INomenclatureFixedPriceRepository nomenclatureFixedPriceRepository)
		{
			_nomenclatureFixedPriceFactory =
				nomenclatureFixedPriceFactory ?? throw new ArgumentNullException(nameof(nomenclatureFixedPriceFactory));
			_nomenclatureFixedPriceRepository =
				nomenclatureFixedPriceRepository ?? throw new ArgumentNullException(nameof(nomenclatureFixedPriceRepository));
		}
		
		public IProgressBarDisplayable ProgressBarDisplayable { get; set; }

		public bool ContainsFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal bottlesCount) => 
			counterparty.ObservableNomenclatureFixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id && bottlesCount >= x.MinCount);

		public bool ContainsFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal bottlesCount) => 
			deliveryPoint.ObservableNomenclatureFixedPrices.Any(x => x.Nomenclature == nomenclature && bottlesCount >= x.MinCount);
		
		public void AddFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice = 0, int minCount = 0)
		{
			if(deliveryPoint == null)
			{
				throw new ArgumentNullException(nameof(deliveryPoint));
			}

			if(nomenclature == null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}
			
			if(nomenclature.Category == NomenclatureCategory.water)
			{
				AddWaterFixedPrice(deliveryPoint, nomenclature, fixedPrice, minCount);
			}
			else 
			{
				throw new NotSupportedException("Не поддерживается.");
			}
		}
		
		public void AddFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice = 0, int minCount = 0) 
		{
			if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			if(nomenclature == null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			if(nomenclature.Category == NomenclatureCategory.water)
			{
				AddWaterFixedPrice(counterparty, nomenclature, fixedPrice, minCount);
			}
			else 
			{
				throw new NotSupportedException("Не поддерживается.");
			}
		}

		public void UpdateFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice, decimal fixedPrice = 0, int minCount = 0)
		{
			if(nomenclatureFixedPrice is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureFixedPrice));
			}

			if(nomenclatureFixedPrice.Nomenclature?.Category == NomenclatureCategory.water)
			{
				UpdateWaterFixedPrice(nomenclatureFixedPrice, fixedPrice, minCount);
			}
			else
			{
				throw new NotSupportedException("Не поддерживается.");
			}
		}

		public void DeleteFixedPrice(DeliveryPoint deliveryPoint, NomenclatureFixedPrice nomenclatureFixedPrice)
		{
			if (deliveryPoint.ObservableNomenclatureFixedPrices.Contains(nomenclatureFixedPrice)) {
				deliveryPoint.ObservableNomenclatureFixedPrices.Remove(nomenclatureFixedPrice);
			}
		}
		
		public void DeleteFixedPrice(Counterparty counterparty, NomenclatureFixedPrice nomenclatureFixedPrice)
		{
			if (counterparty.ObservableNomenclatureFixedPrices.Contains(nomenclatureFixedPrice)) {
				counterparty.ObservableNomenclatureFixedPrices.Remove(nomenclatureFixedPrice);
			}
		}

		public void DeleteAllFixedPricesFromCounterpartyAndDeliveryPoints(Counterparty counterparty)
		{
			counterparty.ObservableNomenclatureFixedPrices.Clear();

			foreach(var deliveryPoint in counterparty.DeliveryPoints)
			{
				deliveryPoint.ObservableNomenclatureFixedPrices.Clear();
			}
		}
		
		public void AddEmployeeFixedPricesToCounterpartyAndDeliveryPoints(
			Counterparty counterparty, IEnumerable<NomenclatureFixedPrice> employeeFixedPrices)
		{
			DeleteAllFixedPricesFromCounterpartyAndDeliveryPoints(counterparty);
			
			foreach(var employeeFixedPrice in employeeFixedPrices)
			{
				AddWaterFixedPrice(counterparty, employeeFixedPrice);

				foreach(var deliveryPoint in counterparty.DeliveryPoints)
				{
					AddWaterFixedPrice(deliveryPoint, employeeFixedPrice);
				}
			}
		}

		public void UpdateAllEmployeeFixedPrices(
			IUnitOfWork uow,
			IEnumerable<NomenclatureFixedPrice> updatedEmployeeFixedPrices,
			IEnumerable<NomenclatureFixedPrice> deletedEmployeeFixedPrices,
			CancellationToken cancellationToken)
		{
			CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);

			var editedFixedPricesCount = updatedEmployeeFixedPrices.Count() + deletedEmployeeFixedPrices.Count();
			
			ProgressBarDisplayable?.Start(editedFixedPricesCount, 0, "Подготовка данных...");
			
			SaveNewEmployeeFixedPrices(
				uow, updatedEmployeeFixedPrices, deletedEmployeeFixedPrices, editedFixedPricesCount, cancellationToken);
			DeleteCounterpartiesAndTheirDeliveryPointsFixedPrices(uow, cancellationToken);
			AddNewEmployeeFixedPrices(uow, updatedEmployeeFixedPrices, cancellationToken);
		}
		
		public IEnumerable<NomenclatureFixedPrice> GetEmployeesNomenclatureFixedPrices(IUnitOfWork uow)
		{
			return _nomenclatureFixedPriceRepository.GetEmployeesNomenclatureFixedPrices(uow);
		}

		private void SaveNewEmployeeFixedPrices(
			IUnitOfWork uow,
			IEnumerable<NomenclatureFixedPrice> updatedEmployeeFixedPrices,
			IEnumerable<NomenclatureFixedPrice> deletedEmployeeFixedPrices,
			int editedFixedPricesCount,
			CancellationToken cancellationToken)
		{
			CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
			var i = 0;
			
			var progress = $"Обновление фиксы сотрудников... ({{0}}/{editedFixedPricesCount})";
			ProgressBarDisplayable?.Update(string.Format(progress, i));

			foreach(var fixedPrice in updatedEmployeeFixedPrices)
			{
				CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
				uow.Save(fixedPrice);
				ProgressBarDisplayable?.Update(string.Format(progress, ++i));
			}
			
			foreach(var fixedPrice in deletedEmployeeFixedPrices)
			{
				CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
				uow.Delete(fixedPrice);
				ProgressBarDisplayable?.Update(string.Format(progress, ++i));
			}
		}

		private void DeleteCounterpartiesAndTheirDeliveryPointsFixedPrices(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);

			var i = 0;
			ProgressBarDisplayable?.Start(text: "Подготовка к очистке старой фиксы сотрудников...");
			
			var counterpartiesFixedPricesForDelete = _nomenclatureFixedPriceRepository.GetAllFixedPricesFromEmployeeCounterparties(uow);
			var deliveryPointsFixedPricesForDelete =
				_nomenclatureFixedPriceRepository.GetAllFixedPricesFromEmployeeCounterpartiesDeliveryPoints(uow);
			var counterpartiesFixedPricesForDeleteCount = counterpartiesFixedPricesForDelete.Count(); 
			var deliveryPointsFixedPricesForDeleteCount = deliveryPointsFixedPricesForDelete.Count();
			var summaryCount = counterpartiesFixedPricesForDeleteCount + deliveryPointsFixedPricesForDeleteCount;
			ProgressBarDisplayable?.Start(summaryCount, 0, "Начинаем очистку старой фиксы сотрудников...");
			
			var clientsProgress = $"Очищаем фиксу у клиентов, привязанных к сотрудникам... ({{0}}/{summaryCount})";
			ProgressBarDisplayable?.Update(string.Format(clientsProgress, i));
			
			foreach(var counterpartyFixedPrice in counterpartiesFixedPricesForDelete)
			{
				CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
				uow.Delete(counterpartyFixedPrice);
				ProgressBarDisplayable?.Update(string.Format(clientsProgress, ++i));
			}
			
			var deliveryPointsProgress = $"Очищаем фиксу у точек доставки клиентов, привязанных к сотрудникам... ({{0}}/{summaryCount})";
			foreach(var deliveryPointFixedPrice in deliveryPointsFixedPricesForDelete)
			{
				CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
				uow.Delete(deliveryPointFixedPrice);
				ProgressBarDisplayable?.Update(string.Format(deliveryPointsProgress, ++i));
			}
		}
		
		private void AddNewEmployeeFixedPrices(IUnitOfWork uow, IEnumerable<NomenclatureFixedPrice> employeeFixedPrices, CancellationToken cancellationToken)
		{
			CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
			
			var i = 0;
			ProgressBarDisplayable?.Start(text: "Подготовка к добавлению фиксы сотрудников...");
			
			var counterpartiesIds = _nomenclatureFixedPriceRepository.GetWorkingEmployeeCounterpartiesIds(uow);
			var deliveryPointsIds = _nomenclatureFixedPriceRepository.GetWorkingEmployeeCounterpartiesDeliveryPointsIds(uow);
			var counterpartiesCount = counterpartiesIds.Count();
			var deliveryPointsCount = deliveryPointsIds.Count();
			var summaryCount = counterpartiesCount + deliveryPointsCount;
			ProgressBarDisplayable?.Start(summaryCount, 0, "Начинаем добавление фиксы сотрудников...");
			
			var clientsProgress = $"Добавляем фиксу клиентам, привязанным к сотрудникам... ({{0}}/{summaryCount})";
			ProgressBarDisplayable?.Update(string.Format(clientsProgress, i));
			
			foreach(var employeeFixedPrice in employeeFixedPrices)
			{
				CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
				foreach(var counterpartyId in counterpartiesIds)
				{
					CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
					var counterpartyFixedPrice = CreateNewNomenclatureFixedPrice(employeeFixedPrice);
					counterpartyFixedPrice.Counterparty = new Counterparty
					{
						Id = counterpartyId
					};
					
					uow.Save(counterpartyFixedPrice);
					ProgressBarDisplayable?.Update(string.Format(clientsProgress, ++i));
				}
				
				var deliveryPointsProgress = $"Добавляем фиксу точкам доставки клиентов, привязанных к сотрудникам... ({{0}}/{summaryCount})";
				foreach(var deliveryPointId in deliveryPointsIds)
				{
					CheckEmployeeFixedPricesUpdateCancellationRequested(cancellationToken);
					var deliveryPointFixedPrice = CreateNewNomenclatureFixedPrice(employeeFixedPrice);
					deliveryPointFixedPrice.DeliveryPoint = new DeliveryPoint
					{
						Id = deliveryPointId
					};
						
					uow.Save(deliveryPointFixedPrice);
					ProgressBarDisplayable?.Update(string.Format(deliveryPointsProgress, ++i));
				}
			}
		}

		private void AddWaterFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal fixedPrice = 0, int minCount = 0, int nomenclatureFixedPriceId = 0)
		{
			var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(nomenclature, fixedPrice, minCount);
			nomenclatureFixedPrice.DeliveryPoint = deliveryPoint;
			nomenclatureFixedPrice.MinCount = minCount;
			deliveryPoint.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
		}

		private void AddWaterFixedPrice(DeliveryPoint deliveryPoint, NomenclatureFixedPrice fromNomenclatureFixedPrice)
		{
			var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(fromNomenclatureFixedPrice);
			nomenclatureFixedPrice.DeliveryPoint = deliveryPoint;
			deliveryPoint.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
		}
		
		private void AddWaterFixedPrice(Counterparty counterparty, NomenclatureFixedPrice fromNomenclatureFixedPrice)
		{
			var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(fromNomenclatureFixedPrice);
			nomenclatureFixedPrice.Counterparty = counterparty;
			counterparty.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
		}

		private void AddWaterFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal fixedPrice = 0, int minCount = 0)
		{
			var nomenclatureFixedPrice = CreateNewNomenclatureFixedPrice(nomenclature, fixedPrice, minCount);
			nomenclatureFixedPrice.Counterparty = counterparty;
			nomenclatureFixedPrice.MinCount = minCount;
			counterparty.ObservableNomenclatureFixedPrices.Add(nomenclatureFixedPrice);
		}

		private void UpdateWaterFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice, decimal fixedPrice = 0, int minCount = 0)
		{
			if(nomenclatureFixedPrice is null)
			{
				throw new ArgumentNullException(nameof(nomenclatureFixedPrice));
			}

			nomenclatureFixedPrice.Price = fixedPrice;
			nomenclatureFixedPrice.MinCount = minCount;
		}

		private NomenclatureFixedPrice CreateNewNomenclatureFixedPrice(Nomenclature nomenclature, decimal fixedPrice, int minCount = 0) 
		{
			var nomenclatureFixedPrice = _nomenclatureFixedPriceFactory.Create();
			nomenclatureFixedPrice.Nomenclature = nomenclature;
			nomenclatureFixedPrice.Price = fixedPrice;
			nomenclatureFixedPrice.MinCount = minCount;

			return nomenclatureFixedPrice;
		}
		
		private NomenclatureFixedPrice CreateNewNomenclatureFixedPrice(NomenclatureFixedPrice fromNomenclatureFixedPrice) 
		{
			var nomenclatureFixedPrice = _nomenclatureFixedPriceFactory.Create();
			nomenclatureFixedPrice.Nomenclature = fromNomenclatureFixedPrice.Nomenclature;
			nomenclatureFixedPrice.Price = fromNomenclatureFixedPrice.Price;
			nomenclatureFixedPrice.MinCount = fromNomenclatureFixedPrice.MinCount;

			return nomenclatureFixedPrice;
		}
		
		private static void CheckEmployeeFixedPricesUpdateCancellationRequested(CancellationToken cancellationToken)
		{
			if(cancellationToken.IsCancellationRequested)
			{
				throw new OperationCanceledException("Отмена операции обновления фиксы сотрудников ВВ");
			}
		}
	}
}
