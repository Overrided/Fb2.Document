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

            // scanning cycle(s) #1, collecting spans and creating meta-table
            var tableMetaData = BuildMetadataTable(rows);

            var tableRowSpans = tableMetaData.MetaTable;
            var columnsCount = tableMetaData.ColumnsCount;

            // scanning cycle #2, 
            // check width of content of each column in literal characters count, string length
            // to make column uniform, we need to select max width
            var columnCharWidths = GetCharacterColumnWidths(tableRowSpans, rowsCount, columnsCount);

            // printing cycle
            var result = StringifyTableRows(tableRowSpans, columnCharWidths, rowsCount, columnsCount);
            return result;
        }

        private static (List<TableCellModel> MetaTable, int ColumnsCount) BuildMetadataTable(List<TableRow> rows)
        {
            var rowsCount = rows.Count;

            var tableRowSpans = new List<TableCellModel>();
            var columnCountsInRow = new int[rowsCount]; // total number of columns in a row, at row's index

            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                var row = rows[rowIndex];

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
                .Where(trs => trs.RenderStartRowIndex < rowIndex && trs.RenderEndRowIndex >= rowIndex);

            if (effectiveRowSpans == null || !effectiveRowSpans.Any())
                return resultingColumn;

            var orderedActuals = effectiveRowSpans
                .OrderBy(trs => trs.RenderStartColumnIndex)
                .ThenBy(trs => trs.RenderStartRowIndex);

            foreach (var cellIndexDelta in orderedActuals)
            {
                if (cellIndexDelta.CellRowIndex > resultingColumn || // check if position in array
                    cellIndexDelta.RenderStartColumnIndex > resultingColumn) // or rendering position is further in table (if position in array is further it's definitely more to the end of a table)
                    break; // so actual cell in work is not affected anyways, and we can break due to list is ordered

                if (cellIndexDelta.ColSpan > 1)
                    resultingColumn += Math.Max(cellIndexDelta.ColSpan - 1, 0); // -1 because cell itself always will take at least 1 unit of width, whatewer "unit of width" is

                resultingColumn++;
            }

            return resultingColumn;
        }

        // find cell by "coordinates" col*row in table model
        private static bool TableCellPredicate(TableCellModel cellModel, int rowIndex, int colunmIndex) =>
            cellModel.RenderStartColumnIndex <= colunmIndex &&
            cellModel.RenderEndColumnIndex >= colunmIndex &&
            cellModel.RenderStartRowIndex <= rowIndex &&
            cellModel.RenderEndRowIndex >= rowIndex;

        // get maximal allowed width in characters of each column
        private static int[] GetCharacterColumnWidths(List<TableCellModel> tableRowSpans, int rowsCount, int columnsCount)
        {
            var columnCharWidths = new int[columnsCount];

            for (var actualColIndex = 0; actualColIndex < columnsCount; actualColIndex++)
            {
                for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
                {
                    var columnCell = tableRowSpans.FirstOrDefault(cm => TableCellPredicate(cm, rowIndex, actualColIndex));
                    if (columnCell == null)
                        continue; // shouldnt be the case, but what do I know

                    var colCharWidth = columnCell.RenderStartColumnIndex == actualColIndex ?
                                        columnCell.Content.Length : 0;

                    var prev = columnCharWidths[actualColIndex];
                    columnCharWidths[actualColIndex] = Math.Max(prev, colCharWidth);
                }
            }

            return columnCharWidths;
        }

        private static string StringifyTableRows(
            List<TableCellModel> tableRowSpans,
            int[] columnCharWidths,
            int rowsCount,
            int columnsCount)
        {
            var rowStrings = new StringBuilder();

            var sumCount = columnCharWidths.Sum(n => n + 1);
            var horLine = new string('-', sumCount + 1); // + 1 because it looks better this way

            rowStrings.AppendLine(horLine);

            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                var rowString = new StringBuilder();
                var horizontalBorder = new StringBuilder();

                for (var actualColIndex = 0; actualColIndex < columnsCount; actualColIndex++)
                {
                    var cell = tableRowSpans.FirstOrDefault(cm => TableCellPredicate(cm, rowIndex, actualColIndex));
                    if (cell == null)
                        continue; // shouldnt be the case, but what do I know

                    if (actualColIndex == 0)
                    {
                        rowString.Append('|');
                        horizontalBorder.Append('-');
                    }

                    var colWidth = columnCharWidths[actualColIndex];
                    var cellString = StringifyTableCell(cell, rowIndex, actualColIndex, colWidth);
                    rowString.Append(cellString.CellContent);
                    horizontalBorder.Append(cellString.HorizontalBorder);
                }

                rowStrings.AppendLine(rowString.ToString());
                rowStrings.AppendLine(horizontalBorder.ToString());
            }

            return rowStrings.ToString();
        }

        private static (string CellContent, string HorizontalBorder) StringifyTableCell(TableCellModel cell, int rowIndex, int colIndex, int columnCharWidth)
        {
            var hasCollSpan = cell.ColSpan > 1;
            var hasRowSpan = cell.RowSpan > 1;
            var shouldPrintColSpanSpace = cell.RenderStartColumnIndex < colIndex && hasCollSpan;
            var shouldPrintRowSpanSpace = cell.RenderStartRowIndex < rowIndex && hasRowSpan;

            bool shouldPrintVerticalBorder = true;
            string contentToPrint = string.Empty;

            if (shouldPrintColSpanSpace || shouldPrintRowSpanSpace)
                shouldPrintVerticalBorder = cell.RenderEndColumnIndex == colIndex;

            if (!shouldPrintColSpanSpace && !shouldPrintRowSpanSpace)
            {
                shouldPrintVerticalBorder = cell.ColSpan == 1;
                contentToPrint = cell.Content;
            }

            var paddingCount = shouldPrintVerticalBorder ? columnCharWidth : columnCharWidth + 1;
            var paddedCellContent = contentToPrint.PadRight(paddingCount, ' ');
            var cellString = shouldPrintVerticalBorder ? $"{paddedCellContent}|" : paddedCellContent;

            var horBorderChar = cell.RenderEndRowIndex > rowIndex ? ' ' : '-';
            var horizontalBorderContent = new string(horBorderChar, paddedCellContent.Length);
            if (shouldPrintVerticalBorder)
                horizontalBorderContent = $"{horizontalBorderContent}-";

            return (CellContent: cellString, HorizontalBorder: horizontalBorderContent);
        }

        private class TableCellModel
        {
            public int CellRowIndex { get; } // index of cell in row.Content
            public int RenderStartRowIndex { get; }
            public int RenderEndRowIndex { get; }

            public int RenderStartColumnIndex { get; } // render column start position
            public int RenderEndColumnIndex { get; }

            public int RowSpan { get; }
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
                CellRowIndex = cellArrayIndex;
                RenderStartRowIndex = rowIndex;
                RenderEndRowIndex = rowIndex + (rowSpan - 1);
                RenderStartColumnIndex = actualColumnIndex;

                ColSpan = colSpan;
                RowSpan = rowSpan;

                RenderEndColumnIndex = RenderStartColumnIndex + (colSpan - 1);

                Content = content;
            }
        }
    }
}
