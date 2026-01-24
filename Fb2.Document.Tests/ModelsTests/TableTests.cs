using System;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests;

public class TableTests
{

    // -------------
    // |0.0|0.1|0.2|
    // |---|---|---|
    // |1.0|1.1|1.2|
    // |---|---|---|
    // |2.0|2.1|2.2|
    // -------------
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
            .Should()
            .NotBeNullOrWhiteSpace()
            .And
            .Be($"-------------{Environment.NewLine}|0.0|0.1|0.2|{Environment.NewLine}|---|---|---|{Environment.NewLine}|1.0|1.1|1.2|{Environment.NewLine}|---|---|---|{Environment.NewLine}|2.0|2.1|2.2|{Environment.NewLine}-------------{Environment.NewLine}");
    }

    // ---------------
    // |0.0|  0.1.2  |
    // |---|         |
    // |1.0|         |
    // |---|---------|
    // |2.0| 2.1 |2.2|
    // ---------------
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
            .Should()
            .NotBeNullOrWhiteSpace()
            .And
            .Be($"---------------{Environment.NewLine}|0.0|  0.1.2  |{Environment.NewLine}|---|         |{Environment.NewLine}|1.0|         |{Environment.NewLine}|---|---------|{Environment.NewLine}|2.0| 2.1 |2.2|{Environment.NewLine}---------------{Environment.NewLine}");
    }

    // ---------------
    // |0.0|         |
    // |---|         |
    // |1.0|    0.1.2|
    // |---|---------|
    // |2.0| 2.1 |2.2|
    // ---------------
    [Fact]
    public void Table3x3_WithColumn_AndRow_Spans_And_Alignments_ToString()
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
                    new Fb2Attribute(AttributeNames.RowSpan, "2"),
                    new Fb2Attribute(AttributeNames.VerticalAlign, "Bottom"),
                    new Fb2Attribute(AttributeNames.Align, "Right"));

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
            .Should()
            .NotBeNullOrWhiteSpace()
            .And
            .Be($"---------------{Environment.NewLine}|0.0|         |{Environment.NewLine}|---|         |{Environment.NewLine}|1.0|    0.1.2|{Environment.NewLine}|---|---------|{Environment.NewLine}|2.0| 2.1 |2.2|{Environment.NewLine}---------------{Environment.NewLine}");
    }

    // -----------------
    // | 0.0 |         |
    // |-----|         |
    // | 1.0 |    0.1.2|
    // |---------------|
    // |2.1.2      |2.0|
    // |           |---|
    // |           |3.0|
    // -----------------
    [Fact]
    public void Table4x3_Multiple_Spans_And_Alignments()
    {
        var table = new Table();

        for (int i = 0; i < 4; i++)
        {
            var tableRow = new TableRow();

            if (i == 0) // first row with rowspan + colspan on last cell
            {
                var firstCell = new TableCell();
                firstCell.AddTextContent($"{i}.0");

                var secondCellWithSpans = new TableCell();
                secondCellWithSpans.AddTextContent($"{i}.1.2");
                secondCellWithSpans.AddAttributes(
                    new Fb2Attribute(AttributeNames.ColumnSpan, "2"),
                    new Fb2Attribute(AttributeNames.RowSpan, "2"),
                    new Fb2Attribute(AttributeNames.VerticalAlign, "Bottom"),
                    new Fb2Attribute(AttributeNames.Align, "Right"));

                tableRow.AddContent(firstCell, secondCellWithSpans);
            }

            if (i == 1 || i == 3)
            {
                var firstCell = new TableCell();
                firstCell.AddTextContent($"{i}.0");
                tableRow.AddContent(firstCell);
            }

            if (i == 2)
            {
                var firstCellWithSpans = new TableCell();
                firstCellWithSpans.AddTextContent($"{i}.1.2");
                firstCellWithSpans.AddAttributes(
                    new Fb2Attribute(AttributeNames.ColumnSpan, "2"),
                    new Fb2Attribute(AttributeNames.RowSpan, "2"),
                    new Fb2Attribute(AttributeNames.VerticalAlign, "Top"),
                    new Fb2Attribute(AttributeNames.Align, "Left"));

                tableRow.AddContent(firstCellWithSpans);

                var secondCells = new TableCell();
                secondCells.AddTextContent($"{i}.0");
                tableRow.AddContent(secondCells);
            }

            table.AddContent(tableRow);
        }
        var tableString = table.ToString();
        tableString
            .Should()
            .NotBeNullOrWhiteSpace()
            .And
            .Be($"-----------------{Environment.NewLine}| 0.0 |         |{Environment.NewLine}|-----|         |{Environment.NewLine}| 1.0 |    0.1.2|{Environment.NewLine}|---------------|{Environment.NewLine}|2.1.2      |2.0|{Environment.NewLine}|           |---|{Environment.NewLine}|           |3.0|{Environment.NewLine}-----------------{Environment.NewLine}");
    }

    // -------------
    // |0.0|0.1|0.2|
    // |---|---|---|
    // |1.0|1.1|1.2|
    // |------------
    [Fact]
    public void Table_ToString_IgnoresEmptyRows()
    {
        var table = new Table();

        for (int i = 0; i < 3; i++)
        {
            var tableRow = new TableRow();

            if (i < 2)
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
            .Should()
            .NotBeNullOrWhiteSpace()
            .And
            .Be($"-------------{Environment.NewLine}|0.0|0.1|0.2|{Environment.NewLine}|---|---|---|{Environment.NewLine}|1.0|1.1|1.2|{Environment.NewLine}|------------{Environment.NewLine}");
    }

    [Fact]
    public void Table_ToString_IgnoresNotTableRows()
    {
        var table = new Table();

        var cell1XElement = new XElement(ElementNames.TableCell, "1");
        var cell2XElement = new XElement(ElementNames.TableCell, "2");
        var tableRowXElement = new XElement(ElementNames.TableRow, cell1XElement, cell2XElement);

        var unsafeParagraphXElement = new XElement(ElementNames.Paragraph, "test parag text.");

        var tableXElement = new XElement(ElementNames.Table, tableRowXElement, unsafeParagraphXElement);

        table.Load(tableXElement);

        var tableString = table.ToString();
        tableString
            .Should()
            .NotBeNullOrWhiteSpace()
            .And
            .Be($"-----{Environment.NewLine}|1|2|{Environment.NewLine}-----{Environment.NewLine}");
    }

    [Fact]
    public void Table_WithoutRows_ToString_ReturnEmptyString()
    {
        var table = new Table();

        var unsafeParagraphXElement = new XElement(ElementNames.Paragraph, "test parag text.");

        var tableXElement = new XElement(ElementNames.Table, unsafeParagraphXElement);

        table.Load(tableXElement);

        var tableString = table.ToString();
        tableString.Should().BeEmpty();
    }

    // --------------
    // |    |0.1|0.2|
    // |    |---|---|
    // |    |1.0|1.1|
    // |    |---|---|
    // |0.0 |2.0|2.1|
    // |    |---|---|
    // |    |3.0|3.1|
    // --------------
    [Fact]
    public void SimpleTable4x4_ToString()
    {
        var table = new Table();

        // Generate table
        for (int i = 0; i < 4; i++)
        {
            var tableRow = new TableRow();

            if (i == 0)
            {
                var firstCellWithSpans = new TableCell();
                firstCellWithSpans.AddTextContent("0.0");
                firstCellWithSpans.AddAttributes(
                    new Fb2Attribute(AttributeNames.ColumnSpan, "2"),
                    new Fb2Attribute(AttributeNames.RowSpan, "4"),
                    new Fb2Attribute(AttributeNames.VerticalAlign, "Middle"));

                var secondCell = new TableCell();
                secondCell.AddTextContent($"{i}.1");

                var thirdCell = new TableCell();
                thirdCell.AddTextContent($"{i}.2");

                tableRow.AddContent(firstCellWithSpans, secondCell, thirdCell);
            }
            else
            {
                for (int j = 0; j < 2; j++)
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
            .Should()
            .NotBeNullOrWhiteSpace()
            .And
            .Be($"--------------{Environment.NewLine}|    |0.1|0.2|{Environment.NewLine}|    |---|---|{Environment.NewLine}|    |1.0|1.1|{Environment.NewLine}|    |---|---|{Environment.NewLine}|0.0 |2.0|2.1|{Environment.NewLine}|    |---|---|{Environment.NewLine}|    |3.0|3.1|{Environment.NewLine}--------------{Environment.NewLine}");
    }
}
