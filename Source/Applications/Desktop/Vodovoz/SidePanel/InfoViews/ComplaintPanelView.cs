using System;
using System.Collections.Generic;
using System.Linq;
using DateTimeHelpers;
using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.Extensions;
using Vodovoz.FilterViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Complaints;
using Vodovoz.Settings.Database.Complaints;
using Vodovoz.SidePanel.InfoProviders;
using static Vodovoz.FilterViewModels.ComplaintFilterViewModel;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintPanelView : Bin, IPanelView
	{
		private readonly IComplaintsRepository complaintsRepository;
		private readonly IComplaintResultsRepository _complaintResultsRepository;
		private readonly IComplaintSettings _complaintSettings;
		private readonly Gdk.Color _primaryBg = GdkColors.PrimaryBase;
		private readonly Gdk.Color _secondaryBg = GdkColors.PrimaryBG;
		private readonly Gdk.Color _red = GdkColors.DangerText;
		private readonly string _primaryTextHtmlColor = GdkColors.PrimaryText.ToHtmlColor();
		private readonly string _redTextHtmlColor = GdkColors.DangerText.ToHtmlColor();

		public ComplaintPanelView(IComplaintsRepository complaintsRepository, IComplaintResultsRepository complaintResultsRepository, IComplaintSettings complaintSettings)
		{
			this.complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_complaintResultsRepository = complaintResultsRepository ?? throw new ArgumentNullException(nameof(complaintResultsRepository));
			_complaintSettings = complaintSettings ?? throw new ArgumentNullException(nameof(complaintSettings));

			Build();
			ConfigureWidget();
		}

		#region Widget

		void ConfigureWidget()
		{
			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<object>()
				.AddColumn("Ответственный")
					.AddTextRenderer(n => GetNodeText(n))
					.AddSetter((c, n) => c.Alignment = n is ComplaintGuiltyNode ? Pango.Alignment.Left : Pango.Alignment.Right)
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => GetCount(n))
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.RowCells()
					.AddSetter<CellRenderer>((c, n) => c.CellBackgroundGdk = GetColor(n))
				.Finish();

			yTVComplaintsResultsOfCounterparty.ColumnsConfig = CreateClosedComplaintResultColumnConfig();
			yTVComplaintsResultsOfEmployees.ColumnsConfig = CreateClosedComplaintResultColumnConfig();
		}

		private IColumnsConfig CreateClosedComplaintResultColumnConfig()
		{
			return ColumnsConfigFactory.Create<ClosedComplaintResultNode>()
				.AddColumn("Итог")
					.AddTextRenderer(n => string.IsNullOrEmpty(n.Name) ? "(результат не выставлен)" : n.Name)
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => n.Count.ToString())
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.Finish();
		}

		private string GetNodeText(object node)
		{
			if(node is ComplaintGuiltyNode guiltyNode)
			{
				return guiltyNode.GuiltyName;
			}
			if(node is ComplaintResultNode resultNode)
			{
				return resultNode.Text ?? "Не указано";
			}
			return "";
		}

		private string GetCount(object node)
		{
			if(node is ComplaintGuiltyNode guiltyNode)
			{
				return guiltyNode.Count.ToString();
			}
			if(node is ComplaintResultNode resultNode)
			{
				return resultNode.Count.ToString();
			}
			return "";
		}

		private Gdk.Color GetColor(object node)
		{
			if(node is ComplaintGuiltyNode) {
				return _secondaryBg;
			}
			if(node is ComplaintResultNode) {
				return _primaryBg;
			}
			return _red;
		}

		#endregion

		DateTime? StartDate { get; set; }
		DateTime? EndDate { get; set; }
		IList<ComplaintGuiltyNode> guilties = new List<ComplaintGuiltyNode>();

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => guilties.Any();

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			var complaintFilterViewModel = (InfoProvider as IComplaintsInfoProvider).ComplaintsFilterViewModel;
			StartDate = complaintFilterViewModel.StartDate;
			EndDate = complaintFilterViewModel.EndDate;

			var totalCount = complaintsRepository.GetUnclosedComplaintsCount(InfoProvider.UoW);
			var overdueCount = complaintsRepository.GetUnclosedComplaintsCount(InfoProvider.UoW, true);

			guilties = new List<ComplaintGuiltyNode>(GetGuilties(complaintFilterViewModel));
			var levels = LevelConfigFactory
						.FirstLevel<ComplaintGuiltyNode, ComplaintResultNode>(x => x.ComplaintResultNodes)
						.LastLevel(c => c.ComplaintGuiltyNode).EndConfig();

			var resultsOfCounterparty =
				_complaintResultsRepository.GetComplaintsResultsOfCounterparty(InfoProvider.UoW, StartDate, EndDate);
			var resultsOfEmployees =
				_complaintResultsRepository.GetComplaintsResultsOfEmployees(InfoProvider.UoW, StartDate, EndDate);

			Gtk.Application.Invoke((s, args) =>
				DrawRefreshed(totalCount, overdueCount, levels, resultsOfCounterparty, resultsOfEmployees));
		}

		#endregion

		private void DrawRefreshed(
			int totalCount,
			int overdueCount,
			ILevelConfig[] levels,
			IList<ClosedComplaintResultNode> resultsOfCounterparty,
			IList<ClosedComplaintResultNode> resultsOfEmployees)
		{
			lblCaption.Markup = string.Format("<u><b>Сводка по рекламациям\nСписок ответственных:</b></u>");
			lblUnclosedCount.Markup = string.Format(
				"<b>Не закрыто <span foreground='{2}'>{0}</span> рекламаций,\nиз них просрочено <span foreground='{2}'>{1}</span> шт.</b>",
				totalCount,
				overdueCount,
				totalCount >= 0 ? _redTextHtmlColor : _primaryTextHtmlColor
			);

			yTreeView.YTreeModel = new LevelTreeModel<ComplaintGuiltyNode>(guilties, levels);
			yTVComplaintsResultsOfCounterparty.SetItemsSource(resultsOfCounterparty);
			yTVComplaintsResultsOfEmployees.SetItemsSource(resultsOfEmployees);
		}

		#region Queries

		private IList<ComplaintGuiltyNode> GetGuilties(ComplaintFilterViewModel filter)
		{
			Complaint complaintAlias = null;
			Subdivision subdivisionAlias = null;
			Subdivision subdivisionForEmployeeAlias = null;
			Employee employeeAlias = null;
			ComplaintGuiltyItem guiltyItemAlias = null;
			ComplaintResultOfCounterparty resultOfCounterpartyAlias = null;
			ComplaintResultOfEmployees resultOfEmployeesAlias = null;
			QueryNode queryNodeAlias = null;
			ComplaintDiscussion discussionAlias = null;
			Responsible responsibleAlias = null;

			var query = InfoProvider.UoW.Session.QueryOver(() => guiltyItemAlias)
			   .Left.JoinAlias(() => guiltyItemAlias.Complaint, () => complaintAlias)
			   .Left.JoinAlias(() => complaintAlias.ComplaintResultOfCounterparty, () => resultOfCounterpartyAlias)
			   .Left.JoinAlias(() => complaintAlias.ComplaintResultOfEmployees, () => resultOfEmployeesAlias)
			   .Left.JoinAlias(() => guiltyItemAlias.Subdivision, () => subdivisionAlias)
			   .Left.JoinAlias(() => guiltyItemAlias.Employee, () => employeeAlias)
			   .Left.JoinAlias(() => guiltyItemAlias.Responsible, () => responsibleAlias)
			   .Left.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionForEmployeeAlias);

			var startDate = filter.StartDate;
			var endDate = filter.EndDate;
			endDate = endDate?.LatestDayTime();

			QueryOver<ComplaintDiscussion, ComplaintDiscussion> dicussionQuery = null;

			if(filter.Subdivision != null) {
				dicussionQuery = QueryOver.Of(() => discussionAlias)
					.Select(Projections.Property<ComplaintDiscussion>(p => p.Id))
					.Where(() => discussionAlias.Subdivision.Id == filter.Subdivision.Id)
					.And(() => discussionAlias.Complaint.Id == complaintAlias.Id);
			}

			switch (filter.FilterDateType)
			{
				case DateFilterType.PlannedCompletionDate:
					if(dicussionQuery == null)
					{
						if(startDate.HasValue)
						{
							query.Where(() => complaintAlias.PlannedCompletionDate >= startDate);
						}
						if(endDate.HasValue)
						{
							query.Where(() => complaintAlias.PlannedCompletionDate <= endDate);
						}
					}
					else
					{
						if(startDate.HasValue)
						{
							dicussionQuery.And(() => discussionAlias.PlannedCompletionDate >= startDate);
						}
						if(endDate.HasValue)
						{
							dicussionQuery.And(() => discussionAlias.PlannedCompletionDate <= endDate);
						}
					}
					break;
				case DateFilterType.ActualCompletionDate:
					if(startDate.HasValue)
					{
						query.Where(() => complaintAlias.ActualCompletionDate >= startDate);
					}
					if(endDate.HasValue)
					{
						query.Where(() => complaintAlias.ActualCompletionDate <= endDate);
					}
					break;
				case DateFilterType.CreationDate:
					if(startDate.HasValue)
					{
						query.Where(() => complaintAlias.CreationDate >= startDate);
					}
					if(endDate.HasValue)
					{
						query.Where(() => complaintAlias.CreationDate <= endDate);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if(dicussionQuery != null)
			{
				query.WithSubquery.WhereExists(dicussionQuery);
			}

			if(filter.ComplaintType != null)
			{
				query = query.Where(() => complaintAlias.ComplaintType == filter.ComplaintType);
			}

			if(filter.ComplaintStatus != null)
			{
				query = query.Where(() => complaintAlias.Status == filter.ComplaintStatus);
			}

			if(filter.Employee != null)
			{
				query = query.Where(() => complaintAlias.CreatedBy.Id == filter.Employee.Id);
			}

			if(filter.GuiltyItemVM?.Entity?.Responsible != null) 
			{
				var subquery = QueryOver.Of<ComplaintGuiltyItem>()
					.Where(g => g.Responsible.Id == filter.GuiltyItemVM.Entity.Responsible.Id);

				if(filter.GuiltyItemVM.Entity.Responsible.IsEmployeeResponsible && filter.GuiltyItemVM.Entity.Employee != null)
				{
					subquery.Where(g => g.Employee.Id == filter.GuiltyItemVM.Entity.Employee.Id);
				}

				if(filter.GuiltyItemVM.Entity.Responsible.IsSubdivisionResponsible && filter.GuiltyItemVM.Entity.Subdivision != null)
				{
					subquery.Where(g => g.Subdivision.Id == filter.GuiltyItemVM.Entity.Subdivision.Id);
				}

				query.WithSubquery.WhereProperty(() => complaintAlias.Id).In(subquery.Select(x => x.Complaint));
			}

			if(filter.ComplaintKind != null)
			{
				query.Where(() => complaintAlias.ComplaintKind.Id == filter.ComplaintKind.Id);
			}

			var result = query.SelectList(list => list
				.SelectGroup(c => c.Complaint.Id)
				.Select(() => complaintAlias.Status).WithAlias(() => queryNodeAlias.Status)
				.Select(() => resultOfCounterpartyAlias.Name).WithAlias(() => queryNodeAlias.ResultOfCounterpartyText)
				.Select(() => resultOfEmployeesAlias.Name).WithAlias(() => queryNodeAlias.ResultOfEmployeesText)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(
						NHibernateUtil.String,
					"GROUP_CONCAT(" +
					"CASE ?1 " +
					$"WHEN '{_complaintSettings.EmployeeResponsibleId}' THEN IFNULL(CONCAT('Отд: ', ?2), 'Отдел ВВ') " +
					$"WHEN '{_complaintSettings.SubdivisionResponsibleId}' THEN IFNULL(CONCAT('Отд: ', ?3), 'Отдел ВВ') " +
					$"ELSE ?4 " +
					"END " +
					"ORDER BY ?5 ASC SEPARATOR '\n')"),
					NHibernateUtil.String,
					Projections.Property(() => responsibleAlias.Id),
					Projections.Property(() => subdivisionForEmployeeAlias.Name),
					Projections.Property(() => subdivisionAlias.Name),
					Projections.Property(() => responsibleAlias.Name),
					Projections.Conditional(
						Restrictions.EqProperty(Projections.Constant(_complaintSettings.EmployeeResponsibleId), Projections.Property(() => responsibleAlias.Id)),
						Projections.Property(() => subdivisionForEmployeeAlias.Name),
						Projections.Conditional(
							Restrictions.EqProperty(Projections.Constant(_complaintSettings.SubdivisionResponsibleId), Projections.Property(() => responsibleAlias.Id)),
							Projections.Property(() => subdivisionAlias.Name),
							Projections.Property(() => responsibleAlias.Name)
							)
						)
					)
				).WithAlias(() => queryNodeAlias.GuiltyName))
			.TransformUsing(Transformers.AliasToBean<QueryNode>())
			.List<QueryNode>();

			var groupedResult = result.GroupBy(p => p.GuiltyName, (guiltyName, guiltiesGroup) => new ComplaintGuiltyNode {
				GuiltyName = guiltyName,
				Count = guiltiesGroup.Count(),
				Guilties = guiltiesGroup.ToList()
			}).ToList();

			//Удаление дублирующихся названий отделов
			for(int i = 0; i < groupedResult.Count; i++) {
				if(groupedResult[i].GuiltyName.Contains("\n")) {
					groupedResult[i].GuiltyName = string.Join("\n", groupedResult[i].GuiltyName.Split('\n').Distinct());
				}
			}

			foreach(var item in groupedResult) {
				item.CreateComplaintResultNodes();
			}
			return groupedResult;
		}

		public class QueryNode
		{
			public ComplaintStatuses Status { get; set; }
			public string ResultOfCounterpartyText { get; set; }
			public string ResultOfEmployeesText { get; set; }
			public string ResultText
			{
				get
				{
					switch(string.IsNullOrWhiteSpace(ResultOfCounterpartyText))
					{
						case true:
							return ResultOfEmployeesText;
						case false:
							return string.IsNullOrWhiteSpace(ResultOfEmployeesText)
								? ResultOfCounterpartyText
								: $"{ResultOfCounterpartyText},\n{ResultOfEmployeesText}";
						default:
							return null;
					}
				}
			}

			public string GuiltyName { get; set; }
		}

		public class ComplaintGuiltyNode
		{
			public int Count { get; set; }
			public string GuiltyName { get; set; }
			public IList<ComplaintResultNode> ComplaintResultNodes { get; set; }

			public IList<QueryNode> Guilties { get; set; }

			public void CreateComplaintResultNodes()
			{
				ComplaintResultNodes = new List<ComplaintResultNode>();

				var resultNodes =
					Guilties.GroupBy(p => new { p.Status, p.ResultText },
						(statusAndResultText, guiltiesGroup) =>
							new ComplaintResultNode
							{
								Count = guiltiesGroup.Count(),
								Status = statusAndResultText.Status,
								Text = statusAndResultText.Status == ComplaintStatuses.Closed
									? statusAndResultText.ResultText
									: ComplaintStatuses.InProcess.GetEnumTitle(),
								ComplaintGuiltyNode = this
							}
					).ToList();

				//Объединяю ноды со статусами "В работе" и "На проверке"
				if(resultNodes.Count(n => n.Status == ComplaintStatuses.InProcess || n.Status == ComplaintStatuses.Checking) > 1) {
					var nodesToUnion = resultNodes.Where(n => n.Status == ComplaintStatuses.InProcess || n.Status == ComplaintStatuses.Checking).ToList();
					nodesToUnion[0].Count = nodesToUnion.Sum(n => n.Count);
					foreach(var node in nodesToUnion.Skip(1)) {
						resultNodes.Remove(node);
					}
				}

				foreach(var node in resultNodes.OrderBy(c => c.Text))
				{
					ComplaintResultNodes.Add(node);
				}
			}
		}

		public class ComplaintResultNode
		{
			public ComplaintGuiltyNode ComplaintGuiltyNode { get; set; }
			public string Text { get; set; }
			public int Count { get; set; }
			public ComplaintStatuses Status { get; set; }
		}

		#endregion

		public override void Destroy()
		{
			yTreeView?.Destroy();
			yTVComplaintsResultsOfCounterparty?.Destroy();
			yTVComplaintsResultsOfEmployees?.Destroy();
			base.Destroy();
		}
	}
}
