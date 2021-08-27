using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.FilterViewModels;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintPanelView : Bin, IPanelView
	{
		readonly IComplaintsRepository complaintsRepository;

		public ComplaintPanelView(IComplaintsRepository complaintsRepository)
		{
			this.complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			this.Build();
			ConfigureWidget();
		}

		#region Widget

		void ConfigureWidget()
		{
			yTreeView.ColumnsConfig = ColumnsConfigFactory.Create<object>()
				.AddColumn("Виновный")
					.AddTextRenderer(n => GetNodeText(n))
					.AddSetter((c ,n) => c.Alignment = GetAlignment(n))
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => GetCount(n))
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.RowCells()
					.AddSetter<CellRenderer>((c, n) => c.CellBackgroundGdk = GetColor(n))
				.Finish();

			yTVComplainsResults.ColumnsConfig = ColumnsConfigFactory.Create<object[]>()
				.AddColumn("Итог")
					.AddTextRenderer(n => n[0] != null ? n[0].ToString() : "(результат не выставлен)")
					.WrapWidth(150).WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Кол-во")
					.AddTextRenderer(n => n[1].ToString())
					.WrapWidth(50).WrapMode(Pango.WrapMode.WordChar)
				.Finish();
		}

		private Pango.Alignment GetAlignment(object node)
		{
			return node is ComplaintGuiltyNode ? Pango.Alignment.Left : Pango.Alignment.Right;
		}

		private string GetNodeText(object node)
		{
			if(node is ComplaintGuiltyNode) {
				return (node as ComplaintGuiltyNode).GuiltyName;
			}
			if(node is ComplaintResultNode) {
				return (node as ComplaintResultNode).Text ?? "Не указано";
			}
			return "";
		}

		private string GetCount(object node)
		{
			if(node is ComplaintGuiltyNode) {
				return (node as ComplaintGuiltyNode).Count.ToString();
			}
			if(node is ComplaintResultNode) {
				return (node as ComplaintResultNode).Count.ToString();
			}
			return "";
		}

		private Gdk.Color GetColor(object node)
		{
			if(node is ComplaintGuiltyNode) {
				return gr;
			}
			if(node is ComplaintResultNode) {
				return wh;
			}
			return red;
		}

		#endregion

		Gdk.Color wh = new Gdk.Color(255, 255, 255);
		Gdk.Color gr = new Gdk.Color(230, 230, 230);
		Gdk.Color red = new Gdk.Color(255, 0, 0);

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
			var overduedCount = complaintsRepository.GetUnclosedComplaintsCount(InfoProvider.UoW, true);

			guilties = new List<ComplaintGuiltyNode>(GetGuilties(complaintFilterViewModel));
			var levels = LevelConfigFactory
						.FirstLevel<ComplaintGuiltyNode, ComplaintResultNode>(x => x.ComplaintResultNodes)
						.LastLevel(c => c.ComplaintGuiltyNode).EndConfig();

			var complaintResults = complaintsRepository.GetComplaintsResults(InfoProvider.UoW, StartDate, EndDate);

			Application.Invoke((s, args) => DrawRefreshed(complaintFilterViewModel, totalCount, overduedCount, levels, complaintResults));
		}

		#endregion

		private void DrawRefreshed(ComplaintFilterViewModel filter, int totalCount, int overduedCount, ILevelConfig[] levels, IList<object[]> complaintResults)
		{
			lblCaption.Markup = string.Format("<u><b>Сводка по рекламациям\nСписок виновных:</b></u>");
			lblUnclosedCount.Markup = string.Format(
				"<b>Не закрыто <span foreground='{2}'>{0}</span> рекламаций,\nиз них просрочено <span foreground='{2}'>{1}</span> шт.</b>",
				totalCount,
				overduedCount,
				totalCount >= 0 ? "red" : "black"
			);

			yTreeView.YTreeModel = new LevelTreeModel<ComplaintGuiltyNode>(guilties, levels);
			yTVComplainsResults.SetItemsSource(complaintResults);
		}

		#region Queries

		private IList<ComplaintGuiltyNode> GetGuilties(ComplaintFilterViewModel filter)
		{
			Complaint complaintAlias = null;
			Subdivision subdivisionAlias = null;
			Subdivision subdivisionForEmployeeAlias = null;
			Employee employeeAlias = null;
			ComplaintGuiltyItem guiltyItemAlias = null;
			ComplaintResult complaintResultAlias = null;
			QueryNode queryNodeAlias = null;
			ComplaintDiscussion discussionAlias = null;

			var query = InfoProvider.UoW.Session.QueryOver(() => guiltyItemAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Complaint, () => complaintAlias)
						   .Left.JoinAlias(() => complaintAlias.ComplaintResult, () => complaintResultAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Subdivision, () => subdivisionAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Employee, () => employeeAlias)
						   .Left.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionForEmployeeAlias)
						   ;

			filter.EndDate = filter.EndDate.Date.AddHours(23).AddMinutes(59);
			if(filter.StartDate.HasValue)
				filter.StartDate = filter.StartDate.Value.Date;

			QueryOver<ComplaintDiscussion, ComplaintDiscussion> dicussionQuery = null;

			if(filter.Subdivision != null) {
				dicussionQuery = QueryOver.Of(() => discussionAlias)
					.Select(Projections.Property<ComplaintDiscussion>(p => p.Id))
					.Where(() => discussionAlias.Subdivision.Id == filter.Subdivision.Id)
					.And(() => discussionAlias.Complaint.Id == complaintAlias.Id);
			}

			if(filter.StartDate.HasValue) {
				switch (filter.FilterDateType) {
					case DateFilterType.PlannedCompletionDate:
						if(dicussionQuery == null) {
							query = query.Where(() => complaintAlias.PlannedCompletionDate <= filter.EndDate)
								.And(() => filter.StartDate == null || complaintAlias.PlannedCompletionDate >= filter.StartDate.Value);
						} else {
							dicussionQuery = dicussionQuery
								.And(() => filter.StartDate == null || discussionAlias.PlannedCompletionDate >= filter.StartDate.Value)
								.And(() => discussionAlias.PlannedCompletionDate <= filter.EndDate);
						}
						break;
					case DateFilterType.ActualCompletionDate:
						query = query.Where(() => complaintAlias.ActualCompletionDate <= filter.EndDate)
								.And(() => filter.StartDate == null || complaintAlias.ActualCompletionDate >= filter.StartDate.Value);
						break;
					case DateFilterType.CreationDate:
						query = query.Where(() => complaintAlias.CreationDate <= filter.EndDate)
							.And(() => filter.StartDate == null || complaintAlias.CreationDate >= filter.StartDate.Value);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if(dicussionQuery != null)
				query.WithSubquery.WhereExists(dicussionQuery);
			if(filter.ComplaintType != null)
				query = query.Where(() => complaintAlias.ComplaintType == filter.ComplaintType);
			if(filter.ComplaintStatus != null)
				query = query.Where(() => complaintAlias.Status == filter.ComplaintStatus);
			if(filter.Employee != null)
				query = query.Where(() => complaintAlias.CreatedBy.Id == filter.Employee.Id);

			if(filter.GuiltyItemVM?.Entity?.GuiltyType != null) {
				var subquery = QueryOver.Of<ComplaintGuiltyItem>()
										.Where(g => g.GuiltyType == filter.GuiltyItemVM.Entity.GuiltyType.Value);
				switch(filter.GuiltyItemVM.Entity.GuiltyType) {
					case ComplaintGuiltyTypes.None:
					case ComplaintGuiltyTypes.Client:
					case ComplaintGuiltyTypes.Supplier:
						break;
					case ComplaintGuiltyTypes.Employee:
						if(filter.GuiltyItemVM.Entity.Employee != null)
							subquery.Where(g => g.Employee.Id == filter.GuiltyItemVM.Entity.Employee.Id);
						break;
					case ComplaintGuiltyTypes.Subdivision:
						if(filter.GuiltyItemVM.Entity.Subdivision != null)
							subquery.Where(g => g.Subdivision.Id == filter.GuiltyItemVM.Entity.Subdivision.Id);
						break;
					default:
						break;
				}
				query.WithSubquery.WhereProperty(() => complaintAlias.Id).In(subquery.Select(x => x.Complaint));
			}

			if(filter.ComplaintKind != null)
				query.Where(() => complaintAlias.ComplaintKind.Id == filter.ComplaintKind.Id);

			var result = query.SelectList(list => list
				.SelectGroup(c => c.Complaint.Id)
				.Select(() => complaintAlias.Status).WithAlias(() => queryNodeAlias.Status)
				.Select(() => complaintResultAlias.Name).WithAlias(() => queryNodeAlias.ResultText)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(
						NHibernateUtil.String,
						"GROUP_CONCAT(" +
						"CASE ?1 " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Employee)}' THEN IFNULL(CONCAT('Отд: ', ?2), 'Отдел ВВ') " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Subdivision)}' THEN IFNULL(CONCAT('Отд: ', ?3), 'Отдел ВВ') " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Client)}' THEN 'Клиент' " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.Supplier)}' THEN 'Поставщик' " +
						$"WHEN '{nameof(ComplaintGuiltyTypes.None)}' THEN 'Нет (не рекламация)' " +
						"ELSE ?1 " +
						"END " +
						"ORDER BY ?1 ASC SEPARATOR '\n')"),
					NHibernateUtil.String,
					Projections.Property(() => guiltyItemAlias.GuiltyType),
					Projections.Property(() => subdivisionForEmployeeAlias.Name),
					Projections.Property(() => subdivisionAlias.Name)))
				.WithAlias(() => queryNodeAlias.GuiltyName))
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
			public string ResultText { get; set; }
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

				var resultNodes = Guilties.GroupBy(p => new { p.Status, p.ResultText }, (statusAndResultText, guiltiesGroup) => new ComplaintResultNode {
					Count = guiltiesGroup.Count(),
					Status = statusAndResultText.Status,
					Text = statusAndResultText.Status == ComplaintStatuses.Closed ? statusAndResultText.ResultText : ComplaintStatuses.InProcess.GetEnumTitle(),
					ComplaintGuiltyNode = this,
				}).ToList();

				//Объединяю ноды со статусами "В работе" и "На проверке"
				if(resultNodes.Count(n => n.Status == ComplaintStatuses.InProcess || n.Status == ComplaintStatuses.Checking) > 1) {
					var nodesToUnion = resultNodes.Where(n => n.Status == ComplaintStatuses.InProcess || n.Status == ComplaintStatuses.Checking).ToList();
					nodesToUnion[0].Count = nodesToUnion.Sum(n => n.Count);
					foreach(var node in nodesToUnion.Skip(1)) {
						resultNodes.Remove(node);
					}
				}

				foreach(var node in resultNodes.OrderBy(c => c.Text)) 
					ComplaintResultNodes.Add(node);
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
	}
}
