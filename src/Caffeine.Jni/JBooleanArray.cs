/* 
 * Copyright (C) 2003 Bruno Fernandez-Ruiz <brunofr@olympum.com>
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

	public class JBooleanArray : JArray
	{
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewBooleanArray (int len);

		public JBooleanArray (JObject other) : base (other)
		{
		}

		public JBooleanArray (int length) : base (NewBooleanArray (length))
		{
			// TODO OutOfMemoryError
			JThrowable.CheckAndThrow ();
		}
		
		public JBooleanArray (bool[] buf) : this (buf.Length)
		{
			SetBooleanArrayRegion (Handle, 0, buf.Length, buf);
			
			// TODO ArrayIndexOutOfBoundsException
			JThrowable.CheckAndThrow ();
		}
		
		public override object Elements {
			get {
				bool[] c = new bool[Length];
				GetBooleanArrayRegion (Handle, 0, Length, c);
				// TODO OutOfMemoryError			
				JThrowable.CheckAndThrow ();
				
				return c;
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool[] GetBooleanArrayElements (IntPtr array, out bool isCopy);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void ReleaseBooleanArrayElements (IntPtr array, bool[] elems, int mode);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void GetBooleanArrayRegion (IntPtr array, int start, int l, bool[] buf);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetBooleanArrayRegion (IntPtr array, int start, int l, bool[] buf);
	}
}
