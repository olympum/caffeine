/* 
 * Copyright (C) 2003, 2004 Bruno Fernandez-Ruiz <brunofr@olympum.com>
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation files
 * (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
 * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
namespace Caffeine.Jni
{
	using System;
	using System.Runtime.InteropServices;
	
	// simulates a union
	// TODO C# <-> Java type mapping
	[StructLayout (LayoutKind.Explicit)]
	public struct JValue
	{
		[FieldOffset (0)] bool z;
		[FieldOffset (0)] byte b;
		[FieldOffset (0)] ushort c;
		[FieldOffset (0)] short s;
		[FieldOffset (0)] int i;
		[FieldOffset (0)] long j;
		[FieldOffset (0)] float f;
		[FieldOffset (0)] double d;
		[FieldOffset (0)] IntPtr l;
		
		public bool JBool {
			set {
				z = value;
			}
		}
		
		public byte JByte {
			set {
				b = value;
			}
		}
		
		public char JChar {
			set {
				c = value;
			}
		}
		
		public short JShort {
			set {
				s = value;
			}
		}
		
		public int JInt {
			set {
				i = value;
			}
		}
		
		public long JLong {
			set {
				j = value;
			}
		}
		
		public float JFloat {
			set {
				f = value;
			}
		}
		
		public double JDouble {
			set {
				d = value;
			}
		}
		
		public JObject JObject {
			set {
				if (value != null)
					l = value.Handle;
			}
		}
		
		internal static JValue[] Convert (params object[] args)
		{
			JValue[] initArgs = JValueInternal.EmptyArgs;
			if (args != null) {
				int l = args.Length;
				initArgs = new JValue [l];
				for (int i = 0; i < l; i++) {
					object arg = args[i];
					if (arg == null) {
						continue;
					}
					if (arg is System.Boolean) {
						initArgs[i].JBool = (bool) arg;
					} else if (arg is System.Byte) {
						initArgs[i].JByte = (byte) arg;					
					} else if (arg is System.Char) {
						initArgs[i].JChar = (char) arg;
					} else if (arg is System.Int16) {
						initArgs[i].JShort = (short) arg;
					} else if (arg is System.Int32) {
						initArgs[i].JInt = (int) arg;
					} else if (arg is System.Int64) {
						initArgs[i].JLong = (long) arg;
					} else if (arg is System.Single) {
						initArgs[i].JFloat = (float) arg;
					} else if (arg is System.Double) {
						initArgs[i].JDouble = (double) arg;
					} else {
						initArgs[i].JObject = Convert (arg);
					}
				}
			}
			return initArgs;
		}
		
		static JObject Convert (object arg)
		{
			JObject array = null;
			if (arg is System.Boolean[]) {
				array = new JBooleanArray ((bool[]) arg);
			} else if (arg is System.Byte[]) {
				array = new JByteArray ((byte[]) arg);
			} else if (arg is System.Char[]) {
				array = new JCharArray ((char[]) arg);
			} else if (arg is System.Int16[]) {
				array = new JShortArray ((short[]) arg);
			} else if (arg is System.Int32[]) {
				array = new JIntArray ((int[]) arg);
			} else if (arg is System.Int64[]) {
				array = new JLongArray ((long[]) arg);
			} else if (arg is System.Single[]) {
				array = new JFloatArray ((float[]) arg);
			} else if (arg is System.Double[]) {
				array = new JDoubleArray ((double[]) arg);
			} else if (arg is JObjectArray) {
				array = (JObjectArray) arg;
			} else if (arg is JObject) {
				return (JObject) arg;
			} else {
				// TODO custom exception
				throw new ApplicationException (
				"Type not supported: " + arg.GetType ());
			}
			return array;
		}
	}
	
	sealed class JValueInternal
	{
		// for optimization purposes
		internal static readonly JValue[] EmptyArgs = new JValue[0];
	}
}
