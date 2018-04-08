using System.Collections.Generic;
using static SQLClient.Repository.Schema;

namespace SQLClient.Models
{
    /// <summary>
    /// Data Manipulation Language (DML) Statements
    /// See https://technet.microsoft.com/en-us/library/ff848766(v=sql.110).aspx
    /// </summary>
    public class DML
    {
        public string MethodName { get; set; }
        public IEnumerable<FirstResultSet> FirstResultSets { get; set; }
        public IEnumerable<UndeclaredParameter> Parameters { get; set; }
        public string QueryText { get; set; }
        public string CleanedQueryText { get; set; }
        // Eventually everything should be put in the XMLSchema format.
        public string XMLSchema { get; set; }
    }

    public class Namespace
    {
        public string Name { get; set; }
        public IEnumerable<DML> DMLs { get; set; }
    }

}
