using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSProjectsLib;
using Vodovoz.Domain.Cash;

namespace Vodovoz.ViewModel
{
	public class AccountableSlipsVM : RepresentationModelWithoutEntityBase<AccountableSlipsVMNode>
	{
		public IAccountableSlipsFilter Filter {
			get {
				return RepresentationFilter as IAccountableSlipsFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			AccountableSlipsVMNode resultAlias = null;
			Expense operationRecivedAlias = null;
			Income operationReturnedAlias = null;
			AdvanceReport operationReportedAlias = null;

			List<AccountableSlipsVMNode> result = new List<AccountableSlipsVMNode> ();

			var recived = UoW.Session.QueryOver<Expense> (() => operationRecivedAlias)
				.Where (e => e.Employee == Filter.RestrictAccountable && e.TypeOperation == ExpenseType.Advance);

			var returned = UoW.Session.QueryOver<Income> (() => operationReturnedAlias)
				.Where (e => e.Employee == Filter.RestrictAccountable && e.TypeOperation == IncomeType.Return);

			var reported = UoW.Session.QueryOver<AdvanceReport> (() => operationReportedAlias)
				.Where (e => e.Accountable == Filter.RestrictAccountable);


			//Добавляем условия по фильтру
			if(Filter.RestrictExpenseCategory != null)
			{
				recived.Where (o => o.ExpenseCategory == Filter.RestrictExpenseCategory);
				returned.Where (o => o.ExpenseCategory == Filter.RestrictExpenseCategory);
				reported.Where (o => o.ExpenseCategory == Filter.RestrictExpenseCategory);
			}
				
			if(Filter.RestrictStartDate.HasValue)
			{
				recived.Where (o => o.Date >= Filter.RestrictStartDate.Value);
				returned.Where (o => o.Date >= Filter.RestrictStartDate.Value);
				reported.Where (o => o.Date >= Filter.RestrictStartDate.Value);
			}
				
			if(Filter.RestrictEndDate.HasValue)
			{
				recived.Where (o => o.Date <= Filter.RestrictEndDate.Value);
				returned.Where (o => o.Date <= Filter.RestrictEndDate.Value);
				reported.Where (o => o.Date <= Filter.RestrictEndDate.Value);
			}
				
			if(!Filter.RestrictEndDate.HasValue && !Filter.RestrictEndDate.HasValue)
			{
				recived.OrderBy (e => e.Date).Desc.Take (20);
				returned.OrderBy (e => e.Date).Desc.Take (20);
				reported.OrderBy (e => e.Date).Desc.Take (20);
			}

			var recivedList = recived.SelectList (list => list
					.Select (e => e.Id).WithAlias (() => resultAlias.Id)
					.Select (e => e.Date).WithAlias (() => resultAlias.Date)
					.Select (e => e.Money).WithAlias (() => resultAlias.Append)
				)
				.TransformUsing(Transformers.AliasToBean<AccountableSlipsVMNode>())
				.List<AccountableSlipsVMNode>();
			recivedList.ToList ().ForEach (i => i.DocType = CashDocumentType.Expense);
			result.AddRange (recivedList);

			var	returnedList = returned.SelectList (list => list
					.Select (e => e.Id).WithAlias (() => resultAlias.Id)
					.Select (e => e.Date).WithAlias (() => resultAlias.Date)
					.Select (e => e.Money).WithAlias (() => resultAlias.Removed)
				)
				.TransformUsing(Transformers.AliasToBean<AccountableSlipsVMNode>())
				.List<AccountableSlipsVMNode>();
			returnedList.ToList ().ForEach (i => i.DocType = CashDocumentType.Income);
			result.AddRange (returnedList);

			var reportedList = reported.SelectList (list => list
					.Select (e => e.Id).WithAlias (() => resultAlias.Id)
					.Select (e => e.Date).WithAlias (() => resultAlias.Date)
					.Select (e => e.Money).WithAlias (() => resultAlias.Removed)
				)
				.TransformUsing(Transformers.AliasToBean<AccountableSlipsVMNode>())
				.List<AccountableSlipsVMNode>();
			reportedList.ToList ().ForEach (i => i.DocType = CashDocumentType.AdvanceReport);
			result.AddRange (reportedList);

			result.Sort ((x, y) => { 
				if (x.Date > y.Date)
					return 1;
				if (x.Date == y.Date)
					return 0;
				return -1;
			});

			SetItemsSource (result);
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<AccountableSlipsVMNode> ()
			.AddColumn("Документ").SetDataProperty (node => node.DocTitle)
			.AddColumn ("Дата").SetDataProperty (node => node.DateText)
			.AddColumn ("Получено").SetDataProperty (node => node.AppendText)
			.AddColumn ("Закрыто").SetDataProperty (node => node.RemovedText)
			//.RowCells ().AddSetter<Gtk.CellRendererText> ((c, n) => c.Foreground = n.RowColor)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get {
				return treeViewConfig;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			if (updatedSubject is Expense)
				return (updatedSubject as Expense).TypeOperation == ExpenseType.Advance;

			if (updatedSubject is Income)
				return (updatedSubject as Income).TypeOperation == IncomeType.Return;

			return true;
		}

		#endregion

		public AccountableSlipsVM (IAccountableSlipsFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public AccountableSlipsVM () 
			: this(UnitOfWorkFactory.CreateWithoutRoot ()) 
		{}

		public AccountableSlipsVM (IUnitOfWork uow) : base( typeof(Expense), typeof(AdvanceReport), typeof(Income))
		{
			this.UoW = uow;
		}
	}
		
	public class AccountableSlipsVMNode
	{
		public int Id{ get; set;}

		public CashDocumentType DocType { get; set;}

		public DateTime Date{ get; set;}

		public decimal Append { get; set;}

		public decimal Removed { get; set;}

		public string DateText{
			get { return Date.ToShortDateString ();
			}
		}

		public string AppendText{
			get { return Append > 0 ? CurrencyWorks.GetShortCurrencyString (Append) : String.Empty;
			}
		}

		public string RemovedText{
			get { return Removed > 0 ? CurrencyWorks.GetShortCurrencyString (Removed) : String.Empty;
			}
		}

		public string DocTitle {
			get {
				return String.Format ("{0} №{1}", DocType.GetEnumTitle(), Id);
			}
		}
	}
}

