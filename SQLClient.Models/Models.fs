namespace SQLClient.Models

open System

type UndeclaredParameter =
    { Name : string
      DbType : string
      Length : int16 }

type FirstResultSet =
    { ColumnOrdinal : int
      ColumnName : string
      IsNullable : bool
      SystemTypeName : string }

/// <summary>
/// Data Manipulation Language (DML) Statements
/// See https://technet.microsoft.com/en-us/library/ff848766(v=sql.110).aspx
/// </summary>
type DML =
    { MethodName : string
      FirstResultSets : FirstResultSet seq
      Parameters : UndeclaredParameter seq
      QueryText : string
      CleanedQueryText : string
      // Eventually everything should be put in the XMLSchema format.
      XMLSchema : string
      Guid: Guid }

type Namespace =
    { Name : string
      DMLs : DML seq }
