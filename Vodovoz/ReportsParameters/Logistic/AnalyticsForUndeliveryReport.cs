using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalyticsForUndeliveryReport : SingleUoWWidgetBase, IParametersWidget
	{
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository = new UndeliveredOrdersRepository();
		private List<string> listOfGuilties = new List<string>();
		private int itogLO = 0;
		private string titleDate;
		
		//FEDOS
		public AnalyticsForUndeliveryReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ConfigureDlg();
		}
		
		void ConfigureDlg()
		{
			pkrDate.StartDate = pkrDate.EndDate = DateTime.Today;
		}

		#region IParametersWidget implementation
		
		public string Title => "Аналитика по недовозам";

		public event EventHandler<LoadReportEventArgs> LoadReport;
		
		#endregion
		
		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null)
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			GetGuilties();
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			//FEDOS можно в параметры тупа обжект передать, нужно это использовать
			return new ReportInfo {
				//ПОПРОБОВАТЬ ИСПОЛЬЗОВАТЬ IENUMERABLE!!!!!!!!!!!!!!!!!!!!!!!
				Identifier = "Logistic.AnalyticsForUndelivery",
				Parameters = new Dictionary<string, object> {
					{ "start_date", pkrDate.StartDate },
					{ "end_date", pkrDate.EndDate },
					{"itogLO",itogLO},
					{"guilties",listOfGuilties},
					{"title_date",titleDate}
				}
			};
		}
		
		public void GetGuilties(/*UndeliveredOrdersFilterViewModel filter*/)
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			UndeliveredOrder undeliveredOrderAlias = null;
			Domain.Orders.Order oldOrderAlias = null;
			Domain.Orders.Order newOrderAlias = null;
			Employee oldOrderAuthorAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint undeliveredOrderDeliveryPointAlias = null;
			Subdivision subdivisionAlias = null;
			GuiltyInUndelivery guiltyInUndeliveryAlias = null;
			Employee authorAlias = null;

			var subquery19LWatterQty = QueryOver.Of<OrderItem>(() => orderItemAlias)
												.Where(() => orderItemAlias.Order.Id == oldOrderAlias.Id)
												.Left.JoinQueryOver(i => i.Nomenclature, () => nomenclatureAlias)
												.Where(n => n.Category == NomenclatureCategory.water && n.TareVolume == TareVolume.Vol19L)
												.Select(Projections.Sum(() => orderItemAlias.Count));

			var query = UoW.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
				.Left.JoinAlias(u => u.OldOrder, () => oldOrderAlias)
				.Left.JoinAlias(u => u.NewOrder, () => newOrderAlias)
				.Left.JoinAlias(() => oldOrderAlias.Client, () => counterpartyAlias)
				.Left.JoinAlias(() => oldOrderAlias.Author, () => oldOrderAuthorAlias)
				.Left.JoinAlias(() => oldOrderAlias.DeliveryPoint, () => undeliveredOrderDeliveryPointAlias)
				.Left.JoinAlias(() => undeliveredOrderAlias.GuiltyInUndelivery, () => guiltyInUndeliveryAlias)
				.Left.JoinAlias(() => guiltyInUndeliveryAlias.GuiltyDepartment, () => subdivisionAlias)
				.Left.JoinAlias(u => u.Author, () => authorAlias);

			/*if(filter?.RestrictDriver != null) {
				var oldOrderIds = _undeliveredOrdersRepository.GetListOfUndeliveryIdsForDriver(UoW , filter.RestrictDriver);
				query.Where(() => oldOrderAlias.Id.IsIn(oldOrderIds.ToArray()));
			}

			if(filter?.RestrictOldOrder != null)
				query.Where(() => oldOrderAlias.Id == filter.RestrictOldOrder.Id);

			if(filter?.RestrictClient != null)
				query.Where(() => counterpartyAlias.Id == filter.RestrictClient.Id);

			if(filter?.RestrictAddress != null)
				query.Where(() => undeliveredOrderDeliveryPointAlias.Id == filter.RestrictAddress.Id);

			if(filter?.RestrictOldOrderAuthor != null)
				query.Where(() => oldOrderAuthorAlias.Id == filter.RestrictOldOrderAuthor.Id);

			if(filter?.RestrictOldOrderStartDate != null)
				query.Where(() => oldOrderAlias.DeliveryDate >= filter.RestrictOldOrderStartDate);

			if(filter?.RestrictOldOrderEndDate != null)
				query.Where(() => oldOrderAlias.DeliveryDate <= filter.RestrictOldOrderEndDate.Value.AddDays(1).AddTicks(-1));

			if(filter?.RestrictNewOrderStartDate != null)
				query.Where(() => newOrderAlias.DeliveryDate >= filter.RestrictNewOrderStartDate);

			if(filter?.RestrictNewOrderEndDate != null)
				query.Where(() => newOrderAlias.DeliveryDate <= filter.RestrictNewOrderEndDate.Value.AddDays(1).AddTicks(-1));

			if(filter?.RestrictGuiltySide != null)
				query.Where(() => guiltyInUndeliveryAlias.GuiltySide == filter.RestrictGuiltySide);

			if(filter != null && filter.RestrictIsProblematicCases)
				query.Where(() => !guiltyInUndeliveryAlias.GuiltySide.IsIn(filter.ExcludingGuiltiesForProblematicCases));

			if(filter?.RestrictGuiltyDepartment != null)
				query.Where(() => subdivisionAlias.Id == filter.RestrictGuiltyDepartment.Id);

			if(filter?.RestrictInProcessAtDepartment != null)
				query.Where(u => u.InProcessAtDepartment.Id == filter.RestrictInProcessAtDepartment.Id);

			if(filter?.NewInvoiceCreated != null) {
				if(filter.NewInvoiceCreated.Value)
					query.Where(u => u.NewOrder != null);
				else
					query.Where(u => u.NewOrder == null);
			}

			if(filter?.RestrictUndeliveryStatus != null)
				query.Where(u => u.UndeliveryStatus == filter.RestrictUndeliveryStatus);

			if(filter?.RestrictUndeliveryAuthor != null)
				query.Where(u => u.Author == filter.RestrictUndeliveryAuthor);


			if(filter?.RestrictAuthorSubdivision != null)
			{
				query.Where(() => authorAlias.Subdivision.Id == filter.RestrictAuthorSubdivision.Id);
			}*/

			if(pkrDate == null)
			{
				ServicesConfig.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, "Не заполнена дата!");
			}
			else
			{
				query.Where(() => oldOrderAlias.DeliveryDate == pkrDate.StartDate);
				titleDate = pkrDate.StartDate.ToShortDateString();
			}

			if(pkrDate.EndDate != null && pkrDate.EndDate != pkrDate.StartDate)
			{
				titleDate = titleDate +" и на " + pkrDate.EndDate.ToShortDateString();
			}

			int position = 0;
			var result = 
				query.SelectList(list => list
					.SelectGroup(u => u.Id)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(
							NHibernateUtil.String,
							"GROUP_CONCAT(" +
							"CASE ?1 " +
							$"WHEN '{nameof(GuiltyTypes.Department)}' THEN IFNULL(CONCAT('Отд: ', ?2), 'Отдел ВВ') " +
							$"WHEN '{nameof(GuiltyTypes.Client)}' THEN 'Клиент' " +
							$"WHEN '{nameof(GuiltyTypes.Driver)}' THEN 'Водитель' " +
							$"WHEN '{nameof(GuiltyTypes.ServiceMan)}' THEN 'Мастер СЦ' " +
							$"WHEN '{nameof(GuiltyTypes.ForceMajor)}' THEN 'Форс-мажор' " +
							$"WHEN '{nameof(GuiltyTypes.None)}' THEN 'Нет (не недовоз)' " +
							$"WHEN '{nameof(GuiltyTypes.Unknown)}' THEN 'Неизвестно' " +
							"ELSE ?1 " +
							"END ORDER BY ?1 ASC SEPARATOR '\n')"
						 ),
						NHibernateUtil.String,
						Projections.Property(() => guiltyInUndeliveryAlias.GuiltySide),
						Projections.Property(() => subdivisionAlias.ShortName)))
					.SelectSubQuery(subquery19LWatterQty))
				.List<object[]>()
				.GroupBy(x => x[1])
				.Select(r => new[] { r.Key, r.Count(), position++, r.Sum(x => x[2] == null ? 0 : (decimal)x[2]) })
				.ToList();
			//FEDOS Итог по ЛО - складываем результаты недовозов соф парнас и боксит
			itogLO = Convert.ToInt32(result[1][1]) + Convert.ToInt32(result[2][1]) + Convert.ToInt32(result[3][1]);
			object[] objLO = {"Итог по ЛО: ", itogLO, 0, (decimal)0 };
			
			result.Add(objLO);
			foreach(var obj in result)
			{
				int i = 0;
				listOfGuilties.Add(obj[0].ToString() +" "+ obj[1].ToString());
			}
			
		}
	}
}
