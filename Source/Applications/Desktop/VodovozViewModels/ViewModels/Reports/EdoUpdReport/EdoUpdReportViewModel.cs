using ClosedXML.Report;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport
{
	public partial class EdoUpdReportViewModel : DialogTabViewModelBase
	{
		private const string _templatePath = @".\Reports\Orders\EdoUpdReport.xlsx";
		private readonly IFileDialogService _fileDialogService;
		private DelegateCommand _generateCommand;
		private DelegateCommand _exportCommand;
		private readonly IInteractiveService _interactiveService;
		private bool _isRunning;

		public EdoUpdReportViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService,
			INavigationManager navigation, IFileDialogService fileDialogService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			Title = "Отчёт об УПД, не отражённых в ЧЗ";

			DateFrom = DateTime.Now.Date;
			DateTo = DateTime.Now.Date.Add(new TimeSpan(0, 23, 59, 59));

			Organizations = UoW.Session.QueryOver<Organization>().List();
			Organization = Organizations.FirstOrDefault();
		}

		private IList<EdoUpdReportRow> GenerateReportRows()
		{
			if(!HasDates || Organization == null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не все фильтры выбраны!");
				return new List<EdoUpdReportRow>();
			}

			Domain.Client.Counterparty counterpartyAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			TrueMarkApiDocument trueMarkApiDocumentAlias = null;
			EdoContainer edoContainerAlias = null;
			EdoUpdReportRow resultAlias = null;

			var orderStatuses = new[] { OrderStatus.OnTheWay, OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };
			var edoDocFlowStatuses = new[] { EdoDocFlowStatus.Succeed, EdoDocFlowStatus.CompletedWithDivergences };

			var query = UoW.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
				.JoinEntityAlias(() => orderItemAlias, () => orderAlias.Id == orderItemAlias.Order.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinEntityAlias(() => trueMarkApiDocumentAlias, () => orderAlias.Id == trueMarkApiDocumentAlias.Order.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => edoContainerAlias, () => orderAlias.Id == edoContainerAlias.Order.Id, JoinType.LeftOuterJoin);

			query
				.Where(() => orderAlias.DeliveryDate >= DateFrom && orderAlias.DeliveryDate <= DateTo)
				.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(orderStatuses)
				.And(() => counterpartyAlias.PersonType == PersonType.legal)
				.And(() => nomenclatureAlias.IsAccountableInChestniyZnak)
				.And(() => nomenclatureAlias.Gtin != null)
				.And(() => counterpartyContractAlias.Organization.Id == Organization.Id)
				.And(() => counterpartyAlias.OrderStatusForSendingUpd != OrderStatusForSendingUpd.Delivered
				           || orderAlias.OrderStatus != OrderStatus.OnTheWay)
				.And(Restrictions.Disjunction()
					.Add(Restrictions.Conjunction()
						.Add(() => counterpartyAlias.PersonType == PersonType.legal)
						.Add(() => orderAlias.PaymentType == PaymentType.cashless)
					)
					.Add(Restrictions.Conjunction()
						.Add(() => orderAlias.PaymentType == PaymentType.barter)
						.Add(Restrictions.Gt(Projections.Property(() => counterpartyAlias.INN), 0))
					)
				)
				.And(() => orderAlias.PaymentType != PaymentType.ContractDoc);

			switch(ReportType)
			{
				case EdoUpdReportType.Successfull:
					query.Where(Restrictions.Disjunction()
						.Add(() => trueMarkApiDocumentAlias.IsSuccess)
						.Add(Restrictions.On(() => edoContainerAlias.EdoDocFlowStatus).IsIn(edoDocFlowStatuses)));
					break;
				case EdoUpdReportType.Missing:
					query.Where(Restrictions.Conjunction()
						.Add(Restrictions.Disjunction()
							.Add(Restrictions.IsNull(Projections.Property(() => trueMarkApiDocumentAlias.Id)))
							.Add(() => !trueMarkApiDocumentAlias.IsSuccess))
						.Add(Restrictions.Disjunction()
							.Add(Restrictions.IsNull(Projections.Property(() => edoContainerAlias.Id)))
							.Add(Restrictions.On(() => edoContainerAlias.EdoDocFlowStatus).Not.IsIn(edoDocFlowStatuses))));
					break;
			}

			var result = query
				.SelectList(list => list
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.UpdDate)
					.Select(() => nomenclatureAlias.Gtin).WithAlias(() => resultAlias.Gtin)
					.Select(() => orderItemAlias.Price).WithAlias(() => resultAlias.Price)
					.Select(() => orderItemAlias.Count).WithAlias(() => resultAlias.Count)
					.Select(() => edoContainerAlias.EdoDocFlowStatus).WithAlias(() => resultAlias.EdoDocFlowStatus)
					.Select(() => edoContainerAlias.ErrorDescription).WithAlias(() => resultAlias.EdoDocError)
					.Select(() => trueMarkApiDocumentAlias.IsSuccess).WithAlias(() => resultAlias.IsTrueMarkApiSuccess)
					.Select(() => trueMarkApiDocumentAlias.ErrorMessage).WithAlias(() => resultAlias.TrueMarkApiError)
				)
				.TransformUsing(Transformers.AliasToBean<EdoUpdReportRow>())
				.List<EdoUpdReportRow>();

			return result;
		}

		#region Commands

		public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(
				() =>
				{
					var dialogSettings = new DialogSettings();
					dialogSettings.Title = "Сохранить";
					dialogSettings.DefaultFileExtention = ".xlsx";
					dialogSettings.FileName = $"Отчёт об УПД, не отраженным в ЧЗ. {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

					var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
					if(result.Successful)
					{
						var template = new XLTemplate(_templatePath);
						template.AddVariable(Report);
						template.Generate();
						template.SaveAs(result.Path);
						_interactiveService.ShowMessage(ImportanceLevel.Info, "Экспорт отчёта в Excel завершён");
					}
				},
				() => true)
			);

		public DelegateCommand GenerateCommand => _generateCommand ?? (_generateCommand = new DelegateCommand(
				() =>
				{
					IsRunning = true;
					Report = new EdoUpdReport
					{
						Rows = GenerateReportRows()
					};
					IsRunning = false;
				},
				() => true)
			);

		#endregion

		public EdoUpdReport Report { get; set; }
		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }
		public bool HasDates => DateFrom != null && DateTo != null;
		public EdoUpdReportType ReportType { get; set; }
		public Organization Organization { get; set; }
		public IList<Organization> Organizations { get; set; }
		public bool IsRunning
		{
			get => _isRunning;
			set => SetField(ref _isRunning, value);
		}
	}
}

