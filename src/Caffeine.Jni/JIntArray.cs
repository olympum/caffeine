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
		
	public class JIntArray : JArray
	{
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewIntArray (int len);
		
		public JIntArray (JObject other) : base (other)
		{
		}

		public JIntArray (int length) : base (NewIntArray (length))
		{
			// TODO OutOfMemoryError
			JThrowable.CheckAndThrow ();
		}
		
		public JIntArray (int[] buf) : this(buf.Length)
		{
			// TODO ArrayIndexOutOfBoundsException
			JThrowable.CheckAndThrow ();
			SetIntArrayRegion (Handle, 0, buf.Length, buf);
		}
		
		public override object Elements {
			get {
				int[] c = new int[Length];
				GetIntArrayRegion (Handle, 0, Length, c);
				// TODO OutOfMemoryError			
				JThrowable.CheckAndThrow ();
				return c;
			}
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int[] GetIntArrayElements (IntPtr array, out bool isCopy);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void ReleaseIntArrayElements (IntPtr array, int[] elems, int mode);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void GetIntArrayRegion (IntPtr array, int start, int len, int[] buf);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetIntArrayRegion (IntPtr array, int start, int len, int[] buf);
	}
}
