using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModel
{
	public class FinesVM : RepresentationModelEntityBase<Fine, FinesVMNode>
	{
		#region Поля
		#endregion

		#region Конструкторы
		public FinesVM()
		{
		}
		#endregion

		#region Свойства
		#endregion

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
		}

		Gamma.ColumnConfig.IColumnsConfig columnsConfig;

		public override Gamma.ColumnConfig.IColumnsConfig ColumnsConfig {
			get {
				return columnsConfig;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelEntityBase

		protected override bool NeedUpdateFunc(Fine updatedSubject)
		{
			return true;
		}

		#endregion

		#region Методы
		#endregion

		public class FinesVMNode
		{
			
		}
	}
}

