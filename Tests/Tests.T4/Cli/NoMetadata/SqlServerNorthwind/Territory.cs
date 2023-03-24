// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using System.Collections.Generic;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.NoMetadata.SqlServerNorthwind
{
	public class Territory
	{
		public string TerritoryId          { get; set; } = null!; // nvarchar(20)
		public string TerritoryDescription { get; set; } = null!; // nchar(50)
		public int    RegionId             { get; set; } // int

		#region Associations
		/// <summary>
		/// FK_EmployeeTerritories_Territories backreference
		/// </summary>
		public IEnumerable<EmployeeTerritory> EmployeeTerritories { get; set; } = null!;

		/// <summary>
		/// FK_Territories_Region
		/// </summary>
		public Region Region { get; set; } = null!;
		#endregion
	}
}