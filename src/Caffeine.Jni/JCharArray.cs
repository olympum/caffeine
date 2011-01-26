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

	public class JCharArray : JArray
	{
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewCharArray (int len);

		public JCharArray (JObject other) : base (other)
		{
		}

		public JCharArray (int length) : base (NewCharArray (length))
		{
			// TODO OutOfMemoryError
			JThrowable.CheckAndThrow ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetCharArrayRegion (IntPtr array, int start, int len, char[] buf);
		
		public JCharArray (char[] buf) : this(buf.Length)
		{
			SetCharArrayRegion (Handle, 0, buf.Length, buf);
		
			// TODO ArrayIndexOutOfBoundsException
			JThrowable.CheckAndThrow ();
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void GetCharArrayRegion (IntPtr array, int start, int len, ushort[] buf);
		
		public override object Elements {
			get {
				ushort[] c = new ushort[Length];
				GetCharArrayRegion (Handle, 0, Length, c);
				// TODO OutOfMemoryError
				JThrowable.CheckAndThrow ();
				char[] r = new char[Length];
				for (int i = 0; i < Length; i++)
					r[i] = (char) c[i];
				return r;
			}
		}		
	}
}
