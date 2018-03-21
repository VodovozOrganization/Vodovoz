using Gamma.ColumnConfig;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModel
{
	public class CounterpartyVM : RepresentationModelEntityBase<Counterparty, CounterpartyVMNode>
	{

		public CounterpartyFilter Filter {
			get {
				return RepresentationFilter as CounterpartyFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			Counterparty counterpartyAlias = null;
			CounterpartyContract contractAlias = null;
			CounterpartyVMNode resultAlias = null;
			QSContacts.Phone phoneAlias = null;
			DeliveryPoint addressAlias = null;

			var query = UoW.Session.QueryOver<Counterparty>(() => counterpartyAlias);

			if(Filter != null && !Filter.RestrictIncludeArhive) {
				query.Where(c => !c.IsArchive);
			}

			var contractsSubquery = QueryOver.Of<CounterpartyContract>(() => contractAlias)
				.Where(c => c.Counterparty.Id == counterpartyAlias.Id)
				.Select(Projections.SqlFunction(
											new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
											NHibernateUtil.String,
											Projections.Property(() => contractAlias.Id),
											Projections.Constant(", ")));

			var addressSubquery = QueryOver.Of<DeliveryPoint>(() => addressAlias)
				.Where(d => d.Counterparty.Id == counterpartyAlias.Id)
				.Where(() => addressAlias.IsActive)
				.Select(Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
					NHibernateUtil.String,
					Projections.Property(() => addressAlias.CompiledAddress),
					Projections.Constant("\n")));

			var counterpartyList = query
				.JoinAlias(c => c.Phones, () => phoneAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
				   .SelectGroup(c => c.Id).WithAlias(() => resultAlias.Id)
				   .Select(c => c.Name).WithAlias(() => resultAlias.Name)
				   .Select(c => c.INN).WithAlias(() => resultAlias.INN)
				   .Select(c => c.IsArchive).WithAlias(() => resultAlias.IsArhive)
				   .SelectSubQuery(contractsSubquery).WithAlias(() => resultAlias.Contracts)
			   .Select(Projections.SqlFunction(
				   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
				   NHibernateUtil.String,
					   Projections.Property(() => phoneAlias.Number),
				   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.Phones)
			   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
					   NHibernateUtil.String,
					   Projections.Property(() => phoneAlias.DigitsNumber),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.PhonesDigits)
				   .SelectSubQuery(addressSubquery).WithAlias(() => resultAlias.Addresses)
				)
				.TransformUsing(Transformers.AliasToBean<CounterpartyVMNode>())
				.List<CounterpartyVMNode>();

			SetItemsSource(counterpartyList);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<CounterpartyVMNode>.Create()
		                                                                      .AddColumn("Код").AddTextRenderer(x => x.IdText)
			.AddColumn("Контрагент").AddTextRenderer(node => node.Name).WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
			.AddColumn("Телефоны").AddTextRenderer(x => x.Phones)
			.AddColumn("ИНН").AddTextRenderer(x => x.INN)
			.AddColumn("Договора").AddTextRenderer(x => x.Contracts)
			.AddColumn("Точки доставки").AddTextRenderer(x => x.Addresses)
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Counterparty updatedSubject)
		{
			return true;
		}

		#endregion

		public CounterpartyVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			CreateRepresentationFilter = () => new CounterpartyFilter(UoW);
		}

		public CounterpartyVM(IUnitOfWork uow)
		{
			this.UoW = uow;
		}

		public CounterpartyVM(CounterpartyFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}
	}

	public class CounterpartyVMNode
	{
		public int Id { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string IdText {get{ return Id.ToString();}}

		public bool IsArhive { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Name { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string INN { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Contracts { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Addresses { get; set; }

		[SearchHighlight]
		public string Phones { get; set; }

		[UseForSearch]
		public string PhonesDigits { get; set; }

		public string RowColor {
			get {
				if (IsArhive)
					return "grey";
				else
					return "black";

			}
		}
	}
}

