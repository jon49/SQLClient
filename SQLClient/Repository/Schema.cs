using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace SQLClient.Repository
{
    public class Schema
    {

        public class FirstResultSet
        {
            public FirstResultSet(int columnOrdinal, string columnName, bool isNullable, string systemTypeName)
            {
                ColumnOrdinal = columnOrdinal;
                ColumnName = columnName;
                IsNullable = isNullable;
                SystemTypeName = systemTypeName;
            }

            public int ColumnOrdinal { get; private set; }
            public string ColumnName { get; private set; }
            public bool IsNullable { get; private set; }
            public string SystemTypeName { get; private set; }
        }

        public Func<string, Task<IEnumerable<FirstResultSet>>> GetFirstResultSet(SqlConnection connection)
            => (string query)
            => connection.QueryAsync<FirstResultSet>(@"
                SELECT
                    t.column_ordinal ColumnOrdinal
                  , t.[name] ColumnName
                  , t.is_nullable IsNullable
                  , t.system_type_name SystemTypeName
                  , t.source_database SourceDatabase
                  , t.source_schema SourceSchema
                  , t.source_table SourceTable
                  , t.source_column SourceColumnName
                FROM sys.dm_exec_describe_first_result_set(@Query, null, 1) t
                WHERE t.is_hidden = 0;
                ", new { query });


        public class UndeclaredParameter
        {
            public string Name { get; set; }
            public string DbType { get; set; }
            public Int16 Length { get; set; }
        }

        public Func<string, Task<IEnumerable<UndeclaredParameter>>> GetUndeclaredParameters(SqlConnection connection)
            => (string query)
            => connection.QueryAsync<UndeclaredParameter>(@"
            DECLARE @query$ nvarchar(max) = @query;
            DECLARE @Result AS TABLE (
                parameter_ordinal INT NOT NULL
              , name SYSNAME NOT NULL
              , suggested_system_type_id INT NOT NULL
              , suggested_system_type_name NVARCHAR(256) NULL
              , suggested_max_length SMALLINT NOT NULL
              , suggested_precision TINYINT NOT NULL
              , suggested_scale TINYINT NOT NULL
              , suggested_user_type_id INT NULL
              , suggested_user_type_database SYSNAME NULL
              , suggested_user_type_schema SYSNAME NULL
              , suggested_user_type_name SYSNAME NULL
              , suggested_assembly_qualified_type_name NVARCHAR(4000) NULL
              , suggested_xml_collection_id INT NULL
              , suggested_xml_collection_database SYSNAME NULL
              , suggested_xml_collection_schema SYSNAME NULL
              , suggested_xml_collection_name SYSNAME NULL
              , suggested_is_xml_document BIT NOT NULL
              , suggested_is_case_sensitive BIT NOT NULL
              , suggested_is_fixed_length_clr_type BIT NOT NULL
              , suggested_is_input BIT NOT NULL
              , suggested_is_output BIT NOT NULL
              , formal_parameter_name SYSNAME NULL
              , suggested_tds_type_id INT NOT NULL
              , suggested_tds_length INT NOT NULL
            );

            INSERT @Result
            EXEC sp_describe_undeclared_parameters @query$;

            SELECT t.[name] [Name], t.suggested_system_type_name DbType, t.suggested_max_length [Length]
            FROM @Result t
            WHERE t.suggested_is_input = 1;
            ", new { query });

        /// <summary>
        /// Retrieve the XML Schema from the query. Make sure to first remove
        /// the last semicolon if it exists.
        /// </summary>
        /// <param name="connection">SQL Connection</param>
        /// <returns>Function which takes the query to be evaluated.</returns>
        public Func<string, Task<string>> GetXMLSchema(SqlConnection connection)
            // Need to make sure there isn't a semicolon at end of query
            => (string query)
            => connection.QueryFirstAsync<string>($@"
            {query}
            FOR XML AUTO, XMLSCHEMA
            ");

    }
}
