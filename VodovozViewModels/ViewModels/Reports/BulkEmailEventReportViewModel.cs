using ClosedXML.Report;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ViewModels.Reports
{
	public class BulkEmailEventReportViewModel : DialogTabViewModelBase
	{
		private const string _templatePath = @".\Reports\Client\BulkEmailEventReport.xlsx";
		private readonly IFileDialogService _fileDialogService;
		private DelegateCommand _generateCommand;
		private DelegateCommand _exportCommand;
		private BulkEmailEventReport _report;

		public BulkEmailEventReportViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService,
			INavigationManager navigation, IFileDialogService fileDialogService, IBulkEmailEventReasonJournalFactory bulkEmailEventReasonJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			BulkEmailEventReasonSelectorFactory =
				(bulkEmailEventReasonJournalFactory ?? throw new ArgumentNullException(nameof(bulkEmailEventReasonJournalFactory)))
				.CreateBulkEmailEventReasonAutocompleteSelectorFactory();

			CounterpartySelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();


			Title = "Отчёт о событиях рассылки";

			EventActionTimeFrom = DateTime.Now.Date;
			EventActionTimeTo = DateTime.Now.Date.Add(new TimeSpan(0, 23, 59, 59));
		}

		private IList<BulkEmailEventReportRow> GenerateReportRows()
		{
			if(!IsHasDates)
			{
				return new List<BulkEmailEventReportRow>();
			}

			BulkEmailEvent bulkEmailEventAlias = null;
			BulkEmailEventReason bulkEmailEventReasonAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			Phone phoneAlias = null;
			Email emailAlias = null;
			BulkEmailEventReportRow resultAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => bulkEmailEventAlias)
				.JoinAlias(() => bulkEmailEventAlias.Counterparty, () => counterpartyAlias)
				.JoinAlias(() => bulkEmailEventAlias.BulkEmailEventReason, () => bulkEmailEventReasonAlias)
				.Where(() => bulkEmailEventAlias.ActionTime >= EventActionTimeFrom.Value.Date
				             && bulkEmailEventAlias.ActionTime <= EventActionTimeTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));

			if(Counterparty != null)
			{
				itemsQuery.Where(() => counterpartyAlias.Id == Counterparty.Id);
			}

			if(BulkEmailEventReason != null)
			{
				itemsQuery.Where(() => bulkEmailEventReasonAlias.Id == BulkEmailEventReason.Id);
			}

			var phoneSubquery = QueryOver.Of(() => phoneAlias)
				.Where(() => phoneAlias.Counterparty.Id == counterpartyAlias.Id)
				.OrderBy(() => phoneAlias.Id).Desc
				.SelectList(list => list
					.Select(() => phoneAlias.Number))
				.Take(1);

			var emailSubquery = QueryOver.Of(() => emailAlias)
				.Where(() => emailAlias.Counterparty.Id == counterpartyAlias.Id)
				.OrderBy(() => emailAlias.Id).Desc
				.SelectList(list => list
					.Select(() => emailAlias.Address))
				.Take(1);

			return itemsQuery
				.SelectList(list => list
					.Select(() => bulkEmailEventAlias.ActionTime).WithAlias(() => resultAlias.ActionDateTime)
					.Select(() => bulkEmailEventAlias.Type).WithAlias(() => resultAlias.BulkEmailEventType)
					.Select(() => bulkEmailEventReasonAlias.Name).WithAlias(() => resultAlias.Reason)
					.Select(() => bulkEmailEventAlias.OtherReason).WithAlias(() => resultAlias.OtherReason)
					.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.SelectSubQuery(phoneSubquery).WithAlias(() => resultAlias.Phone)
					.SelectSubQuery(emailSubquery).WithAlias(() => resultAlias.Email)
				).OrderBy(() => bulkEmailEventAlias.ActionTime).Desc
				.TransformUsing(Transformers.AliasToBean<BulkEmailEventReportRow>())
				.List<BulkEmailEventReportRow>();
		}

		private string GenerateSelectedFiltersString()
		{
			var selectedFilters = new StringBuilder().AppendLine("Выбранные фильтры:");
			
			if(EventActionTimeFrom != null && EventActionTimeTo != null)
			{
				selectedFilters.AppendLine(
					$"Время события: с {EventActionTimeFrom.Value.ToShortDateString()} по {EventActionTimeTo.Value.ToShortDateString()}; ");
			}

			if(Counterparty != null)
			{
				selectedFilters.AppendLine($"Контрагент: {Counterparty.Name}; ");
			}
			if(BulkEmailEventReason != null)
			{
				selectedFilters.AppendLine($"Причина: {BulkEmailEventReason.Name}; ");
			}

			return selectedFilters.ToString();
		}


		#region Commands

		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
				() =>
				{
					var dialogSettings = new DialogSettings();
					dialogSettings.Title = "Сохранить";
					dialogSettings.DefaultFileExtention = ".xlsx";
					dialogSettings.FileName = $"Отчёт о событиях рассылки {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

					var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
					if(result.Successful)
					{
						var template = new XLTemplate(_templatePath);
						template.AddVariable(Report);
						template.Generate();
						template.SaveAs(result.Path);
					}
				},
				() => true)
			);

		public DelegateCommand GenerateCommand => _generateCommand ?? (_generateCommand = new DelegateCommand(
				() =>
				{
					Report = new BulkEmailEventReport
					{
						Rows = GenerateReportRows(),
						SelectedFilters = GenerateSelectedFiltersString()
					};
				},
				() => true)
			);

		#endregion

		public BulkEmailEventReport Report { get; set; }
		public DateTime? EventActionTimeFrom { get; set; }
		public DateTime? EventActionTimeTo { get; set; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory BulkEmailEventReasonSelectorFactory { get; }
		public Domain.Client.Counterparty Counterparty { get; set; }
		public BulkEmailEventReason BulkEmailEventReason { get; set; }
		public bool IsHasDates => EventActionTimeFrom != null && EventActionTimeTo != null;
	}

	public class BulkEmailEventReport
	{
		public IList<BulkEmailEventReportRow> Rows { get; set; }
		public string SelectedFilters { get; set; }
	}

	public class BulkEmailEventReportRow
	{
		public DateTime ActionDateTime { get; set; }
		public int CounterpartyId { get; set; }
		public string CounterpartyName { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public string ActionDatetimeString => ActionDateTime.ToString("g");
		public BulkEmailEvent.BulkEmailEventType BulkEmailEventType { get; set; }
		public string BulkEmailEventTypeString => Gamma.Utilities.AttributeUtil.GetEnumTitle(BulkEmailEventType);
		public string Reason { get; set; }
		public string OtherReason { get; set; }

		public string FullReasonString
		{
			get
			{
				var reason = Reason;

				if(!string.IsNullOrWhiteSpace(OtherReason))
				{
					reason += $": {OtherReason}";
				}

				return reason;
			}
		}

		public string SelectedFilters { get; set; }
	}
}

