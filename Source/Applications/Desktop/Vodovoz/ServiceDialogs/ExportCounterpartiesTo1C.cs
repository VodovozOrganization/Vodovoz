using System.Collections.Generic;
using QS.DomainModel.UoW;
using QSProjectsLib;
using System;
using Vodovoz.EntityRepositories.Counterparties;
using QS.Project.Services;
using Autofac;
using ExportTo1c.Library;

namespace Vodovoz.ServiceDialogs
{
	public class ExportCounterpartiesTo1C : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ICounterpartyRepository _counterpartyRepository = ScopeProvider.Scope.Resolve<ICounterpartyRepository>();
		private IList<CounterpartyTo1CNode> _counterparties;

		public int Steps => _counterparties.Count;
		public ExportCounterpariesData Result { get; private set; }

		public ExportCounterpartiesTo1C()
		{
			this._uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
		}

		public void Run(IWorker worker)
		{
			worker.OperationName = "Подготовка данных";
			worker.ReportProgress(0, "Получение контрагентов");
			_counterparties = _counterpartyRepository.GetCounterpartiesWithInnAndAnyContact(_uow);
			worker.OperationName = "Выгрузка имён и контактных данных";
			worker.StepsCount = Steps;
			Result = new ExportCounterpariesData(_uow);
			int i = 0;
			while(!worker.IsCancelled && i < Steps) {
				worker.ReportProgress(i, "Контрагент");
				Result.AddCounterparty(_counterparties[i]);
				i++;
			}
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}
