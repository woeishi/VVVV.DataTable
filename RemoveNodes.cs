using System;
using System.Collections.Generic;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Table
{
	#region PluginInfo
	[PluginInfo(Name = "RemoveRow", Category = TableDefaults.CATEGORY, Help = "Remove rows from Tables", Tags = TableDefaults.TAGS, Author = "elliotwoods, "+TableDefaults.AUTHOR, AutoEvaluate = true)]
	#endregion PluginInfo
	public class RemoveRowNode : TablePluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Index")]
		ISpread<int> FIndex;
		
		[Input("Remove", IsBang = true)]
		IDiffSpread<bool> FRemove;
		#pragma warning restore
		#endregion

		protected override void EvaluateTables(Spread<Table> tables, bool isChanged)
		{
			if (FRemove.IsChanged)
			{
				int spreadMax = tables.SliceCount.CombineWith(FRemove);
				for (int i = 0; i < spreadMax; i++)
				{
					if (FRemove[i] && tables[i].Rows.Count > 0)
					{
						int index = VVVV.Utils.VMath.VMath.Zmod(FIndex[i], tables[i].Rows.Count);
						tables[i].Rows.RemoveAt(index);
						tables[i].OnDataChange(this);
					}
				}
			}
		}
	}
}
