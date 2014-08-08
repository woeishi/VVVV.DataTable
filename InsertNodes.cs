using System;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Table
{
	#region InsertRowNodes
	public class InsertRowNode<T> : TablePluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		ISpread<ISpread<T>> FInput;
		
		[Input("Index")]
		ISpread<int> FIndex;
		
		[Input("Insert", IsBang = true)]
		IDiffSpread<bool> FInsert;
		#endregion

		protected override void EvaluateTables(Spread<Table> tables, bool isChanged)
		{
			if (FInsert.IsChanged)
			{
				var spreadMax = tables.SliceCount.CombineWith(FInput).CombineWith(FInsert).CombineWith(FIndex);
				for (int i = 0; i < spreadMax; i++)
				{
					if (FInsert[i])
					{
						tables[i].InsertRow(FInput[i], FIndex[i]);
						tables[i].OnDataChange(this);
					}
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "InsertRow", Category = TableDefaults.CATEGORY, Version = "Value", Help = "Insert values into Tables (akin to Queue)", Tags = TableDefaults.TAGS, Author = "elliotwoods, "+TableDefaults.AUTHOR, AutoEvaluate = true)]
	#endregion PluginInfo
	public class InsertRowValueNode : InsertRowNode<double> {}
	#endregion InsertRowNodes
}
