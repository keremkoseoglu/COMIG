using System;

namespace Comig
{
	/// <summary>
	/// Summary description for ComigUser.
	/// </summary>
	public class ComigUser
	{
		public string	username;
		public string	password;
		public string[]	authTable;
		public int		authTableCount;

		public ComigUser()
		{
			//
			// TODO: Add constructor logic here
			//
			clear();
		}

		public void clear()
		{
			username		= "";
			password		= "";
			authTable		= new string[255];
			authTableCount	= 0;
		}

		public void addTable(string Name)
		{
			for (int n = 0; n < authTableCount; n++)
			{
				if (authTable[n] == Name) return;
			}

			authTable[authTableCount] = Name;
			authTableCount++;
		}

		public void removeTable(string Name)
		{
			int index = getTableIndex (Name);
			if (index < 0) return;

			for (int n = index; n < authTableCount - 1; n++)
			{
				authTable[n] = authTable[n + 1];
			}
			authTableCount--;
		}

		public int getTableIndex(string Name)
		{
			int r = -1;

			for (int n = 0; n < authTableCount; n++)
			{
				if (authTable[n] == Name) r = n;
			}

			return r;
		}
	}
}
