using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Table : Fb2Container
    {
        public override string Name => ElementNames.Table;

        public override bool CanContainText => false;

        public override ImmutableHashSet<string> AllowedAttributes => ImmutableHashSet.Create(AttributeNames.Id);

        public override ImmutableHashSet<string> AllowedElements => ImmutableHashSet.Create(ElementNames.TableRow);

        public sealed override string ToString()
        {
            if (IsEmpty)
                return string.Empty;

            // ommiting unsafe stuff etc
            var rows = GetChildren<TableRow>().ToList();

            var rowsCount = rows.Count;

            if (rowsCount == 0)
                return string.Empty;

            var tableRowSpans = new Dictionary<int, List<TableCellModel>>();
            var columnCountsInRow = new int[rowsCount]; // total number of columns in a row, at row's index

            // scanning cycle(s) #1, collecting spans and creating meta-table
            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                var collSpanDelta = 0;

                var row = rows[rowIndex];

                if (row == null)
                    continue;

                var cellsInRow = row.GetChildren<TableCellBase>().ToList();

                var rowModel = new List<TableCellModel>();

                // array index position is not indicative of actual cell position in table
                for (int columnIndex = 0; columnIndex < cellsInRow.Count; columnIndex++)
                {
                    var renderColumnIndex = collSpanDelta != 0 ? columnIndex + collSpanDelta : columnIndex;

                    var cellNode = cellsInRow[columnIndex];

                    renderColumnIndex = ApplyColumnSpanDelta(tableRowSpans.Values.SelectMany(v => v).ToList(), rowIndex, renderColumnIndex);

                    var collSpanNum = GetSpan(cellNode, AttributeNames.ColumnSpan);

                    // to know how far to move next cell in table row - save "total span" number in row
                    // if no ColSpan -> collSpanNum - 1 = 0
                    collSpanDelta += collSpanNum - 1; // -1 because of cell itself has width

                    var rowSpanNum = GetSpan(cellNode, AttributeNames.RowSpan);

                    var cellContent = cellNode.ToString();

                    rowModel.Add(
                        new TableCellModel(
                            columnIndex,
                            rowIndex,
                            renderColumnIndex,
                            cellContent,
                            collSpanNum,
                            rowSpanNum));

                    // check how many actual columns are in row - take column spans into account, colSpan=2 means there's single cell for 2 columns
                    var colCount = columnCountsInRow[rowIndex];
                    var actualColCount = renderColumnIndex + collSpanNum; // no -1 due to 0 index column is one column already, count, not index!
                    columnCountsInRow[rowIndex] = Math.Max(colCount, actualColCount); // if there's no collSpan then "collSpanNum - 1 = 0"
                }

                tableRowSpans.Add(rowIndex, rowModel);
            }

            // preventive measure
            var columnsCountInTable = columnCountsInRow.Max();

            // check width of content of each column in literal characters count, string length
            // to make column uniform, we need to select max width
            var columnCharWidths = new int[columnsCountInTable];

            for (var actualColIndex = 0; actualColIndex < columnsCountInTable; actualColIndex++)
            {
                for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
                {
                    var row = tableRowSpans[rowIndex];
                    var columnCell = row.FirstOrDefault(cm =>
                        cm.RenderColumnStartIndex <= actualColIndex && cm.RenderColumnEndIndex >= actualColIndex);

                    if (columnCell == null)
                    {
                        var affectiveRowsCells = tableRowSpans
                               .Values
                               .SelectMany(l => l)
                               .Where(trs => trs.StartRowIndex < rowIndex && trs.EndRowIndex >= rowIndex);

                        columnCell = affectiveRowsCells.FirstOrDefault(cm => cm.RenderColumnStartIndex <= actualColIndex && cm.RenderColumnEndIndex >= actualColIndex);

                        if (columnCell == null)
                            continue;
                    }

                    var colCharWidth = columnCell.RenderColumnStartIndex == actualColIndex ?
                                        columnCell.Content.Length : 0;

                    var prev = columnCharWidths[actualColIndex];
                    columnCharWidths[actualColIndex] = Math.Max(prev, colCharWidth);
                }
            }

            var rowStrings = new List<string>(rowsCount);

            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                var row = tableRowSpans[rowIndex];
                var rowString = new StringBuilder();

                for (var actualColIndex = 0; actualColIndex < columnsCountInTable; actualColIndex++)
                {
                    if (actualColIndex == 0)
                        rowString.Append('|');

                    var columnCell = row.FirstOrDefault(cm => cm.RenderColumnStartIndex <= actualColIndex && cm.RenderColumnEndIndex >= actualColIndex);
                    if (columnCell == null)
                    {
                        var affectiveRowsCells = tableRowSpans
                            .Values
                            .SelectMany(l => l)
                            .Where(trs => trs.StartRowIndex < rowIndex && trs.EndRowIndex >= rowIndex);

                        columnCell = affectiveRowsCells.FirstOrDefault(cm => cm.RenderColumnStartIndex <= actualColIndex && cm.RenderColumnEndIndex >= actualColIndex);

                        if (columnCell == null)
                            continue;
                    }

                    var colWidth = columnCharWidths[actualColIndex];

                    var hasCollSpan = columnCell.ColSpan > 1;
                    var hasRowSpan = columnCell.RowSpan > 1;
                    var shouldPrintColSpanSpace = columnCell.RenderColumnStartIndex < actualColIndex && hasCollSpan;
                    var shouldPrintRowSpanSpace = columnCell.StartRowIndex < rowIndex && hasRowSpan;

                    if (shouldPrintColSpanSpace || shouldPrintRowSpanSpace)
                    {
                        var shouldPrintVerticalBorder = columnCell.RenderColumnEndIndex == actualColIndex;

                        var paddingCount = shouldPrintVerticalBorder ? colWidth : colWidth + 1; // 
                        var paddedCellContent = string.Empty.PadRight(paddingCount, ' ');

                        var cellString = shouldPrintVerticalBorder ? $"{paddedCellContent}|" : paddedCellContent;
                        rowString.Append(cellString);
                    }
                    else
                    {
                        var content = columnCell.Content;
                        var shouldPrintVerticalBorder = columnCell.ColSpan == 1;
                        var paddingCount = shouldPrintVerticalBorder ? colWidth : colWidth + 1;
                        var paddedCellContent = content.PadRight(paddingCount, ' ');
                        var cellString = shouldPrintVerticalBorder ? $"{paddedCellContent}|" : paddedCellContent;
                        rowString.Append(cellString);
                    }
                }

                rowStrings.Add(rowString.ToString());
            }

            var result = string.Join(Environment.NewLine, rowStrings);
            return result;
        }

        // retun int always instead of bool, if no span found return 1 as minimal cell unit width/height
        private int GetSpan(Fb2Node cell, string spanAttrName)
        {
            if (cell == null || string.IsNullOrWhiteSpace(spanAttrName))
                throw new ArgumentNullException();

            if (cell.TryGetAttribute(spanAttrName, out var span, true) &&
                int.TryParse(span.Value, out int spanNumber) &&
                spanNumber > 1)
            {
                return spanNumber;
            }

            return 1;
        }

        private int ApplyColumnSpanDelta(List<TableCellModel> tableRowSpans, int rowIndex, int resultingColumn)
        {
            if (!tableRowSpans.Any())
                return resultingColumn;

            var effectiveRowSpans = tableRowSpans.Where(trs => trs.StartRowIndex < rowIndex && trs.EndRowIndex >= rowIndex);

            if (effectiveRowSpans == null || !effectiveRowSpans.Any())
                return resultingColumn;

            var orderedActuals = effectiveRowSpans.OrderBy(trs => trs.RenderColumnStartIndex).ThenBy(trs => trs.StartRowIndex);

            foreach (var cellIndexDelta in orderedActuals)
            {
                if (cellIndexDelta.CellArrayIndex > resultingColumn || // check if position in array
                    cellIndexDelta.RenderColumnStartIndex > resultingColumn) // or rendering position is further in table (if position in array is further it's definitely more to the end of a table)
                    break; // so actual cell in work is not affected anyways, and we can break due to list is ordered

                if (cellIndexDelta.ColSpan != 0)
                    resultingColumn += Math.Max(cellIndexDelta.ColSpan - 1, 0); // -1 because cell itself always will take at least 1 unit of width, whatewer "unit of width" is

                resultingColumn++;
            }

            return resultingColumn;
        }

        private class TableCellModel
        {
            public int StartRowIndex { get; }
            public int EndRowIndex { get; }
            public int RowSpan { get; }

            public int CellArrayIndex { get; } // index of cell in row.Content
            public int RenderColumnStartIndex { get; } // render column start position
            public int RenderColumnEndIndex { get; }
            public int ColSpan { get; }

            public string Content { get; } // stringnified cell content

            public TableCellModel(
                int cellArrayIndex,
                int rowIndex,
                int actualColumnIndex,
                string content,
                int colSpan,
                int rowSpan)
            {
                CellArrayIndex = cellArrayIndex;
                StartRowIndex = rowIndex;
                EndRowIndex = rowIndex + (rowSpan - 1);
                RenderColumnStartIndex = actualColumnIndex;

                ColSpan = colSpan;
                RowSpan = rowSpan;

                RenderColumnEndIndex = RenderColumnStartIndex + (colSpan - 1);

                Content = content;
            }
        }
    }
}
