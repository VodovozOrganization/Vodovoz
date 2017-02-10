using System;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;

namespace Vodovoz
{
	public partial class RouteListAddressesTransferring : OrmGtkDialogBase<RouteList>
	{
		#region Конструкторы

		public RouteListAddressesTransferring()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<RouteList>();
			TabName = "Перенос адресов маршрутных листов";
			ConfigureDlg();
		}

		#endregion

		private void ConfigureDlg()
		{
			yentryreferenceRLFrom.SubjectType = typeof(RouteList);
			yentryreferenceRLTo	 .SubjectType = typeof(RouteList);

			yentryreferenceRLFrom.ItemsQuery = QueryOver.Of<RouteList>()
				.Where(rl => rl.Status == RouteListStatus.EnRoute);
			
			yentryreferenceRLTo.ItemsQuery = QueryOver.Of<RouteList>()
				.Where(rl => rl.Status == RouteListStatus.EnRoute 
						  || rl.Status == RouteListStatus.InLoading
						  || rl.Status == RouteListStatus.New);
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

