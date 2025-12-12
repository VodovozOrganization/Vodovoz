using ClosedXML.Excel;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Services.FileDialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Goods;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport
{
	public partial class EdoUpdReportViewModel : DialogTabViewModelBase
	{
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
			TrueMarkDocument trueMarkApiDocumentAlias = null;
			EdoContainer edoContainerAlias = null;
			EdoUpdReportRow resultAlias = null;
			Gtin gtinAlias = null;
			PrimaryEdoRequest orderEdoRequestAlias = null;
			PrimaryEdoRequest orderEdoRequestAlias2 = null;
			OrderEdoDocument orderEdoDocumentAlias = null;
			OrderEdoDocument orderEdoDocumentAlias2 = null;

			var orderStatuses = new[] { OrderStatus.OnTheWay, OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };
			var edoDocFlowStatuses = new[] { EdoDocFlowStatus.Succeed, EdoDocFlowStatus.CompletedWithDivergences };

			var edoContainerMaxDateSubquery = QueryOver.Of(() => edoContainerAlias)
					.Where(() => edoContainerAlias.Order.Id == orderAlias.Id)
					.And(() => edoContainerAlias.Type == DocumentContainerType.Upd)
					.OrderBy(() => edoContainerAlias.Created).Desc
				.Select(Projections.Max(() => edoContainerAlias.Created))
				.Take(1);

			var edoUpdLastStatusNewDocflowSubquery = QueryOver.Of(() => orderEdoRequestAlias)
				.JoinEntityAlias(
					() => orderEdoRequestAlias2,
					() => orderEdoRequestAlias2.Order.Id == orderEdoRequestAlias.Order.Id
						&& orderEdoRequestAlias2.Id > orderEdoRequestAlias.Id,
					JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => orderEdoDocumentAlias, () => orderEdoRequestAlias.Task.Id == orderEdoDocumentAlias.DocumentTaskId)
				.JoinEntityAlias(
					() => orderEdoDocumentAlias2,
					() => orderEdoDocumentAlias2.DocumentTaskId == orderEdoDocumentAlias.DocumentTaskId
						&& orderEdoDocumentAlias2.Id > orderEdoDocumentAlias.Id,
					JoinType.LeftOuterJoin)
				.Where(() => orderEdoRequestAlias.Order.Id == orderAlias.Id)
				.And(() => orderEdoRequestAlias.DocumentType == EdoDocumentType.UPD)
				.And(() => orderEdoRequestAlias2.Id == null)
				.And(() => orderEdoDocumentAlias2.Id == null)
				.Select(Projections.Property(() => orderEdoDocumentAlias.Status))
				.Take(1);

			var trueApiMaxDateSubquery = QueryOver.Of(() => trueMarkApiDocumentAlias)
				.Where(() => trueMarkApiDocumentAlias.Order.Id == orderAlias.Id)
				.And(() => trueMarkApiDocumentAlias.Type == TrueMarkDocument.TrueMarkDocumentType.Withdrawal)
				.OrderBy(() => trueMarkApiDocumentAlias.CreationDate).Desc
				.Select(Projections.Max(() => trueMarkApiDocumentAlias.CreationDate))
				.Take(1);

			var gtinsProjection = CustomProjections.GroupConcat(
				() => gtinAlias.GtinNumber,
				separator: ", "
			);

			var gtinsSubquery = QueryOver.Of(() => gtinAlias)
				.Where(() => gtinAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Select(gtinsProjection);

			var query = UoW.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinAlias(() => orderAlias.Contract, () => counterpartyContractAlias)
				.JoinEntityAlias(() => orderItemAlias, () => orderAlias.Id == orderItemAlias.Order.Id, JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.JoinEntityAlias(() => trueMarkApiDocumentAlias, () => orderAlias.Id == trueMarkApiDocumentAlias.Order.Id, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => edoContainerAlias, () => orderAlias.Id == edoContainerAlias.Order.Id && edoContainerAlias.Type == DocumentContainerType.Upd, JoinType.LeftOuterJoin);

			Junction reportTypeRestriction = null;

			switch(ReportType)
			{
				case EdoUpdReportType.Successfull:
					reportTypeRestriction = Restrictions.Disjunction()
						.Add(() => trueMarkApiDocumentAlias.IsSuccess)
						.Add(Restrictions.On(() => edoContainerAlias.EdoDocFlowStatus).IsIn(edoDocFlowStatuses));
					break;

				case EdoUpdReportType.Missing:
					reportTypeRestriction = Restrictions.Conjunction()
						.Add(Restrictions.Disjunction()
							.Add(Restrictions.IsNull(Projections.Property(() => trueMarkApiDocumentAlias.Id)))
							.Add(() => !trueMarkApiDocumentAlias.IsSuccess))
						.Add(Restrictions.Disjunction()
							.Add(Restrictions.IsNull(Projections.Property(() => edoContainerAlias.Id)))
							.Add(Restrictions.On(() => edoContainerAlias.EdoDocFlowStatus).Not.IsIn(edoDocFlowStatuses)));
					break;
			}

			var lastDateRecordOnlyRestriction = Restrictions.Conjunction()
				.Add(Restrictions.Disjunction()
					.Add(Restrictions.IsNull(Projections.SubQuery(trueApiMaxDateSubquery)))
					.Add(Restrictions.EqProperty(Projections.SubQuery(trueApiMaxDateSubquery), Projections.Property(() => trueMarkApiDocumentAlias.CreationDate))))
				.Add(Restrictions.Disjunction()
					.Add(Restrictions.IsNull(Projections.SubQuery(edoContainerMaxDateSubquery)))
					.Add(Restrictions.EqProperty(Projections.SubQuery(edoContainerMaxDateSubquery), Projections.Property(() => edoContainerAlias.Created))));

			query
				.WhereRestrictionOn(() => orderAlias.OrderStatus).IsIn(orderStatuses)
				.Where(reportTypeRestriction)
				.And(lastDateRecordOnlyRestriction)
				.And(() => orderAlias.DeliveryDate >= DateFrom && orderAlias.DeliveryDate <= DateTo)
				.And(() => counterpartyAlias.PersonType == PersonType.legal)
				.And(() => nomenclatureAlias.IsAccountableInTrueMark)
				.And(Restrictions.IsNotNull(Projections.SubQuery(gtinsSubquery)))
				.And(() => counterpartyContractAlias.Organization.Id == Organization.Id)
				.And(() => counterpartyAlias.OrderStatusForSendingUpd != OrderStatusForSendingUpd.Delivered
						   || orderAlias.OrderStatus != OrderStatus.OnTheWay)
				.And(Restrictions.Disjunction()
					.Add(Restrictions.Conjunction()
						.Add(() => counterpartyAlias.PersonType == PersonType.legal)
						.Add(() => orderAlias.PaymentType == PaymentType.Cashless))
					.Add(Restrictions.Conjunction()
						.Add(() => orderAlias.PaymentType == PaymentType.Barter)
						.Add(Restrictions.Gt(Projections.Property(() => counterpartyAlias.INN), 0))))
				.And(() => orderAlias.PaymentType != PaymentType.ContractDocumentation);

			var result = query
				.SelectList(list => list
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.UpdDate)
					.SelectSubQuery(gtinsSubquery).WithAlias(() => resultAlias.Gtin)
					.Select(() => orderItemAlias.Price).WithAlias(() => resultAlias.Price)
					.Select(() => orderItemAlias.Count).WithAlias(() => resultAlias.Count)
					.Select(() => orderItemAlias.DiscountMoney).WithAlias(() => resultAlias.DiscountMoney)
					.Select(() => edoContainerAlias.EdoDocFlowStatus).WithAlias(() => resultAlias.EdoDocFlowStatus)
					.SelectSubQuery(edoUpdLastStatusNewDocflowSubquery).WithAlias(() => resultAlias.NewEdoDocFlowStatus)
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
					dialogSettings.FileName = $"Отчёт об УПД, не отраженных в ЧЗ {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

					var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

					if(result.Successful)
					{
						SaveReport(result.Path);

						_interactiveService.ShowMessage(ImportanceLevel.Info, "Экспорт отчёта в Excel завершён");
					}
				})
			);

		public DelegateCommand GenerateCommand => _generateCommand ?? (_generateCommand = new DelegateCommand(
				() =>
				{
					Report = new EdoUpdReport
					{
						Rows = GenerateReportRows()
					};
				})
			);

		#endregion

		private void SaveReport(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Отчёт об УПД");

				worksheet.Column(1).Width = 5;
				worksheet.Column(2).Width = 15;
				worksheet.Column(3).Width = 50;
				worksheet.Column(4).Width = 12;
				worksheet.Column(5).Width = 15;
				worksheet.Column(6).Width = 20;
				worksheet.Column(7).Width = 10;
				worksheet.Column(8).Width = 10;
				worksheet.Column(9).Width = 15;
				worksheet.Column(10).Width = 45;
				worksheet.Column(11).Width = 45;

				worksheet.Cell(1, 1).Value = "№";
				worksheet.Cell(1, 2).Value = "ИНН";
				worksheet.Cell(1, 3).Value = "Название контрагента";
				worksheet.Cell(1, 4).Value = "№ заказа";
				worksheet.Cell(1, 5).Value = "Дата";
				worksheet.Cell(1, 6).Value = "GTIN";
				worksheet.Cell(1, 7).Value = "Кол-во";
				worksheet.Cell(1, 8).Value = "Цена с НДС";
				worksheet.Cell(1, 9).Value = "Стоимость\nстроки с НДС";
				worksheet.Cell(1, 10).Value = "Статус УПД в ЭДО";
				worksheet.Cell(1, 11).Value = "Статус прямого вывода\nиз оборота в Честном Знаке";

				var rows = Report.Rows;

				for(int i = 0; i < rows.Count; i++)
				{
					worksheet.Cell(i + 2, 1).Value = i + 1;
					worksheet.Cell(i + 2, 2).Value = rows[i].Inn;
					worksheet.Cell(i + 2, 3).Value = rows[i].CounterpartyName;
					worksheet.Cell(i + 2, 4).Value = rows[i].OrderId;
					worksheet.Cell(i + 2, 5).Value = rows[i].UpdDate;
					worksheet.Cell(i + 2, 6).Value = rows[i].Gtin;
					worksheet.Cell(i + 2, 7).Value = rows[i].Count;
					worksheet.Cell(i + 2, 8).Value = rows[i].Price;
					worksheet.Cell(i + 2, 9).Value = rows[i].Sum;
					worksheet.Cell(i + 2, 10).Value = rows[i].EdoDocFlowStatusString;
					worksheet.Cell(i + 2, 11).Value = rows[i].TrueMarkApiStatusString;
				}

				worksheet.Column(2).CellsUsed().SetDataType(XLDataType.Text);
				worksheet.Column(6).CellsUsed().SetDataType(XLDataType.Text);

				for(int c = 1; c <= 7; c++)
				{
					for(int r = 1; r <= rows.Count + 1; r++)
					{
						worksheet.Cell(r, c).Style.Alignment.WrapText = true;
					}
				}

				workbook.SaveAs(path);
			}
		}

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

