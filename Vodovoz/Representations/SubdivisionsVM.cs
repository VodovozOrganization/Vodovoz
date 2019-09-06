using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using NHibernate.Criterion;

namespace Vodovoz.Representations
{
	public class SubdivisionsVM : RepresentationModelEntityBase<Subdivision, SubdivisionVMNode>
	{
		public IList<SubdivisionVMNode> Result { get; set; }
		private IList<SubdivisionVMNode> allSubdivisionNodes;
		int? parentId;

		public SubdivisionsVM(IUnitOfWork uow) : base() => this.UoW = uow;

		public SubdivisionsVM(IUnitOfWork uow, Subdivision parent) : this(uow) => parentId = parent.Id;

		public SubdivisionsVM() : this(UnitOfWorkFactory.CreateWithoutRoot()) { }

		public bool WithLeveling { get; set; } = true;

		public override void UpdateNodes()
		{
			Employee chiefAlias = null;
			SubdivisionVMNode resultAlias = null;
			var query = UoW.Session.QueryOver<Subdivision>();

			if(!WithLeveling) {
				var firstLevelSubQuery = QueryOver.Of<Subdivision>().WhereRestrictionOn(x => x.ParentSubdivision).IsNull().Select(x => x.Id);
				var secondLevelSubquery = QueryOver.Of<Subdivision>().WithSubquery.WhereProperty(x => x.ParentSubdivision.Id).In(firstLevelSubQuery).Select(x => x.Id);

				query
					.WithSubquery.WhereProperty(x => x.Id).NotIn(firstLevelSubQuery)
					.WithSubquery.WhereProperty(x => x.Id).NotIn(secondLevelSubquery);
			}

			allSubdivisionNodes = query
				.Left.JoinAlias(o => o.Chief, () => chiefAlias)
				.SelectList(list => list
				   .Select(s => s.Id).WithAlias(() => resultAlias.Id)
				   .Select(s => s.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => chiefAlias.Name).WithAlias(() => resultAlias.ChiefFirstName)
				   .Select(() => chiefAlias.Patronymic).WithAlias(() => resultAlias.ChiefMiddleName)
				   .Select(() => chiefAlias.LastName).WithAlias(() => resultAlias.ChiefLastName)
				   .Select(s => s.ParentSubdivision.Id).WithAlias(() => resultAlias.ParentId)
				)
				.TransformUsing(Transformers.AliasToBean<SubdivisionVMNode>())
				.List<SubdivisionVMNode>();

			if(WithLeveling) {
				Result = allSubdivisionNodes.Where(s => s.ParentId == parentId).ToList();
				foreach(var r in Result)
					SetChildren(r);
			} else {
				Result = allSubdivisionNodes;
			}
			SetItemsSource(Result);
		}

		void SetChildren(SubdivisionVMNode node)
		{
			var children = allSubdivisionNodes.Where(s => s.ParentId == node.Id);
			node.Children = children.ToList();
			foreach(var n in children) {
				n.Parent = node;
				SetChildren(n);
			}
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<SubdivisionVMNode>.Create()
			.AddColumn("Название").AddTextRenderer(node => node.Name)
			.AddColumn("Руководитель").AddTextRenderer(node => node.ChiefName)
			.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Subdivision updatedSubject) => true;

		#endregion
	}

	public class SubdivisionVMNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ChiefFirstName { get; set; }
		public string ChiefMiddleName { get; set; }
		public string ChiefLastName { get; set; }

		public string ChiefName => PersonHelper.PersonNameWithInitials(ChiefLastName, ChiefFirstName, ChiefMiddleName);
		public virtual SubdivisionVMNode Parent { get; set; }
		public virtual int? ParentId { get; set; }
		public virtual IList<SubdivisionVMNode> Children { get; set; }
	}
}
