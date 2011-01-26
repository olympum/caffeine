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
	using System.Text;
	using System.Runtime.InteropServices;
		
	public class JMethod : JObject, IMember
	{		
		readonly JClass declaringClass;
		readonly string name;
		readonly bool isStatic;
		// used by Invoke
		readonly string sig;
		bool isArray;
		JMethodReturnType rType;
		
		internal JMethod (JClass declaringClass, 
			string name,
			string sig,
			bool isStatic,
			IntPtr methodID) 
			: base (methodID)
		{
			this.declaringClass = declaringClass;
			this.name = name;
			this.isStatic = isStatic;
			this.sig = sig;
			this.rType = JMethodReturnType.INVALID;
		}

		public JClass DeclaringClass { 
			get {
				return declaringClass;
			}
		}
				
		public string Name { 
			get {
				return name;
			}			
		}

		// convenience method
		// not used by generated code since it requires 
		// boxing/unboxing on return type (performance)
		public object Invoke (JObject obj, params object[] args)
		{
			return Invoke (obj, JValue.Convert (args));
		}

		public object Invoke (JObject obj, JValue[] args)
		{
			if (sig != null && rType == JMethodReturnType.INVALID) {
				DetermineReturnType ();
			}
			
			// all arrays are objects
			if (isArray) {
				return CallObject (obj, args);
			}
			
			// if not an array, call right method based on signature
			switch (rType) {
			case JMethodReturnType.BOOL:
				return CallBoolean (obj, args);
			case JMethodReturnType.BYTE:
				return CallByte (obj, args);
			case JMethodReturnType.CHAR:
				return CallChar (obj, args);
			case JMethodReturnType.SHORT:
				return CallShort (obj, args);
			case JMethodReturnType.INT:
				return CallInt (obj, args);
			case JMethodReturnType.LONG:
				return CallLong (obj, args);
			case JMethodReturnType.FLOAT:
				return CallFloat (obj, args);
			case JMethodReturnType.DOUBLE:
				return CallDouble (obj, args);
			case JMethodReturnType.VOID:
				CallVoid (obj, args);
				return null;
			case JMethodReturnType.OBJECT:
				return CallObject (obj, args);
			default:
				throw new SystemException ("Unreachable reached");
			}
		}
		
		void DetermineReturnType ()
		{
				int l = sig.LastIndexOf (')');
				if (l < -1) {
					throw new SystemException ("Unreachable reached");
				}
				
				string ending = sig.Substring (l + 1);
				isArray = (ending[0] == '[');			
				char c = isArray ? ending[1] : ending[0];
				switch (c) {
				case 'Z':
					rType = JMethodReturnType.BOOL;
					break;
				case 'B':
					rType = JMethodReturnType.BYTE;
					break;
				case 'C':
					rType = JMethodReturnType.CHAR;
					break;
				case 'S':
					rType = JMethodReturnType.SHORT;
					break;
				case 'I':
					rType = JMethodReturnType.INT;
					break;
				case 'J':
					rType = JMethodReturnType.LONG;
					break;
				case 'F':
					rType = JMethodReturnType.FLOAT;
					break;
				case 'D':
					rType = JMethodReturnType.DOUBLE;
					break;
				case 'V':
					rType = JMethodReturnType.VOID;
					break;
				case 'L':
					rType = JMethodReturnType.OBJECT;
					break;
				default:
					throw new SystemException ("Unreachable reached");
				}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool CallStaticBooleanMethod (IntPtr clazz, IntPtr methodID, JValue[] value);		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool CallBooleanMethod (IntPtr obj, IntPtr methodID, JValue[] value);
				
		public bool CallBoolean (JObject obj, JValue[] args)
		{
			bool r;
			
			if (isStatic) {
				r = CallStaticBooleanMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallBooleanMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();

			return r;
		}
		
		public bool CallBoolean (JObject obj, params object[] args)
		{			
			return CallBoolean (obj, JValue.Convert (args));
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern sbyte CallStaticByteMethod (IntPtr clazz, IntPtr methodID, JValue[] value);
				
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern sbyte CallByteMethod (IntPtr obj, IntPtr methodID, JValue[] value);
		
		public sbyte CallByte (JObject obj, JValue[] args)
		{
			sbyte r;
			
			if (isStatic) {
				r = CallStaticByteMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallByteMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();

			return r;
		}
		
		public sbyte CallByte (JObject obj, params object[] args)
		{			
			return CallByte (obj, JValue.Convert (args));
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern ushort CallStaticCharMethod (IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern ushort CallCharMethod (IntPtr obj, IntPtr methodID, JValue[] value);
		
		public char CallChar (JObject obj, JValue[] args)
		{
			ushort r;
			
			if (isStatic) {
				r = CallStaticCharMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallCharMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();

			return (char) r;
		}

		public char CallChar (JObject obj, params object[] args)
		{			
			return CallChar (obj, JValue.Convert (args));
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern short CallStaticShortMethod (IntPtr clazz, IntPtr methodID, JValue[] value);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern short CallShortMethod (IntPtr obj, IntPtr methodID, JValue[] value);
		
		public short CallShort (JObject obj, JValue[] args)
		{
			short r;
			
			if (isStatic) {
				r = CallStaticShortMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallShortMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();

			return r;
		}

		public short CallShort (JObject obj, params object[] args)
		{			
			return CallShort (obj, JValue.Convert (args));
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int CallStaticIntMethod (IntPtr clazz, IntPtr methodID, JValue[] value);
				
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int CallIntMethod (IntPtr obj, IntPtr methodID, JValue[] value);
		
		public int CallInt (JObject obj, JValue[] args)
		{
			int r;
			
			if (isStatic) {
				r = CallStaticIntMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallIntMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();

			return r;
		}

		public int CallInt (JObject obj, params object[] args)
		{			
			return CallInt (obj, JValue.Convert (args));
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern long CallStaticLongMethod (IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern long CallLongMethod (IntPtr obj, IntPtr methodID, JValue[] value);
		
		public long CallLong (JObject obj, JValue[] args)
		{
			long r;
			
			if (isStatic) {
				r = CallStaticLongMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallLongMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();

			return r;
		}

		public long CallLong (JObject obj, params object[] args)
		{			
			return CallLong (obj, JValue.Convert (args));
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern float CallStaticFloatMethod (IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern float CallFloatMethod (IntPtr obj, IntPtr methodID, JValue[] value);

		public float CallFloat (JObject obj, JValue[] args)
		{
			float  r;
			
			if (isStatic) {
				r = CallStaticFloatMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallFloatMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();

			return r;
		}

		public float CallFloat (JObject obj, params object[] args)
		{			
			return CallFloat (obj, JValue.Convert (args));
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern double CallStaticDoubleMethod (IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern double CallDoubleMethod (IntPtr obj, IntPtr methodID, JValue[] value);
		
		public double CallDouble (JObject obj, JValue[] args)
		{
			double r;
			
			if (isStatic) {
				r = CallStaticDoubleMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallDoubleMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();

			return r;
		}

		public double CallDouble (JObject obj, params object[] args)
		{			
			return CallDouble (obj, JValue.Convert (args));
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void CallStaticVoidMethod (IntPtr cls, IntPtr methodID, JValue[] args);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void CallVoidMethod (IntPtr obj, IntPtr methodID, JValue[] value);

		public void CallVoid (JObject obj, JValue[] args)
		{			
			if (isStatic) {
				CallStaticVoidMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				CallVoidMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();
		}

		public void CallVoid (JObject obj, params object[] args)
		{
			CallVoid (obj, JValue.Convert (args));
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr CallStaticObjectMethod (IntPtr cls, IntPtr methodID, JValue[] args);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr CallObjectMethod (IntPtr obj, IntPtr methodID, JValue[] value);

		public JObject CallObject (JObject obj, JValue[] args)
		{
			IntPtr r;
			
			if (isStatic) {
				r = CallStaticObjectMethod (
					DeclaringClass.Handle,
					Handle,
					args);
			} else {
				r = CallObjectMethod (
					obj.Handle,
					Handle,
					args);
			}
			
			JThrowable.CheckAndThrow ();
			
			return new JObject (r);
		}

		public JObject CallObject (JObject obj, params object[] args)
		{			
			return CallObject (obj, JValue.Convert (args));
		}
				
		// I can hardly see any need for any of the following methods
		// only used in situations like super.foo ();
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr CallNonvirtualObjectMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool CallNonvirtualBooleanMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern byte CallNonvirtualByteMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern ushort CallNonvirtualCharMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern short CallNonvirtualShortMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int CallNonvirtualIntMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern long CallNonvirtualLongMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern float CallNonvirtualFloatMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern double CallNonvirtualDoubleMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void CallNonvirtualVoidMethod (IntPtr obj, IntPtr clazz, IntPtr methodID, JValue[] value);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr FromReflectedMethod (IntPtr method);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr ToReflectedMethod (IntPtr cls, IntPtr methodID,  bool isStatic);
		
		enum JMethodReturnType
		{
			INVALID,
			OBJECT,
			BOOL,
			BYTE,
			CHAR,
			SHORT,
			INT,
			LONG,
			FLOAT,
			DOUBLE,
			VOID
		}
	}
}
