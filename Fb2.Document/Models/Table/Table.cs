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
            if (!HasContent)
                return string.Empty;

            // omitting unsafe stuff etc
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

        #region ToString implementation - black magic fuckery ahead

        private static (List<TableCellModel> MetaTable, int ColumnsCount) BuildMetadataTable(List<TableRow> rows)
        {
            var rowsCount = rows.Count;

            var tableRowSpans = new List<TableCellModel>();
            var columnCountsInRow = new int[rowsCount]; // total number of columns in a row, at row's index

            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                var row = rows[rowIndex];

                var colSpanDeltaInRow = 0;

                var cellsInRow = row.GetChildren<TableCellBase>().ToList();
                var cellsInRowCount = cellsInRow.Count;

                if (cellsInRowCount == 0)
                    continue; // skip empty rows

                // array index position is not indicative of actual cell position in table
                for (int arrayColumnIndex = 0; arrayColumnIndex < cellsInRowCount; arrayColumnIndex++)
                {
                    var cellNode = cellsInRow[arrayColumnIndex];

                    var renderColumnIndex = colSpanDeltaInRow != 0 ? arrayColumnIndex + colSpanDeltaInRow : arrayColumnIndex;
                    // check previous rows and check if any cell are impacting current cell
                    renderColumnIndex = UpdateRenderColumnIndex(tableRowSpans, rowIndex, renderColumnIndex);

                    var colSpanNum = GetCellSpan(cellNode, AttributeNames.ColumnSpan);
                    columnCountsInRow[rowIndex] = Math.Max(columnCountsInRow[rowIndex], renderColumnIndex + 1); // absolute unit width of a cell

                    // to know how far to move next cell in table row - save "total span" number in row
                    // if no ColSpan -> colSpanNum - 1 = 0
                    colSpanDeltaInRow += colSpanNum - 1; // -1 because of cell itself has width

                    var rowSpanNum = GetCellSpan(cellNode, AttributeNames.RowSpan);
                    var cellContent = cellNode.ToString();

                    var verticalAlignRaw = GetCellAlignment(cellNode, AttributeNames.VerticalAlign);
                    var horizontalAlignRaw = GetCellAlignment(cellNode, AttributeNames.Align);

                    var verticalAlign = ParseVerticalCellAlignment(verticalAlignRaw);
                    var horizontalCellAlign = ParseHorizontalCellAlignment(horizontalAlignRaw);

                    tableRowSpans.Add(
                        new TableCellModel(
                            arrayColumnIndex,
                            rowIndex,
                            renderColumnIndex,
                            cellContent,
                            colSpanNum,
                            rowSpanNum,
                            verticalAlign,
                            horizontalCellAlign));
                }
            }

            // prevention to some miscalculation we can fix, but with performance penalties
            var columnsCount = columnCountsInRow.Max();

            return (MetaTable: tableRowSpans, ColumnsCount: columnsCount);
        }

        private static HorizontalCellAlign ParseHorizontalCellAlignment(string horizontalAlignRaw) =>
            Enum.TryParse<HorizontalCellAlign>(horizontalAlignRaw, true, out var result) ?
                result : HorizontalCellAlign.Center;

        private static VerticalCellAlign ParseVerticalCellAlignment(string verticalAlignRaw) =>
            Enum.TryParse<VerticalCellAlign>(verticalAlignRaw, true, out var result) ?
                result : VerticalCellAlign.Middle;

        private static string GetCellAlignment(Fb2Node cell, string alignAttributeName)
        {
            if (cell == null || string.IsNullOrWhiteSpace(alignAttributeName))
                throw new ArgumentNullException();

            if (cell.TryGetAttribute(alignAttributeName, true, out var align))
                return align.Value;

            return string.Empty;
        }

        // if no span found return 1 as minimal cell unit width/height
        private static int GetCellSpan(Fb2Node cell, string spanAttributeName)
        {
            if (cell == null || string.IsNullOrWhiteSpace(spanAttributeName))
                throw new ArgumentNullException();

            if (cell.TryGetAttribute(spanAttributeName, true, out var span) &&
                int.TryParse(span.Value, out int spanNumber) && spanNumber > 1)
                return spanNumber;

            return 1;
        }

        private static int UpdateRenderColumnIndex(List<TableCellModel> tableRowSpans, int rowIndex, int resultingColumn)
        {
            if (!tableRowSpans.Any())
                return resultingColumn;

            var effectiveRowSpans = tableRowSpans // all row spans that are "higher" than actual row
                .Where(trs => trs.RenderStartRowIndex < rowIndex && trs.RenderEndRowIndex >= rowIndex);

            if (effectiveRowSpans == null || !effectiveRowSpans.Any())
                return resultingColumn;

            var orderedActuals = effectiveRowSpans
                .OrderBy(trs => trs.RenderStartColumnIndex)
                .ThenBy(trs => trs.RenderStartRowIndex)
                .ToList();

            for (int i = 0; i < orderedActuals.Count; i++)
            {
                var cellIndexDelta = orderedActuals[i];

                if (cellIndexDelta.CellRowIndex > resultingColumn || // check if position in array
                    cellIndexDelta.RenderStartColumnIndex > resultingColumn) // or rendering position is further in table (if position in array is further it's definitely more to the end of a table)
                    break; // so actual cell in work is not affected anyways, and we can break due to list is ordered

                resultingColumn += cellIndexDelta.ColSpan;
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
                        continue; // shouldn't be the case, but what do I know

                    var colCharWidth = columnCell.RenderStartColumnIndex == actualColIndex ?
                                        columnCell.Content.Length : 0;

                    var previous = columnCharWidths[actualColIndex];
                    columnCharWidths[actualColIndex] = Math.Max(previous, colCharWidth);
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
                var rowString = new StringBuilder("|");
                var horizontalBorder = new StringBuilder(rowIndex == rowsCount - 1 ? "-" : "|");

                for (var actualColIndex = 0; actualColIndex < columnsCount; actualColIndex++)
                {
                    var cell = tableRowSpans.FirstOrDefault(cm => TableCellPredicate(cm, rowIndex, actualColIndex));
                    if (cell == null)
                        continue; // shouldn't be the case, but what do I know

                    int colWidth = CalculateCellCharWidth(columnCharWidths, actualColIndex, cell);

                    if (cell.ColSpan > 1)
                        actualColIndex = actualColIndex + (cell.ColSpan - 1); // move to next cell

                    var cellString = StringifyTableCell(tableRowSpans, cell, rowIndex, actualColIndex, colWidth);

                    rowString.Append(cellString.CellContent);
                    horizontalBorder.Append(cellString.HorizontalBorder);
                }

                if (rowString.Length > 1)
                    rowStrings.AppendLine(rowString.ToString());

                if (horizontalBorder.Length > 1)
                    rowStrings.AppendLine(horizontalBorder.ToString());
            }

            return rowStrings.ToString();
        }

        private static int CalculateCellCharWidth(int[] columnCharWidths, int colIndex, TableCellModel cell)
        {
            if (cell.ColSpan > 1)
            {
                var allColumnCharWidths = columnCharWidths
                    .Skip(cell.RenderStartColumnIndex)
                    .Take(cell.ColSpan).ToList();

                return allColumnCharWidths
                    .Select((v, i) => i < allColumnCharWidths.Count - 1 ? v + 1 : v)
                    .Sum();
            }

            return columnCharWidths[colIndex];
        }

        private static (string CellContent, string HorizontalBorder) StringifyTableCell(
            List<TableCellModel> allCells,
            TableCellModel cell,
            int rowIndex,
            int colIndex,
            int cellCharWidth)
        {
            var hasRowSpan = cell.RowSpan > 1;
            var shouldPrintRowSpanSpace = cell.RenderContentRowIndex != rowIndex && hasRowSpan;
            var contentToPrint = shouldPrintRowSpanSpace ? string.Empty : cell.Content;

            var paddedCellContent = PadCellContent(contentToPrint, cellCharWidth, cell.HorizontalAlign);
            var cellString = $"{paddedCellContent}|";

            var horBorderChar = cell.RenderEndRowIndex > rowIndex ? ' ' : '-';
            var horizontalBorderContent = new string(horBorderChar, paddedCellContent.Length);

            var rowBelowIndex = rowIndex + 1;
            var cellBelow = allCells.FirstOrDefault(cm => TableCellPredicate(cm, rowBelowIndex, colIndex));
            var flatVerticalDelimiter = cellBelow == null || cellBelow.RowSpan > 0 && cellBelow.RenderEndColumnIndex > colIndex;

            var verticalDelimiterChar = flatVerticalDelimiter ? '-' : '|';
            horizontalBorderContent = $"{horizontalBorderContent}{verticalDelimiterChar}";

            return (CellContent: cellString, HorizontalBorder: horizontalBorderContent);
        }

        private static string PadCellContent(string contentToPrint, int cellCharWidth, HorizontalCellAlign horizontalCellAlign)
        {
            if (horizontalCellAlign == HorizontalCellAlign.Left)
                return contentToPrint.PadRight(cellCharWidth, ' ');

            if (horizontalCellAlign == HorizontalCellAlign.Right)
                return contentToPrint.PadLeft(cellCharWidth, ' ');

            var availablePadding = cellCharWidth - contentToPrint.Length;
            if (availablePadding > 0)
            {
                var leftPadding = availablePadding / 2;
                var leftPaddingString = new string(' ', leftPadding);
                var rightPaddingString = new string(' ', availablePadding - leftPadding);
                return $"{leftPaddingString}{contentToPrint}{rightPaddingString}";
            }

            return contentToPrint;
        }

        private class TableCellModel
        {
            public int CellRowIndex { get; } // index of cell in row.Content
            public int RenderStartRowIndex { get; }
            public int RenderEndRowIndex { get; }
            public int RenderContentRowIndex { get; }

            public int RenderStartColumnIndex { get; } // render column start position
            public int RenderEndColumnIndex { get; }

            public int RowSpan { get; }
            public int ColSpan { get; }

            public string Content { get; } // stringnified cell content

            public VerticalCellAlign VerticalAlign { get; }
            public HorizontalCellAlign HorizontalAlign { get; }

            public TableCellModel(
                int cellArrayIndex,
                int rowIndex,
                int actualColumnIndex,
                string content,
                int colSpan,
                int rowSpan,
                VerticalCellAlign verticalAlign,
                HorizontalCellAlign horizontalCellAlign)
            {
                CellRowIndex = cellArrayIndex;

                RenderStartRowIndex = rowIndex;
                RenderEndRowIndex = rowIndex + (rowSpan - 1);

                RenderStartColumnIndex = actualColumnIndex;
                RenderEndColumnIndex = RenderStartColumnIndex + (colSpan - 1);

                ColSpan = colSpan;
                RowSpan = rowSpan;

                Content = content;

                VerticalAlign = verticalAlign;
                HorizontalAlign = horizontalCellAlign;

                if (VerticalAlign == VerticalCellAlign.Top)
                    RenderContentRowIndex = RenderStartRowIndex;
                else if (VerticalAlign == VerticalCellAlign.Bottom)
                    RenderContentRowIndex = RenderEndRowIndex;
                else if (VerticalAlign == VerticalCellAlign.Middle)
                {
                    if (RenderStartRowIndex == RenderEndRowIndex || RowSpan < 3)
                        RenderContentRowIndex = RenderStartRowIndex;
                    else
                    {
                        var halfSize = RowSpan / 2;
                        RenderContentRowIndex = RenderStartRowIndex + halfSize;
                    }
                }
            }
        }

        private enum VerticalCellAlign
        {
            Top,
            Middle,
            Bottom
        }

        private enum HorizontalCellAlign
        {
            Left,
            Center,
            Right
        }

        #endregion
    }
}
