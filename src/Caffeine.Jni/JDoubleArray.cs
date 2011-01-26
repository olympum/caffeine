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
			
	public class JDoubleArray : JArray
	{
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewDoubleArray (int len);
		
		public JDoubleArray (JObject other) : base (other)
		{
		}

		public JDoubleArray (int length) : base (NewDoubleArray (length))
		{
			// TODO OutOfMemoryError
			JThrowable.CheckAndThrow ();
		}
		
		public JDoubleArray (double[] buf) : this(buf.Length)
		{
			SetDoubleArrayRegion (Handle, 0, buf.Length, buf);
			// TODO ArrayIndexOutOfBoundsException
			JThrowable.CheckAndThrow ();
		}

		public override object Elements {
			get {
				double[] c = new double[Length];
				GetDoubleArrayRegion (Handle, 0, Length, c);
				// TODO OutOfMemoryError			
				JThrowable.CheckAndThrow ();
				return c;
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void ReleaseDoubleArrayElements (IntPtr array, double[] elems, int mode);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetDoubleArrayRegion (IntPtr array, int start, int len, double[] buf);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern double[] GetDoubleArrayElements (IntPtr array, out bool isCopy);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void GetDoubleArrayRegion (IntPtr array, int start, int len, double[] buf);
	}
}
