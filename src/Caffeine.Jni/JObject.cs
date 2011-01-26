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
	using System.Runtime.InteropServices;
	using System.Threading;	
	
	public class JObject : IDisposable
	{
		HandleRef native;
		bool avoidDelete;
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewObject (IntPtr clazz, IntPtr methodID, JValue[] args);
		
		public JObject (JConstructor ctr, params object[] args)
		{
			JValue[] initArgs = JValue.Convert (args);
			IntPtr raw = NewObject (ctr.DeclaringClass.Handle, ctr.Handle, initArgs);
			if (raw == IntPtr.Zero) {
				JThrowable.CheckAndThrow ();
			}
			native = new HandleRef(this, raw);
			this.avoidDelete = false;
		}
		
		internal JObject (IntPtr raw)
		{
			if (raw == IntPtr.Zero)
			{
				JThrowable.Clear ();
				throw new OutOfMemoryException ();
			}
			native = new HandleRef(this, raw);
			this.avoidDelete = false;
		}
		
		protected JObject (JObject other)
		{
			if (other != null) {
				this.native = other.native;
				this.avoidDelete = false;
				// stop the destructor of the object being 
				// copied to delete the local reference to the
				// Java object
				other.avoidDelete = true;
			}
		}
		
		~JObject ()
		{
			CleanUp();
		}

		public void Dispose()
		{
			CleanUp();
			// prevent the object from being placed in the
			// finalization queue
			System.GC.SuppressFinalize (this);
		}

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void DeleteLocalRef (IntPtr obj);
			
		void CleanUp() 
		{
			if (!avoidDelete) 
			{
				DeleteLocalRef (native.Handle);
			}
		}
		
		internal IntPtr Handle {
			get {
				return native.Handle;
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr GetObjectClass (IntPtr obj);
		
		public JClass GetClass ()
		{
			IntPtr ret = GetObjectClass (Handle);
			return new JClass (ret);
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int MonitorEnter (IntPtr obj);
		
		public void MonitorEnter ()
		{
			int r = MonitorEnter (Handle);
			if (r < 0) {
				JThrowable.Clear ();
				throw new OutOfMemoryException ();
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int MonitorExit (IntPtr obj);
		
		public void MonitorExit ()
		{
			int r = MonitorExit (Handle);
			if (r < 0) {
				JThrowable.Clear ();
				throw new SynchronizationLockException ();
			}
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern bool IsSameObject (IntPtr obj1, IntPtr obj2);
		
		public override bool Equals (object other)
		{
			if (other == null)
				return false;
			if (other == this)
				return true;
			JObject obj = other as JObject;
			if (obj == null)
				return false;
			if (this.Handle == IntPtr.Zero && obj.Handle != IntPtr.Zero ||
				this.Handle != IntPtr.Zero && obj.Handle == IntPtr.Zero) {
				return false;
			}
			if (this.Handle == IntPtr.Zero && obj.Handle == IntPtr.Zero) {
				return true;
			}
			
			return IsSameObject (this.Handle, obj.Handle);
		}
		
		public override int GetHashCode ()
		{
			return native.Handle.ToInt32 ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewGlobalRef (IntPtr lobj);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void DeleteGlobalRef (IntPtr gref);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr NewLocalRef (IntPtr reference);
	}
}
