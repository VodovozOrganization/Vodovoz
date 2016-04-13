using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
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

		public override void UpdateNodes ()
		{
			Counterparty counterpartyAlias = null;
			CounterpartyVMNode resultAlias = null;

			var query = UoW.Session.QueryOver<Counterparty> (() => counterpartyAlias);

			if (Filter.RestrictCounterpartyType != null) {
				query.Where (c => c.CounterpartyType == Filter.RestrictCounterpartyType);
			}

			var counterpartyList = query.SelectList (list => list
					.Select (c => c.Id).WithAlias (() => resultAlias.Id)
					.Select (c => c.Name).WithAlias (() => resultAlias.Name)
			                       )
				.TransformUsing (Transformers.AliasToBean<CounterpartyVMNode> ())
				.List<CounterpartyVMNode> ();

			SetItemsSource (counterpartyList);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <CounterpartyVMNode>.Create ()
			.AddColumn ("Контрагент").SetDataProperty (node => node.Name)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Counterparty updatedSubject)
		{
			return true;
		}

		#endregion

		public CounterpartyVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new CounterpartyFilter (UoW);
		}

		public CounterpartyVM (IUnitOfWork uow)
		{
			this.UoW = uow;
		}

		public CounterpartyVM (CounterpartyFilter filter) : this (filter.UoW)
		{
			Filter = filter;
		}
	}

	public class CounterpartyVMNode
	{
		public int Id{ get; set; }

		[UseForSearch]
		public string Name { get; set; }
	}
}

