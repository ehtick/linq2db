// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.Tools.Comparers;
using System;
using System.Collections.Generic;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.All.ClickHouse.Client
{
	[Table("TestMerge2")]
	public class TestMerge2 : IEquatable<TestMerge2>
	{
		[Column("Id"             , DataType = DataType.Int32     , DbType = "Int32"          , IsPrimaryKey = true, SkipOnUpdate = true)] public int             Id              { get; set; } // Int32
		[Column("Field1"         , DataType = DataType.Int32     , DbType = "Int32"                                                    )] public int?            Field1          { get; set; } // Int32
		[Column("Field2"         , DataType = DataType.Int32     , DbType = "Int32"                                                    )] public int?            Field2          { get; set; } // Int32
		[Column("Field3"         , DataType = DataType.Int32     , DbType = "Int32"                                                    )] public int?            Field3          { get; set; } // Int32
		[Column("Field4"         , DataType = DataType.Int32     , DbType = "Int32"                                                    )] public int?            Field4          { get; set; } // Int32
		[Column("Field5"         , DataType = DataType.Int32     , DbType = "Int32"                                                    )] public int?            Field5          { get; set; } // Int32
		[Column("FieldInt64"     , DataType = DataType.Int64     , DbType = "Int64"                                                    )] public long?           FieldInt64      { get; set; } // Int64
		[Column("FieldBoolean"   , DataType = DataType.Boolean   , DbType = "Bool"                                                     )] public bool?           FieldBoolean    { get; set; } // Bool
		[Column("FieldString"    , DataType = DataType.NVarChar  , DbType = "String"                                                   )] public string?         FieldString     { get; set; } // String
		[Column("FieldNString"   , DataType = DataType.NVarChar  , DbType = "String"                                                   )] public string?         FieldNString    { get; set; } // String
		[Column("FieldChar"      , DataType = DataType.NChar     , DbType = "FixedString(1)" , Length       = 1                        )] public char?           FieldChar       { get; set; } // FixedString(1)
		[Column("FieldNChar"     , DataType = DataType.NChar     , DbType = "FixedString(2)" , Length       = 2                        )] public string?         FieldNChar      { get; set; } // FixedString(2)
		[Column("FieldFloat"     , DataType = DataType.Single    , DbType = "Float32"                                                  )] public float?          FieldFloat      { get; set; } // Float32
		[Column("FieldDouble"    , DataType = DataType.Double    , DbType = "Float64"                                                  )] public double?         FieldDouble     { get; set; } // Float64
		[Column("FieldDateTime"  , DataType = DataType.DateTime64, DbType = "DateTime64(3)"  , Precision    = 3                        )] public DateTimeOffset? FieldDateTime   { get; set; } // DateTime64(3)
		[Column("FieldDateTime2" , DataType = DataType.DateTime64, DbType = "DateTime64(7)"  , Precision    = 7                        )] public DateTimeOffset? FieldDateTime2  { get; set; } // DateTime64(7)
		[Column("FieldBinary"    , DataType = DataType.NVarChar  , DbType = "String"                                                   )] public string?         FieldBinary     { get; set; } // String
		[Column("FieldGuid"      , DataType = DataType.Guid      , DbType = "UUID"                                                     )] public Guid?           FieldGuid       { get; set; } // UUID
		[Column("FieldDecimal"   , DataType = DataType.Decimal256, DbType = "Decimal(38, 10)", Precision    = 38  , Scale        = 10  )] public decimal?        FieldDecimal    { get; set; } // Decimal(38, 10)
		[Column("FieldDate"      , DataType = DataType.Date      , DbType = "Date"                                                     )] public DateTime?       FieldDate       { get; set; } // Date
		[Column("FieldTime"      , DataType = DataType.Int64     , DbType = "Int64"                                                    )] public long?           FieldTime       { get; set; } // Int64
		[Column("FieldEnumString", DataType = DataType.NVarChar  , DbType = "String"                                                   )] public string?         FieldEnumString { get; set; } // String
		[Column("FieldEnumNumber", DataType = DataType.Int32     , DbType = "Int32"                                                    )] public int?            FieldEnumNumber { get; set; } // Int32

		#region IEquatable<T> support
		private static readonly IEqualityComparer<TestMerge2> _equalityComparer = ComparerBuilder.GetEqualityComparer<TestMerge2>(c => c.Id);

		public bool Equals(TestMerge2? other)
		{
			return _equalityComparer.Equals(this, other!);
		}

		public override int GetHashCode()
		{
			return _equalityComparer.GetHashCode(this);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as TestMerge2);
		}
		#endregion
	}
}
