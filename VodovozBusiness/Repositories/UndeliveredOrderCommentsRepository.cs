using System;
using System.Collections.Generic;
using NHibernate.Transform;
using NHibernate.Util;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Repositories
{
	public static class UndeliveredOrderCommentsRepository
	{
		public static IList<UndeliveredOrderComment> GetComments(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, CommentedFields field)
		{
			return uow.Session.QueryOver<UndeliveredOrderComment>()
					  .Where(c => c.UndeliveredOrder.Id == undeliveredOrder.Id)
					  .Where(c => c.CommentedField == field)
					  .List<UndeliveredOrderComment>();
		}

		public static IList<UndeliveredOrderCommentsNode> GetCommentNodes(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, CommentedFields field)
		{
			UndeliveredOrderCommentsNode resultAlias = null;
			UndeliveredOrderComment undeliveredOrderAllais = null;
			Employee employeeAlias = null;
			bool clr = false;

			var nodes = uow.Session.QueryOver<UndeliveredOrderComment>(() => undeliveredOrderAllais)
			               .Where(c => c.UndeliveredOrder.Id == undeliveredOrder.Id)
			               .Where(c => c.CommentedField == field)
			               .JoinAlias(() => undeliveredOrderAllais.Employee, () => employeeAlias)
			               .SelectList(
				               list => list
				               .Select(() => undeliveredOrderAllais.Comment).WithAlias(() => resultAlias.Comment)
				               .Select(() => undeliveredOrderAllais.CommentDate).WithAlias(() => resultAlias.Date)
				               .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.FName)
				               .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.MName)
				               .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LName)
				              )
			               .OrderBy(i=>i.CommentDate).Desc
			               .TransformUsing(Transformers.AliasToBean<UndeliveredOrderCommentsNode>())
			               .List<UndeliveredOrderCommentsNode>();
			
			foreach(var node in nodes) {
				node.Color = clr ? "red" : "blue";
				clr = !clr;
			}
			return nodes;
		}
	}

	public class UndeliveredOrderCommentsNode
	{
		public string Comment { get; set; }

		public string MarkedupComment => String.Format(
			"<span foreground=\"{0}\">{1}</span>",
			Color,
			Comment
		);

		public string UserDateAndName => String.Format(
			"<span foreground=\"{0}\"><b>{1}\n{2}: </b></span>",
			Color,
			Date.ToString("d MMM, HH:mm:ss"),
			StringWorks.PersonNameWithInitials(LName, FName, MName)
		);
		public DateTime Date { get; set; }
		public string FName { get; set; }
		public string MName { get; set; }
		public string LName { get; set; }
		public string Color { get; set; }
	}
}
