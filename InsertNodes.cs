using System;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Table
{
	#region InsertRowNodes
	public class InsertRowNode<T> : TablePluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Input")]
		ISpread<ISpread<T>> FInput;
		
		[Input("Index")]
		ISpread<int> FIndex;
		
		[Input("Insert", IsBang = true)]
		IDiffSpread<bool> FInsert;
		#pragma warning restore
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
	
	[PluginInfo(Name = "InsertRow", Category = TableDefaults.CATEGORY, Version = "Value", Help = "Insert rows of values into tables", Tags = TableDefaults.TAGS, Author = "elliotwoods, "+TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class InsertRowValueNode : InsertRowNode<double> {}
	
	[PluginInfo(Name = "InsertRow", Category = TableDefaults.CATEGORY, Version = "String", Help = "Insert rows of strings into tables", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class InsertRowStringNode : InsertRowNode<string> {}
	#endregion InsertRowNodes
}
