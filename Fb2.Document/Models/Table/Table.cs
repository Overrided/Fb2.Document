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

            var metaTable = BuildMetadataTable(rows);

            var tableRowSpans = metaTable.MetaTable;
            var columnsCount = metaTable.ColumnsCount;

            // scanning cycle #2, 
            // check width of content of each column in literal characters count, string length
            // to make column uniform, we need to select max width
            var columnCharWidths = GetCharacterColumnWidths(tableRowSpans, rowsCount, columnsCount);

            // printing cycle
            var result = StringnifyTableRows(tableRowSpans, columnCharWidths, rowsCount, columnsCount);
            return result;
        }

        private static (List<TableCellModel> MetaTable, int ColumnsCount) BuildMetadataTable(List<TableRow> rows)
        {
            var rowsCount = rows.Count;

            var tableRowSpans = new List<TableCellModel>();
            var columnCountsInRow = new int[rowsCount]; // total number of columns in a row, at row's index

            // scanning cycle(s) #1, collecting spans and creating meta-table
            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                var row = rows[rowIndex];

                if (row == null)
                    continue;

                var collSpanDeltaInRow = 0;

                var cellsInRow = row.GetChildren<TableCellBase>().ToList();
                var cellsInRowCount = cellsInRow.Count;

                if (cellsInRowCount == 0)
                    continue; // skip empty rows

                // array index position is not indicative of actual cell position in table
                for (int arrayColumnIndex = 0; arrayColumnIndex < cellsInRowCount; arrayColumnIndex++)
                {
                    var cellNode = cellsInRow[arrayColumnIndex];

                    var renderColumnIndex = collSpanDeltaInRow != 0 ? arrayColumnIndex + collSpanDeltaInRow : arrayColumnIndex;
                    renderColumnIndex = UpdateRenderColumnIndex(tableRowSpans, rowIndex, renderColumnIndex);

                    var collSpanNum = GetNodeSpan(cellNode, AttributeNames.ColumnSpan);
                    columnCountsInRow[rowIndex] = Math.Max(columnCountsInRow[rowIndex], renderColumnIndex + 1); // absolute unit width of a cell

                    // to know how far to move next cell in table row - save "total span" number in row
                    // if no ColSpan -> collSpanNum - 1 = 0
                    collSpanDeltaInRow += collSpanNum - 1; // -1 because of cell itself has width

                    var rowSpanNum = GetNodeSpan(cellNode, AttributeNames.RowSpan);
                    var cellContent = cellNode.ToString();

                    tableRowSpans.Add(
                        new TableCellModel(
                            arrayColumnIndex,
                            rowIndex,
                            renderColumnIndex,
                            cellContent,
                            collSpanNum,
                            rowSpanNum));
                }
            }

            // prevention to some miscalculation we can fix, but with performance penalties
            var columnsCount = columnCountsInRow.Max();

            return (MetaTable: tableRowSpans, ColumnsCount: columnsCount);
        }

        // if no span found return 1 as minimal cell unit width/height
        private static int GetNodeSpan(Fb2Node cell, string spanAttrName)
        {
            if (cell == null || string.IsNullOrWhiteSpace(spanAttrName))
                throw new ArgumentNullException();

            if (cell.TryGetAttribute(spanAttrName, out var span, true) &&
                int.TryParse(span.Value, out int spanNumber) && spanNumber > 1)
                return spanNumber;

            return 1;
        }

        private static int UpdateRenderColumnIndex(List<TableCellModel> tableRowSpans, int rowIndex, int resultingColumn)
        {
            if (!tableRowSpans.Any())
                return resultingColumn;

            var effectiveRowSpans = tableRowSpans
                .Where(trs => trs.StartRowIndex < rowIndex && trs.EndRowIndex >= rowIndex);

            if (effectiveRowSpans == null || !effectiveRowSpans.Any())
                return resultingColumn;

            var orderedActuals = effectiveRowSpans
                .OrderBy(trs => trs.RenderColumnStartIndex)
                .ThenBy(trs => trs.StartRowIndex);

            foreach (var cellIndexDelta in orderedActuals)
            {
                if (cellIndexDelta.CellArrayIndex > resultingColumn || // check if position in array
                    cellIndexDelta.RenderColumnStartIndex > resultingColumn) // or rendering position is further in table (if position in array is further it's definitely more to the end of a table)
                    break; // so actual cell in work is not affected anyways, and we can break due to list is ordered

                if (cellIndexDelta.ColSpan > 1)
                    resultingColumn += Math.Max(cellIndexDelta.ColSpan - 1, 0); // -1 because cell itself always will take at least 1 unit of width, whatewer "unit of width" is

                resultingColumn++;
            }

            return resultingColumn;
        }

        private static bool TableCellPredicate(TableCellModel cellModel, int rowIndex, int colunmIndex) =>
            cellModel.RenderColumnStartIndex <= colunmIndex &&
            cellModel.RenderColumnEndIndex >= colunmIndex &&
            cellModel.StartRowIndex <= rowIndex &&
            cellModel.EndRowIndex >= rowIndex;

        private static int[] GetCharacterColumnWidths(List<TableCellModel> tableRowSpans, int rowsCount, int columnsCount)
        {
            var columnCharWidths = new int[columnsCount];

            for (var actualColIndex = 0; actualColIndex < columnsCount; actualColIndex++)
            {
                for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
                {
                    var columnCell = tableRowSpans.FirstOrDefault(cm => TableCellPredicate(cm, rowIndex, actualColIndex));
                    if (columnCell == null)
                        continue;

                    var colCharWidth = columnCell.RenderColumnStartIndex == actualColIndex ?
                                        columnCell.Content.Length : 0;

                    var prev = columnCharWidths[actualColIndex];
                    columnCharWidths[actualColIndex] = Math.Max(prev, colCharWidth);
                }
            }

            return columnCharWidths;
        }

        private static string StringnifyTableRows(
            List<TableCellModel> tableRowSpans,
            int[] columnCharWidths,
            int rowsCount,
            int columnsCount)
        {
            var rowStrings = new StringBuilder();

            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                var rowString = new StringBuilder();

                for (var actualColIndex = 0; actualColIndex < columnsCount; actualColIndex++)
                {
                    var cell = tableRowSpans.FirstOrDefault(cm => TableCellPredicate(cm, rowIndex, actualColIndex));

                    if (cell == null)
                        continue;

                    if (actualColIndex == 0)
                        rowString.Append('|');

                    var colWidth = columnCharWidths[actualColIndex];

                    var cellString = StringnifyTableCell(cell, rowIndex, actualColIndex, colWidth);
                    rowString.Append(cellString);
                }

                rowStrings.AppendLine(rowString.ToString());
            }

            return rowStrings.ToString();
        }

        private static string StringnifyTableCell(TableCellModel cell, int rowIndex, int colIndex, int columnCharWidth)
        {
            var hasCollSpan = cell.ColSpan > 1;
            var hasRowSpan = cell.RowSpan > 1;
            var shouldPrintColSpanSpace = cell.RenderColumnStartIndex < colIndex && hasCollSpan;
            var shouldPrintRowSpanSpace = cell.StartRowIndex < rowIndex && hasRowSpan;

            bool shouldPrintVerticalBorder;
            string contentToPrint;

            if (shouldPrintColSpanSpace || shouldPrintRowSpanSpace)
            {
                shouldPrintVerticalBorder = cell.RenderColumnEndIndex == colIndex;
                contentToPrint = string.Empty;
            }
            else
            {
                shouldPrintVerticalBorder = cell.ColSpan == 1;
                contentToPrint = cell.Content;
            }

            var paddingCount = shouldPrintVerticalBorder ? columnCharWidth : columnCharWidth + 1;
            var paddedCellContent = contentToPrint.PadRight(paddingCount, ' ');
            var cellString = shouldPrintVerticalBorder ? $"{paddedCellContent}|" : paddedCellContent;

            return cellString;
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
