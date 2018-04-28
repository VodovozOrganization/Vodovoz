using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using System.Linq;
using Vodovoz.JournalFilters;

namespace Vodovoz.ViewModel
{
	public class NomenclatureDependsFromVM : RepresentationModelEntityBase<Nomenclature, NomenclatureDependsFromVMNode>
	{

		public NomenclatureRepFilter Filter {
			get {
				return RepresentationFilter as NomenclatureRepFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureDependsFromVMNode resultAlias = null;

			var items = UoW.Session.QueryOver<Nomenclature>(() => nomenclatureAlias);
			if(excludingIds != null)
				items = items.Where(n => n.Category == nomenclatureCategory && !n.Id.IsIn(excludingIds));
			items = items.SelectList(list => list
					  .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					  .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					  .Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
				)
				.TransformUsing(Transformers.AliasToBean<NomenclatureDependsFromVMNode>());

			List<NomenclatureDependsFromVMNode> forDependence = new List<NomenclatureDependsFromVMNode>();
			forDependence.AddRange(items.List<NomenclatureDependsFromVMNode>());
			forDependence.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.CurrentCulture));
			SetItemsSource(forDependence);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<NomenclatureDependsFromVMNode>.Create()
			.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
			.AddColumn("Категория").AddTextRenderer(node => node.Category.GetEnumTitle())
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		public NomenclatureDependsFromVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{ }

		public NomenclatureDependsFromVM(IUnitOfWork uow)
		{
			this.UoW = uow;
		}

		public NomenclatureDependsFromVM(NomenclatureRepFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		List<int> excludingIds = null;
		NomenclatureCategory nomenclatureCategory;

		public NomenclatureDependsFromVM(Nomenclature nomenclature) : this()
		{
			this.excludingIds = new List<int>(nomenclature.Id);
			if(nomenclature.DependsOnNomenclature != null)
				excludingIds.Add(nomenclature.DependsOnNomenclature.Id);
			this.nomenclatureCategory = nomenclature.Category;
		}

		#region implemented abstract members of RepresentationModelWithoutEntityBase

		protected override bool NeedUpdateFunc(Nomenclature updatedSubject)
		{
			return true;
		}

		#endregion
	}

	public class NomenclatureDependsFromVMNode
	{
		public int Id { get; set; }

		[UseForSearch]
		public string Name { get; set; }
		public NomenclatureCategory Category { get; set; }
	}
}