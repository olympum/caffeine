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
	
	public class JField : JObject, IMember
	{
		JClass declaringClass;
		string name;
		bool isStatic;
		
		internal JField (JClass declaringClass, 
			string name,
			string sig,
			bool isStatic,			
			IntPtr methodID) 
			: base (methodID)
		{
			this.declaringClass = declaringClass;
			this.name = name;
			this.isStatic = isStatic;
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

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetStaticObjectField (IntPtr clazz, IntPtr fieldID);		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetObjectField (IntPtr obj, IntPtr fieldID);
		
		public JObject GetObject (JObject obj)
		{
			IntPtr raw;
			
			if (isStatic) {
				raw = GetStaticObjectField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					raw = GetObjectField (obj.Handle, Handle);
				}
			}
			
			return new JObject (raw);
		}
		
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticObjectField (IntPtr clazz, IntPtr fieldID, IntPtr value);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetObjectField (IntPtr obj, IntPtr fieldID, IntPtr val);
		
		public void SetObject (JObject obj, JObject value)
		{
			if (isStatic) {
				SetStaticObjectField (
					DeclaringClass.Handle,
					Handle,
					value.Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetObjectField (
						obj.Handle,
						Handle,
						value.Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool GetStaticBooleanField (IntPtr clazz, IntPtr fieldID);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool GetBooleanField (IntPtr obj, IntPtr fieldID);
		
		public bool GetBool (JObject obj)
		{
			bool r;
			
			if (isStatic) {
				r = GetStaticBooleanField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					r = GetBooleanField (obj.Handle, Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
			
			return r;
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticBooleanField (IntPtr clazz, IntPtr fieldID, bool value);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetBooleanField (IntPtr obj, IntPtr fieldID, bool val);
		
		public void SetBool (JObject obj, bool value)
		{
			if (isStatic) {
				SetStaticBooleanField (
					DeclaringClass.Handle,
					Handle,
					value);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetBooleanField (
						obj.Handle,
						Handle,
						value);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}
			
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern byte GetStaticByteField (IntPtr clazz, IntPtr fieldID);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern byte GetByteField (IntPtr obj, IntPtr fieldID);
		
		public byte GetByte (JObject obj)
		{
			byte r;
			
			if (isStatic) {
				r = GetStaticByteField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					r = GetByteField (obj.Handle, Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
			
			return r;
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticByteField (IntPtr clazz, IntPtr fieldID, byte value);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetByteField (IntPtr obj, IntPtr fieldID, byte val);

		public void SetByte (JObject obj, byte value)
		{
			if (isStatic) {
				SetStaticByteField (
					DeclaringClass.Handle,
					Handle,
					value);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetByteField (
						obj.Handle,
						Handle,
						value);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern ushort GetStaticCharField (IntPtr clazz, IntPtr fieldID);		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern ushort GetCharField (IntPtr obj, IntPtr fieldID);
		
		public char GetChar (JObject obj)
		{
			ushort r;
			
			if (isStatic) {
				r = GetStaticCharField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					r = GetCharField (obj.Handle, Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
			
			return (char) r;
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticCharField (IntPtr clazz, IntPtr fieldID, char value);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetCharField (IntPtr obj, IntPtr fieldID, char val);
		
		public void SetChar (JObject obj, char value)
		{
			if (isStatic) {
				SetStaticCharField (
					DeclaringClass.Handle,
					Handle,
					value);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetCharField (
						obj.Handle,
						Handle,
						value);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern short GetStaticShortField (IntPtr clazz, IntPtr fieldID);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern short GetShortField (IntPtr obj, IntPtr fieldID);

		public short GetShort (JObject obj)
		{
			short r;
			
			if (isStatic) {
				r = GetStaticShortField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					r = GetShortField (obj.Handle, Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
			
			return r;
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticShortField (IntPtr clazz, IntPtr fieldID, short value);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetShortField (IntPtr obj, IntPtr fieldID, short val);
		
		public void SetShort (JObject obj, short value)
		{
			if (isStatic) {
				SetStaticShortField (
					DeclaringClass.Handle,
					Handle,
					value);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetShortField (
						obj.Handle,
						Handle,
						value);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int GetStaticIntField (IntPtr clazz, IntPtr fieldID);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int GetIntField (IntPtr obj, IntPtr fieldID);
				
		public int GetInt (JObject obj)
		{
			int r;
			
			if (isStatic) {
				r = GetStaticIntField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					r = GetIntField (obj.Handle, Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
			
			return r;
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticIntField (IntPtr clazz, IntPtr fieldID, int value);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetIntField (IntPtr obj, IntPtr fieldID, int val);
		
		public void SetInt (JObject obj, int value)
		{
			if (isStatic) {
				SetStaticIntField (
					DeclaringClass.Handle,
					Handle,
					value);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetIntField (
						obj.Handle,
						Handle,
						value);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern long GetStaticLongField (IntPtr clazz, IntPtr fieldID);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern long GetLongField (IntPtr obj, IntPtr fieldID);
		
		public long GetLong (JObject obj)
		{
			long r;
			
			if (isStatic) {
				r = GetStaticLongField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					r = GetLongField (obj.Handle, Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
			
			return r;
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticLongField (IntPtr clazz, IntPtr fieldID, long value);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetLongField (IntPtr obj, IntPtr fieldID, long val);
		
		public void SetLong (JObject obj, long value)
		{
			if (isStatic) {
				SetStaticLongField (
					DeclaringClass.Handle,
					Handle,
					value);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetLongField (
						obj.Handle,
						Handle,
						value);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern float GetStaticFloatField (IntPtr clazz, IntPtr fieldID);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern float GetFloatField (IntPtr obj, IntPtr fieldID);
		
		public float GetFloat (JObject obj)
		{
			float r;
			
			if (isStatic) {
				r = GetStaticFloatField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					r = GetFloatField (obj.Handle, Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
			
			return r;
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticFloatField (IntPtr clazz, IntPtr fieldID, float value);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetFloatField (IntPtr obj, IntPtr fieldID, float val);
		
		public void SetFloat (JObject obj, float value)
		{
			if (isStatic) {
				SetStaticFloatField (
					DeclaringClass.Handle,
					Handle,
					value);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetFloatField (
						obj.Handle,
						Handle,
						value);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern double GetStaticDoubleField (IntPtr clazz, IntPtr fieldID);
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern double GetDoubleField (IntPtr obj, IntPtr fieldID);
		
		public double GetDouble (JObject obj)
		{
			double r;
			
			if (isStatic) {
				r = GetStaticDoubleField (
					DeclaringClass.Handle,
					Handle);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					r = GetDoubleField (obj.Handle, Handle);
				}
			}
			
			JThrowable.CheckAndThrow ();
			
			return r;
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetStaticDoubleField (IntPtr clazz, IntPtr fieldID, double value);		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void SetDoubleField (IntPtr obj, IntPtr fieldID, double val);
		
		public void SetDouble (JObject obj, double value)
		{
			if (isStatic) {
				SetStaticDoubleField (
					DeclaringClass.Handle,
					Handle,
					value);
			} else {
				if (obj == null) {
					throw new NullReferenceException (Name);
				} else {
					SetDoubleField (
						obj.Handle,
						Handle,
						value);
				}
			}
			
			JThrowable.CheckAndThrow ();
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr FromReflectedField (IntPtr field);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr ToReflectedField (IntPtr cls, IntPtr fieldID,  bool isStatic);
	}
}
