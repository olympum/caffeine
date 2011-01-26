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
	
	public abstract class JArray : JObject
	{
		protected JArray (IntPtr raw) : base (raw)
		{
		}
		
		protected JArray (JObject other) : base (other)
		{
		}
				
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int GetArrayLength (IntPtr array);

		public int Length {
			get {
				return GetArrayLength (this.Handle);
			}
		}
		
		public abstract object Elements { get;}
				
		//[DllImport(JNIEnv.DLL_JAVA)]
		//static extern object[] GetPrimitiveArrayCritical (IntPtr array, out bool copy);
		
		//[DllImport(JNIEnv.DLL_JAVA)]
		//static extern void ReleasePrimitiveArrayCritical (IntPtr array, object[] carray, int mode);
	}
}
