using System;

namespace Comig
{
	/// <summary>
	/// Summary description for ComigTable.
	/// </summary>
	public class ComigTable
	{
		public enum			ORDERTYPE: long {ASCENDING, DESCENDING}

		public string		name;
		public string		displayName;
		public ComigField[]	field;
		public int			fieldCount;
		public bool			canInsert;
		public bool			canUpdate;
		public bool			canDelete;
		public ORDERTYPE	orderType;

		public ComigTable()
		{
			//
			// TODO: Add constructor logic here
			//
			clear();
		}

		public void clear()
		{
			name		= "";
			field		= new ComigField[255];
			fieldCount	= 0;
			canInsert	= false;
			canUpdate	= false;
			canDelete	= false;
			orderType	= ORDERTYPE.ASCENDING;
		}

		public bool addField(string Name, ComigField.TYPE Type)
		{
			if (Name.Trim().Length <= 0) return false;

			for (int n = 0; n < fieldCount; n++)
			{
				if (field[n].name == Name) return false;
			}

			field[fieldCount]		= new ComigField();
			field[fieldCount].name	= Name;
			field[fieldCount].table	= name;
			field[fieldCount].type	= Type;
			fieldCount++;

			return true;
		}

		public void removeField(int Index)
		{
			if (Index < 0) return;

			for (int n = Index; n < fieldCount - 1; n++)
			{
				field[n] = field[n + 1];
			}
			fieldCount--;
		}

		public int getFieldIndex(string Name)
		{
			int r = -1;

			for (int n = 0; n < fieldCount; n++)
			{
				if (field[n].name == Name) r = n;
			}

			return r;
		}

		public void moveFieldUp(int Index)
		{
			if (Index <= 0) return;

			ComigField t = field[Index - 1];
			field[Index - 1] = field[Index];
			field[Index] = t;
		}
	
		public void moveFieldDown(int Index)
		{
			if (Index >= fieldCount - 1) return;

			ComigField t = field[Index + 1];
			field[Index + 1] = field[Index];
			field[Index] = t;
		}

		public void setKeyField(int Index)
		{
			for (int n = 0; n < fieldCount; n++)
			{
				if (n == Index) field[n].isKey = true; else field[n].isKey = false;
			}
		}

		public void setNameField(int Index)
		{
			for (int n = 0; n < fieldCount; n++)
			{
				if (n == Index) field[n].isName = true; else field[n].isName = false;
			}
		}

		public string getKeyField()
		{
			string r = "";

			for (int n = 0; n < fieldCount; n++)
			{
				if (field[n].isKey) r = field[n].name;
			}

			return r;
		}

		public string getNameField()
		{
			string r = "";

			for (int n = 0; n < fieldCount; n++)
			{
				if (field[n].isName) r = field[n].name;
			}

			return r;
		}

		public string getOrderType()
		{
			string ret = "";

			switch (orderType)
			{
				case ORDERTYPE.ASCENDING:
					ret = "ASC";
					break;
				case ORDERTYPE.DESCENDING:
					ret = "DESC";
					break;
			}

			return ret;
		}
	}
}
