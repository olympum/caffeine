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
	
	public class JFloatArray : JArray
	{
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewFloatArray (int len);

		public JFloatArray (JObject other) : base (other)
		{
		}

		public JFloatArray (int length) : base (NewFloatArray (length))
		{
			// TODO OutOfMemoryError
			JThrowable.CheckAndThrow ();
		}
		
		public JFloatArray (float[] buf) : this(buf.Length)
		{
			// TODO ArrayIndexOutOfBoundsException
			JThrowable.CheckAndThrow ();
			SetFloatArrayRegion (Handle, 0, buf.Length, buf);
		}
		
		public override object Elements {
			get {
				float[] c = new float[Length];
				GetFloatArrayRegion (Handle, 0, Length, c);
				// TODO OutOfMemoryError			
				JThrowable.CheckAndThrow ();
				return c;
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern float[] GetFloatArrayElements (IntPtr array, out bool isCopy);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void ReleaseFloatArrayElements (IntPtr array, float[] elems, int mode);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void GetFloatArrayRegion (IntPtr array, int start, int len, float[] buf);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetFloatArrayRegion (IntPtr array, int start, int len, float[] buf);
	}
}
