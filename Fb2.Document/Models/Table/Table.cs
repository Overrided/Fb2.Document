using System.Collections.Generic;
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

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.TableRow
        };

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Id
        };

        public sealed override string ToString()
        {
            if (this.Content == null || !this.Content.Any())
                return string.Empty;

            var table = GetTableCellContent();

            // assume that table is valid, all rows have same length, so max is same as min
            var rowLenght = table.Max(r => r.Length);

            List<List<string>> processedColumns = new List<List<string>>();

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

        private string[][] GetTableCellContent()
        {
            return this.Content.Select(row => (row as Fb2Container)
                                    .Content.Select(cell => cell.ToString() ?? string.Empty).ToArray())
                            .ToArray();
        }

        private IEnumerable<string> ProcessColumn(IEnumerable<string> column)
        {
            var desiredColumnWidth = column.Max(c => c.Length) + 1;

            return column.Select(cell => cell.PadRight(desiredColumnWidth) + "|");
        }

        private string ToStringInternal(List<List<string>> columns)
        {
            var tableConcatanator = new StringBuilder();

            foreach (var item in columns)
            {
                tableConcatanator.AppendLine(string.Join(" ", item));
            }

            return tableConcatanator.ToString();
        }
    }
}
