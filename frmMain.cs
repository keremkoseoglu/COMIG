using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Comig
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu menuMain;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBasic;
		private System.Windows.Forms.TextBox txtConn;
		private System.Windows.Forms.Label lblConn;
		private System.Windows.Forms.TextBox txtProject;
		private System.Windows.Forms.Label lblProject;
		private System.Windows.Forms.Label lblOutput;
		private System.Windows.Forms.GroupBox groupItem;
		private System.Windows.Forms.MenuItem menuNew;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.MenuItem menuSave;
		private System.Windows.Forms.MenuItem menuSaveAs;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ImageList ilMain;

		private ComigDoc doc;
		private TreeNode[] tableNode;
		private System.Windows.Forms.TreeView tree;
		private TreeNode[] fieldNode;
		private int tableIndex, fieldIndex;
		private System.Windows.Forms.TextBox txtPath;
		private System.Windows.Forms.SaveFileDialog sfDialog;
		private System.Windows.Forms.OpenFileDialog ofDialog;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.TabControl tabMain;
		private System.Windows.Forms.TabPage tabTable;
		private System.Windows.Forms.Button btnTableDelete;
		private System.Windows.Forms.Button btnTableOk;
		private System.Windows.Forms.CheckBox chkDelete;
		private System.Windows.Forms.CheckBox chkUpdate;
		private System.Windows.Forms.CheckBox chkInsert;
		private System.Windows.Forms.TextBox txtTableName;
		private System.Windows.Forms.Label lblTableName;
		private System.Windows.Forms.TabPage tabField;
		private System.Windows.Forms.Button btnFieldDelete;
		private System.Windows.Forms.CheckBox chkFieldName;
		private System.Windows.Forms.CheckBox chkFieldKey;
		private System.Windows.Forms.Button btnFieldOk;
		private System.Windows.Forms.ComboBox cmbFieldType;
		private System.Windows.Forms.Label lblFieldType;
		private System.Windows.Forms.TextBox txtFieldName;
		private System.Windows.Forms.Label lblFieldName;
		private System.Windows.Forms.Label lblDisp;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtTableDisp;
		private System.Windows.Forms.TextBox txtFieldDisp;
		private System.Windows.Forms.Button btnTreeUp;
		private System.Windows.Forms.Button btnTreeDown;
		private System.Windows.Forms.Label lblForTable;
		private System.Windows.Forms.TextBox txtForTable;
		private System.Windows.Forms.Label lblForKey;
		private System.Windows.Forms.TextBox txtForKey;
		private System.Windows.Forms.Label lblForName;
		private System.Windows.Forms.TextBox txtForName;
		private System.Windows.Forms.CheckBox chkSqlUnicode;
		private System.Windows.Forms.Label lblEncoding;
		private System.Windows.Forms.TextBox txtEncoding;

		private string fileName;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.Label lblUsername;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblPassword;
		private System.Windows.Forms.ListBox lstUser;
		private System.Windows.Forms.Button btnUserOk;
		private System.Windows.Forms.Button btnUserDelete;
		private System.Windows.Forms.GroupBox groupUser;
		private System.Windows.Forms.CheckedListBox lstUserTab;
		private System.Windows.Forms.RadioButton radAsc;
		private System.Windows.Forms.RadioButton radDesc;
		private bool progChange;

		public frmMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.Text = "COMig " + Application.ProductVersion.Substring(0, 3) + " - written by Kerem Koseoglu";
			tableNode = new TreeNode[255];
			fieldNode = new TreeNode[255];
			doc = new ComigDoc();
			doc.appVersion = Application.ProductVersion.Substring(0, 3);
			fileName = "";

			cmbFieldType.Items.Add(ComigField.TYPE.DATE.ToString());
			cmbFieldType.Items.Add(ComigField.TYPE.FOREIGN_KEY.ToString());
			cmbFieldType.Items.Add(ComigField.TYPE.MEMO.ToString());
			cmbFieldType.Items.Add(ComigField.TYPE.NUMBER.ToString());
			cmbFieldType.Items.Add(ComigField.TYPE.TEXT.ToString());

			clearForm();
		}

		private void clearForm()
		{
			progChange = true;
			txtProject.Text				= "";
			txtPath.Text				= "";
			txtConn.Text				= "";
			txtUsername.Text			= "";
			txtPassword.Text			= "";
			txtTableName.Text			= "";
			txtTableDisp.Text			= "";
			chkInsert.Checked			= false;
			chkUpdate.Checked			= false;
			chkDelete.Checked			= false;
			txtFieldName.Text			= "";
			txtFieldDisp.Text			= "";
			txtForTable.Text			= "";
			txtForKey.Text				= "";
			txtForName.Text				= "";
			txtForTable.Enabled			= false;
			txtForKey.Enabled			= false;
			txtForName.Enabled			= false;
			cmbFieldType.SelectedIndex	= 4;
			chkSqlUnicode.Checked		= false;
			chkFieldKey.Checked			= false;
			chkFieldName.Checked		= false;
			txtEncoding.Text			= "";
			tree.Nodes.Clear();
			progChange = false;
		}

		private void paintFormFromDoc()
		{
			clearForm();
			txtProject.Text				= doc.name;
			txtPath.Text				= doc.outputPath;
			txtConn.Text				= doc.connectionString;
			txtEncoding.Text			= doc.encoding;
			chkSqlUnicode.Checked		= doc.sqlServerUnicodeSupport;
			paintUsersFromDoc();
			paintTreeFromDoc();
		}

		private void paintUsersFromDoc()
		{
			lstUser.Items.Clear();

			for (int n = 0; n < doc.userCount; n++)
			{
				lstUser.Items.Add(doc.user[n].username);
			}
		}

		private void paintTreeFromDoc()
		{
			int lastIndex = -1;
			
			if (tree.SelectedNode != null) lastIndex = tree.SelectedNode.Index;

			tree.Nodes.Clear();
			lstUserTab.Items.Clear();

			tableNode = new TreeNode[255];
			for (int t = 0; t < doc.tableCount; t++)
			{
				fieldNode = new TreeNode[doc.table[t].fieldCount];
				for (int f = 0; f < doc.table[t].fieldCount; f++)
				{
					fieldNode[f] = new TreeNode(doc.table[t].field[f].name, 1, 1);
				}

				if (doc.table[t].fieldCount == 0)
				{
					tableNode[t] = new TreeNode(doc.table[t].name, 0, 0);
				}
				else
				{
					tableNode[t] = new TreeNode(doc.table[t].name, 0, 0, fieldNode);
				}
				
				tree.Nodes.Add(tableNode[t]);
				tree.ExpandAll();
				lstUserTab.Items.Add(doc.table[t].name);
		}
	}

		private void displayTableInfo()
		{
			txtTableName.Text = doc.table[tableIndex].name;
			txtTableDisp.Text = doc.table[tableIndex].displayName;
			if (doc.table[tableIndex].canInsert) chkInsert.Checked = true; else chkInsert.Checked = false;
			if (doc.table[tableIndex].canUpdate) chkUpdate.Checked = true; else chkUpdate.Checked = false;
			if (doc.table[tableIndex].canDelete) chkDelete.Checked = true; else chkDelete.Checked = false;
			//tabMain.SelectedIndex = 0;
		}

		private void displayFieldInfo()
		{
			txtFieldName.Text = doc.table[tableIndex].field[fieldIndex].name;
			txtFieldDisp.Text = doc.table[tableIndex].field[fieldIndex].displayName;
			if (doc.table[tableIndex].field[fieldIndex].isKey) chkFieldKey.Checked = true; else chkFieldKey.Checked = false;
			if (doc.table[tableIndex].field[fieldIndex].isName) 
			{
				chkFieldName.Checked = true; 
				switch(doc.table[tableIndex].orderType)
				{
					case ComigTable.ORDERTYPE.ASCENDING:
						radAsc.Checked = true;
						break;
					case ComigTable.ORDERTYPE.DESCENDING:
						radDesc.Checked = true;
						break;
				}
			}
			else chkFieldName.Checked = false;
			switch (doc.table[tableIndex].field[fieldIndex].type)
			{
				case ComigField.TYPE.DATE:
					cmbFieldType.SelectedIndex = 0;
					break;
				case ComigField.TYPE.FOREIGN_KEY:
					cmbFieldType.SelectedIndex = 1;
					txtForTable.Text	= doc.table[tableIndex].field[fieldIndex].fkTable;
					txtForKey.Text		= doc.table[tableIndex].field[fieldIndex].fkKeyField;
					txtForName.Text		= doc.table[tableIndex].field[fieldIndex].fkNameField;
					break;
				case ComigField.TYPE.MEMO:
					cmbFieldType.SelectedIndex = 2;
					break;
				case ComigField.TYPE.NUMBER:
					cmbFieldType.SelectedIndex = 3;
					break;
				case ComigField.TYPE.TEXT:
					cmbFieldType.SelectedIndex = 4;
					break;
			}
			//tabMain.SelectedIndex = 1;

		}

		private void getTableIndex()
		{
			tableIndex = -1;
			fieldIndex = -1;
			if (tree.SelectedNode == null) return;
			tableIndex = doc.getTableIndex(tree.SelectedNode.Text);
		}

		private void getFieldIndex()
		{
			tableIndex = -1;
			fieldIndex = -1;
			if (tree.SelectedNode == null) return;
			tableIndex = doc.getTableIndex(tree.SelectedNode.Parent.Text);
			fieldIndex = doc.table[tableIndex].getFieldIndex(tree.SelectedNode.Text);
		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
			this.menuMain = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuNew = new System.Windows.Forms.MenuItem();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.menuSave = new System.Windows.Forms.MenuItem();
			this.menuSaveAs = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBasic = new System.Windows.Forms.GroupBox();
			this.txtEncoding = new System.Windows.Forms.TextBox();
			this.lblEncoding = new System.Windows.Forms.Label();
			this.chkSqlUnicode = new System.Windows.Forms.CheckBox();
			this.txtConn = new System.Windows.Forms.TextBox();
			this.lblConn = new System.Windows.Forms.Label();
			this.txtProject = new System.Windows.Forms.TextBox();
			this.lblProject = new System.Windows.Forms.Label();
			this.txtPath = new System.Windows.Forms.TextBox();
			this.lblOutput = new System.Windows.Forms.Label();
			this.groupItem = new System.Windows.Forms.GroupBox();
			this.btnTreeDown = new System.Windows.Forms.Button();
			this.btnTreeUp = new System.Windows.Forms.Button();
			this.tabMain = new System.Windows.Forms.TabControl();
			this.tabTable = new System.Windows.Forms.TabPage();
			this.txtTableDisp = new System.Windows.Forms.TextBox();
			this.lblDisp = new System.Windows.Forms.Label();
			this.btnTableDelete = new System.Windows.Forms.Button();
			this.btnTableOk = new System.Windows.Forms.Button();
			this.chkDelete = new System.Windows.Forms.CheckBox();
			this.chkUpdate = new System.Windows.Forms.CheckBox();
			this.chkInsert = new System.Windows.Forms.CheckBox();
			this.txtTableName = new System.Windows.Forms.TextBox();
			this.lblTableName = new System.Windows.Forms.Label();
			this.tabField = new System.Windows.Forms.TabPage();
			this.txtForName = new System.Windows.Forms.TextBox();
			this.lblForName = new System.Windows.Forms.Label();
			this.ilMain = new System.Windows.Forms.ImageList(this.components);
			this.txtForKey = new System.Windows.Forms.TextBox();
			this.lblForKey = new System.Windows.Forms.Label();
			this.txtForTable = new System.Windows.Forms.TextBox();
			this.lblForTable = new System.Windows.Forms.Label();
			this.txtFieldDisp = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnFieldDelete = new System.Windows.Forms.Button();
			this.chkFieldName = new System.Windows.Forms.CheckBox();
			this.chkFieldKey = new System.Windows.Forms.CheckBox();
			this.btnFieldOk = new System.Windows.Forms.Button();
			this.cmbFieldType = new System.Windows.Forms.ComboBox();
			this.lblFieldType = new System.Windows.Forms.Label();
			this.txtFieldName = new System.Windows.Forms.TextBox();
			this.lblFieldName = new System.Windows.Forms.Label();
			this.tree = new System.Windows.Forms.TreeView();
			this.sfDialog = new System.Windows.Forms.SaveFileDialog();
			this.ofDialog = new System.Windows.Forms.OpenFileDialog();
			this.groupUser = new System.Windows.Forms.GroupBox();
			this.lstUserTab = new System.Windows.Forms.CheckedListBox();
			this.btnUserDelete = new System.Windows.Forms.Button();
			this.btnUserOk = new System.Windows.Forms.Button();
			this.lstUser = new System.Windows.Forms.ListBox();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.lblUsername = new System.Windows.Forms.Label();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			this.radAsc = new System.Windows.Forms.RadioButton();
			this.radDesc = new System.Windows.Forms.RadioButton();
			this.groupBasic.SuspendLayout();
			this.groupItem.SuspendLayout();
			this.tabMain.SuspendLayout();
			this.tabTable.SuspendLayout();
			this.tabField.SuspendLayout();
			this.groupUser.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuMain
			// 
			this.menuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuItem1});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuNew,
																					  this.menuOpen,
																					  this.menuSave,
																					  this.menuSaveAs,
																					  this.menuItem5,
																					  this.menuItem4,
																					  this.menuItem2,
																					  this.menuExit});
			this.menuItem1.Text = "&File";
			// 
			// menuNew
			// 
			this.menuNew.Index = 0;
			this.menuNew.Text = "&New";
			this.menuNew.Click += new System.EventHandler(this.menuNew_Click);
			// 
			// menuOpen
			// 
			this.menuOpen.Index = 1;
			this.menuOpen.Text = "&Open...";
			this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// menuSave
			// 
			this.menuSave.Index = 2;
			this.menuSave.Text = "&Save";
			this.menuSave.Click += new System.EventHandler(this.menuSave_Click);
			// 
			// menuSaveAs
			// 
			this.menuSaveAs.Index = 3;
			this.menuSaveAs.Text = "Save &As...";
			this.menuSaveAs.Click += new System.EventHandler(this.menuSaveAs_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 4;
			this.menuItem5.Text = "-";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 5;
			this.menuItem4.Text = "&Export";
			this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 6;
			this.menuItem2.Text = "-";
			// 
			// menuExit
			// 
			this.menuExit.Index = 7;
			this.menuExit.Text = "E&xit";
			this.menuExit.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(16, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			// 
			// groupBasic
			// 
			this.groupBasic.Controls.Add(this.txtEncoding);
			this.groupBasic.Controls.Add(this.lblEncoding);
			this.groupBasic.Controls.Add(this.chkSqlUnicode);
			this.groupBasic.Controls.Add(this.txtConn);
			this.groupBasic.Controls.Add(this.lblConn);
			this.groupBasic.Controls.Add(this.txtProject);
			this.groupBasic.Controls.Add(this.lblProject);
			this.groupBasic.Controls.Add(this.txtPath);
			this.groupBasic.Controls.Add(this.lblOutput);
			this.groupBasic.Location = new System.Drawing.Point(8, 8);
			this.groupBasic.Name = "groupBasic";
			this.groupBasic.Size = new System.Drawing.Size(272, 152);
			this.groupBasic.TabIndex = 1;
			this.groupBasic.TabStop = false;
			this.groupBasic.Text = "Basic Settings";
			// 
			// txtEncoding
			// 
			this.txtEncoding.Location = new System.Drawing.Point(120, 120);
			this.txtEncoding.Name = "txtEncoding";
			this.txtEncoding.Size = new System.Drawing.Size(48, 20);
			this.txtEncoding.TabIndex = 19;
			this.txtEncoding.Text = "";
			this.txtEncoding.TextChanged += new System.EventHandler(this.txtEncoding_TextChanged);
			// 
			// lblEncoding
			// 
			this.lblEncoding.Location = new System.Drawing.Point(16, 120);
			this.lblEncoding.Name = "lblEncoding";
			this.lblEncoding.Size = new System.Drawing.Size(80, 23);
			this.lblEncoding.TabIndex = 18;
			this.lblEncoding.Text = "Encoding";
			// 
			// chkSqlUnicode
			// 
			this.chkSqlUnicode.Location = new System.Drawing.Point(176, 120);
			this.chkSqlUnicode.Name = "chkSqlUnicode";
			this.chkSqlUnicode.Size = new System.Drawing.Size(90, 24);
			this.chkSqlUnicode.TabIndex = 17;
			this.chkSqlUnicode.Text = "SQL Unicode";
			this.chkSqlUnicode.CheckedChanged += new System.EventHandler(this.chkSqlUnicode_CheckedChanged);
			// 
			// txtConn
			// 
			this.txtConn.Location = new System.Drawing.Point(120, 88);
			this.txtConn.Name = "txtConn";
			this.txtConn.Size = new System.Drawing.Size(136, 20);
			this.txtConn.TabIndex = 12;
			this.txtConn.Text = "";
			this.txtConn.TextChanged += new System.EventHandler(this.txtConn_TextChanged);
			// 
			// lblConn
			// 
			this.lblConn.Location = new System.Drawing.Point(16, 88);
			this.lblConn.Name = "lblConn";
			this.lblConn.TabIndex = 11;
			this.lblConn.Text = "Connection String";
			// 
			// txtProject
			// 
			this.txtProject.Location = new System.Drawing.Point(120, 24);
			this.txtProject.Name = "txtProject";
			this.txtProject.Size = new System.Drawing.Size(136, 20);
			this.txtProject.TabIndex = 10;
			this.txtProject.Text = "";
			this.txtProject.TextChanged += new System.EventHandler(this.txtProject_TextChanged);
			// 
			// lblProject
			// 
			this.lblProject.Location = new System.Drawing.Point(16, 24);
			this.lblProject.Name = "lblProject";
			this.lblProject.Size = new System.Drawing.Size(80, 23);
			this.lblProject.TabIndex = 9;
			this.lblProject.Text = "Project Name";
			// 
			// txtPath
			// 
			this.txtPath.Location = new System.Drawing.Point(120, 56);
			this.txtPath.Name = "txtPath";
			this.txtPath.Size = new System.Drawing.Size(136, 20);
			this.txtPath.TabIndex = 8;
			this.txtPath.Text = "";
			this.txtPath.TextChanged += new System.EventHandler(this.txtPath_TextChanged);
			// 
			// lblOutput
			// 
			this.lblOutput.Location = new System.Drawing.Point(16, 56);
			this.lblOutput.Name = "lblOutput";
			this.lblOutput.Size = new System.Drawing.Size(80, 23);
			this.lblOutput.TabIndex = 7;
			this.lblOutput.Text = "Output Path";
			// 
			// groupItem
			// 
			this.groupItem.Controls.Add(this.btnTreeDown);
			this.groupItem.Controls.Add(this.btnTreeUp);
			this.groupItem.Controls.Add(this.tabMain);
			this.groupItem.Controls.Add(this.tree);
			this.groupItem.Location = new System.Drawing.Point(8, 168);
			this.groupItem.Name = "groupItem";
			this.groupItem.Size = new System.Drawing.Size(536, 264);
			this.groupItem.TabIndex = 2;
			this.groupItem.TabStop = false;
			this.groupItem.Text = "Items";
			// 
			// btnTreeDown
			// 
			this.btnTreeDown.Image = ((System.Drawing.Image)(resources.GetObject("btnTreeDown.Image")));
			this.btnTreeDown.Location = new System.Drawing.Point(192, 224);
			this.btnTreeDown.Name = "btnTreeDown";
			this.btnTreeDown.Size = new System.Drawing.Size(24, 24);
			this.btnTreeDown.TabIndex = 14;
			this.btnTreeDown.Click += new System.EventHandler(this.btnTreeDown_Click);
			// 
			// btnTreeUp
			// 
			this.btnTreeUp.Image = ((System.Drawing.Image)(resources.GetObject("btnTreeUp.Image")));
			this.btnTreeUp.Location = new System.Drawing.Point(160, 224);
			this.btnTreeUp.Name = "btnTreeUp";
			this.btnTreeUp.Size = new System.Drawing.Size(24, 24);
			this.btnTreeUp.TabIndex = 13;
			this.btnTreeUp.Click += new System.EventHandler(this.btnTreeUp_Click);
			// 
			// tabMain
			// 
			this.tabMain.Controls.Add(this.tabTable);
			this.tabMain.Controls.Add(this.tabField);
			this.tabMain.ImageList = this.ilMain;
			this.tabMain.Location = new System.Drawing.Point(224, 24);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(304, 224);
			this.tabMain.TabIndex = 7;
			// 
			// tabTable
			// 
			this.tabTable.Controls.Add(this.txtTableDisp);
			this.tabTable.Controls.Add(this.lblDisp);
			this.tabTable.Controls.Add(this.btnTableDelete);
			this.tabTable.Controls.Add(this.btnTableOk);
			this.tabTable.Controls.Add(this.chkDelete);
			this.tabTable.Controls.Add(this.chkUpdate);
			this.tabTable.Controls.Add(this.chkInsert);
			this.tabTable.Controls.Add(this.txtTableName);
			this.tabTable.Controls.Add(this.lblTableName);
			this.tabTable.ImageIndex = 0;
			this.tabTable.Location = new System.Drawing.Point(4, 23);
			this.tabTable.Name = "tabTable";
			this.tabTable.Size = new System.Drawing.Size(296, 197);
			this.tabTable.TabIndex = 0;
			this.tabTable.Text = "Table";
			// 
			// txtTableDisp
			// 
			this.txtTableDisp.Location = new System.Drawing.Point(88, 48);
			this.txtTableDisp.Name = "txtTableDisp";
			this.txtTableDisp.Size = new System.Drawing.Size(200, 20);
			this.txtTableDisp.TabIndex = 15;
			this.txtTableDisp.Text = "";
			// 
			// lblDisp
			// 
			this.lblDisp.Location = new System.Drawing.Point(8, 48);
			this.lblDisp.Name = "lblDisp";
			this.lblDisp.Size = new System.Drawing.Size(72, 23);
			this.lblDisp.TabIndex = 14;
			this.lblDisp.Text = "Display As";
			// 
			// btnTableDelete
			// 
			this.btnTableDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnTableDelete.Image")));
			this.btnTableDelete.Location = new System.Drawing.Point(208, 168);
			this.btnTableDelete.Name = "btnTableDelete";
			this.btnTableDelete.Size = new System.Drawing.Size(75, 24);
			this.btnTableDelete.TabIndex = 13;
			this.btnTableDelete.Click += new System.EventHandler(this.btnTableDelete_Click);
			// 
			// btnTableOk
			// 
			this.btnTableOk.Image = ((System.Drawing.Image)(resources.GetObject("btnTableOk.Image")));
			this.btnTableOk.Location = new System.Drawing.Point(120, 168);
			this.btnTableOk.Name = "btnTableOk";
			this.btnTableOk.Size = new System.Drawing.Size(75, 24);
			this.btnTableOk.TabIndex = 12;
			this.btnTableOk.Click += new System.EventHandler(this.btnTableOk_Click);
			// 
			// chkDelete
			// 
			this.chkDelete.Location = new System.Drawing.Point(192, 80);
			this.chkDelete.Name = "chkDelete";
			this.chkDelete.Size = new System.Drawing.Size(88, 24);
			this.chkDelete.TabIndex = 11;
			this.chkDelete.Text = "Allow Delete";
			// 
			// chkUpdate
			// 
			this.chkUpdate.Location = new System.Drawing.Point(96, 80);
			this.chkUpdate.Name = "chkUpdate";
			this.chkUpdate.Size = new System.Drawing.Size(96, 24);
			this.chkUpdate.TabIndex = 10;
			this.chkUpdate.Text = "Allow Update";
			// 
			// chkInsert
			// 
			this.chkInsert.Location = new System.Drawing.Point(8, 80);
			this.chkInsert.Name = "chkInsert";
			this.chkInsert.Size = new System.Drawing.Size(88, 24);
			this.chkInsert.TabIndex = 9;
			this.chkInsert.Text = "Allow Insert";
			// 
			// txtTableName
			// 
			this.txtTableName.Location = new System.Drawing.Point(88, 16);
			this.txtTableName.Name = "txtTableName";
			this.txtTableName.Size = new System.Drawing.Size(200, 20);
			this.txtTableName.TabIndex = 8;
			this.txtTableName.Text = "";
			// 
			// lblTableName
			// 
			this.lblTableName.Location = new System.Drawing.Point(8, 16);
			this.lblTableName.Name = "lblTableName";
			this.lblTableName.Size = new System.Drawing.Size(72, 23);
			this.lblTableName.TabIndex = 7;
			this.lblTableName.Text = "Name";
			// 
			// tabField
			// 
			this.tabField.Controls.Add(this.radDesc);
			this.tabField.Controls.Add(this.radAsc);
			this.tabField.Controls.Add(this.txtForName);
			this.tabField.Controls.Add(this.lblForName);
			this.tabField.Controls.Add(this.txtForKey);
			this.tabField.Controls.Add(this.lblForKey);
			this.tabField.Controls.Add(this.txtForTable);
			this.tabField.Controls.Add(this.lblForTable);
			this.tabField.Controls.Add(this.txtFieldDisp);
			this.tabField.Controls.Add(this.label1);
			this.tabField.Controls.Add(this.btnFieldDelete);
			this.tabField.Controls.Add(this.chkFieldName);
			this.tabField.Controls.Add(this.chkFieldKey);
			this.tabField.Controls.Add(this.btnFieldOk);
			this.tabField.Controls.Add(this.cmbFieldType);
			this.tabField.Controls.Add(this.lblFieldType);
			this.tabField.Controls.Add(this.txtFieldName);
			this.tabField.Controls.Add(this.lblFieldName);
			this.tabField.ImageIndex = 1;
			this.tabField.Location = new System.Drawing.Point(4, 23);
			this.tabField.Name = "tabField";
			this.tabField.Size = new System.Drawing.Size(296, 197);
			this.tabField.TabIndex = 1;
			this.tabField.Text = "Field";
			// 
			// txtForName
			// 
			this.txtForName.Location = new System.Drawing.Point(240, 112);
			this.txtForName.Name = "txtForName";
			this.txtForName.Size = new System.Drawing.Size(40, 20);
			this.txtForName.TabIndex = 25;
			this.txtForName.Text = "";
			// 
			// lblForName
			// 
			this.lblForName.ImageIndex = 3;
			this.lblForName.ImageList = this.ilMain;
			this.lblForName.Location = new System.Drawing.Point(216, 112);
			this.lblForName.Name = "lblForName";
			this.lblForName.Size = new System.Drawing.Size(24, 23);
			this.lblForName.TabIndex = 24;
			// 
			// ilMain
			// 
			this.ilMain.ImageSize = new System.Drawing.Size(16, 16);
			this.ilMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilMain.ImageStream")));
			this.ilMain.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// txtForKey
			// 
			this.txtForKey.Location = new System.Drawing.Point(176, 112);
			this.txtForKey.Name = "txtForKey";
			this.txtForKey.Size = new System.Drawing.Size(40, 20);
			this.txtForKey.TabIndex = 23;
			this.txtForKey.Text = "";
			// 
			// lblForKey
			// 
			this.lblForKey.ImageIndex = 2;
			this.lblForKey.ImageList = this.ilMain;
			this.lblForKey.Location = new System.Drawing.Point(152, 112);
			this.lblForKey.Name = "lblForKey";
			this.lblForKey.Size = new System.Drawing.Size(24, 23);
			this.lblForKey.TabIndex = 22;
			// 
			// txtForTable
			// 
			this.txtForTable.Location = new System.Drawing.Point(112, 112);
			this.txtForTable.Name = "txtForTable";
			this.txtForTable.Size = new System.Drawing.Size(40, 20);
			this.txtForTable.TabIndex = 21;
			this.txtForTable.Text = "";
			// 
			// lblForTable
			// 
			this.lblForTable.ImageIndex = 0;
			this.lblForTable.ImageList = this.ilMain;
			this.lblForTable.Location = new System.Drawing.Point(88, 112);
			this.lblForTable.Name = "lblForTable";
			this.lblForTable.Size = new System.Drawing.Size(24, 23);
			this.lblForTable.TabIndex = 20;
			// 
			// txtFieldDisp
			// 
			this.txtFieldDisp.Location = new System.Drawing.Point(88, 48);
			this.txtFieldDisp.Name = "txtFieldDisp";
			this.txtFieldDisp.Size = new System.Drawing.Size(200, 20);
			this.txtFieldDisp.TabIndex = 19;
			this.txtFieldDisp.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 18;
			this.label1.Text = "Display As";
			// 
			// btnFieldDelete
			// 
			this.btnFieldDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnFieldDelete.Image")));
			this.btnFieldDelete.Location = new System.Drawing.Point(208, 184);
			this.btnFieldDelete.Name = "btnFieldDelete";
			this.btnFieldDelete.Size = new System.Drawing.Size(75, 24);
			this.btnFieldDelete.TabIndex = 17;
			this.btnFieldDelete.Click += new System.EventHandler(this.btnFieldDelete_Click);
			// 
			// chkFieldName
			// 
			this.chkFieldName.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.chkFieldName.ImageList = this.ilMain;
			this.chkFieldName.Location = new System.Drawing.Point(80, 152);
			this.chkFieldName.Name = "chkFieldName";
			this.chkFieldName.Size = new System.Drawing.Size(64, 24);
			this.chkFieldName.TabIndex = 16;
			this.chkFieldName.Text = "Name";
			this.chkFieldName.CheckedChanged += new System.EventHandler(this.chkFieldName_CheckedChanged);
			// 
			// chkFieldKey
			// 
			this.chkFieldKey.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.chkFieldKey.ImageList = this.ilMain;
			this.chkFieldKey.Location = new System.Drawing.Point(8, 152);
			this.chkFieldKey.Name = "chkFieldKey";
			this.chkFieldKey.Size = new System.Drawing.Size(64, 24);
			this.chkFieldKey.TabIndex = 15;
			this.chkFieldKey.Text = "Key";
			// 
			// btnFieldOk
			// 
			this.btnFieldOk.Image = ((System.Drawing.Image)(resources.GetObject("btnFieldOk.Image")));
			this.btnFieldOk.Location = new System.Drawing.Point(120, 184);
			this.btnFieldOk.Name = "btnFieldOk";
			this.btnFieldOk.Size = new System.Drawing.Size(75, 24);
			this.btnFieldOk.TabIndex = 14;
			this.btnFieldOk.Click += new System.EventHandler(this.btnFieldOk_Click);
			// 
			// cmbFieldType
			// 
			this.cmbFieldType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbFieldType.Location = new System.Drawing.Point(88, 80);
			this.cmbFieldType.Name = "cmbFieldType";
			this.cmbFieldType.Size = new System.Drawing.Size(200, 21);
			this.cmbFieldType.TabIndex = 13;
			this.cmbFieldType.SelectedIndexChanged += new System.EventHandler(this.cmbFieldType_SelectedIndexChanged);
			// 
			// lblFieldType
			// 
			this.lblFieldType.Location = new System.Drawing.Point(8, 80);
			this.lblFieldType.Name = "lblFieldType";
			this.lblFieldType.Size = new System.Drawing.Size(56, 23);
			this.lblFieldType.TabIndex = 12;
			this.lblFieldType.Text = "Type";
			// 
			// txtFieldName
			// 
			this.txtFieldName.Location = new System.Drawing.Point(88, 16);
			this.txtFieldName.Name = "txtFieldName";
			this.txtFieldName.Size = new System.Drawing.Size(200, 20);
			this.txtFieldName.TabIndex = 11;
			this.txtFieldName.Text = "";
			// 
			// lblFieldName
			// 
			this.lblFieldName.Location = new System.Drawing.Point(8, 16);
			this.lblFieldName.Name = "lblFieldName";
			this.lblFieldName.Size = new System.Drawing.Size(56, 23);
			this.lblFieldName.TabIndex = 10;
			this.lblFieldName.Text = "Name";
			// 
			// tree
			// 
			this.tree.HotTracking = true;
			this.tree.ImageList = this.ilMain;
			this.tree.Location = new System.Drawing.Point(16, 24);
			this.tree.Name = "tree";
			this.tree.Size = new System.Drawing.Size(200, 192);
			this.tree.TabIndex = 6;
			this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_AfterSelect);
			// 
			// sfDialog
			// 
			this.sfDialog.DefaultExt = "cmg";
			this.sfDialog.Filter = "CMG Files (*.cmg)|*.cmg|All Files (*.*)|*.*";
			// 
			// ofDialog
			// 
			this.ofDialog.DefaultExt = "cmg";
			this.ofDialog.Filter = "CMG Files (*.cmg)|*.cmg|All Files (*.*)|*.*";
			// 
			// groupUser
			// 
			this.groupUser.Controls.Add(this.lstUserTab);
			this.groupUser.Controls.Add(this.btnUserDelete);
			this.groupUser.Controls.Add(this.btnUserOk);
			this.groupUser.Controls.Add(this.lstUser);
			this.groupUser.Controls.Add(this.txtUsername);
			this.groupUser.Controls.Add(this.lblUsername);
			this.groupUser.Controls.Add(this.txtPassword);
			this.groupUser.Controls.Add(this.lblPassword);
			this.groupUser.Location = new System.Drawing.Point(288, 8);
			this.groupUser.Name = "groupUser";
			this.groupUser.Size = new System.Drawing.Size(256, 152);
			this.groupUser.TabIndex = 3;
			this.groupUser.TabStop = false;
			this.groupUser.Text = "Users";
			// 
			// lstUserTab
			// 
			this.lstUserTab.Location = new System.Drawing.Point(136, 24);
			this.lstUserTab.Name = "lstUserTab";
			this.lstUserTab.Size = new System.Drawing.Size(112, 49);
			this.lstUserTab.TabIndex = 28;
			// 
			// btnUserDelete
			// 
			this.btnUserDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnUserDelete.Image")));
			this.btnUserDelete.Location = new System.Drawing.Point(200, 120);
			this.btnUserDelete.Name = "btnUserDelete";
			this.btnUserDelete.Size = new System.Drawing.Size(48, 24);
			this.btnUserDelete.TabIndex = 27;
			// 
			// btnUserOk
			// 
			this.btnUserOk.Image = ((System.Drawing.Image)(resources.GetObject("btnUserOk.Image")));
			this.btnUserOk.Location = new System.Drawing.Point(200, 88);
			this.btnUserOk.Name = "btnUserOk";
			this.btnUserOk.Size = new System.Drawing.Size(48, 24);
			this.btnUserOk.TabIndex = 26;
			this.btnUserOk.Click += new System.EventHandler(this.btnUserOk_Click);
			// 
			// lstUser
			// 
			this.lstUser.Location = new System.Drawing.Point(16, 24);
			this.lstUser.Name = "lstUser";
			this.lstUser.Size = new System.Drawing.Size(112, 56);
			this.lstUser.TabIndex = 25;
			this.lstUser.SelectedIndexChanged += new System.EventHandler(this.lstUser_SelectedIndexChanged);
			// 
			// txtUsername
			// 
			this.txtUsername.Location = new System.Drawing.Point(96, 96);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(96, 20);
			this.txtUsername.TabIndex = 24;
			this.txtUsername.Text = "";
			// 
			// lblUsername
			// 
			this.lblUsername.Location = new System.Drawing.Point(16, 96);
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size(80, 23);
			this.lblUsername.TabIndex = 23;
			this.lblUsername.Text = "Username";
			// 
			// txtPassword
			// 
			this.txtPassword.Location = new System.Drawing.Point(96, 120);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.PasswordChar = '*';
			this.txtPassword.Size = new System.Drawing.Size(96, 20);
			this.txtPassword.TabIndex = 22;
			this.txtPassword.Text = "";
			// 
			// lblPassword
			// 
			this.lblPassword.Location = new System.Drawing.Point(16, 120);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(80, 23);
			this.lblPassword.TabIndex = 21;
			this.lblPassword.Text = "Password";
			// 
			// radAsc
			// 
			this.radAsc.Checked = true;
			this.radAsc.Location = new System.Drawing.Point(152, 152);
			this.radAsc.Name = "radAsc";
			this.radAsc.Size = new System.Drawing.Size(48, 24);
			this.radAsc.TabIndex = 26;
			this.radAsc.TabStop = true;
			this.radAsc.Text = "ASC";
			// 
			// radDesc
			// 
			this.radDesc.Location = new System.Drawing.Point(200, 152);
			this.radDesc.Name = "radDesc";
			this.radDesc.Size = new System.Drawing.Size(56, 24);
			this.radDesc.TabIndex = 27;
			this.radDesc.Text = "DESC";
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(554, 443);
			this.Controls.Add(this.groupUser);
			this.Controls.Add(this.groupItem);
			this.Controls.Add(this.groupBasic);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.menuMain;
			this.Name = "frmMain";
			this.Text = "Form1";
			this.Resize += new System.EventHandler(this.frmMain_Resize);
			this.groupBasic.ResumeLayout(false);
			this.groupItem.ResumeLayout(false);
			this.tabMain.ResumeLayout(false);
			this.tabTable.ResumeLayout(false);
			this.tabField.ResumeLayout(false);
			this.groupUser.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new frmMain());
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void btnTableOk_Click(object sender, System.EventArgs e)
		{
			if (tree.SelectedNode == null) 
			{
				addTable();
			}
			else
			{
				getTableIndex();
				if (tableIndex < 0) addTable(); else updateTable();
			}
			paintTreeFromDoc();
			tabMain.SelectedTab = tabMain.TabPages[0];
		}

		private void addTable()
		{
			if (doc.addTable(txtTableName.Text))
			{
				doc.table[doc.tableCount - 1].displayName = txtTableDisp.Text;
				if (chkInsert.Checked) doc.table[doc.tableCount - 1].canInsert = true;
				if (chkUpdate.Checked) doc.table[doc.tableCount - 1].canUpdate = true;
				if (chkDelete.Checked) doc.table[doc.tableCount - 1].canDelete = true;
			}
			paintTreeFromDoc();
		}

		private void updateTable()
		{
			doc.table[tableIndex].name = txtTableName.Text;
			doc.table[tableIndex].displayName = txtTableDisp.Text;
			if (chkInsert.Checked) doc.table[tableIndex].canInsert = true; else doc.table[tableIndex].canInsert = false;
			if (chkUpdate.Checked) doc.table[tableIndex].canUpdate = true; else doc.table[tableIndex].canUpdate = false;
			if (chkDelete.Checked) doc.table[tableIndex].canDelete = true; else doc.table[tableIndex].canDelete = false;
		}

		private void tree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			switch (tree.SelectedNode.ImageIndex)
			{
				case 0:
					getTableIndex();
					displayTableInfo();
					break;
				case 1:
					getFieldIndex();
					displayFieldInfo();
					break;
			}
		}

		private void btnFieldOk_Click(object sender, System.EventArgs e)
		{
			ComigField.TYPE type = ComigField.TYPE.TEXT;

			if (tree.SelectedNode == null) return;

			switch (cmbFieldType.SelectedIndex)
			{
				case 0:
					type = ComigField.TYPE.DATE;
					break;
				case 1:
					type = ComigField.TYPE.FOREIGN_KEY;
					break;
				case 2:
					type = ComigField.TYPE.MEMO;
					break;
				case 3:
					type = ComigField.TYPE.NUMBER;
					break;
				case 4:
					type = ComigField.TYPE.TEXT;
					break;
			}

			if (tree.SelectedNode.ImageIndex == 0) 
			{
				getTableIndex();

				if (doc.table[tableIndex].addField(txtFieldName.Text, type))
				{
					doc.table[tableIndex].field[doc.table[tableIndex].fieldCount - 1].displayName = txtFieldDisp.Text;
					if (chkFieldKey.Checked) doc.table[tableIndex].setKeyField(doc.table[tableIndex].fieldCount - 1);
					if (chkFieldName.Checked) 
					{
						doc.table[tableIndex].setNameField(doc.table[tableIndex].fieldCount - 1);
						if (radAsc.Checked)		doc.table[tableIndex].orderType = ComigTable.ORDERTYPE.ASCENDING;
						if (radDesc.Checked)	doc.table[tableIndex].orderType = ComigTable.ORDERTYPE.DESCENDING;
					}

					if (type == ComigField.TYPE.FOREIGN_KEY)
					{
						doc.table[tableIndex].field[doc.table[tableIndex].fieldCount - 1].fkTable		= txtForTable.Text;
						doc.table[tableIndex].field[doc.table[tableIndex].fieldCount - 1].fkKeyField	= txtForKey.Text;
						doc.table[tableIndex].field[doc.table[tableIndex].fieldCount - 1].fkNameField	= txtForName.Text;
					}
				}
				paintTreeFromDoc();
			}
			else
			{
				getFieldIndex();
				doc.table[tableIndex].field[fieldIndex].name = txtFieldName.Text;
				doc.table[tableIndex].field[fieldIndex].displayName = txtFieldDisp.Text;
				doc.table[tableIndex].field[fieldIndex].type = type;
				if (chkFieldKey.Checked) doc.table[tableIndex].setKeyField(fieldIndex);
				if (chkFieldName.Checked) 
				{
					doc.table[tableIndex].setNameField(fieldIndex);
					if (radAsc.Checked)		doc.table[tableIndex].orderType = ComigTable.ORDERTYPE.ASCENDING;
					if (radDesc.Checked)	doc.table[tableIndex].orderType = ComigTable.ORDERTYPE.DESCENDING;
				}

				if (type == ComigField.TYPE.FOREIGN_KEY)
				{
					doc.table[tableIndex].field[fieldIndex].fkTable		= txtForTable.Text;
					doc.table[tableIndex].field[fieldIndex].fkKeyField	= txtForKey.Text;
					doc.table[tableIndex].field[fieldIndex].fkNameField	= txtForName.Text;
				}
				paintTreeFromDoc();
			}

			tabMain.SelectedTab = tabMain.TabPages[1];
		}

		private void btnTableDelete_Click(object sender, System.EventArgs e)
		{
			getTableIndex();
			doc.removeTable(tableIndex);
			paintTreeFromDoc();
		}

		private void btnFieldDelete_Click(object sender, System.EventArgs e)
		{
			getFieldIndex();
			if (tableIndex < 0) return;
			doc.table[tableIndex].removeField(fieldIndex);
			paintTreeFromDoc();
		}

		private void menuNew_Click(object sender, System.EventArgs e)
		{
			doc.clear();
			clearForm();
		}

		private void menuSave_Click(object sender, System.EventArgs e)
		{
			if (fileName == "")
			{
				if (sfDialog.ShowDialog() == DialogResult.OK)
				{
					fileName = sfDialog.FileName;
					doc.saveFile(fileName);	
				}
			}
			else
			{
				doc.saveFile(fileName);
			}
		}

		private void txtProject_TextChanged(object sender, System.EventArgs e)
		{
			if (progChange) return;
			doc.name = txtProject.Text;
		}

		private void txtPath_TextChanged(object sender, System.EventArgs e)
		{
			if (progChange) return;
			doc.outputPath = txtPath.Text;
		}

		private void txtConn_TextChanged(object sender, System.EventArgs e)
		{
			if (progChange) return;
			doc.connectionString = txtConn.Text;
		}

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
			if (ofDialog.ShowDialog() == DialogResult.OK)
			{
				fileName = ofDialog.FileName;
				doc.loadFile(fileName);
				paintFormFromDoc();
			}			
		}

		private void menuSaveAs_Click(object sender, System.EventArgs e)
		{
			if (sfDialog.ShowDialog() == DialogResult.OK)
			{
				fileName = sfDialog.FileName;
				doc.saveFile(fileName);	
			}		
		}

		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			doc.exportToWeb();
		}

		private void btnTreeUp_Click(object sender, System.EventArgs e)
		{
			if (tree.SelectedNode == null) return;

			if (tree.SelectedNode.ImageIndex == 0)
			{
				getTableIndex();
				doc.moveTableUp(tableIndex);
			}
			else
			{
				getFieldIndex();
				doc.table[tableIndex].moveFieldUp(fieldIndex);
			}

			paintTreeFromDoc();
		}

		private void btnTreeDown_Click(object sender, System.EventArgs e)
		{
			if (tree.SelectedNode == null) return;

			if (tree.SelectedNode.ImageIndex == 0)
			{
				getTableIndex();
				doc.moveTableDown(tableIndex);
			}
			else
			{
				getFieldIndex();
				doc.table[tableIndex].moveFieldDown(fieldIndex);
			}

			paintTreeFromDoc();		
		}

		private void frmMain_Resize(object sender, System.EventArgs e)
		{
			int diff = 25;

			groupBasic.Width	= (this.Width / 2) - diff;
			txtProject.Width	= (this.Width - lblProject.Width - lblUsername.Width - lblProject.Left) / 3;
			txtPath.Width		= txtProject.Width;
			txtConn.Width		= txtPath.Width;
			txtEncoding.Left	= txtPath.Left;
			txtEncoding.Width	= txtConn.Width / 2;
			chkSqlUnicode.Left	= txtEncoding.Left + txtEncoding.Width + diff;

			groupUser.Left		= groupBasic.Left + groupBasic.Width + diff;
			groupUser.Width		= groupBasic.Width;
			lstUser.Width		= (groupUser.Width / 2) - diff;
			lstUserTab.Left		= lstUser.Left + lstUser.Width + diff;
			lstUserTab.Width	= lstUser.Width;
			txtUsername.Width	= groupBasic.Width - btnUserOk.Width - lblUsername.Width - diff * 2;
			txtPassword.Width	= txtUsername.Width;
			btnUserOk.Left		= txtUsername.Left + txtUsername.Width + diff;
			btnUserDelete.Left	= btnUserOk.Left;

			groupItem.Width		= this.Width - diff;;
			groupItem.Height	= this.Height - groupItem.Top - (diff * 2);
			btnTreeUp.Top		= groupItem.Height - btnTreeUp.Height - diff;
			btnTreeDown.Top		= btnTreeUp.Top;
			tree.Height			= btnTreeUp.Top - diff;
			tabMain.Width		= groupItem.Width - tree.Right - diff;
			tabMain.Top			= tree.Top;
			tabMain.Height		= tree.Height + btnTreeUp.Height;
			txtTableName.Width	= tabMain.Width - lblTableName.Right - diff;
			txtTableDisp.Width	= txtTableName.Width;
			txtFieldName.Width	= txtTableName.Width;
			txtFieldDisp.Width	= txtTableName.Width;
			cmbFieldType.Width	= txtTableName.Width;

			btnFieldDelete.Left	= cmbFieldType.Right - btnFieldDelete.Width;
			btnFieldOk.Left		= btnFieldDelete.Left - btnFieldOk.Width;
			btnTableDelete.Left	= btnFieldDelete.Left;
			btnTableOk.Left		= btnFieldOk.Left;

			txtForTable.Width	= (tabMain.Width - lblForTable.Right) / 5;
			lblForKey.Left		= txtForTable.Right + 1;
			txtForKey.Left		= lblForKey.Right + 1;
			txtForKey.Width		= txtForTable.Width;
			lblForName.Left		= txtForKey.Right + 1;
			txtForName.Left		= lblForName.Right + 1;
			txtForName.Width	= txtForTable.Width;
		}

		private void cmbFieldType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (cmbFieldType.Text == "FOREIGN_KEY")
			{
				txtForTable.Enabled = true;
				txtForKey.Enabled	= true;
				txtForName.Enabled	= true;
			}
			else
			{
				txtForTable.Enabled = false;
				txtForKey.Enabled	= false;
				txtForName.Enabled	= false;
			}
		}

		private void chkSqlUnicode_CheckedChanged(object sender, System.EventArgs e)
		{
			if (progChange) return;
			doc.sqlServerUnicodeSupport = chkSqlUnicode.Checked;
		}

		private void txtEncoding_TextChanged(object sender, System.EventArgs e)
		{
			if (progChange) return;
			doc.encoding = txtEncoding.Text;
		}

		private void lstUser_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (lstUser.SelectedIndex < 0) return;

			txtUsername.Text = doc.user[lstUser.SelectedIndex].username;
			txtPassword.Text = doc.user[lstUser.SelectedIndex].password;

			for (int n = 0; n < lstUserTab.Items.Count; n++)
			{
				lstUserTab.SetItemChecked(n, false);
				for (int m = 0; m < doc.user[lstUser.SelectedIndex].authTableCount; m++)
				{
					if (doc.user[lstUser.SelectedIndex].authTable[m] == lstUserTab.Items[n].ToString())
					{
						lstUserTab.SetItemChecked(n, true);
					}
				}
			}
		}

		private void btnUserOk_Click(object sender, System.EventArgs e)
		{
			if (lstUser.SelectedIndex >= 0)
			{
				doc.user[lstUser.SelectedIndex].clear();
				doc.user[lstUser.SelectedIndex].username = txtUsername.Text;
				doc.user[lstUser.SelectedIndex].password = txtPassword.Text;
				
				for (int n = 0; n < lstUserTab.Items.Count; n++)
				{
					if (lstUserTab.GetItemChecked(n))
					{
						doc.user[lstUser.SelectedIndex].addTable(lstUserTab.Items[n].ToString());
					}
				}
			}
			else
			{
				doc.addUser(txtUsername.Text, txtPassword.Text);
			}

			paintUsersFromDoc();
		}

		private void chkFieldName_CheckedChanged(object sender, System.EventArgs e)
		{
			radAsc.Enabled	= chkFieldName.Checked;
			radDesc.Enabled = chkFieldName.Checked;
		}





	}
}
