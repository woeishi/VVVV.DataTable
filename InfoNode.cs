using System;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Table
{
	#region PluginInfo
	[PluginInfo(Name = "Info", Category = TableDefaults.CATEGORY, Help = "retrieves information about the structure of a table", Tags = TableDefaults.TAGS, Author = TableDefaults.TAGS, AutoEvaluate = true)]
	#endregion PluginInfo
	public class TableInfoNode : TablePluginEvaluate
	{
		#region fields & pins
		[Output("Table Name")]
		ISpread<string> FName;

		[Output("Column Names")]
		ISpread<string> FColumnNames;
		
		[Output("Column Types")]
		ISpread<string> FColumnTypes;
		
		[Output("Row Count")]
		ISpread<int> FRowCount;
		#endregion

		protected override void EvaluateTables(Spread<Table> tables, bool isChanged)
		{
			if (isChanged)
			{
				int spreadMax = tables.SliceCount;
				FName.SliceCount = spreadMax;
				FColumnNames.SliceCount = spreadMax;
				FColumnTypes.SliceCount = spreadMax;
				FRowCount.SliceCount = spreadMax;
				for (int i=0; i<spreadMax; i++)
				{
					if (tables[i] == null)
					{
						FName[i] = string.Empty;
						FColumnNames[i] = string.Empty;
						FRowCount[i] = 0;
					}
					else
					{
						FName[i] = tables[i].TableName;
						
						string colNames = string.Empty;
						string colTypes = string.Empty;
						foreach (System.Data.DataColumn c in tables[i].Columns)
						{
							colNames+=c.ColumnName+",";
							colTypes+=c.DataType.Name.ToLower()[0]+",";
						}
						colNames = colNames.TrimEnd(new char[]{','});
						colTypes = colTypes.TrimEnd(new char[]{','});
						FColumnNames[i] = colNames;
						FColumnTypes[i] = colTypes;
						
						FRowCount[i] = tables[i].Rows.Count;
					}
				}
			}
		}
	}
	
}
