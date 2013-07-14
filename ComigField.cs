using System;

namespace Comig
{
	/// <summary>
	/// Summary description for ComigField.
	/// </summary>
	public class ComigField
	{
		public enum TYPE: long {DATE, FOREIGN_KEY, MEMO, NUMBER, TEXT};

		public string table;
		public string name;
		public string displayName;
		public TYPE type;
		public bool isKey;
		public bool isName;
		public string fkTable;
		public string fkKeyField;
		public string fkNameField;

		public ComigField()
		{
			//
			// TODO: Add constructor logic here
			//
			clear();
		}

		public void clear()
		{
			table	= "";
			name	= "";
			type	= TYPE.TEXT;
			isKey	= false;
			isName	= false;
		}



	}
}
