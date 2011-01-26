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
	
	public sealed class JClass : JObject
	{
		readonly static JavaVM vm = new JavaVM ();
		
		internal JClass (IntPtr raw) : base (raw)
		{
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr FindClass (string name);
		
		public static JClass ForName (string name)
		{
			IntPtr raw = FindClass (name);
			if (raw != IntPtr.Zero) {
				return new JClass (raw);
			}
			
			// raw is 0 if and only if an exception occurred
			JThrowable.Clear ();
			
			// ClassFormatError 
			// ClassCircularityError 
			// NoClassDefFoundError 
			// OutOfMemoryError 
			// ExceptionInInitializerError
			throw new TypeLoadException (name);
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetSuperclass (IntPtr sub);
		
		public JClass SuperClass {
			get {
				IntPtr raw = GetSuperclass (this.Handle);
				if (raw != IntPtr.Zero) {
					return new JClass (raw);
				} else {
					return null;
				}
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetStaticMethodID (IntPtr clazz, string name, string sig);
				
		public JMethod GetStaticMethod (string name, string sig)
		{
			if (name.Equals ("<init>") || name.Equals ("clinit")) {
				throw new MissingMethodException (name);
			}
			IntPtr raw = GetStaticMethodID (this.Handle, name, sig);
			if (raw != IntPtr.Zero) {
				return new JMethod (this, name, sig, true, raw);
			} else {
				// raw is 0 if and only if an exception occurred
				JThrowable.Clear ();
				throw new MissingMethodException (name);
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetMethodID (IntPtr clazz, string name, string sig);
		
		public JMethod GetMethod (string name, string sig)
		{
			if (name.Equals ("<init>") || name.Equals ("clinit")) {
				throw new MissingMethodException (name);
			}
			IntPtr raw = GetMethodID (this.Handle, name, sig);
			if (raw != IntPtr.Zero) {
				return new JMethod (this, name, sig, false, raw);
			} else {
				// raw is 0 if and only if an exception occurred
				JThrowable.Clear ();
				throw new MissingMethodException (name);
			}
		}
		
		public JConstructor GetConstructor (string sig)
		{
			IntPtr raw = GetMethodID (this.Handle, "<init>", sig);
			if (raw != IntPtr.Zero) {
				return new JConstructor (this, raw);
			} else {
				// raw is 0 if and only if an exception occurred
				JThrowable.Clear ();
				throw new MissingMethodException ("<init>");
			}
		}
		
		public JConstructor GetConstructor ()
		{
			return GetConstructor ("()V");
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetStaticFieldID (IntPtr clazz, string name, string sig);
		
		public JField GetStaticField (string name, string sig)
		{
			IntPtr raw = GetStaticFieldID (this.Handle, name, sig);
			if (raw != IntPtr.Zero) {
				return new JField (this, name, sig, true, raw);
			} else {
				// raw is 0 if and only if an exception occurred
				JThrowable.Clear ();
				throw new MissingFieldException (name);
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetFieldID (IntPtr clazz, string name, string sig);
		
		public JField GetField (string name, string sig)
		{
			IntPtr raw = GetFieldID (this.Handle, name, sig);
			if (raw != IntPtr.Zero) {
				return new JField (this, name, sig, false, raw);
			} else {
				// raw is 0 if and only if an exception occurred
				JThrowable.Clear ();
				throw new MissingFieldException (name);
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr AllocObject (IntPtr clazz);
				
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewObject (IntPtr clazz, IntPtr methodID, JValue[] args);
		
		public JObject NewInstance ()
		{
			IntPtr ctr = GetMethodID (this.Handle, "<init>", "()V");
			if (ctr == IntPtr.Zero) {
				// ctr is 0 if and only if an exception occurred
				JThrowable.Clear ();
				throw new MissingMethodException ("<init>");
			}
			IntPtr raw = NewObject (this.Handle, ctr, JValue.Convert ()); 
			if (raw == IntPtr.Zero) {
				// raw is 0 if and only if an exception occurred
 				JThrowable.CheckAndThrow ();
			}
			return new JObject (raw);
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool IsInstanceOf (IntPtr obj, IntPtr clazz);
		
		public bool IsInstance (JObject obj)
		{
			return IsInstanceOf (obj.Handle, this.Handle);
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool IsAssignableFrom ( IntPtr sub, IntPtr sup);

		public bool IsAssignableFrom (JClass other)
		{
			return IsAssignableFrom (this.Handle, other.Handle);
		}		
	}
}
