// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB.Mapping;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.Default.Firebird
{
	[Table("PersonView", IsView = true)]
	public class PersonView
	{
		[Column("PersonID"  )] public int?    PersonId   { get; set; } // integer
		[Column("FirstName" )] public string? FirstName  { get; set; } // varchar(50)
		[Column("LastName"  )] public string? LastName   { get; set; } // varchar(50)
		[Column("MiddleName")] public string? MiddleName { get; set; } // varchar(50)
		[Column("Gender"    )] public char?   Gender     { get; set; } // char(1)
	}
}