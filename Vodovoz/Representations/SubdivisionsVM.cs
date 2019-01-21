using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Representations
{
	public class SubdivisionsVM : RepresentationModelEntityBase<Subdivision, SubdivisionVMNode>
	{
		public IList<SubdivisionVMNode> Result { get; set; }
		IList<SubdivisionVMNode> AllSubdivisionNodes { get; set; }
		int? parentId;

		public SubdivisionsVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

		public SubdivisionsVM(IUnitOfWork uow, Subdivision parent) : this(uow)
		{
			parentId = parent.Id;
		}

		public override void UpdateNodes()
		{
			Employee chiefAlias = null;
			SubdivisionVMNode resultAlias = null;
			var query = UoW.Session.QueryOver<Subdivision>();

			AllSubdivisionNodes = query
				.Left.JoinAlias(o => o.Chief, () => chiefAlias)
				.SelectList(list => list
				   .Select(s => s.Id).WithAlias(() => resultAlias.Id)
				   .Select(s => s.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => chiefAlias.LastName).WithAlias(() => resultAlias.ChiefName)
				   .Select(s => s.ParentSubdivision.Id).WithAlias(() => resultAlias.ParentId)
				)
				.TransformUsing(Transformers.AliasToBean<SubdivisionVMNode>())
				.List<SubdivisionVMNode>();

			Result = AllSubdivisionNodes.Where(s => s.ParentId == parentId).ToList();
			foreach(var r in Result)
				SetChildren(r);
		}

		void SetChildren(SubdivisionVMNode node)
		{
			var children = AllSubdivisionNodes.Where(s => s.ParentId == node.Id);
			node.Children = children.ToList();
			foreach(var n in children) {
				n.Parent = node;
				SetChildren(n);
			}
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<SubdivisionVMNode>.Create()
			.AddColumn("Код").SetDataProperty(node => node.Id.ToString())
			.AddColumn("Название").SetDataProperty(node => node.Name)
			.AddColumn("Руководитель").SetDataProperty(node => node.ChiefName)
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
		public string ChiefName { get; set; }

		//public virtual Subdivision Self { get; set; }
		public virtual SubdivisionVMNode Parent { get; set; }
		public virtual int? ParentId { get; set; }
		public virtual IList<SubdivisionVMNode> Children { get; set; }
	}
}
