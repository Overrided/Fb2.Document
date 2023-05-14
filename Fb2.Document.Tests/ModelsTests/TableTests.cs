using System;
using System.Collections.Generic;
using System.Text;
using Fb2.Document.Constants;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests
{
    public class TableTests
    {
        [Fact]
        public void SimpleTable3x3_ToString()
        {
            var table = new Table();

            // Generate table
            for (int i = 0; i < 3; i++)
            {
                var tableRow = new TableRow();

                for (int j = 0; j < 3; j++)
                {
                    var cell = new TableCell();
                    cell.AddTextContent($"{i}.{j}");
                    tableRow.AddContent(cell);
                }

                table.AddContent(tableRow);
            }

            var tableString = table.ToString();
            tableString
                .Should().NotBeNullOrEmpty()
                .And
                .Be("-------------\r\n|0.0|0.1|0.2|\r\n|---|---|---|\r\n|1.0|1.1|1.2|\r\n|---|---|---|\r\n|2.0|2.1|2.2|\r\n-------------\r\n");
        }

        [Fact]
        public void Table3x3_WithColumn_AndRow_Spans_ToString()
        {
            var table = new Table();

            // Generate table
            for (int i = 0; i < 3; i++)
            {
                var tableRow = new TableRow();

                if (i == 0) // first row with rowspan + colspan cell
                {
                    var firstCell = new TableCell();
                    firstCell.AddTextContent($"{i}.0");

                    var secondCellWithSpans = new TableCell();
                    secondCellWithSpans.AddTextContent($"{i}.1.2");
                    secondCellWithSpans.AddAttributes(
                        new Fb2Attribute(AttributeNames.ColumnSpan, "2"),
                        new Fb2Attribute(AttributeNames.RowSpan, "2"));

                    tableRow.AddContent(firstCell, secondCellWithSpans);
                }

                if (i == 1) // second row wit 1 cell only
                {
                    var firstCell = new TableCell();
                    firstCell.AddTextContent($"{i}.0");
                    tableRow.AddContent(firstCell);
                }

                if (i == 2) // third row with 3 cells
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var cell = new TableCell();
                        cell.AddTextContent($"{i}.{j}");
                        tableRow.AddContent(cell);
                    }
                }

                table.AddContent(tableRow);
            }

            var tableString = table.ToString();
            tableString
                .Should().NotBeNullOrEmpty()
                .And
                .Be("---------------\r\n|0.0|  0.1.2  |\r\n|---|         |\r\n|1.0|         |\r\n|---|---------|\r\n|2.0| 2.1 |2.2|\r\n---------------\r\n");
        }

    }
}
