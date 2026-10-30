using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;

namespace Vodovoz.ViewModels.Services.Edo
{
	/// <summary>Получение сохранённых УПД для журнала и просмотра.</summary>
	public class EdoUpdDocumentService : IEdoUpdDocumentService
	{
		private readonly IEdoUpdDocumentRepository _repository;

		/// <summary>Создаёт сервис получения сохранённых УПД.</summary>
		/// <param name="repository">Репозиторий сохранённых УПД.</param>
		public EdoUpdDocumentService(IEdoUpdDocumentRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		/// <inheritdoc/>
		public IList<EdoUpdJournalNode> GetItems(IUnitOfWork uow, EdoUpdJournalFilter filter, int? offset, int? limit)
		{
			return _repository.GetItems(uow, CreateFilter(filter), offset, limit)
				.Select(document => new EdoUpdJournalNode
				{
					Id = document.Id,
					Number = document.Number,
					Date = document.Date,
					Sum = document.Sum,
					Seller = document.Seller,
					Customer = document.Customer,
					CustomerInn = document.CustomerInn,
					TaskType = document.TaskType,
					TaskId = document.TaskId
				}).ToList();
		}

		/// <inheritdoc/>
		public int GetItemsCount(IUnitOfWork uow, EdoUpdJournalFilter filter)
			=> _repository.GetItemsCount(uow, CreateFilter(filter));

		/// <inheritdoc/>
		public EdoUpdDocument GetDocument(IUnitOfWork uow, int documentId) => _repository.GetDocument(uow, documentId);

		private static EdoUpdDocumentFilter CreateFilter(EdoUpdJournalFilter filter) => new EdoUpdDocumentFilter
		{
			DateFrom = filter.DateFrom?.Date,
			DateToExclusive = filter.DateTo?.Date.AddDays(1),
			TaskType = filter.DocumentType.HasValue
				? (filter.DocumentType == EdoUpdDocumentType.Transfer ? EdoTaskType.Transfer : EdoTaskType.Document)
				: (EdoTaskType?)null,
			CustomerInn = string.IsNullOrWhiteSpace(filter.CustomerInn) ? null : filter.CustomerInn.Trim()
		};
	}
}
