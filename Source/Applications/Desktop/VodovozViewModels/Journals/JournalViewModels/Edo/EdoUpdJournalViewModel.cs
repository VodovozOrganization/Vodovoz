using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.ViewModels.Edo;
using Vodovoz.ViewModels.Journals.FilterViewModels.Edo;
using Vodovoz.ViewModels.Journals.JournalNodes.Edo;
using Vodovoz.ViewModels.Services.Edo;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo
{
	/// <summary>
	/// Журнал сохранённых УПД клиентских заказов и трансферов.
	/// </summary>
	public class EdoUpdJournalViewModel : JournalViewModelBase
	{
		private readonly EdoUpdFilterViewModel _filter;
		private readonly IEdoUpdDocumentService _documentService;

		/// <summary>Создаёт журнал УПД.</summary>
		/// <param name="unitOfWorkFactory">Фабрика контекстов данных.</param>
		/// <param name="interactiveService">Сервис сообщений пользователю.</param>
		/// <param name="navigationManager">Навигация.</param>
		/// <param name="filter">Фильтр журнала.</param>
		/// <param name="documentService">Сервис получения сохранённых УПД.</param>
		public EdoUpdJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService, INavigationManager navigationManager, EdoUpdFilterViewModel filter,
			IEdoUpdDocumentService documentService)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
			_documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
			Title = "Журнал ЭДО УПД";
			SearchEnabled = false;
			SelectionMode = JournalSelectionMode.Single;
			JournalFilter = _filter;
			_filter.IsShow = true;
			DataLoader = new ThreadSqlDataLoader<EdoUpdJournalNode>(unitOfWorkFactory, GetItems, GetItemsCount);
			_filter.OnFiltered += OnFiltered;
			UpdateOnChanges(typeof(EdoUpdDocument), typeof(DocumentEdoTask), typeof(TransferEdoTask));
			NodeActionsList.Clear();
			PopupActionsList.Clear();
			var open = new JournalAction("Открыть УПД", selected => selected.Count() == 1, selected => true,
				selected => NavigationManager.OpenViewModel<EdoUpdDocumentViewModel, int>(this,
					((EdoUpdJournalNode)selected.Single()).Id));
			RowActivatedAction = open;
			NodeActionsList.Add(open);
			PopupActionsList.Add(open);
		}

		private IList<EdoUpdJournalNode> GetItems(IUnitOfWork uow, int? offset, int? limit)
			=> _documentService.GetItems(uow, GetFilter(), offset, limit);

		private void OnFiltered(object sender, EventArgs e) => Refresh();

		/// <summary>Освобождает подписки журнала.</summary>
		public override void Dispose()
		{
			_filter.OnFiltered -= OnFiltered;
			base.Dispose();
		}

		private int GetItemsCount(IUnitOfWork uow)
			=> _documentService.GetItemsCount(uow, GetFilter());

		private EdoUpdJournalFilter GetFilter() => new EdoUpdJournalFilter
		{
			DateFrom = _filter.DateFrom,
			DateTo = _filter.DateTo,
			DocumentType = _filter.DocumentType,
			CustomerInn = _filter.CustomerInn
		};
	}
}
