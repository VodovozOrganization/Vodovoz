using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QSOrmProject;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (JournalName = "Производство воды", ObjectName = "Документ производства")]
	public class IncomingWater: Document
	{
		int amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		Warehouse warehouse;

		[Required (ErrorMessage = "Склад должен быть указан.")]
		[Display (Name = "Склад")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField (ref warehouse, value, () => Warehouse); }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Документ производства"; }
		}

		new public virtual string Description {
			get { return String.Format ("Количество: {0}; Склад поступления: {1};", 
				Amount,
				Warehouse == null ? "не указан" : Warehouse.Name); 
			}
		}

		#endregion

		GoodsMovementOperation produceWaterOperation = new GoodsMovementOperation ();

		public GoodsMovementOperation ProduceWaterOperation {
			get { return produceWaterOperation; }
			set { SetField (ref produceWaterOperation, value, () => ProduceWaterOperation); }
		}

		GoodsMovementOperation expenseBottlesOperation = new GoodsMovementOperation ();

		public GoodsMovementOperation ExpenseBottlesOperation {
			get { return expenseBottlesOperation; }
			set { SetField (ref expenseBottlesOperation, value, () => ExpenseBottlesOperation); }
		}

	}
}

