




using System;

namespace MD5
{

	public enum MD5InitializerConstant : uint
	{
		A =0x67452301,
		B=0xEFCDAB89,
		C=0x98BADCFE,
		D=0X10325476
	}

	sealed public class Digest
	{
		public uint A;
		public uint B;
		public uint C;
		public uint D;

		public Digest()
		{
			A=(uint)MD5InitializerConstant.A;
			B=(uint)MD5InitializerConstant.B;
			C=(uint)MD5InitializerConstant.C;
			D=(uint)MD5InitializerConstant.D;
       	}
		public override string ToString()
		{
			string st ;
			st= MD5Helper.ReverseByte(A).ToString("X8")+
				MD5Helper.ReverseByte(B).ToString("X8")+
                MD5Helper.ReverseByte(C).ToString("X8")+
				MD5Helper.ReverseByte(D).ToString("X8");
			return st;
			
		}

	}

	sealed public class MD5Helper
	{

		private MD5Helper(){}

		public static uint RotateLeft(uint uiNumber , ushort shift)
		{
			return ((uiNumber >> 32-shift)|(uiNumber<<shift));
		}

		public static uint ReverseByte(uint uiNumber)
		{
			return ( (	(uiNumber & 0x000000ff) <<24) |
						(uiNumber >>24) |
					(	(uiNumber & 0x00ff0000) >>8)  |
					(	(uiNumber & 0x0000ff00) <<8) );
		}
	}

	public class MD5ChangingEventArgs:EventArgs
	{
		public readonly byte[] NewData;
		
		public MD5ChangingEventArgs(byte [] data)
		{
			byte [] NewData = new byte[data.Length];
			for (int i =0; i<data.Length;i++)
				NewData[i]=data[i];
		}

		public MD5ChangingEventArgs(string data)
		{
			byte [] NewData = new byte[data.Length];
			for (int i =0; i<data.Length;i++)
				NewData[i]=(byte)data[i];
		}

	}

	public class MD5ChangedEventArgs:EventArgs
	{
		public readonly byte[] NewData;
		public readonly string FingerPrint;
		
		public MD5ChangedEventArgs(byte [] data,string HashedValue)
		{
			byte [] NewData = new byte[data.Length];
			for (int i =0; i<data.Length;i++)
				NewData[i]=data[i];
			FingerPrint=HashedValue;
		}

		public MD5ChangedEventArgs(string data,string HashedValue)
		{
			byte [] NewData = new byte[data.Length];
			for (int i =0; i<data.Length;i++)
				NewData[i]=(byte)data[i];

			FingerPrint=HashedValue;
		}

	}




}