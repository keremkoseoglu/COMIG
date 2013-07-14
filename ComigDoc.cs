using System;
using System.Collections;
using System.Data;
using System.IO;

namespace Comig
{
	/// <summary>
	/// Summary description for ComigDoc.
	/// </summary>
	public class ComigDoc
	{
		public string		name;
		public string		outputPath;
		public string		connectionString;
		public string		encoding;
		public string		appVersion;
		public ComigTable[]	table;
		public ComigUser[]	user;
		public int			tableCount;
		public int			userCount;
		public bool			sqlServerUnicodeSupport;

		private DataSet		dsFile;

		public ComigDoc()
		{
			//
			// TODO: Add constructor logic here
			//
			clear();
		}

		public void clear()
		{
			name					= "";
			outputPath				= "";
			connectionString		= "";
			encoding				= "";
			table					= new ComigTable[255];
			user					= new ComigUser[255];
			tableCount				= 0;
			userCount				= 0;
			sqlServerUnicodeSupport = false;
		}

		public bool addTable(string Name)
		{
			if (Name.Trim().Length <= 0) return false;

			for (int n = 0; n < tableCount; n++)
			{
				if (table[n].name == Name) return false;
			}

			table[tableCount]		= new ComigTable();
			table[tableCount].name	= Name;
			tableCount++;

			return true;
		}

		public void removeTable(int Index)
		{
			if (Index < 0) return;

			for (int n = 0; n < userCount - 1; n++)
			{
				user[n].removeTable(table[Index].name);
			}

			for (int n = Index; n < tableCount - 1; n++)
			{
				table[n] = table[n + 1];
			}
			tableCount--;
		}

		public void moveTableUp(int Index)
		{
			if (Index <= 0) return;

			ComigTable t = table[Index - 1];
			table[Index - 1] = table[Index];
			table[Index] = t;
		}
	
		public void moveTableDown(int Index)
		{
			if (Index >= tableCount - 1) return;

			ComigTable t = table[Index + 1];
			table[Index + 1] = table[Index];
			table[Index] = t;
		}

		public bool addUser(string Username, string Password)
		{
			if (Username.Trim().Length <= 0) return false;

			for (int n = 0; n < userCount; n++)
			{
				if (user[n].username == Username) return false;
			}

			user[userCount]				= new ComigUser();
			user[userCount].username	= Username;
			user[userCount].password	= Password;
			userCount++;

			return true;
		}

		public void removeUser(int Index)
		{
			if (Index < 0) return;

			for (int n = Index; n < tableCount - 1; n++)
			{
				user[n] = user[n + 1];
			}
			userCount--;
		}

		public int getTableIndex(string Name)
		{
			int r = -1;

			for (int n = 0; n < tableCount; n++)
			{
				if (table[n].name == Name) r = n;
			}

			return r;
		}

		public void saveFile(string Path)
		{
			DataRow dr;
			resetDataset();

			dr = dsFile.Tables["head"].NewRow();
			dr["formatversion"] = "3";
			dsFile.Tables["head"].Rows.Add(dr);

			dr = dsFile.Tables["body"].NewRow();
			dr["name"]						= name;
			dr["outputpath"]				= outputPath;
			dr["connectionstring"]			= connectionString;
			dr["tablecount"]				= tableCount.ToString();
			dr["sqlserverunicodesupport"]	= sqlServerUnicodeSupport.ToString();
			dr["encoding"]					= encoding;
			dsFile.Tables["body"].Rows.Add(dr);

			for (int n = 0; n < userCount; n++)
			{
				dr = dsFile.Tables["user"].NewRow();
				dr["username"] = user[n].username;
				dr["password"] = user[n].password;
				dsFile.Tables["user"].Rows.Add(dr);

				for (int m = 0; m < user[n].authTableCount; m++)
				{
					dr = dsFile.Tables["usertab"].NewRow();
					dr["username"]	= user[n].username;
					dr["table"]		= user[n].authTable[m].ToString();
					dsFile.Tables["usertab"].Rows.Add(dr);
				}
		}

			for (int n = 0; n < tableCount; n++)
			{
				dr = dsFile.Tables["table"].NewRow();
				dr["tablename"]			= table[n].name;
				dr["tabledisplayname"]	= table[n].displayName;
				dr["caninsert"]			= table[n].canInsert;
				dr["canupdate"]			= table[n].canUpdate;
				dr["candelete"]			= table[n].canDelete;
				dr["fieldcount"]		= table[n].fieldCount;
				dr["ordertype"]			= table[n].getOrderType();
				dsFile.Tables["table"].Rows.Add(dr);

				for (int m = 0; m < table[n].fieldCount; m++)
				{
					dr = dsFile.Tables["field"].NewRow();
					dr["parenttable"]			= table[n].name;
					dr["fieldname"]				= table[n].field[m].name;
					dr["fielddisplayname"]		= table[n].field[m].displayName;
					dr["type"]					= table[n].field[m].type;
					dr["iskey"]					= table[n].field[m].isKey;
					dr["isname"]				= table[n].field[m].isName;
					if (table[n].field[m].type == ComigField.TYPE.FOREIGN_KEY)
					{
						dr["fktable"]			= table[n].field[m].fkTable;
						dr["fkid"]				= table[n].field[m].fkKeyField;
						dr["fkname"]			= table[n].field[m].fkNameField;
					}
					dsFile.Tables["field"].Rows.Add(dr);
				}
			}

			dsFile.WriteXml(Path);
		}

		public void loadFile(string Path)
		{
			DataRow dr;
			ComigField.TYPE type = ComigField.TYPE.TEXT;

			clear();
			resetDataset();
			dsFile.ReadXml(Path);
			dr					= dsFile.Tables["body"].Rows[0];

			// Basic stuff
			name				= dr["name"].ToString();
			outputPath			= dr["outputpath"].ToString();
			connectionString	= dr["connectionstring"].ToString();

			// V1.5:
			// + Users are a list now
			try
			{
				foreach(DataRow drUser in dsFile.Tables["user"].Rows)
				{
					addUser(drUser["username"].ToString(), drUser["password"].ToString());

					foreach(DataRow drUserTab in dsFile.Tables["usertab"].Rows)
					{
						if (drUserTab["username"].ToString() == drUser["username"].ToString())
						{
							user[userCount - 1].addTable(drUserTab["table"].ToString());
						}
					}
				}
			}
			catch(Exception ex) {}

			try
			{
				addUser(dr["username"].ToString(), dr["password"].ToString());
			}
			catch(Exception ex) {}

			// V1.3: 
			// + Sql Server Unicode Character Support
			// + Encoding
			try
			{
				switch (dr["sqlServerUnicodeSupport"].ToString())
				{
					case "True":
						sqlServerUnicodeSupport	= true;
						break;
					case "False":
						sqlServerUnicodeSupport	= false;
						break;
				}

				encoding = dr["encoding"].ToString();
			}
			catch (Exception ex) {}

			// Tables
			foreach(DataRow drTab in dsFile.Tables["table"].Rows)
			{
				addTable(drTab["tablename"].ToString());
				table[tableCount - 1].displayName = drTab["tabledisplayname"].ToString();
				if (drTab["caninsert"].ToString().ToUpper() == "TRUE") table[tableCount - 1].canInsert = true; else table[tableCount - 1].canInsert = false;
				if (drTab["canupdate"].ToString().ToUpper() == "TRUE") table[tableCount - 1].canUpdate = true; else table[tableCount - 1].canUpdate = false;
				if (drTab["candelete"].ToString().ToUpper() == "TRUE") table[tableCount - 1].canDelete = true; else table[tableCount - 1].canDelete = false;

				// V1.7:
				// + Ascending / Descending support
				try
				{
					if (drTab["ordertype"].ToString().ToUpper().Substring(0, 3) == "DES") table[tableCount - 1].orderType = ComigTable.ORDERTYPE.DESCENDING; else table[tableCount - 1].orderType = ComigTable.ORDERTYPE.ASCENDING;
				}
				catch
				{
					table[tableCount - 1].orderType = ComigTable.ORDERTYPE.ASCENDING;
				}

				foreach(DataRow drFie in dsFile.Tables["field"].Rows)
				{
					if (drFie["parenttable"].ToString() == drTab["tablename"].ToString())
					{
						switch (drFie["type"].ToString().ToUpper())
						{
							case "DATE":
								type = ComigField.TYPE.DATE;
								break;
							case "FOREIGN_KEY":
								type = ComigField.TYPE.FOREIGN_KEY;
								break;
							case "MEMO":
								type = ComigField.TYPE.MEMO;
								break;
							case "NUMBER":
								type = ComigField.TYPE.NUMBER;
								break;
							case "TEXT":
								type = ComigField.TYPE.TEXT;
								break;
						}
						table[tableCount - 1].addField(drFie["fieldname"].ToString(), type);
						table[tableCount - 1].field[table[tableCount - 1].fieldCount - 1].displayName = drFie["fielddisplayname"].ToString();

						if (drFie["isKey"].ToString().ToUpper() == "TRUE") table[tableCount - 1].field[table[tableCount - 1].fieldCount - 1].isKey = true; else table[tableCount - 1].field[table[tableCount - 1].fieldCount - 1].isKey = false;
						if (drFie["isName"].ToString().ToUpper() == "TRUE") table[tableCount - 1].field[table[tableCount - 1].fieldCount - 1].isName = true; else table[tableCount - 1].field[table[tableCount - 1].fieldCount - 1].isName = false;

						if (type == ComigField.TYPE.FOREIGN_KEY)
						{
							table[tableCount - 1].field[table[tableCount - 1].fieldCount - 1].fkTable		= drFie["fktable"].ToString();
							table[tableCount - 1].field[table[tableCount - 1].fieldCount - 1].fkKeyField	= drFie["fkid"].ToString();
							table[tableCount - 1].field[table[tableCount - 1].fieldCount - 1].fkNameField	= drFie["fkname"].ToString();
						}
					}
				}
			}

		}

		private void resetDataset()
		{
			dsFile = new DataSet();

			dsFile.Tables.Add("head");
			dsFile.Tables["head"].Columns.Add("formatversion");

			dsFile.Tables.Add("body");
			dsFile.Tables["body"].Columns.Add("name");
			dsFile.Tables["body"].Columns.Add("outputpath");
			dsFile.Tables["body"].Columns.Add("connectionstring");
			dsFile.Tables["body"].Columns.Add("username");
			dsFile.Tables["body"].Columns.Add("password");
			dsFile.Tables["body"].Columns.Add("tablecount");
			dsFile.Tables["body"].Columns.Add("sqlserverunicodesupport");
			dsFile.Tables["body"].Columns.Add("encoding");

			dsFile.Tables.Add("user");
			dsFile.Tables["user"].Columns.Add("username");
			dsFile.Tables["user"].Columns.Add("password");

			dsFile.Tables.Add("usertab");
			dsFile.Tables["usertab"].Columns.Add("username");
			dsFile.Tables["usertab"].Columns.Add("table");

			dsFile.Tables.Add("table");
			dsFile.Tables["table"].Columns.Add("tablename");
			dsFile.Tables["table"].Columns.Add("tabledisplayname");
			dsFile.Tables["table"].Columns.Add("caninsert");
			dsFile.Tables["table"].Columns.Add("canupdate");
			dsFile.Tables["table"].Columns.Add("candelete");
			dsFile.Tables["table"].Columns.Add("fieldcount");
			dsFile.Tables["table"].Columns.Add("ordertype");

			dsFile.Tables.Add("field");
			dsFile.Tables["field"].Columns.Add("parenttable");
			dsFile.Tables["field"].Columns.Add("fieldname");
			dsFile.Tables["field"].Columns.Add("fielddisplayname");
			dsFile.Tables["field"].Columns.Add("fktable");
			dsFile.Tables["field"].Columns.Add("fkid");
			dsFile.Tables["field"].Columns.Add("fkname");
			dsFile.Tables["field"].Columns.Add("type");
			dsFile.Tables["field"].Columns.Add("iskey");
			dsFile.Tables["field"].Columns.Add("isname");
		}

		public void exportToWeb()
		{
			TextWriter tw;
			string s;

			// CSS File
			tw = new StreamWriter(outputPath + "\\comig.css", false);
			tw.WriteLine("A:HOVER");
			tw.WriteLine("{");
			tw.WriteLine("color: #ffffff;");
			tw.WriteLine("}");
			tw.WriteLine("A:LINK");
			tw.WriteLine("{");
			tw.WriteLine("color: #ffffff;");
			tw.WriteLine("}");
			tw.WriteLine("A:VISITED");
			tw.WriteLine("{");
			tw.WriteLine("color: #ffffff;");
			tw.WriteLine("}");
			tw.WriteLine("BODY");
			tw.WriteLine("{");
			tw.WriteLine("margin-bottom: 0;");
			tw.WriteLine("margin-left: 0;");
			tw.WriteLine("margin-right: 0;");
			tw.WriteLine("margin-top: 0;");
			tw.WriteLine("}");
			tw.WriteLine("H1");
			tw.WriteLine("{");
			tw.WriteLine("font-family: Arial;");
			tw.WriteLine("font-size: 16pt;");
			tw.WriteLine("font-weight: bold;");
			tw.WriteLine("margin-bottom: 2;");
			tw.WriteLine("}");
			tw.WriteLine("H2");
			tw.WriteLine("{");
			tw.WriteLine("font-family: Arial;");
			tw.WriteLine("font-size: 12pt;");
			tw.WriteLine("font-weight: bold;");
			tw.WriteLine("margin-bottom: 2;");
			tw.WriteLine("}");
			tw.WriteLine("TD");
			tw.WriteLine("{");
			tw.WriteLine("font-family: Arial;");
			tw.WriteLine("font-size: 8pt;");
			tw.WriteLine("}");
			tw.WriteLine(".menuCell");
			tw.WriteLine("{");
			tw.WriteLine("background-color: #5555ff;");
			tw.WriteLine("padding-left: 7;");
			tw.WriteLine("padding-right: 7;");	
			tw.WriteLine("}");
			tw.WriteLine(".titleCell");
			tw.WriteLine("{");
			tw.WriteLine("background-color: #ffcc00;");
			tw.WriteLine("font-weight: bold;");
			tw.WriteLine("}");
			tw.WriteLine(".areaCell");
			tw.WriteLine("{");
			tw.WriteLine("background-color: #999999;");
			tw.WriteLine("font-weight: bold;");
			tw.WriteLine("padding-top: 7;");
			tw.WriteLine("}");
			tw.Close();

			// Default
			tw = new StreamWriter(outputPath + "\\default.asp", false);
			tw.WriteLine("<html>");
			tw.WriteLine("<head>");
			tw.WriteLine("<title>" + name + " Content Management</title>");
			tw.WriteLine("<meta name=GENERATOR Content=COMIG>");
			tw.WriteLine("<meta http-equiv=Content-Type content=text/html; charset=" + encoding + ">");
			tw.WriteLine("<link rel=stylesheet type=text/css href=comig.css>");
			tw.WriteLine("</head>");
			tw.WriteLine("<body>");
			tw.WriteLine("<table border=0 width=100% height=100%><tr><td align=center valign=middle class=menuCell>");
			tw.WriteLine("<h1>" + name + " Content Management System</h1>");
			tw.WriteLine("generated with <a href=http://www.doublekey.org/portprog.aspx?id=20 target=_blank>COMIG " + appVersion + "</a>");
			tw.WriteLine("<form name=frmLogin action=login.asp method=post>");
			tw.WriteLine("<table border=0>");
			tw.WriteLine("<tr>");
			tw.WriteLine("<td align=left valign=top class=titleCell>Username</td>");
			tw.WriteLine("<td align=left valign=top><input name=username type=text></td>");
			tw.WriteLine("</tr>");
			tw.WriteLine("<tr>");
			tw.WriteLine("<td align=left valign=top class=titleCell>Password</td>");
			tw.WriteLine("<td align=left valign=top><input name=password type=password></td>");
			tw.WriteLine("</tr>");
			tw.WriteLine("<tr>");
			tw.WriteLine("<td></td>");
			tw.WriteLine("<td align=right valign=bottom><input type=submit value=Login></td>");
			tw.WriteLine("</tr>");
			tw.WriteLine("</table>");
			tw.WriteLine("</form>");
			tw.WriteLine("</td></tr></table>");
			tw.WriteLine("</body>");
			tw.WriteLine("</html>");
			tw.Close();

			// Login
			tw = new StreamWriter(outputPath + "\\login.asp", false);
			tw.WriteLine("<%");
			
			tw.WriteLine("Select Case Request.QueryString(\"action\"):");
			tw.WriteLine("Case \"bye\":");
			tw.WriteLine("Session(\"LOGGED_IN\") = \"\"");
			tw.WriteLine("Session(\"LOGGED_USER\") = \"NOBODY\"");
			tw.WriteLine("Session.Abandon");
			tw.WriteLine("Response.Write(\"Bye Bye! =)\")");
			tw.WriteLine("Case \"\":");
			s = "If";
			for (int n = 0; n < userCount; n++)
			{
				if (n > 0) s += " OR";
				s += " (";
				s += " Request.Form(\"username\") = \"" + user[n].username + "\" and Request.Form(\"password\") = \"" + user[n].password + "\"";
				s += ")";
			}
			s += " Then";
			tw.WriteLine(s);
			
			tw.WriteLine("Session(\"LOGGED_IN\") = \"X\"");
			tw.WriteLine("Session(\"LOGGED_USER\") = Request.Form(\"username\")");
			tw.WriteLine("Response.Redirect(\"menu.asp\")");
			tw.WriteLine("Else");
			tw.WriteLine("Session(\"LOGGED_IN\") = \"\"");
			tw.WriteLine("Response.Redirect(\"default.asp\")");
			tw.WriteLine("End If");
			tw.WriteLine("End Select");
			tw.WriteLine("%>");
			tw.Close();

			////////
			// Menu
			////////
			///
			tw = new StreamWriter(outputPath + "\\menu.asp", false);
			// Basic Stuff
			tw.WriteLine("<%@LANGUAGE=\"VBSCRIPT\" CODEPAGE=\"1254\"%>");
			tw.WriteLine("<%");
			tw.WriteLine("Dim conn");
			tw.WriteLine("Dim reco");
			tw.WriteLine("Dim recf");
			tw.WriteLine("Dim comm");
			tw.WriteLine("Const connString = \"" + connectionString + "\"");
			tw.WriteLine("set conn = Server.CreateObject(\"ADODB.Connection\")");
			tw.WriteLine("set reco = Server.CreateObject(\"ADODB.Recordset\")");
			tw.WriteLine("set recf = Server.CreateObject(\"ADODB.Recordset\")");
			tw.WriteLine("If Session(\"LOGGED_IN\") <> \"X\" Then Response.Redirect(\"default.asp\")");
			tw.WriteLine("%>");
			tw.WriteLine("<script language=\"javascript\">");
			tw.WriteLine("");
			tw.WriteLine("function add2form(objTarget, tag)");
			tw.WriteLine("{");
			tw.WriteLine("box = eval(\"document.frmMenu.\" + objTarget);");
			tw.WriteLine("selText = document.selection.createRange().text;");
			tw.WriteLine("");
			tw.WriteLine(" ");
			tw.WriteLine("if (tag == \"br\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<br><br>\\n\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<br><br>\\n\" + selText; ");
			tw.WriteLine("}");
			tw.WriteLine(" ");
			tw.WriteLine("if (tag == \"hr\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<hr><br>\\n\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<hr><br>\\n\" + selText;");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"li\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<li></li><br>\\n\\n\";");
			tw.WriteLine("if (selText != \"\")");
			tw.WriteLine("{");
			tw.WriteLine(" var szEnter = String.fromCharCode(13);");
			tw.WriteLine(" if (selText.indexOf(szEnter) <= 0) ");
			tw.WriteLine("{");
			tw.WriteLine("document.selection.createRange().text = \"<li>\" + selText + \"</li><br>\\n\\n\";");
			tw.WriteLine("}");
			tw.WriteLine("if (selText.indexOf(szEnter) >0)");
			tw.WriteLine("{");
			tw.WriteLine("nCount = 0;");
			tw.WriteLine("newText = \"<li>\";");
			tw.WriteLine("while (nCount <= selText.length)");
			tw.WriteLine("{");
			tw.WriteLine(" var currentLetter = selText.substr(nCount, 1);");
			tw.WriteLine(" if (currentLetter != szEnter) newText = newText + currentLetter;");
			tw.WriteLine(" if (currentLetter == szEnter) newText = newText + \"</li><br>\\n<li>\";");
			tw.WriteLine(" nCount++;");
			tw.WriteLine("}");
			tw.WriteLine("newText = newText + \"</li><br><br>\\n\";");
			tw.WriteLine("document.selection.createRange().text = newText;");
			tw.WriteLine("}");
			tw.WriteLine("}");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"ol\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText != \"\")");
			tw.WriteLine("{");
			tw.WriteLine(" var szEnter = String.fromCharCode(13);");
			tw.WriteLine("if (selText.indexOf(szEnter) >0)");
			tw.WriteLine("{");
			tw.WriteLine("nCount = 0;");
			tw.WriteLine("newText = \"<ol><li>\";");
			tw.WriteLine("while (nCount <= selText.length)");
			tw.WriteLine("{");
			tw.WriteLine(" var currentLetter = selText.substr(nCount, 1);");
			tw.WriteLine(" if (currentLetter != szEnter) newText = newText + currentLetter;");
			tw.WriteLine(" if (currentLetter == szEnter) newText = newText + \"</li><br>\\n<li>\";");
			tw.WriteLine(" nCount++;");
			tw.WriteLine("}");
			tw.WriteLine("newText = newText + \"</li></ol><br><br>\\n\";");
			tw.WriteLine("document.selection.createRange().text = newText;");
			tw.WriteLine("}");
			tw.WriteLine("}");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"b\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<b></b>\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<b>\" + selText + \"</b>\";");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"i\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<i></i>\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<i>\" + selText + \"</i>\";");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"big\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<big></big>\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<big>\" + selText + \"</big>\";");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"small\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<small></small>\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<small>\" + selText + \"</small>\";");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"a\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<a href='ENTER_LINK_HERE'></a>\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<a href='LINKI_YAZIN'>\" + selText + \"</a>\";");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"ablank\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<a href='ENTER_LINK_HERE' target='_blank'></a>\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<a href='ENTER_LINK_HERE' target='_blank'>\" + selText + \"</a>\";");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"img\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"\\n\\n<br><br><img src='ENTER_IMAGE_HERE'><br><br>\\n\\n\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"\\n\\n<br><br><img src='ENTER_IMAGE_HERE'><br><br>\\n\\n\" + selText;");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"imgleft\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<img src='ENTER_IMAGE_HERE' align='left'>\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<img src='ENTER_IMAGE_HERE' align='left'>\" + selText;");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("if (tag == \"imgright\")");
			tw.WriteLine("{");
			tw.WriteLine("if (selText == \"\") box.value = box.value + \"<img src='ENTER_IMAGE_HERE' align='right'>\";");
			tw.WriteLine("if (selText != \"\") document.selection.createRange().text = \"<img src='ENTER_IMAGE_HERE' align='right'>\" + selText;");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("");
			tw.WriteLine("function displayHTML(objSource) ");
			tw.WriteLine("{");
			tw.WriteLine(" box = eval(\"document.frmMenu.\" + objSource);");
			tw.WriteLine(" win = window.open(\", \", 'popup', 'toolbar = no, status = no, scrollbars = yes');");
			tw.WriteLine(" win.document.write(\"<html><head><title>Test</title><meta http-equiv='Content-Type' content='text/html; charset=" + encoding + "'></head>\");");
			tw.WriteLine(" win.document.write(\"<body bgcolor='#ffffff' text='#000000' link='#0000ff' alink='#0000ff' vlink='#0000ff'><font face='Verdana' size='2'>\");");
			tw.WriteLine(" win.document.write(\"\" + box.value + \"\");");
			tw.WriteLine(" win.document.write(\"</font></body></html>\");");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("var secs");
			tw.WriteLine("var timerID = null");
			tw.WriteLine("var timerRunning = false");
			tw.WriteLine("var delay = 1000");
			tw.WriteLine("");
			tw.WriteLine("function InitializeTimer()");
			tw.WriteLine("{");
			tw.WriteLine("	secs = 300;");
			tw.WriteLine("	StopTheClock();");
			tw.WriteLine("	StartTheTimer();");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("function StopTheClock()");
			tw.WriteLine("{");
			tw.WriteLine("	if(timerRunning) clearTimeout(timerID);");
			tw.WriteLine("	timerRunning = false;");
			tw.WriteLine("}");
			tw.WriteLine("");
			tw.WriteLine("function StartTheTimer()");
			tw.WriteLine("{");
			tw.WriteLine("	if (secs==0)");
			tw.WriteLine("	{");
			tw.WriteLine("		StopTheClock();");
			tw.WriteLine("		alert(\"You have been idle for too long. Save your data now or it might be lost.\");");
			tw.WriteLine("	}");
			tw.WriteLine("	else");
			tw.WriteLine("	{");
			tw.WriteLine("		secs = secs - 1;");
			tw.WriteLine("		timerRunning = true;");
			tw.WriteLine("		timerID = self.setTimeout(\"StartTheTimer()\", delay);");
			tw.WriteLine("	}");
			tw.WriteLine("}");
			tw.WriteLine("</script>");
			tw.WriteLine("<html>");
			tw.WriteLine("<head>");
			tw.WriteLine("<title>" + name + " Content Management</title>");
			tw.WriteLine("<meta name=\"GENERATOR\" Content=\"COMIG\">");
			tw.WriteLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=" + encoding + "\">");
			tw.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"comig.css\">");
			tw.WriteLine("</head>");
			tw.WriteLine("<body onLoad=\"InitializeTimer();\">");
			tw.WriteLine("<table border=0 width=100% height=100% cellspacing=0 cellpadding=0>");
			tw.WriteLine("<tr><td align=center valign=middle class=titleCell height=30>");
			tw.WriteLine("<h1>" + name + " Content Management System</h1>");
			tw.WriteLine("generated with <a href=http://www.doublekey.org/portprog.aspx?id=20 target=_blank>COMIG " + appVersion + "</a>");
			tw.WriteLine("</td></tr>");
			tw.WriteLine("<tr><td align=left valign=top>");
			tw.WriteLine("<table border=0 width=100% height=100% cellspacing=0 cellpadding=0>");
			tw.WriteLine("<tr>");
			tw.WriteLine("<td align=left valign=top width=100 class=menuCell>");
			tw.WriteLine("<table border=0>");
			for (int n = 0; n < tableCount; n++)
			{
				s = "<% If 1 = 0";
				for (int m = 0; m < userCount; m++)
				{
					for (int x = 0; x < user[m].authTableCount; x++)
					{
						if (user[m].authTable[x] == table[n].name)
						{
							s += " OR";
							s += " Session(\"LOGGED_USER\") = \"" + user[m].username + "\"";
						}
					}
				}
				s += " Then %>";
				tw.WriteLine(s);

				tw.WriteLine("<tr>");
				tw.WriteLine("<td align=left valign=top><b>" + table[n].displayName + "</b></td>");
				if (table[n].canInsert) tw.WriteLine("<td align=left valign=top>&nbsp;&nbsp;<a href=\"menu.asp?table=" + table[n].name + "&action=insert\">Insert</a></td>");
				if (table[n].canUpdate) tw.WriteLine("<td align=left valign=top>&nbsp;&nbsp;<a href=\"menu.asp?table=" + table[n].name + "&action=update\">Update</a></td>");
				if (table[n].canDelete) tw.WriteLine("<td align=left valign=top>&nbsp;&nbsp;<a href=\"menu.asp?table=" + table[n].name + "&action=delete\">Delete</a></td>");
				tw.WriteLine("</tr>");
				tw.WriteLine("<% End If %>");
			}
			tw.WriteLine("</table>");
			tw.WriteLine("<br><br><a href=login.asp?action=bye><b>EXIT SYSTEM</b></a>");
			tw.WriteLine("</td>");
			tw.WriteLine("<td align=center valign=top class=areaCell>");
			tw.WriteLine("<%");
			tw.WriteLine("Select Case Request.QueryString(\"action\")");
			// INSERT
			tw.WriteLine("case \"insert\":");
			tw.WriteLine("Response.Write(\"<h2>INSERT:</h2>\")");
			tw.WriteLine("Select Case Request.QueryString(\"table\")");
			for (int t = 0; t < tableCount; t++)
			{
				tw.WriteLine("case \"" + table[t].name + "\":");
				tw.WriteLine("%>");
				tw.WriteLine("<form name=frmMenu action=menu.asp method=post>");
				tw.WriteLine("<input type=hidden name=action value=insert2>");
				tw.WriteLine("<input type=hidden name=table value=" + table[t].name + ">");
				tw.WriteLine("<table border=0>");
				for (int f = 0; f < table[t].fieldCount; f++)
				{
					if (!table[t].field[f].isKey)
					{
						tw.WriteLine("<tr>");
						tw.WriteLine("<td align=left valign=top class=titleCell><b>" + table[t].field[f].displayName + "</b></td>");
						tw.WriteLine("<td align=left valign=top>");
						switch(table[t].field[f].type)
						{
							case ComigField.TYPE.DATE:
								tw.WriteLine("<input name=" + table[t].field[f].name + " type=text>");
								break;
							case ComigField.TYPE.FOREIGN_KEY:
								tw.WriteLine("<select name=" + table[t].field[f].name + ">");								
								tw.WriteLine("<%");
								tw.WriteLine("conn.Open connString");
								tw.WriteLine("comm = \"SELECT * FROM " + table[t].field[f].fkTable + " ORDER BY " + table[t].field[f].fkNameField + "\"");
								tw.WriteLine("recf.Open comm, conn");
								tw.WriteLine("while NOT recf.EOF");
								tw.WriteLine("%>");
								tw.WriteLine("<option value='<%=FormatText(recf(\"" + table[t].field[f].fkKeyField + "\"))%>'>");
								tw.WriteLine("<%=recf(\"" + table[t].field[f].fkNameField + "\") & \" (\" & recf(\"" + table[t].field[f].fkKeyField + "\") & \")\"%>");
								tw.WriteLine("</option>");
								tw.WriteLine("<%");
								tw.WriteLine("recf.MoveNext");
								tw.WriteLine("wend");
								tw.WriteLine("recf.Close");
								tw.WriteLine("conn.Close");
								tw.WriteLine("%>");
								tw.WriteLine("</select>");
								break;
							case ComigField.TYPE.MEMO:
								paintTextarea(ref tw, table[t].field[f].name, "");
								break;
							case ComigField.TYPE.NUMBER:
								tw.WriteLine("<input name=" + table[t].field[f].name + " type=text>");
								break;
							case ComigField.TYPE.TEXT:
								tw.WriteLine("<input name=" + table[t].field[f].name + " type=text>");
								break;
						}
						tw.WriteLine("</td>");
						tw.WriteLine("</tr>");
					}
				}
				tw.WriteLine("<tr>");
				tw.WriteLine("<td></td>");
				tw.WriteLine("<td align=right valign=bottom><input type=Submit value=Save></td>");
				tw.WriteLine("</tr>");
				tw.WriteLine("</table>");
				tw.WriteLine("</form>");
				tw.WriteLine("<%");
			}			
			tw.WriteLine("End Select");
			// UPDATE
			tw.WriteLine("case \"update\":");
			tw.WriteLine("Response.Write(\"<h2>UPDATE:</h2>\")");
			tw.WriteLine("Select Case Request.QueryString(\"table\")");
			for (int t = 0; t < tableCount; t++)
			{
				tw.WriteLine("case \"" + table[t].name + "\":");
				tw.WriteLine("conn.Open connString");
				tw.WriteLine("comm = \"SELECT * FROM " + table[t].name + " ORDER BY " + table[t].getNameField() + " " + table[t].getOrderType() + "\"");
				tw.WriteLine("reco.Open comm, conn");
				tw.WriteLine("While Not reco.EOF");
				tw.WriteLine("Response.Write(\"<a href=menu.asp?table=" + table[t].name + "&action=update2&" + table[t].getKeyField() + "=\" & reco(\"" + table[t].getKeyField() + "\") & \">\" & reco(\"" + table[t].getNameField() + "\") & \"</a><br>\")");
				tw.WriteLine("reco.MoveNext");
				tw.WriteLine("Wend");
				tw.WriteLine("reco.Close");
				tw.WriteLine("conn.Close");
			}
			tw.WriteLine("End Select");
			// UPDATE2
			tw.WriteLine("case \"update2\":");
			tw.WriteLine("Response.Write(\"<h2>UPDATE:</h2>\")");
			tw.WriteLine("Select Case Request.QueryString(\"table\")");
			for (int t = 0; t < tableCount; t++)
			{
				tw.WriteLine("case \"" + table[t].name + "\":");
				tw.WriteLine("conn.Open connString");
				tw.WriteLine("comm = \"SELECT * FROM " + table[t].name + " WHERE " + table[t].getKeyField() + " = \" & Request.QueryString(\"" + table[t].getKeyField() + "\")");
				tw.WriteLine("reco.Open comm, conn");
				tw.WriteLine("if NOT reco.EOF Then");
				tw.WriteLine("%>");
				tw.WriteLine("<form name=frmMenu action=menu.asp method=post>");
				tw.WriteLine("<input type=hidden name=action value=update3>");
				tw.WriteLine("<input type=hidden name=" + table[t].getKeyField() + " value='<%=Request.QueryString(\"" + table[t].getKeyField() + "\")%>'>");
				tw.WriteLine("<input type=hidden name=table value=" + table[t].name + ">");
				tw.WriteLine("<table border=0>");

				for (int f = 0; f < table[t].fieldCount; f++)
				{
					if (!table[t].field[f].isKey)
					{
						tw.WriteLine("<tr>");
						tw.WriteLine("<td align=left valign=top class=titleCell><b>" + table[t].field[f].displayName + "</b></td>");
						tw.WriteLine("<td align=left valign=top>");
						switch(table[t].field[f].type)
						{
							case ComigField.TYPE.DATE:
								tw.WriteLine("<input name=" + table[t].field[f].name + " type=text value='<%=FormatText(reco(\"" + table[t].field[f].name + "\"))%>'>");
								break;
							case ComigField.TYPE.FOREIGN_KEY:
								tw.WriteLine("<select name=" + table[t].field[f].name + ">");								
								tw.WriteLine("<%");
								tw.WriteLine("comm = \"SELECT * FROM " + table[t].field[f].fkTable + " ORDER BY " + table[t].field[f].fkNameField + "\"");
								tw.WriteLine("recf.Open comm, conn");
								tw.WriteLine("while NOT recf.EOF");
								tw.WriteLine("%>");
								tw.WriteLine("<option value='<%=recf(\"" + table[t].field[f].fkKeyField + "\")%>'");
								tw.WriteLine("<% if recf(\"" + table[t].field[f].fkKeyField + "\") = reco(\"" + table[t].field[f].name + "\") Then Response.Write(\" SELECTED\")%>");
								tw.WriteLine(">");
								tw.WriteLine("<%=recf(\"" + table[t].field[f].fkNameField + "\") & \" (\" & recf(\"" + table[t].field[f].fkKeyField + "\") & \")\"%>");
								tw.WriteLine("</option>");
								tw.WriteLine("<%");
								tw.WriteLine("recf.MoveNext");
								tw.WriteLine("wend");
								tw.WriteLine("recf.Close");
								tw.WriteLine("%>");
								tw.WriteLine("</select>");
								break;
							case ComigField.TYPE.MEMO:
								paintTextarea(ref tw, table[t].field[f].name, "<%=reco(\"" + table[t].field[f].name + "\")%>");
								break;
							case ComigField.TYPE.NUMBER:
								tw.WriteLine("<input name=" + table[t].field[f].name + " type=text value=<%=reco(\"" + table[t].field[f].name + "\")%>>");
								break;
							case ComigField.TYPE.TEXT:
								tw.WriteLine("<input name=" + table[t].field[f].name + " type=text value='<%=FormatText(reco(\"" + table[t].field[f].name + "\"))%>'>");
								break;
						}
						tw.WriteLine("</td>");
						tw.WriteLine("</tr>");
					}
				}
				tw.WriteLine("<tr>");
				tw.WriteLine("<td></td>");
				tw.WriteLine("<td align=right valign=bottom><input type=Submit value=Save></td>");
				tw.WriteLine("</tr>");
				tw.WriteLine("</table>");
				tw.WriteLine("</form>");
				tw.WriteLine("<%");
				tw.WriteLine("End If");
				tw.WriteLine("reco.Close");
				tw.WriteLine("conn.Close");
			}
			tw.WriteLine("End Select");
			// DELETE
			tw.WriteLine("case \"delete\":");
			tw.WriteLine("Response.Write(\"<h2>DELETE:</h2>\")");
			tw.WriteLine("Select Case Request.QueryString(\"table\")");
			for (int t = 0; t < tableCount; t++)
			{
				tw.WriteLine("case \"" + table[t].name + "\":");
				tw.WriteLine("conn.Open connString");
				tw.WriteLine("comm = \"SELECT * FROM " + table[t].name + " ORDER BY " + table[t].getNameField() + " " + table[t].getOrderType() + "\"");
				tw.WriteLine("reco.Open comm, conn");
				tw.WriteLine("While Not reco.EOF");
				tw.WriteLine("Response.Write(\"<a href=menu.asp?table=" + table[t].name + "&action=delete2&" + table[t].getKeyField() + "=\" & reco(\"" + table[t].getKeyField() + "\") & \">\" & reco(\"" + table[t].getNameField() + "\") & \"</a><br>\")");
				tw.WriteLine("reco.MoveNext");
				tw.WriteLine("Wend");
				tw.WriteLine("reco.Close");
				tw.WriteLine("conn.Close");
			}
			tw.WriteLine("End Select");
			// DELETE2
			tw.WriteLine("case \"delete2\":");
			tw.WriteLine("Response.Write(\"<h2>DELETE:</h2>\")");
			tw.WriteLine("Select Case Request.QueryString(\"table\")");
			for (int t = 0; t < tableCount; t++)
			{
				tw.WriteLine("case \"" + table[t].name + "\":");
				tw.WriteLine("conn.Open connString");
				tw.WriteLine("comm = \"DELETE FROM " + table[t].name + " WHERE " + table[t].getKeyField() + " = \" & Request.QueryString(\"" + table[t].getKeyField() + "\")");
				tw.WriteLine("conn.Execute comm");
				tw.WriteLine("conn.Close");
				tw.WriteLine("Response.Write(\"OK\")");
			}
			tw.WriteLine("End Select");

			tw.WriteLine("End Select");
			tw.WriteLine("Select Case Request.Form(\"action\")");
			// INSERT2
			tw.WriteLine("case \"insert2\":");
			tw.WriteLine("Response.Write(\"<h2>INSERT:</h2>\")");
			tw.WriteLine("conn.Open connString");
			tw.WriteLine("Select Case Request.Form(\"table\")");
			for (int t = 0; t < tableCount; t++)
			{
				tw.WriteLine("case \"" + table[t].name + "\":");
					
				s = "comm = \"INSERT INTO " + table[t].name;
				s += "(";
				bool b = false;
				for (int f = 0; f < table[t].fieldCount; f++)
				{
					if (!table[t].field[f].isKey)
					{
						if (b) s += ", ";
						s += table[t].field[f].name;
						b = true;
					}
				}				
				s += ") VALUES (";
				b = false;
				for (int f = 0; f < table[t].fieldCount; f++)
				{
					if (!table[t].field[f].isKey)
					{
						if (b) s += ", ";
						switch(table[t].field[f].type)
						{
							case ComigField.TYPE.DATE:
								s += "'\" & Request.Form(\"" + table[t].field[f].name + "\") & \"'";
								break;
							case ComigField.TYPE.FOREIGN_KEY:
								s += "\" & Request.Form(\"" + table[t].field[f].name + "\") & \"";
								break;
							case ComigField.TYPE.MEMO:
								if (sqlServerUnicodeSupport) s += "N";
								s += "'\" & FormatText(Request.Form(\"" + table[t].field[f].name + "\")) & \"'";
								break;
							case ComigField.TYPE.NUMBER:
								s += "\" & Request.Form(\"" + table[t].field[f].name + "\") & \"";
								break;
							case ComigField.TYPE.TEXT:
								if (sqlServerUnicodeSupport) s += "N";
								s += "'\" & FormatText(Request.Form(\"" + table[t].field[f].name + "\")) & \"'";	
								break;
						}
						b = true;
					}
				}
				s += ")\"";
				
				tw.WriteLine(s);
			}
			tw.WriteLine("End Select");
			tw.WriteLine("conn.Execute comm");
			tw.WriteLine("conn.Close");
			tw.WriteLine("Response.Write(\"OK\")");
			// UPDATE3
			tw.WriteLine("case \"update3\":");
			tw.WriteLine("Response.Write(\"<h2>UPDATE:</h2>\")");
			tw.WriteLine("conn.Open connString");
			tw.WriteLine("Select Case Request.Form(\"table\")");
			for (int t = 0; t < tableCount; t++)
			{
				tw.WriteLine("case \"" + table[t].name + "\":");
					
				s = "comm = \"UPDATE " + table[t].name;
				s += " SET ";
				bool b = false;
				for (int f = 0; f < table[t].fieldCount; f++)
				{
					if (!table[t].field[f].isKey)
					{
						if (b) s += ", ";
						s += table[t].field[f].name + "=";
						switch(table[t].field[f].type)
						{
							case ComigField.TYPE.DATE:
								s += "'\" & Request.Form(\"" + table[t].field[f].name + "\") & \"'";
								break;
							case ComigField.TYPE.FOREIGN_KEY:
								s += "\" & Request.Form(\"" + table[t].field[f].name + "\") & \"";
								break;
							case ComigField.TYPE.MEMO:
								if (sqlServerUnicodeSupport) s += "N";
								s += "'\" & FormatText(Request.Form(\"" + table[t].field[f].name + "\")) & \"'";
								break;
							case ComigField.TYPE.NUMBER:
								s += "\" & Request.Form(\"" + table[t].field[f].name + "\") & \"";
								break;
							case ComigField.TYPE.TEXT:
								if (sqlServerUnicodeSupport) s += "N";
								s += "'\" & FormatText(Request.Form(\"" + table[t].field[f].name + "\")) & \"'";	
								break;
						}
						b = true;
					}
				}
				s += " WHERE " + table[t].getKeyField() + " = \" & Request.Form(\"" + table[t].getKeyField() + "\")";
				
				tw.WriteLine(s);
			}
			tw.WriteLine("End Select");
			tw.WriteLine("conn.Execute comm");
			tw.WriteLine("conn.Close");
			tw.WriteLine("Response.Write(\"OK\")");
			tw.WriteLine("End Select");
			tw.WriteLine("%>");
			tw.WriteLine("</td>");
			tw.WriteLine("</tr>");
			tw.WriteLine("</table>");
			tw.WriteLine("</td></tr>");
			tw.WriteLine("</body>");
			tw.WriteLine("</html>");
			tw.WriteLine("<%");
			tw.WriteLine("Function FormatText(x)");
			tw.WriteLine("  if Len(x) > 0 then");
			tw.WriteLine("	  x = Replace(x, \"'\", \"`\")");
			tw.WriteLine("	  x = Replace(x, \"\"\"\", \"`\")");
			tw.WriteLine("  end if");
			tw.WriteLine("	FormatText = x");
			tw.WriteLine("End Function");
			tw.WriteLine("%>");
			tw.Close();
		}

		private void paintTextarea(ref TextWriter tw, string name, string val)
		{
			tw.WriteLine("<table border=0>");
			tw.WriteLine("<tr><td align=left valign=top>");
			tw.WriteLine("<textarea rows=10 cols=40 name=" + name + ">" + val + "</textarea>");
			tw.WriteLine("</td></tr><tr><td align=left valign=top>");
			tw.WriteLine("<table border=0>");
			tw.WriteLine("<tr><td align=left valign=middle bgcolor=#cccccc><b>Lines</b></td><td align=left valign=top>");
			tw.WriteLine("<input type=button name=b_br_" + name + " value='Break' onClick='add2form(\"" + name + "\", \"br\");'>");
			tw.WriteLine("<input type=button name=b_hr_" + name + " value='Line' onClick='add2form(\"" + name + "\", \"hr\");'>");
			tw.WriteLine("<input type=button name=b_li_" + name + " value='Bullets' onClick='add2form(\"" + name + "\", \"li\");'>");
			tw.WriteLine("<input type=button name=b_ol_" + name + " value='Numbers' onClick='add2form(\"" + name + "\", \"ol\");'>");
			tw.WriteLine("</td></tr><tr><td align=left valign=middle bgcolor=#cccccc><b>Text Format</b></td><td align=left valign=top>");
			tw.WriteLine("<input type=button name=b_b_" + name + " value='Bold' onClick='add2form(\"" + name + "\", \"b\");'>");
			tw.WriteLine("<input type=button name=b_i_" + name + " value='Italic' onClick='add2form(\"" + name + "\", \"i\");'>");
			tw.WriteLine("<input type=button name=b_big_" + name + " value='Big' onClick='add2form(\"" + name + "\", \"big\");'>");
			tw.WriteLine("<input type=button name=b_small_" + name + " value='Small' onClick='add2form(\"" + name + "\", \"small\");'>");
			tw.WriteLine("</td></tr><tr><td align=left valign=middle bgcolor=#cccccc><b>Links</b></td><td align=left valign=top>");
			tw.WriteLine("<input type=button name=b_a_" + name + " value='Link' onClick='add2form(\"" + name + "\", \"a\");'>");
			tw.WriteLine("<input type=button name=b_ablank_" + name + " value='Blank Link' onClick='add2form(\"" + name + "\", \"ablank\");'>");
			tw.WriteLine("</td></tr><tr><td align=left valign=middle bgcolor=#cccccc><b>Images</b></td><td align=left valign=top>");
			tw.WriteLine("<input type=button name=b_img_" + name + " value='Image' onClick='add2form(\"" + name + "\", \"img\");'>");
			tw.WriteLine("<input type=button name=b_imgleft_" + name + " value='Image Left' onClick='add2form(\"" + name + "\", \"imgleft\");'>");
			tw.WriteLine("<input type=button name=b_imgright_" + name + " value='Image Right' onClick='add2form(\"" + name + "\", \"imgright\");'>");
			tw.WriteLine("</td></tr><tr><td align=left valign=middle bgcolor=#cccccc><b>Test</b></td><td align=left valign=top>");
			tw.WriteLine("<input type=button name=b_test_" + name + " value='TEST HTML' onClick='displayHTML(\"" + name + "\")'>");
			tw.WriteLine("</td></tr></table>");
			tw.WriteLine("</td></tr></table>");
		}
	}


}
