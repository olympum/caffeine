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

	public class JObjectArray : JArray
	{
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewObjectArray (int len, IntPtr clazz, IntPtr init);
		
		protected JObjectArray (IntPtr raw) : base (raw)
		{
		}
		
		public JObjectArray (JObject other) : base (other)
		{
		}
		
		public JObjectArray (int length, JClass clazz) : base (NewObjectArray (length, clazz.Handle, IntPtr.Zero))
		{
			// out of memory
			JThrowable.CheckAndThrow ();
		}
		
		public JObjectArray (JObject[] source, JClass clazz) : this (source.Length, clazz)
		{
			int l = source.Length;
			for (int i = 0; i < l; i++) {
				JObject o = source[i];
				if (o != null) {
					SetObjectArrayElement (this.Handle, i, o.Handle);
				}
			}
			
			// TODO ArrayIndexOutOfBoundsException
			// TODO ArrayStoreException
			JThrowable.CheckAndThrow ();
		}

		public override object Elements {
			get {
				int l = Length;
				JObject[] c = new JObject[Length];
				for (int i = 0; i < l; i++) {
					// TODO synchronization - not thread safe
					IntPtr raw = GetObjectArrayElement (Handle, i);
					// zero will happen with array 
					// containing a null object reference
					// not an actual exception
					if (raw != IntPtr.Zero) {
						c[i] = new JObject (raw);
					}
				}
				
				//TODO ArrayIndexOutOfBoundsException
				JThrowable.CheckAndThrow ();				
				return c;
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetObjectArrayElement (IntPtr array, int index);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetObjectArrayElement (IntPtr array, int index, IntPtr val);
	}
}
