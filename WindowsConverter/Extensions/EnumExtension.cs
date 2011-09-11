using System;
using System.Data;
using System.Linq;

namespace WindowsConverter.Extensions
{
    static class EnumExtension
    {
        public const string DefaultCaptionColumnName = "Caption";
        public const string DefaultValueColumnName = "Id";

        public static DataTable ToDataTable(Type enumType,
            string CaptionColumnName = DefaultCaptionColumnName, string ValueColumnName = DefaultValueColumnName,
            Func<int, int> ValueSorter = null)
        {
            if (!(enumType.IsEnum))
            {
                throw new ArgumentException("enumType should be enumeration type");
            }

            var result = new DataTable();

            result.Columns.Add(CaptionColumnName, typeof(string));
            result.Columns.Add(ValueColumnName, typeof(int));

            var sourceValues = Enum.GetValues(enumType).Cast<int>();
            if (ValueSorter != null)
            {
                sourceValues = sourceValues.OrderBy(ValueSorter);
            }
            var values = sourceValues.ToArray();

            var captions = Enumerable.Range(0, values.Length).
                Select(i => Enum.GetName(enumType, values[i])).ToArray();

            for (var i = 0; i < captions.Length && i < values.Length; i++)
            {
                var row = result.NewRow();
                row[CaptionColumnName] = captions[i];
                row[ValueColumnName] = values[i];
                result.Rows.Add(row);
            }

            return result;
        }
    }
}
