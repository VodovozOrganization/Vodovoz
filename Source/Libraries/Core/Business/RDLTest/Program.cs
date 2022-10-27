using System;
using Vodovoz.RDL.Elements;
using Vodovoz.RDL.Utilities;

namespace RDLTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

			QueryParameter sdsd = new QueryParameter();
			sdsd.ItemsList.Add("tttttyyy");
			sdsd.Name = "Test sdsd";
			var qp1111 = Functions.ToXElement<QueryParameter>(sdsd);

			Style style = new Style();
			style.BackgroundColor = "dgfgsfd";
			style.BackgroundGradientEndColor = "BackgroundGradientEndColor232323";
			var styleElement = Functions.ToXElement<Style>(style);

			Header header = new Header();
			TableRow row = new TableRow();
			TableCell cell = new TableCell();
			row.Cells.Add(cell);
			header.TableRows.Add(row);
			var headerElement = Functions.ToXElement<Header>(header);

			

		}
	}
}
