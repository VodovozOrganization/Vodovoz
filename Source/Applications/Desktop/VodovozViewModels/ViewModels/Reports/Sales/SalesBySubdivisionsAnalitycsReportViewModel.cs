using ClosedXML.Report;
using DateTimeHelpers;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public class SalesBySubdivisionsAnalitycsReportViewModel : DialogTabViewModelBase,
		IClosedXmlAsyncReportViewModel<SalesBySubdivisionsAnalitycsReport>
	{
		private const string _templatePath = @".\Reports\Sales\SalesBySubdivisionsAnalitycsReport.xlsx";

		private readonly IUnitOfWork _unitOfWork;
		private readonly IInteractiveService _interactiveService;
		private bool _isSaving;
		private bool _canSave;
		private bool _isGenerating;
		private bool _canCancelGenerate;

		private IEnumerable<string> _lastGenerationErrors = Enumerable.Empty<string>();
		private SalesBySubdivisionsAnalitycsReport _report;
		private bool _splitByNomenclatures;
		private bool _splitBySubdivisions;
		private bool _splitByWarehouses;

		public SalesBySubdivisionsAnalitycsReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			TabName = "Аналитика продаж КБ";

			_unitOfWork = UnitOfWorkFactory.CreateWithoutRoot();
			_unitOfWork.Session.DefaultReadOnly = true;

			FirstPeriod = new DateTimePeriod();
			SecondPeriod = new DateTimePeriod();
		}

		public DateTimePeriod FirstPeriod { get; }

		public DateTimePeriod SecondPeriod { get; }

		public bool SplitByNomenclatures
		{
			get => _splitByNomenclatures;
			set => SetField(ref _splitByNomenclatures, value);
		}

		public bool SplitBySubdivisions
		{
			get => _splitBySubdivisions;
			set => SetField(ref _splitBySubdivisions, value);
		}

		public bool SplitByWarehouses
		{
			get => _splitByWarehouses;
			set => SetField(ref _splitByWarehouses, value);
		}

		#region Reporting properties

		public CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }

		public SalesBySubdivisionsAnalitycsReport Report
		{
			get => _report;
			set => SetField(ref _report, value);
		}

		public bool CanSave
		{
			get => _canSave;
			set => SetField(ref _canSave, value);
		}

		public bool IsSaving
		{
			get => _isSaving;
			set
			{
				SetField(ref _isSaving, value);
				CanSave = !IsSaving;
			}
		}

		public bool CanGenerate => !IsGenerating;

		public bool CanCancelGenerate
		{
			get => _canCancelGenerate;
			set => SetField(ref _canCancelGenerate, value);
		}

		public bool IsGenerating
		{
			get => _isGenerating;
			set
			{
				SetField(ref _isGenerating, value);
				OnPropertyChanged(nameof(CanGenerate));
				CanCancelGenerate = IsGenerating;
			}
		}

		public IEnumerable<string> LastGenerationErrors
		{
			get => _lastGenerationErrors;
			set => SetField(ref _lastGenerationErrors, value);
		}

		#endregion

		public void ShowWarning(string message)
		{
			_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
		}

		public async Task<SalesBySubdivisionsAnalitycsReport> GenerateReport(CancellationToken cancellationToken)
		{
			try
			{
				var report = await Generate(cancellationToken);
				return report;
			}
			finally
			{
				UoW.Session.Clear();
			}
		}

		public async Task<SalesBySubdivisionsAnalitycsReport> Generate(CancellationToken cancellationToken)
		{
			return await Task.Run(() =>
			{
				return SalesBySubdivisionsAnalitycsReport.Create(
					FirstPeriod,
					SecondPeriod,
					SplitByNomenclatures,
					SplitBySubdivisions,
					SplitByWarehouses,
					GetData);
			}, cancellationToken);
		}

		private IEnumerable<SalesBySubdivisionsAnalitycsReport.DataNode> GetData(
			DateTimePeriod firstPeriod,
			DateTimePeriod secondPeriod,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses)
		{
			SalesBySubdivisionsAnalitycsReport.DataNode resultItemAlias = null;

			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			ProductGroup productGroupAlias = null;
			Order orderAlias = null;
			Employee authorAlias = null;
			Subdivision subdivisionAlias = null;

			var query = _unitOfWork.Session.QueryOver(() => orderItemAlias);

			query.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => nomenclatureAlias.ProductGroup, () => productGroupAlias)
				.Left.JoinAlias(() => orderItemAlias.Order, () => orderAlias)
				.Left.JoinAlias(() => orderAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => authorAlias.Subdivision, () => subdivisionAlias);

			return query.SelectList(list =>
				list.Select(() => nomenclatureAlias.Id).WithAlias(() => resultItemAlias.nomenclatureId)
					.Select(() => nomenclatureAlias.OfficialName).WithAlias(() => resultItemAlias.nomenclatureName)
					.Select(() => productGroupAlias.Id).WithAlias(() => resultItemAlias.productGroupId)
					.Select(() => productGroupAlias.Name).WithAlias(() => resultItemAlias.productGroupName)
					.Select(() => subdivisionAlias.Id).WithAlias(() => resultItemAlias.subdivisionId)
					.Select(() => subdivisionAlias.ShortName).WithAlias(() => resultItemAlias.subdivisionName))
				.TransformUsing(Transformers.AliasToBean<SalesBySubdivisionsAnalitycsReport.DataNode>())
				.ReadOnly()
				.List<SalesBySubdivisionsAnalitycsReport.DataNode>();
		}

		public void ExportReport(string path)
		{
			string templatePath = GetTemplatePath();

			var template = new XLTemplate(templatePath);

			template.AddVariable(Report);
			template.Generate();

			template.SaveAs(path);
		}

		private string GetTemplatePath()
		{
			return _templatePath;
		}

		public override void Dispose()
		{
			ReportGenerationCancelationTokenSource?.Dispose();
			base.Dispose();
		}
	}
}
