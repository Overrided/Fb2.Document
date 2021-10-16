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

        // TODO : fix ToString with col-span and row-span
        public sealed override string ToString()
        {
            if (IsEmpty)
                return string.Empty;

            var table = GetTableCellContent();

            // assume that table is valid, all rows have same length, so max is same as min
            var rowLenght = table.Max(r => r.Length);

            List<List<string>> processedColumns = new();

            for (int c = 0; c < rowLenght; c++)
            {
                // default error handling
                var column = table.Select(row => c >= row.Length ? string.Empty : row[c]);

                // new List for each row
                if (!processedColumns.Any())
                    processedColumns.AddRange(column.Select(col => new List<string>()));

                var formattedColumn = ProcessColumn(column).ToList();

                for (int cl = 0; cl < formattedColumn.Count; cl++)
                {
                    processedColumns[cl].Add(formattedColumn[cl]);
                }
            }

            return ToStringInternal(processedColumns);
        }

        // taking unsafe elements into account
        private string[][] GetTableCellContent()
        {
            return Content.Select(row =>
            {
                if (row is Fb2Container rowContainer)
                    return rowContainer.Content.Select(cell => cell.ToString() ?? string.Empty).ToArray();
                else if (row is Fb2Element elementRow)
                    return new string[1] { elementRow.ToString() };

                return Array.Empty<string>();
            }).ToArray();
        }

        private static IEnumerable<string> ProcessColumn(IEnumerable<string> column)
        {
            var desiredColumnWidth = column.Max(c => c.Length) + 1;

            return column.Select(cell => $"{cell.PadRight(desiredColumnWidth)}|");
        }

        private static string ToStringInternal(List<List<string>> columns)
        {
            var tableConcatanator = new StringBuilder();

            foreach (var item in columns)
                tableConcatanator.AppendLine(string.Join(" ", item));

            return tableConcatanator.ToString();
        }
    }
}
