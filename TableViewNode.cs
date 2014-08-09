#region usings
using System;
using System.Drawing;
using System.Windows.Forms;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using System.Diagnostics;
#endregion usings

namespace VVVV.Nodes.Table
{
	#region PluginInfo
	[PluginInfo(Name = "TableView", Category = TableDefaults.CATEGORY, Help = "View and edit the data of a Table", Tags = TableDefaults.TAGS, Author="elliotwoods, "+TableDefaults.AUTHOR, AutoEvaluate = true)]
	#endregion PluginInfo
	public class TableViewNode : UserControl, IPluginEvaluate
	{
		#region fields & pins
		[Input("Table")]
		ISpread<Table> FPinInTable;
		
		[Input("Table Index", IsSingle = true)]
		ISpread<int> FTableIndex;
		
		[Input("Allow Add Row", IsSingle = true, DefaultBoolean = true)]
		IDiffSpread<bool> FAllowAddRow;
		
		[Input("Up", IsSingle = true, IsBang = true)]
		ISpread<bool> FUp;

		[Input("Down", IsSingle = true, IsBang = true)]
		ISpread<bool> FDown;

		[Output("Selected Row")]
		ISpread<int> FCurrentIndex;
		
		[Import()]
		ILogger FLogger;

		private DataGridView FDataGridView;
		private Table FData;
		private bool FNeedsUpdate = false;
		
		internal DataGridViewCellStyle numberStyle;
		internal DataGridViewCellStyle textStyle;
		#endregion fields & pins

		#region constructor and init

		public TableViewNode()
		{
			//setup the gui
			InitializeComponent();
			FDataGridView.CellValueChanged += FDataGridView_CellValueChanged;
			FDataGridView.RowsRemoved += FDataGridView_RowsRemoved;
		}

		void FDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			OnDataChanged();
		}

		void FDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			OnDataChanged();
		}

		void OnDataChanged()
		{
			if (this.FData != null)
			{
				FData.OnDataChange(this);
			}
		}

		void InitializeComponent()
		{
			DataGridViewCellStyle defaultStyle = new DataGridViewCellStyle();
			defaultStyle.SelectionBackColor = System.Drawing.Color.Gray;
			defaultStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			defaultStyle.Format = string.Empty;
			defaultStyle.NullValue = "+";
			
			DataGridViewCellStyle defaultAltStyle = new DataGridViewCellStyle(defaultStyle);
			defaultAltStyle.BackColor = System.Drawing.Color.FromArgb(224,224,224);
			defaultAltStyle.SelectionBackColor = System.Drawing.Color.Gray;
			
			numberStyle = new DataGridViewCellStyle();
			numberStyle.Font = DefaultFont;
			numberStyle.Format = "N4";
			numberStyle.NullValue = TableDefaults.DOUBLE;
			numberStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			
			textStyle = new DataGridViewCellStyle();
			textStyle.Font = DefaultFont;
			textStyle.Format = string.Empty;
			textStyle.NullValue = TableDefaults.STRING;
			textStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			
			this.FDataGridView = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// FDataGridView
			// 
			this.FDataGridView.Name = "FDataGridView";
			this.FDataGridView.Location = new System.Drawing.Point(0, 0);
			this.FDataGridView.Size = this.Size;
			this.FDataGridView.Cursor = System.Windows.Forms.Cursors.Default;
			this.FDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.FDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			
			this.FDataGridView.RowsDefaultCellStyle = defaultStyle;
			this.FDataGridView.AlternatingRowsDefaultCellStyle = defaultAltStyle;
			this.FDataGridView.DefaultCellStyle = new DataGridViewCellStyle();

			this.FDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridView1_CellValidating);
			this.FDataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(FDataGridView_MouseMove);
			this.FDataGridView.ColumnAdded += new DataGridViewColumnEventHandler(TableViewNode_ColumnAdded);
			// 
			// TableViewNode
			// 
			this.Controls.Add(this.FDataGridView);
			this.Name = "TableViewNode";
			this.Size = new System.Drawing.Size(344, 368);
			this.Resize += new System.EventHandler(this.TableViewWindow_Resize);
			((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).EndInit();
			this.ResumeLayout(false);

		}
		
		#region Layout Stuff
		void TableViewNode_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
		{
			if (e.Column.ValueType != null)
			{
					switch (e.Column.ValueType.Name)
					{
						case "String":
							e.Column.DefaultCellStyle = (textStyle);
							break;
						default:
							e.Column.DefaultCellStyle = (numberStyle);
							break;
					}
			}
		}

		
		
		private void TableViewWindow_Resize(object sender, EventArgs e)
		{
			this.FDataGridView.Size = this.Size;
		}
		#endregion Layout Stuff
		
		private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			try
			{
				System.Convert.ChangeType(e.FormattedValue,this.FDataGridView.Columns[e.ColumnIndex].ValueType);
			}
			catch
			{
				e.Cancel = true;
			}			
		}
	
		Point FMouseLast;
		bool FMouseDragging = false;
		void FDataGridView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Right))
			{
				if (FMouseDragging)
				{
					int stepOrder = 0;
					Debug.Print(Form.ModifierKeys.ToString());
					stepOrder += Form.ModifierKeys.HasFlag(Keys.Shift) ? 1 : 0;
					stepOrder += Form.ModifierKeys.HasFlag(Keys.Control)? 1 : 0;
					stepOrder *= Form.ModifierKeys.HasFlag(Keys.Alt) ? 1 : -1;
					double step = 0.01 * Math.Pow(10, stepOrder);

					double delta = - step * (double)(e.Y - FMouseLast.Y);
					foreach (DataGridViewCell cell in FDataGridView.SelectedCells)
						if (cell.Value.GetType() == typeof(System.Double) && cell.RowIndex < FData.Rows.Count) //avoids selection of the 'new row' at bottom or invalid cells
							cell.Value = (double)cell.Value + delta;
					FData.OnDataChange(this);
				}
				else
				{
					FMouseDragging = true;
				}
				FMouseLast = e.Location;
			}
			else
				FMouseDragging = false;
		}
		#endregion constructor and init

		public void Evaluate(int SpreadMax)
		{
			if (FPinInTable.SliceCount == 0)
			{
				FData = null;
				FDataGridView.DataSource = null;
				return;
			}
			
			if (FPinInTable[FTableIndex[0]] != FData)
			{
				FData = FPinInTable[FTableIndex[0]];
				FDataGridView.DataSource = FData;

				if (FData != null)
				{
					FDataGridView.TopLeftHeaderCell.Value = FData.NiceName;
					FDataGridView.AllowUserToAddRows = FAllowAddRow[0];
					foreach(DataGridViewColumn column in FDataGridView.Columns)
					{
						column.SortMode = DataGridViewColumnSortMode.NotSortable;
					}
				}
			}

			if (FData == null)
				return;
			
			if (FAllowAddRow.IsChanged)
				FDataGridView.AllowUserToAddRows = FAllowAddRow[0];

			if (FData.Rows.Count > 0)
			{
				bool moveRow = false;
				int selectedRow = 0;

				if (FDataGridView.SelectedCells.Count > 0)
					selectedRow = FDataGridView.SelectedCells[0].RowIndex;
				else
					selectedRow = 0;

				if (FUp[0])
				{
					selectedRow++;
					selectedRow %= FData.Rows.Count;
					moveRow = true;
				}

				if (FDown[0])
				{
					selectedRow--;
					if (selectedRow < 0)
						selectedRow += FData.Rows.Count;
					moveRow = true;
				}

				if (moveRow)
				{
					FDataGridView.ClearSelection();
					FDataGridView.Rows[selectedRow].Selected = true;
				}
			}

			if (FData.Rows.Count == 0)
			{
				FCurrentIndex.SliceCount = 1;
				FCurrentIndex[0] = 0;
			}
			else
			{
				var rows = FDataGridView.SelectedRows;
				if (rows.Count > 0)
				{
					FCurrentIndex.SliceCount = 0;
					foreach (DataGridViewRow row in rows)
					{
						FCurrentIndex.Add(row.Index);
					}
				}
				else
				{
					int row = FDataGridView.CurrentCellAddress.Y;
					FCurrentIndex.SliceCount = 1;
					FCurrentIndex[0] = row;
				}
			}
		}
	}
}
