using System.Collections.Generic;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Repository;
using Vodovoz.ServiceDialogs.ExportTo1c;

namespace Vodovoz.ServiceDialogs
{
	public class ExportCounterpartiesTo1C
	{
		private readonly IUnitOfWork uow;
		private IList<CounterpartyTo1CNode> counterparties;

		public int Steps => counterparties.Count;
		public ExportCounterpariesData Result { get; private set; }

		public ExportCounterpartiesTo1C()
		{
			this.uow = UnitOfWorkFactory.CreateWithoutRoot();
		}

		public void Run(IWorker worker)
		{
			worker.OperationName = "Подготовка данных";
			worker.ReportProgress(0, "Получение контрагентов");
			counterparties = CounterpartyRepository.GetCounterpartiesWithInnAndAnyContact(uow);
			worker.OperationName = "Выгрузка имён и контактных данных";
			worker.StepsCount = Steps;
			Result = new ExportCounterpariesData(uow);
			int i = 0;
			while(!worker.IsCancelled && i < Steps) {
				worker.ReportProgress(i, "Контрагент");
				Result.AddCounterparty(counterparties[i]);
				i++;
			}
		}
	}
}
