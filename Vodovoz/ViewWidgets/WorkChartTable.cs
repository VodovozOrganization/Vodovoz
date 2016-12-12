using System;
using Cairo;
using System.Collections.Generic;
using System.Linq;
using Gdk;

namespace ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public class WorkChartTable : Gtk.DrawingArea
	{
		#region Свойства

		public DateTime Date
		{
			get {return date;}
			set {
				date = value;
				month = value.Month;
				year = value.Year;
			}
		}
		#endregion

		#region Поля

		private int month			 = 1;
		private int year			 = 1;
		private int daysInMonth 	 = 1;
		private int topHeadersCount  = 2;
		private int leftHeadersCount = 1;

		private int   totalColums;
		private int   totalRows;
		private int   cellWidth;
		private int   cellHeight;
		private int   tableWidth;
		private int   tableHeight;
		private int[] weekends;

		private DateTime date;
		private HalfCell mouseOnCell = new HalfCell { 
			CellNumber = new CellNumber { X = -1, Y = -1 }, 
			half = Half.Top };
		private DrawState isDraw = DrawState.Nothing;

		IEnumerable<string> headerData;
		List<HalfCell> activeHalfCells = new List<HalfCell>();

		#endregion

		#region Структуры, перечисления, внутренние классы

		private struct CellNumber
		{
			public int X;
			public int Y;

			public bool Equals(CellNumber cn)
			{
				return this.X == cn.X && Y == cn.Y;
			}
		}

		private enum Half 
		{
			Top,
			Bottom
		}

		enum DrawState
		{
			Draw,
			Erase,
			Nothing
		}

		private class HalfCell
		{
			public CellNumber 	CellNumber 	{ get; set;}
			public Half 		half 		{ get; set;}

			public bool Equals(HalfCell cell)
			{
				return (this.CellNumber.Equals(cell.CellNumber) && this.half == cell.half);
			}
		}

		#endregion

		public WorkChartTable()
		{
			AddEvents((int) EventMask.ButtonPressMask);
			AddEvents((int) EventMask.ButtonReleaseMask);
			AddEvents((int) EventMask.PointerMotionMask);
			AddEvents((int) EventMask.LeaveNotifyMask);
		}

		#region События

		protected override bool OnExposeEvent(Gdk.EventExpose ev)
		{
			base.OnExposeEvent(ev);
			CalculateSize();
			weekends = CalculateWeekends(month, year);

			using (Cairo.Context g = Gdk.CairoHelper.Create(this.GdkWindow))
			{
				FillWhite(g);
				FillWeekends();
				FillHeaders();
				FillBackLight();
				FillNow();
				FillActiveCells();
				DrawHeaders(g);
				DrawGrid(g);

				g.GetTarget().Dispose();
			}
			return true;
		}

		protected override bool OnButtonPressEvent(Gdk.EventButton ev)
		{
			// Insert button press handling code here.
			HalfCell cell = activeHalfCells.FirstOrDefault(ahc => ahc.Equals(GetHalfCell((int)ev.X, (int)ev.Y)));
			if (cell == null)
			{
				isDraw = DrawState.Draw;
			} else {
				isDraw = DrawState.Erase;
			}
			ActivateOrDeactivateHalfCell(GetHalfCell((int)ev.X, (int)ev.Y), isDraw);

			return base.OnButtonPressEvent(ev);
		}

		protected override bool OnButtonReleaseEvent(EventButton evnt)
		{
			isDraw = DrawState.Nothing;
			return base.OnButtonReleaseEvent(evnt);
		}

		protected override bool OnMotionNotifyEvent(Gdk.EventMotion evnt)
		{
			HalfCell hc = GetHalfCell((int)evnt.X, (int)evnt.Y);

			if (!mouseOnCell.Equals(hc))
			{
				mouseOnCell = hc;
				ActivateOrDeactivateHalfCell(GetHalfCell((int)evnt.X, (int)evnt.Y), isDraw);
				this.QueueDraw();
			}

			return base.OnMotionNotifyEvent(evnt);
		}

		protected override bool OnLeaveNotifyEvent(EventCrossing evnt)
		{
			CellNumber cn = new CellNumber{ X = -1, Y = -1 };
			mouseOnCell.CellNumber = cn;
			this.QueueDraw();
			return true;
		}

		#endregion

		#region Заполнение цветом

		private void FillWhite(Cairo.Context g)
		{
			g.SetSourceRGB(1, 1, 1);
			g.Rectangle(new PointD(0, 0), this.Allocation.Size.Width, this.Allocation.Size.Height);
			g.Fill();
		}

		private void FillWeekends()
		{
			for (int i = 0; i < weekends.Length; i++)
			{
				if (weekends[i] == 0)
					return;
				FillColumn(weekends[i], new Cairo.Color(0.7, 0.7, 0.7));
			}
		}

		private void FillColumn (int columnNumber, Cairo.Color color)
		{
			for (int i = 0; i < totalRows; i++)
			{
				PointD p = GetCellCoordinates(i, columnNumber);
				FillCell(p, color);
			}
		}

		private void FillRow (int rowNumber, Cairo.Color color)
		{
			for (int i = 0; i < totalColums; i++)
			{
				PointD p = GetCellCoordinates(rowNumber, i);
				FillCell(p, color);
			}
		}

		private void FillHeaders ()
		{
			Cairo.Color color = new Cairo.Color(0.5, 0.5, 1);
			FillRow(0, color);
			FillRow(1, color);
			FillColumn(0, color);
		}

		private void FillBackLight ()
		{
			if (mouseOnCell.CellNumber.X < 0 || mouseOnCell.CellNumber.Y < 0)
				return;
			if (mouseOnCell.CellNumber.X >= totalColums || mouseOnCell.CellNumber.Y >= totalRows)
				return;

			FillRow(mouseOnCell.CellNumber.Y, new Cairo.Color(1, 0.5, 0.5));
			FillColumn(mouseOnCell.CellNumber.X, new Cairo.Color(1, 0.5, 0.5));
		}

		private void FillCell(PointD coord, Cairo.Color color)
		{
			using (Cairo.Context g = Gdk.CairoHelper.Create(this.GdkWindow))
			{
				g.SetSourceColor(color);
				g.MoveTo(coord);
				g.Rectangle(coord,cellWidth, cellHeight);
				g.Fill();
				g.GetTarget().Dispose();
			}
		}

		private void FillCell(CellNumber cell, Cairo.Color color)
		{
			using (Cairo.Context g = Gdk.CairoHelper.Create(this.GdkWindow))
			{
				g.SetSourceColor(color);
				g.MoveTo(GetCellCoordinates(cell.Y, cell.X));
				g.Rectangle(GetCellCoordinates(cell.Y, cell.X), cellWidth, cellHeight);
				g.Fill();
				g.GetTarget().Dispose();
			}
		}

		private void FillActiveCells () {
			foreach (var cell in activeHalfCells)
			{
				FillHalf(cell, new Cairo.Color(0.2, 1, 0.2));
			}
		}

		private void FillNow ()
		{
			Cairo.Color color = new Cairo.Color(1, 0.5, 1);

			DateTime d = DateTime.Now;
			if (d.Month != Date.Month || d.Year != Date.Year)
				return;
			int row = DateTime.Now.Hour + topHeadersCount;
			int column = d.Day;

			Half half;
			if (d.Minute >= 30)
				half = Half.Bottom;
			else
				half = Half.Top;

			FillHalf(GetCellCoordinates(row, column), color, half);
		}

		private void FillHalf (PointD coord, Cairo.Color color, Half half)
		{
			if (half == Half.Bottom)
				coord.Y += cellHeight / 2;
			
			using (Cairo.Context g = Gdk.CairoHelper.Create(this.GdkWindow))
			{
				g.SetSourceColor(color);
				g.MoveTo(coord);
				g.Rectangle(coord, cellWidth, cellHeight / 2);
				g.Fill();
				g.GetTarget().Dispose();
			}
		}

		private void FillHalf (HalfCell cell,Cairo.Color color)
		{
			PointD coord = GetCellCoordinates(cell.CellNumber.Y, cell.CellNumber.X);
			if (cell.half == Half.Bottom)
				coord.Y += cellHeight / 2;

			using (Cairo.Context g = Gdk.CairoHelper.Create(this.GdkWindow))
			{
				g.SetSourceColor(color);
				g.MoveTo(coord);
				g.Rectangle(coord, cellWidth, cellHeight / 2);
				g.Fill();
				g.GetTarget().Dispose();
			}
		}

		#endregion

		#region Методы рисования

		private void DrawGrid(Cairo.Context g)
		{
			if (daysInMonth < 28 && daysInMonth > 32)
				return;

			g.SetSourceRGB(0, 0, 0);
			for (int i = 0; i <= totalColums; i++)
			{
				PointD pTop = new PointD(cellWidth * i, 0);
				PointD pBot = new PointD(cellWidth * i, tableHeight);

				g.MoveTo(pTop);
				g.LineTo(pBot);
			}
			for (int i = 0; i <= totalRows; i++)
			{
				PointD pTop = new PointD(0, cellHeight * i);
				PointD pBot = new PointD(tableWidth, cellHeight * i);

				g.MoveTo(pTop);
				g.LineTo(pBot);
			}
			g.ClosePath();
			g.Stroke();
		}

		private void DrawHeaders(Cairo.Context g)
		{
			DrawTopHeaders(g);
			DrawLeftHeaders(g);
		}

		private void DrawTopHeaders(Cairo.Context g)
		{
			int startCellIndex = leftHeadersCount;

			int i = startCellIndex;
			foreach (var item in headerData)
			{
				DrawText(g, item, 0, i);
				i++;
			}

			for (int j = startCellIndex; j <= daysInMonth; j++) {
				DrawText(g, j.ToString(), 1, j);
			}
		}

		private void DrawLeftHeaders(Cairo.Context g)
		{
			int startCellIndex = topHeadersCount;

			for (int i = startCellIndex, hour = 0; i < totalRows; i++, hour++) {
				DrawText(g, hour.ToString() + ":00", i, 0);
			}
		}

		private void DrawText (Cairo.Context g, string text, int top, int left)
		{
			g.SetSourceRGB(0, 0, 0);
			g.SetFontSize(cellHeight / 1.5);
			FontExtents fe = g.FontExtents;
			TextExtents te = g.TextExtents(text);
			g.MoveTo(left * cellWidth +(cellWidth - te.Width) / 2, top * cellHeight + (cellHeight + te.YBearing + fe.Height) / 2);
			g.ShowText(text);
		}

		private void DrawText (string text, int top, int left)
		{
			using (Cairo.Context g = Gdk.CairoHelper.Create(this.GdkWindow))
			{
				g.SetSourceRGB(0, 0, 0);
				g.SetFontSize(cellHeight / 1.5);
				FontExtents fe = g.FontExtents;
				TextExtents te = g.TextExtents(text);
				g.MoveTo(left * cellWidth + (cellWidth - te.Width) / 2, top * cellHeight + (cellHeight + te.YBearing + fe.Height) / 2);
				g.ShowText(text);

				g.GetTarget().Dispose();
			}
		}

		#endregion

		#region Вычисления

		private void CalculateSize() {
			daysInMonth = DateTime.DaysInMonth(year, month);
			totalColums = daysInMonth + leftHeadersCount;
			totalRows	= 24 + topHeadersCount;
			cellWidth	= this.Allocation.Size.Width / totalColums;
			cellHeight	= this.Allocation.Size.Height / totalRows;
			tableWidth 	= cellWidth * totalColums;
			tableHeight = cellHeight * totalRows;


			DateTime t = new DateTime(year, month, 1);
			List<string> daysName = new List<string>();
			for (int i = 0; i < daysInMonth; i++)
			{
				daysName.Add(string.Format("{0:ddd}", t));
				t = t.AddDays(1);
			}
			headerData = daysName;
		}

		private int[] CalculateWeekends(int month, int year)
		{
			int[] result = new int[10];

			int i = 0;
			for (DateTime dt = new DateTime(year, month, 1); dt.Month == month; dt = dt.AddDays(1))
			{
				if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
				{
					result[i] = dt.Day;
					i++;
				}
			}
			return result;
		}

		private PointD GetCellCoordinates(int numberTop, int numberLeft)
		{
			int left = numberLeft * cellWidth;
			int top = numberTop * cellHeight;
			return new PointD(left, top);
		}

		private HalfCell GetHalfCell(int x, int y)
		{
			return new HalfCell
			{
				CellNumber = new CellNumber
				{ 
					X = GetCellColumnNumber(x),
					Y = GetCellRowNumber(y)
				},
				half = y % cellHeight > cellHeight / 2 ? Half.Bottom : Half.Top
			};
		}

		private int GetCellColumnNumber(int pointX)
		{
			return pointX / cellWidth;
		}

		private int GetCellRowNumber(int pointY)
		{
			return pointY / cellHeight;
		}

		public void SetTopHeaderData(IEnumerable<string> data, int headerIndex)
		{
			headerData = data;
		}

		private void ActivateOrDeactivateHalfCell(HalfCell cell, DrawState action)
		{
			if (action == DrawState.Nothing)
				return;
			if (CellIsHeader(cell))
				return;
			
			HalfCell ahc = activeHalfCells.FirstOrDefault(ac => ac.Equals(cell));
			
			if (action == DrawState.Draw)
			{
				if (ahc == null)
					activeHalfCells.Add(cell);
			}
			if(action == DrawState.Erase)
			{
				if (ahc != null)
					activeHalfCells.Remove(ahc);
			}

			this.QueueDraw();
		}

		private bool CellIsHeader(HalfCell cell)
		{
			if (cell.CellNumber.X >= 0 && cell.CellNumber.X < leftHeadersCount)
				return true;
			if (cell.CellNumber.Y >= 0 && cell.CellNumber.Y < topHeadersCount)
				return true;
			if (cell.CellNumber.X >= totalColums)
				return true;
			if (cell.CellNumber.Y >= totalRows)
				return true;
			return false;
		}

		#endregion
  	}
}

