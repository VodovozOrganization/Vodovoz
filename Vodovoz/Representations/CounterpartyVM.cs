using System;
using System.Collections.Generic;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Gtk.DataBindings;

namespace Vodovoz.ViewModel
{
	public class CounterpartyVM : RepresentationModelBase<Counterparty, CounterpartyVMNode>
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

		IMappingConfig treeViewConfig = FluentMappingConfig<CounterpartyVMNode>.Create ()
			.AddColumn ("Контрагент").SetDataProperty (node => node.Name)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (Counterparty updatedSubject)
		{
			return true;
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			throw new NotImplementedException ();
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

