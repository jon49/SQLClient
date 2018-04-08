using SQLClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SQLClient.Repository.Schema;

namespace SQLClient.Convert
{
    public static class TypeScript
    {

        private const int PropertyNumberOfSpaces = 8;
        private const int InterfaceNumberofSpaces = 4;

        public static string FromDbType(string dbType)
        {
            switch (dbType.ToLower())
            {
                case "bigint": case "decimal": case "money": case "smallmoney":
                case "float": case "real": case "smallint": case "tinyint":
                case "int":
                    return  "number";
                case "binary": case "image": case "timestamp": case "varbinary":
                case "char": case "nchar": case "ntext": case "nvarchar":
                case "varchar": case "text": case "xml":
                case "uniqueidentifier":
                    return "string";
                case "bit":
                    return "boolean";
                case "date": case "datetime": case "datetime2":
                case "smalldatetime": case "time": case "datetimeoffset":
                    return "Date";
                case "sql_variant": case "variant": case "udt": case "structured":
                    return "any";
                default:
                    throw new Exception("type not matched : " + dbType);
            }
        }

        public static string ToPropertyKey(string name)
        {
            // For now keep it basic. But to add unicode characters see
            // https://github.com/Microsoft/TypeScript/blob/master/src/compiler/scanner.ts
            // Naive implementation
            var doesNotNeedQuotes =
                name.All(char.IsDigit) ||
                name.All(x => Char.IsLetterOrDigit(x) || x == '_') && (Char.IsLetter(name[0]) || name[0] == '_');
            return doesNotNeedQuotes ? name : $@"""{name}""";
        }

        public static string ToProperty(string name, string dbType, bool isNullable)
            => ToPropertyKey(name) + (isNullable ? "?: " : ": ") + FromDbType(dbType);

        public static string ToProperties(IEnumerable<FirstResultSet> firstResultSet)
        {
            var spaces = new string(' ', PropertyNumberOfSpaces);
            var delimiter = "\n" + spaces;
            var parameters = String.Join(
                delimiter,
                firstResultSet.Select(x => ToProperty(x.ColumnName, x.SystemTypeName, x.IsNullable))
            );
            return spaces + parameters;
        }

        public static string ToInterface(string methodName, IEnumerable<FirstResultSet> firstResultSet)
        {
            var spaces = new string(' ', InterfaceNumberofSpaces);
            var interfaceDeclaration = $@"{spaces}interface {methodName} {{
{ToProperties(firstResultSet)}
{spaces}}}";
            return "";
        }

        public static string ToInterfaces(Namespace space)
            => $@"namespace {space.Name} {{\n{String.Join('\n', space.DMLs.Select(x => ToInterface(x.MethodName, x.FirstResultSets)))}\n}}";

    }
}
