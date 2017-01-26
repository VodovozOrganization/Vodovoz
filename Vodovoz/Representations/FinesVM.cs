using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Gamma.ColumnConfig;

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

		public FineFilter Filter {
			get {
				return RepresentationFilter as FineFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <FinesVMNode>.Create()
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
			.Finish();

		public override IColumnsConfig ColumnsConfig {
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
	}

	public class FinesVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public DateTime Date { get; set; }
	}
}

