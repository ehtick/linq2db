// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB.Mapping;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.NewCliFeatures.SQLite
{
	[Table("FKTestPosition")]
	public class FkTestPosition
	{
		[Column("Company"   , IsPrimaryKey = true , PrimaryKeyOrder = 0)] public long   Company    { get; internal set; } // integer
		[Column("Department", IsPrimaryKey = true , PrimaryKeyOrder = 1)] public long   Department { get; internal set; } // integer
		[Column("PositionID", IsPrimaryKey = true , PrimaryKeyOrder = 2)] public long   PositionId { get; internal set; } // integer
		[Column("Name"      , CanBeNull    = false                     )] public string Name       { get; internal set; } = null!; // nvarchar(50)
	}
}
