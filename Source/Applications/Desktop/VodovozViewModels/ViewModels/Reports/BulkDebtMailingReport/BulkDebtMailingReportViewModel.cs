using Autofac;
using ClosedXML.Report;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Reports.BulkDebtMailingReport
{
	public class BulkDebtMailingReportViewModel : DialogTabViewModelBase
	{
		private const string _templateReportPath = @".\Reports\Client\BulkDebtMailingReport.xlsx";
		private const string _templateSummaryReportPath = @".\Reports\Client\BulkDebtMailingSummaryReport.xlsx";
		private readonly IFileDialogService _fileDialogService;
		private bool _isReportSelected;
		private bool _isSummaryReportSelected;
		private DelegateCommand _generateCommand;
		private DelegateCommand _exportCommand;
		private DelegateCommand _generateSummaryCommand;
		private DelegateCommand _exportSummaryCommand;
		private readonly CounterpartyEmailType[] _emailTypes = new[]
		{
			CounterpartyEmailType.Bulk,
			CounterpartyEmailType.GeneralBillDocument,
			CounterpartyEmailType.ClosingDeliveries,
			CounterpartyEmailType.LetterOfClaim,
			CounterpartyEmailType.InformationLetter
		};
		
		public BulkDebtMailingReportViewModel(
			ILifetimeScope lifetimeScope,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			ICounterpartyJournalFactory counterpartyJournalFactory
			): base(unitOfWorkFactory, interactiveService, navigation)
		{
			var _ = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));

			CounterpartySelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);

			Title = "Отчет по рассылкам писем";

			EventActionTimeFrom = DateTime.Now.Date;
			EventActionTimeTo = DateTime.Now.Date.Add(new TimeSpan(0, 23, 59, 59));
		}

		public BulkDebtMailingReport Report { get; set; }

		public BulkDebtMailingSummaryReport SummaryReport { get; set; }

		public DateTime? EventActionTimeFrom { get; set; }

		public DateTime? EventActionTimeTo { get; set; }

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public Domain.Client.Counterparty Counterparty { get; set; }

		public bool HasDates => EventActionTimeFrom != null && EventActionTimeTo != null;

		public bool IsSummaryReportSelected
		{
			get => _isSummaryReportSelected;
			set
			{
				if(SetField(ref _isSummaryReportSelected, value) && value)
				{
					_isReportSelected = false;
					OnPropertyChanged(nameof(IsReportSelected));
				}
			}
		}

		public bool IsReportSelected
		{
			get => _isReportSelected;
			set
			{
				if(SetField(ref _isReportSelected, value) && value)
				{
					_isSummaryReportSelected = false;
					OnPropertyChanged(nameof(IsSummaryReportSelected));
				}
			}
		}

		private IList<BulkDebtMailingReportRow> GenerateReportRows()
		{
			if(!HasDates)
			{
				return new List<BulkDebtMailingReportRow>();
			}

			CounterpartyEmail counterpartyEmailAlias = null;
			StoredEmail storedEmailAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			Phone phoneAlias = null;
			BulkDebtMailingReportRow resultAlias = null;


			var itemsQuery = UoW.Session.QueryOver(() => counterpartyEmailAlias)
				.Left.JoinAlias(() => counterpartyEmailAlias.StoredEmail, () => storedEmailAlias)
				.Left.JoinAlias(() => counterpartyEmailAlias.Counterparty, () => counterpartyAlias)
				.WhereRestrictionOn(() => counterpartyEmailAlias.Type).IsIn(_emailTypes)
				.Where(() => storedEmailAlias.SendDate >= EventActionTimeFrom.Value.Date)
				.Where(() => storedEmailAlias.SendDate <= EventActionTimeTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));

			if(Counterparty != null)
			{
				itemsQuery.Where(() => counterpartyAlias.Id == Counterparty.Id);
			}

			var phoneSubquery = QueryOver.Of(() => phoneAlias)
				.Where(() => phoneAlias.Counterparty.Id == counterpartyAlias.Id)
				.And(() => !phoneAlias.IsArchive)
				.OrderBy(() => phoneAlias.Id).Desc
				.Select(Projections.Property(() => phoneAlias.Number))
				.Take(1);

			var result = itemsQuery
				.SelectList(list => list
					.Select(() => storedEmailAlias.SendDate).WithAlias(() => resultAlias.ActionDateTime)
					.Select(() => storedEmailAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => counterpartyEmailAlias.Type).WithAlias(() => resultAlias.EmailType)
					.Select(() => storedEmailAlias.RecipientAddress).WithAlias(() => resultAlias.Email)
					.SelectSubQuery(phoneSubquery).WithAlias(() => resultAlias.Phone)
				).OrderBy(() => storedEmailAlias.SendDate).Desc
				.TransformUsing(Transformers.AliasToBean<BulkDebtMailingReportRow>())
				.List<BulkDebtMailingReportRow>();

			IsReportSelected = true;

			return result;
		}

		private IList<BulkDebtMailingSummaryReportRow> GenerateSummaryReportRows()
		{
			if(!HasDates)
			{
				return new List<BulkDebtMailingSummaryReportRow>();
			}

			CounterpartyEmail counterpartyEmailAlias = null;
			StoredEmail storedEmailAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;
			BulkDebtMailingSummaryReportRow resultAlias = null;

			var itemsQuery = UoW.Session.QueryOver(() => counterpartyEmailAlias)
				.Left.JoinAlias(() => counterpartyEmailAlias.StoredEmail, () => storedEmailAlias)
				.Left.JoinAlias(() => counterpartyEmailAlias.Counterparty, () => counterpartyAlias)
				.JoinEntityAlias(() => organizationAlias,
					() => organizationAlias.Id == counterpartyEmailAlias.OrganizationId,
					JoinType.LeftOuterJoin)
				.WhereRestrictionOn(() => counterpartyEmailAlias.Type).IsIn(_emailTypes)
				.Where(() => storedEmailAlias.SendDate >= EventActionTimeFrom.Value.Date)
				.Where(() => storedEmailAlias.SendDate <= EventActionTimeTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));

			if(Counterparty != null)
			{
				itemsQuery.Where(() => counterpartyAlias.Id == Counterparty.Id);
			}

			var result = itemsQuery
				.SelectList(list => list
					.SelectGroup(() => counterpartyEmailAlias.Type).WithAlias(() => resultAlias.EmailType)
					.SelectGroup(() => storedEmailAlias.State).WithAlias(() => resultAlias.State)
					.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.OrganizationName)
					.SelectCount(() => counterpartyEmailAlias.Id).WithAlias(() => resultAlias.Count)
				)
				.OrderBy(() => storedEmailAlias.SendDate).Desc
				.TransformUsing(Transformers.AliasToBean<BulkDebtMailingSummaryReportRow>())
				.List<BulkDebtMailingSummaryReportRow>();

			var finalResult = new List<BulkDebtMailingSummaryReportRow>();

			var groupsByTypeAndOrg = result.GroupBy(x => new { x.EmailType, x.OrganizationName });
			foreach(var group in groupsByTypeAndOrg.OrderBy(g => g.Key.EmailType).ThenBy(g => g.Key.OrganizationName))
			{
				finalResult.AddRange(group.OrderBy(x => x.State));

				var totalForGroup = group.Sum(x => x.Count);
				var totalRow = new BulkDebtMailingSummaryReportRow
				{
					EmailType = group.Key.EmailType,
					StateString = "Всего отправлено",
					OrganizationName = group.Key.OrganizationName,
					Count = totalForGroup
				};
				finalResult.Add(totalRow);
			}

			IsSummaryReportSelected = true;

			return finalResult;
		}

		private string GenerateSelectedFiltersString()
		{
			var selectedFilters = new StringBuilder("Выбранные фильтры:");
			
			if(EventActionTimeFrom != null && EventActionTimeTo != null)
			{
				selectedFilters.Append(
					$"Время события: с {EventActionTimeFrom.Value.ToShortDateString()} по {EventActionTimeTo.Value.ToShortDateString()}; ");
			}

			if(Counterparty != null)
			{
				selectedFilters.Append($"Контрагент: {Counterparty.Name}; ");
			}

			return selectedFilters.ToString();
		}

		#region Commands

		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
				() =>
				{
					if(Report == null && SummaryReport == null)
					{
						base.ShowWarningMessage("Сначала сгенерируйте отчет", 
							"Предупреждение");
						return;
					}

					var dialogSettings = new DialogSettings
					{
						Title = "Сохранить",
						DefaultFileExtention = ".xlsx",
						FileName = IsSummaryReportSelected
							? $"Сводный отчет о рассылке писем о задолженности {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
							: $"Отчет о рассылке писем о задолженности {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
					};

					var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
					if(result.Successful)
					{
						if(IsSummaryReportSelected)
						{
							var template = new XLTemplate(_templateSummaryReportPath);
							template.AddVariable(SummaryReport);
							template.Generate();
							template.SaveAs(result.Path);
						}
						else
						{
							var template = new XLTemplate(_templateReportPath);
							template.AddVariable(Report);
							template.Generate();
							template.SaveAs(result.Path);
						}
					}
				})
			);

		public DelegateCommand GenerateCommand => _generateCommand ?? (_generateCommand = new DelegateCommand(
				() =>
				{
					if(!HasDates)
					{
						base.ShowWarningMessage("Для генерации отчета необходимо выбрать период",
							"Предупреждение");
						return;
					}

					Report = new BulkDebtMailingReport
					{
						Rows = GenerateReportRows(),
						SelectedFilters = GenerateSelectedFiltersString()
					};
				})
			);

		public DelegateCommand GenerateSummaryCommand => _generateSummaryCommand ?? (_generateSummaryCommand = new DelegateCommand(
				() =>
				{
					if(!HasDates)
					{
						base.ShowWarningMessage("Для генерации отчета необходимо выбрать период",
							"Предупреждение");
						return;
					}

					SummaryReport = new BulkDebtMailingSummaryReport
					{
						Rows = GenerateSummaryReportRows(),
						SelectedFilters = GenerateSelectedFiltersString()
					};
				})
			);

		#endregion
	}
}

