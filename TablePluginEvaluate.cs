using System;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Table
{
	public abstract class TablePluginEvaluate : IPluginEvaluate
	{
		#region fields & pins
		[Input("Table", Order = int.MinValue)]
		private IDiffSpread<Table> FPinInTable;

		private Spread<Table> FTables = new Spread<Table>(0);
		private bool IsChanged = false; //flag is raised when DataChanged event is called
		#endregion

		private Table AddTable(int slice)
		{
			Table t = null;
			if (FPinInTable[slice] != null)
			{
				t = FPinInTable[slice];
				t.DataChanged += new Table.DataChangedHandler(Tables_Changed);
			}
			
			IsChanged = true;
			return t;
		}
		
		private void ReleaseTable(Table t)
		{
			t.DataChanged -= new Table.DataChangedHandler(Tables_Changed);
			IsChanged = true;
		}
		
		public void Evaluate(int SpreadMax)
		{
			FTables.Resize(FPinInTable.SliceCount, AddTable, (t) => ReleaseTable(t));

			if (FPinInTable.IsChanged)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					if (FPinInTable[i] == null)
					{
						if (FTables[i] != null)
						{
							FTables[i].DataChanged -= new Table.DataChangedHandler(Tables_Changed);
							IsChanged = true;
						}
					}
					else if (FTables[i] != FPinInTable[i])
					{
						if (FTables[i] != null)
							FTables[i].DataChanged -= new Table.DataChangedHandler(Tables_Changed);
						FTables[i] = FPinInTable[i];
						FTables[i].DataChanged += new Table.DataChangedHandler(Tables_Changed);
						IsChanged = true;
					}
				}
				this.FData_Connected();
			}

			this.EvaluateTables(FTables, IsChanged);
			this.IsChanged = false;
		}

		protected abstract void EvaluateTables(Spread<Table> tables, bool isChanged);

		void FData_Connected() { }
		void Tables_Changed(object sender, TableEventArgs e) 
		{
			if (sender != this)
			{
				IsChanged = true;
			}
		}
	}
}
