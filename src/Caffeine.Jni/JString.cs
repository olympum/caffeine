/* 
 * Copyright (C) 2003-2005 Bruno Fernandez-Ruiz <brunofr@olympum.com>
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
	
	public class JString : JObject
	{
		public JString (JObject obj) : base (obj.Handle) {
		}
		
		public JString (JConstructor ctr, params object[] args) : base (ctr, args) {
		}

		public JString (IntPtr raw) : base (raw)
		{
		}
		
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern IntPtr NewString (char[] unicode, int len);
		
		public JString (string str) : base (NewString (str.ToCharArray(), str.Length))
		{
		}
				
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern void GetStringRegion (IntPtr str, int start, int len, char[] buf);
		
		public string String {
			get {

				int length = Length;
				char[] buffer = new char[length];
				GetStringRegion (Handle, 0, length, buffer);
				return new String (buffer);
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int GetStringLength (IntPtr str);
		
		public int Length {
			get {
				return GetStringLength (Handle);
			}
		}
				
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern IntPtr NewStringUTF (char[] utf);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int GetStringUTFLength (IntPtr str);
		
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern void GetStringUTFRegion (IntPtr str, int start, int len, char[] buf);
		
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern char[] GetStringUTFChars (IntPtr str, out bool isCopy);
		
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern void ReleaseStringUTFChars (IntPtr str, char[] chars);
		
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern char[] GetStringChars (IntPtr str, out bool isCopy);
		
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern void ReleaseStringChars (IntPtr str, char[] chars);
		
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern char[] GetStringCritical (IntPtr str, out bool isCopy);
		
		[DllImport(JNIEnv.DLL_JAVA, CharSet=CharSet.Unicode)]
		static extern void ReleaseStringCritical (IntPtr str, char[] cstring);
	}
}
