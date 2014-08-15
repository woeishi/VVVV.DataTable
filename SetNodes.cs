using System;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Table
{
	#region SetRowNodes
	public class SetRowNode<T> : TablePluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Input")]
		ISpread<ISpread<T>> FInput;

		[Input("Index")]
		ISpread<int> FIndex;

		[Input("Set", IsBang = true)]
		IDiffSpread<bool> FSet;
		#pragma warning restore
		#endregion

		protected override void EvaluateTables(Spread<Table> tables, bool isChanged)
		{
			if (FSet.IsChanged)
			{
				var spreadMax = tables.SliceCount.CombineWith(FInput).CombineWith(FIndex).CombineWith(FSet);
				for (int i = 0; i < spreadMax; i++)
				{
					if (FSet[i])
					{
						tables[i].SetRow(FInput[i], FIndex[i]);
						tables[i].OnDataChange(this);
					}
				}
			}
		}
	}
	
	[PluginInfo(Name = "SetRow", Category = TableDefaults.CATEGORY, Version = "Value", Help = "Row-wise sets values in tables", Tags = TableDefaults.TAGS, Author = "elliotwoods, "+TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class SetRowValueNode : SetRowNode<double> {}
	
	[PluginInfo(Name = "SetRow", Category = TableDefaults.CATEGORY, Version = "String", Help = "Row-wise sets strings in tables", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class SetRowStringNode : SetRowNode<string> {}
	#endregion SetRowNodes
	
	#region SetColumnNodes
	public class SetColumnNode<T> : TablePluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Input")]
		ISpread<ISpread<T>> FInput;

		[Input("Index")]
		ISpread<int> FIndex;

		[Input("Set", IsBang = true)]
		IDiffSpread<bool> FSet;
		#pragma warning restore
		#endregion

		protected override void EvaluateTables(Spread<Table> tables, bool isChanged)
		{
			if (FSet.IsChanged)
			{
				int spreadMax = tables.SliceCount.CombineWith(FInput).CombineWith(FIndex).CombineWith(FSet);
				for (int i = 0; i < spreadMax; i++)
				{
					if (FSet[i])
					{
						tables[i].SetColumn(FInput[i], FIndex[i]);
						tables[i].OnDataChange(this);
					}
				}
			}
		}
	}
	
	[PluginInfo(Name = "SetColumn", Category = TableDefaults.CATEGORY, Version = "Value", Help = "Column-wise sets values in tables", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class SetColumnValueNode : SetColumnNode<double> {}
	
	[PluginInfo(Name = "SetColumn", Category = TableDefaults.CATEGORY, Version = "String", Help = "Column-wise sets strings in tables", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class SetColumnStringNode : SetColumnNode<string> {}
	#endregion SetColumnNodes
	
	#region SetCellNodes
	public class SetCellNode<T> : TablePluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Input")]
		ISpread<T> FInput;

		[Input("Row Index")]
		ISpread<int> FRowId;
		
		[Input("Column Index")]
		ISpread<int> FColId;

		[Input("Set", IsBang = true)]
		IDiffSpread<bool> FSet;
		#pragma warning restore
		#endregion

		protected override void EvaluateTables(Spread<Table> tables, bool isChanged)
		{
			if (FSet.IsChanged)
			{
				int spreadMax = tables.SliceCount.CombineWith(FInput).CombineWith(FRowId).CombineWith(FColId).CombineWith(FSet);
				for (int i = 0; i < spreadMax; i++)
				{
					if (FSet[i])
					{
						tables[i].Set(FInput[i], FRowId[i], FColId[i]);
						tables[i].OnDataChange(this);
					}
				}
			}
		}
	}
	
	[PluginInfo(Name = "SetCell", Category = TableDefaults.CATEGORY, Version="Value", Help = "Sets values cell-wise in tables", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class SetCellValueNode : SetCellNode<double> {}
	
	[PluginInfo(Name = "SetCell", Category = TableDefaults.CATEGORY, Version="String", Help = "Sets strings cell-wise in tables", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class SetCellStringNode : SetCellNode<string> {}
	#endregion SetCellNodes
}
