using SQLClient.Convert;
using SQLClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using static SQLClient.Repository.Schema;

namespace SQLClient.Tests
{
    public class ConvertTests
    {
        private readonly ITestOutputHelper output;

        public ConvertTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private Namespace GetNamespace()
            => new Namespace
            {
                Name = "Films",
                DMLs = new[]
                {
                    new DML
                    {
                        CleanedQueryText = @"",
                        FirstResultSets = new[]
                        {
                            new FirstResultSet
                            (
                                columnName: "film_id",
                                columnOrdinal: 1,
                                isNullable: false,
                                systemTypeName: "int"
                            ),
                            new FirstResultSet
                            (
                                columnName: "description",
                                columnOrdinal: 2,
                                isNullable: true,
                                systemTypeName: "text"
                            ),
                            new FirstResultSet
                            (
                                columnOrdinal: 3,
                                columnName: "length",
                                isNullable: true,
                                systemTypeName: "smallint"
                            )
                        }
                    }
                }
            };

        [Fact] public void FromDbTypeMethodReturns_ANumber() { Assert.Equal("number", TypeScript.FromDbType("bigint")); }
        [Fact] public void FromDbTypeMethodReturns_AString() { Assert.Equal("string", TypeScript.FromDbType("binary")); }
        [Fact] public void FromDbTypeMethodReturns_ABoolean() { Assert.Equal("boolean", TypeScript.FromDbType("bit")); }
        [Fact] public void FromDbTypeMethodReturns_ADate() { Assert.Equal("Date", TypeScript.FromDbType("date")); }
        [Fact] public void FromDbTypeMethodReturns_AnAny() { Assert.Equal("any", TypeScript.FromDbType("sql_variant")); }

        [Fact] public void ToPropertyShouldReturnAProperty() { Assert.Equal(@"film_id: number", TypeScript.ToProperty("film_id", "int", false)); }
        [Fact] public void ToPropertyShouldBeQuotedForKeysBeginningWithANumber() { Assert.Equal(@"""5films""?: string", TypeScript.ToProperty("5films", "varchar", true)); }
        [Fact] public void ToPropertyShouldNotBeQuotedForAlphanumericKeys() { Assert.Equal(@"film5?: string", TypeScript.ToProperty("film5", "nvarchar", true)); }

        [Fact]
        public void CorrectlyCreatesNamespace()
        {
            var actual = TypeScript.ToInterfaces(GetNamespace());
            output.WriteLine(actual);
            Assert.Equal("", actual);
        }

    }
}
