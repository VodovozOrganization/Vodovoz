using Autofac;
using ClosedXML.Excel;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.NHibernateProjections.Employees;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Widgets.Search;


namespace Vodovoz.Journals.JournalViewModels.Employees
{
	public class FinesJournalViewModel : EntityJournalViewModelBase<Fine, FineViewModel, FineJournalNode>
	{
		private readonly FineFilterViewModel _filterViewModel;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly CompositeAlgebraicSearchViewModel _compositeAlgebraicSearchViewModel;
		private readonly IFileDialogService _fileDialogService;

		public FinesJournalViewModel(
			FineFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			CompositeAlgebraicSearchViewModel compositeAlgebraicSearchViewModel,		
			IFileDialogService fileDialogService,
			Action<FineFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			if(filterViewModel is null)
			{
				throw new ArgumentNullException(nameof(filterViewModel));
			}

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_compositeAlgebraicSearchViewModel = compositeAlgebraicSearchViewModel ?? throw new ArgumentNullException(nameof(compositeAlgebraicSearchViewModel));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));


			Search = _compositeAlgebraicSearchViewModel;
			Search.OnSearch += OnFiltered;

			JournalFilter = filterViewModel;
			_filterViewModel = filterViewModel;
			filterViewModel.JournalViewModel = this;

			filterViewModel.OnFiltered += OnFiltered;

			if(filterConfig != null)
			{
				filterViewModel.ConfigureWithoutFiltering(filterConfig);
			}

			TabName = $"Журнал {typeof(Fine).GetClassUserFriendlyName().GenitivePlural}";
			UpdateOnChanges(typeof(Fine), typeof(FineItem));

			UseSlider = true;
		}

		private void OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public ILifetimeScope Scope => _lifetimeScope;

		private string GetTotalSumInfo()
		{
			var total = Items.Cast<FineJournalNode>().Sum(node => node.FineSum);
			return CurrencyWorks.GetShortCurrencyString(total);
		}

		public override string FooterInfo
		{
			get => $"Сумма отфильтрованных штрафов:{GetTotalSumInfo()}. {base.FooterInfo}";
			set { }
		}

		private new ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
			=> _compositeAlgebraicSearchViewModel.GetSearchCriterion(aliasPropertiesExpr);

		protected override IQueryOver<Fine> ItemsQuery(IUnitOfWork unitOfWork)
		{
			FineJournalNode resultAlias = null;
			Fine fineAlias = null;
			FineItem fineItemAlias = null;
			Employee finedEmployeeAlias = null;
			Subdivision finedEmployeeSubdivision = null;
			Employee fineAuthorAlias = null;
			RouteList routeListAlias = null;

			var query = unitOfWork.Session.QueryOver(() => fineAlias)
				.Left.JoinAlias(() => fineAlias.Author, () => fineAuthorAlias)
				.Left.JoinAlias(f => f.Items, () => fineItemAlias)
				.Left.JoinAlias(() => fineItemAlias.Employee, () => finedEmployeeAlias)
				.Left.JoinAlias(() => finedEmployeeAlias.Subdivision, () => finedEmployeeSubdivision)
				.Left.JoinAlias(f => f.RouteList, () => routeListAlias);

			if(_filterViewModel.Subdivision != null)
			{
				query.Where(() => finedEmployeeAlias.Subdivision.Id == _filterViewModel.Subdivision.Id);
			}

			if(_filterViewModel.Author != null)
			{
				query.Where(() => fineAuthorAlias.Id == _filterViewModel.Author.Id);
			}

			if(_filterViewModel.FineDateStart.HasValue)
			{
				query.Where(() => fineAlias.Date >= _filterViewModel.FineDateStart.Value);
			}

			if(_filterViewModel.FineDateEnd.HasValue)
			{
				query.Where(() => fineAlias.Date <= _filterViewModel.FineDateEnd.Value);
			}

			if(_filterViewModel.RouteListDateStart.HasValue)
			{
				query.Where(() => routeListAlias.Date >= _filterViewModel.RouteListDateStart.Value);
			}

			if(_filterViewModel.RouteListDateEnd.HasValue)
			{
				query.Where(() => routeListAlias.Date <= _filterViewModel.RouteListDateEnd.Value);
			}

			if(_filterViewModel.ExcludedIds != null && _filterViewModel.ExcludedIds.Any())
			{
				query.WhereRestrictionOn(() => fineAlias.Id).Not.IsIn(_filterViewModel.ExcludedIds);
			}

			if(_filterViewModel.FindFinesWithIds != null && _filterViewModel.FindFinesWithIds.Any())
			{
				query.WhereRestrictionOn(() => fineAlias.Id).IsIn(_filterViewModel.FindFinesWithIds);
			}

			if(_filterViewModel.SelectedFineCategoryIds != null)
			{
				query.WhereRestrictionOn(() => fineAlias.FineCategory).IsIn(_filterViewModel.SelectedFineCategoryIds);
			}	

			CarEvent carEventAlias = null;
			CarEventType carEventTypeAliase = null;
			Fine finesAlias = null;

			var carEventProjection = CustomProjections.Concat(
					Projections.Property(() => carEventAlias.Id),
					Projections.Constant(" - "),
					Projections.Property(() => carEventTypeAliase.ShortName));

			var carEventSubquery = QueryOver.Of<CarEvent>(() => carEventAlias)
				.JoinAlias(() => carEventAlias.Fines, () => finesAlias)
				.JoinAlias(() => carEventAlias.CarEventType, () => carEventTypeAliase)
				.Where(() => finesAlias.Id == fineAlias.Id)
				.Select(CustomProjections.GroupConcat(carEventProjection, separator: ", "));

			query.Where(GetSearchCriterion(
				() => fineAlias.Id,
				() => fineAlias.TotalMoney,
				() => fineAlias.FineReasonString,
				() => EmployeeProjections.FinedEmployeeFioProjection));

			return query
				.SelectList(list => list
					.SelectGroup(() => fineAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fineAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
							NHibernateUtil.String,
							Projections.Constant(" "),
							Projections.Property(() => finedEmployeeAlias.LastName),
							Projections.Property(() => finedEmployeeAlias.Name),
							Projections.Property(() => finedEmployeeAlias.Patronymic)
						),
						Projections.Constant("\n"))).WithAlias(() => resultAlias.FinedEmployeesNames)
					.Select(() => fineAlias.FineCategory.Name).WithAlias(() => resultAlias.FineCategoryName)
					.Select(() => fineAlias.TotalMoney).WithAlias(() => resultAlias.FineSum)
					.Select(() => fineAlias.FineReasonString).WithAlias(() => resultAlias.FineReason)
					.Select(Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
							NHibernateUtil.String,
							Projections.Constant(" "),
							Projections.Property(() => fineAuthorAlias.LastName),
							Projections.Property(() => fineAuthorAlias.Name),
							Projections.Property(() => fineAuthorAlias.Patronymic)
						)).WithAlias(() => resultAlias.AuthorName)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property(() => finedEmployeeSubdivision.Name),
						Projections.Constant("\n"))).WithAlias(() => resultAlias.FinedEmployeesSubdivisions)
					.SelectSubQuery(carEventSubquery).WithAlias(() => resultAlias.CarEvent)
				)
				.OrderBy(o => o.Date).Desc
				.OrderBy(o => o.Id).Desc
				.TransformUsing(Transformers.AliasToBean<FineJournalNode>());
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			base.CreateNodeActions();
			CreateXLExportAction();
		}

		private void CreateXLExportAction()
		{
			var xlExportAction = new JournalAction("Экспорт в Excel",
				(selected) => true,
				(selected) => true,
				(selected) =>
				{
					var journalNodes = ItemsQuery(UoW).List<FineJournalNode>();

					var rows = from row in journalNodes
							   select new
							   {
								   row.Id,
								   row.Date,
								   row.FinedEmployeesNames,
								   row.FineCategoryName,
								   row.FineSum,
								   row.FineReason,
								   row.AuthorName,
								   row.FinedEmployeesSubdivisions
							   };

					using(var wb = new XLWorkbook())
					{
						var sheetName = $"{DateTime.Now:dd.MM.yyyy}";
						var ws = wb.Worksheets.Add(sheetName);
						var columnNames = new List<string> { "Номер", "Дата", "Сотрудники", "Сумма штрафа", "Причина штрафа", "Автор штрафа", "Подразделения сотрудников" };
						var index = 1;

						foreach(var name in columnNames)
						{
							ws.Cell(1, index).Value = name;
							index++;
						}

						ws.Cell(2, 1).InsertData(rows);
						ws.Columns().AdjustToContents();

						var extension = ".xlsx";
						var dialogSettings = new DialogSettings
						{
							Title = "Сохранить",
							FileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}{extension}"
						};

						dialogSettings.FileFilters.Add(new DialogFileFilter("XLSX File (*.xlsx)", $"*{extension}"));
						var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
						if(result.Successful)
						{
							wb.SaveAs(result.Path);
						}						
					}
				}
			);

			NodeActionsList.Add(xlExportAction);
		}
	}
}
