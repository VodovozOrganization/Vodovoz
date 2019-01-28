using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Store;
using Gamma.Binding;

namespace Vodovoz.Representations
{
	public class WarehousesVM : RepresentationModelEntityBase<Warehouse, SubdivisionWithWarehousesVMNode>
	{
		public WarehousesVM(IUnitOfWork uow) : base() => this.UoW = uow;
		public WarehousesVM(IUnitOfWork uow, Warehouse parent) : this(uow) => parentId = parent.Id;
		public WarehousesVM() : this(UnitOfWorkFactory.CreateWithoutRoot()) { }

		public IList<SubdivisionWithWarehousesVMNode> Result { get; set; }
		public IyTreeModel TreeModel { get; set; }
		int? parentId;

		public override void UpdateNodes()
		{
			SubdivisionWithWarehousesVMNode resultAlias = null;
			Subdivision subdivisionAlias = null;
			Warehouse warehouseAlias = null;

			var allSubdivisionNodes = UoW.Session.QueryOver(() => warehouseAlias)
				.Right.JoinQueryOver(() => warehouseAlias.OwningSubdivision, () => subdivisionAlias)
				.SelectList(list => list
				   .Select(() => warehouseAlias.Id).WithAlias(() => resultAlias.WarehouseId)
				   .Select(() => subdivisionAlias.Id).WithAlias(() => resultAlias.SubdivisionId)
				   .Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.SubdivisionName)
				   .Select(() => subdivisionAlias.ParentSubdivision.Id).WithAlias(() => resultAlias.ParentId)
				)
				.TransformUsing(Transformers.AliasToBean<SubdivisionWithWarehousesVMNode>())
				.List<SubdivisionWithWarehousesVMNode>();

			List<SubdivisionWithWarehousesVMNode> subdivisionHierarchy = AddWarehouseNodesAndBuildHierarchy(allSubdivisionNodes);

			TreeModel = new RecursiveTreeModel<SubdivisionWithWarehousesVMNode>(subdivisionHierarchy, x => x.Parent, x => x.Children);
			SetItemsSource(subdivisionHierarchy);
		}

		List<SubdivisionWithWarehousesVMNode> AddWarehouseNodesAndBuildHierarchy(IList<SubdivisionWithWarehousesVMNode> allSubdivisionNodes)
		{
			Result = new List<SubdivisionWithWarehousesVMNode>();
			foreach(var whGroup in allSubdivisionNodes.GroupBy(x => x.SubdivisionId)) {
				var w = whGroup.FirstOrDefault();
				w.Warehouses = new List<Warehouse>();
				foreach(var n in whGroup.Select(g => g.WarehouseId)) {
					if(n.HasValue) {
						var wh = UoW.GetById<Warehouse>(n.Value);
						w.Warehouses.Add(wh);
						w.WarehouseId = null;

						w.Children.Add(
							new SubdivisionWithWarehousesVMNode {
								ParentId = w.SubdivisionId,
								Parent = w,
								IsArchiveWarehouse = wh.IsArchive,
								WarehouseId = wh.Id,
								WarehouseName = wh.Name,
								Warehouses = new List<Warehouse>()
							}
						);
					}
				}
				Result.Add(w);
			}

			var subdivisionHierarchy = Result.Where(s => s.ParentId == parentId).ToList();
			foreach(var sHierarchy in subdivisionHierarchy)
				SetChildrenRecoursivelyIfTheyHaveWarehouses(sHierarchy);
			return subdivisionHierarchy;
		}

		/// <summary>
		/// Заполнение дочерних нод при условви наличия в них или их дочерних нодах складов
		/// </summary>
		/// <returns><c>true</c>, если есть дети или дети детей со складами, <c>false</c> если таковых нет.</returns>
		bool SetChildrenRecoursivelyIfTheyHaveWarehouses(SubdivisionWithWarehousesVMNode node)
		{
			bool val = false;
			//node.Children = new List<SubdivisionWithWarehousesVMNode>();
			var children = Result.Where(s => s.ParentId == node.SubdivisionId);
			foreach(var n in children) {
				bool childrenHaveWarehouses = SetChildrenRecoursivelyIfTheyHaveWarehouses(n) || n.Warehouses.Any();
				val = val || childrenHaveWarehouses;
				if(childrenHaveWarehouses) {
					n.Parent = node;
					node.Children.Add(n);
				}
			}
			return val;
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<SubdivisionWithWarehousesVMNode>.Create()
			.AddColumn("Название").AddTextRenderer(node => node.Name)
			.AddColumn("Архивный?").AddTextRenderer(node => node.IsArchiveWarehouse ? "Да" : string.Empty)
			.AddColumn("Код").AddTextRenderer(node => node.WarehouseId.ToString())
			.RowCells()
				.AddSetter<CellRendererText>(
					(c, n) => c.Foreground = n.WarehouseId.HasValue?"black":"grey"
				)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Warehouse updatedSubject) => true;

		#endregion
	}

	public class SubdivisionWithWarehousesVMNode
	{
		public int SubdivisionId { get; set; }
		public string SubdivisionName { get; set; }
		public int? WarehouseId { get; set; }
		public string WarehouseName { get; set; }
		public bool IsArchiveWarehouse { get; set; }

		public int? ParentId { get; set; }
		public IList<SubdivisionWithWarehousesVMNode> Children { get; set; } = new List<SubdivisionWithWarehousesVMNode>();

		public IList<Warehouse> Warehouses { get; set; }
		public SubdivisionWithWarehousesVMNode Parent { get; set; }

		public string Name => WarehouseName ?? string.Format("Отд. \"{0}\"", SubdivisionName);
	}
}
