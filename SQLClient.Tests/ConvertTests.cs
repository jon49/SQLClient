using SQLClient.Convert;
using SQLClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SQLClient.Tests
{
    public class ConvertTests
    {
        private readonly string QueryText;

        public ConvertTests()
        {
        }

        private Namespace GetNamespace()
            => new Namespace
            (
                name: "Films",
                dMLs: new[]
                {
                    new DML
                    (
                        methodName: "GetMyFilm",
                        cleanedQueryText: @"",
                        queryText: @"
SELECT t.film_id, t.[description], t.[length]
FROM dbo.film t
WHERE t.film_id = @Id;
",
                        xMLSchema: "",
                        parameters: new List<UndeclaredParameter>{ },
                        guid: new Guid(),
                        firstResultSets: new[]
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
                    )
                }
            );

        [Fact] public void FromDbTypeMethodReturns_ANumber() { Assert.Equal("number", TypeScript.FromDbType("bigint")); }
        [Fact] public void FromDbTypeMethodReturns_AString() { Assert.Equal("string", TypeScript.FromDbType("binary")); }
        [Fact] public void FromDbTypeMethodReturns_ABoolean() { Assert.Equal("boolean", TypeScript.FromDbType("bit")); }
        [Fact] public void FromDbTypeMethodReturns_ADate() { Assert.Equal("Date", TypeScript.FromDbType("date")); }
        [Fact] public void FromDbTypeMethodReturns_AnAny() { Assert.Equal("any", TypeScript.FromDbType("sql_variant")); }

        [Fact] public void ToPropertyShouldReturnAProperty() { Assert.Equal(@"film_id: number", TypeScript.ToProperty("film_id", "int", false)); }
        [Fact] public void ToPropertyShouldBeQuotedForKeysBeginningWithANumber() { Assert.Equal(@"""5films""?: string", TypeScript.ToProperty("5films", "varchar", true)); }
        [Fact] public void ToPropertyShouldNotBeQuotedForAlphanumericKeys() { Assert.Equal(@"film5?: string", TypeScript.ToProperty("film5", "nvarchar", true)); }

        [Fact] public void ToPropertiesShouldReturnAListOfProperties()
        {
            var space = GetNamespace();
            var actual = TypeScript.ToProperties(space.DMLs.First().FirstResultSets);
            var expected = "        film_id: number\n        description?: string\n        length?: number";
            Assert.Equal(expected, actual);
        }

        [Fact] public void ToInterfaceShouldReturnAnInterface()
        {
            var dml = GetNamespace().DMLs.First();
            var actual = TypeScript.ToInterface(dml.MethodName, dml.FirstResultSets);
            var expected = "    interface GetMyFilm {\r\n        film_id: number\n        description?: string\n        length?: number\r\n    }";
            Assert.Equal(expected, actual);
        }

        [Fact] public void CorrectlyCreatesNamespace()
        {
            var actual = TypeScript.ToInterfaces(GetNamespace());
            var expected = "namespace Films {\n    interface GetMyFilm {\r\n        film_id: number\n        description?: string\n        length?: number\r\n    }\n}";
            Assert.Equal(expected, actual);
        }

    }
}
