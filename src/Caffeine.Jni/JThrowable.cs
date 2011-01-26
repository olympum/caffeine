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
	
	public class JThrowable : JObject
	{
		private JThrowable (IntPtr raw) : base (raw)
		{
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr ExceptionOccurred ();
		
		public static void CheckAndThrow() {
			if (!ExceptionCheck())
				// fast path
				// the performance of ExceptionCheck is
				// better than ExceptionOccurred
				return;
			
			IntPtr ret = ExceptionOccurred ();
			if (ret == IntPtr.Zero)
				return;
			
			ExceptionClear ();
			
			throw new JNIException(new JThrowable (ret));
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		public static extern bool ExceptionCheck ();
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void ExceptionClear ();
		
		public static void Clear ()
		{
			ExceptionClear ();
		}
		
		// describe also clears
		[DllImport(JNIEnv.DLL_JAVA)]
		public static extern void ExceptionDescribe ();
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int Throw (IntPtr throwable);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int ThrowNew (IntPtr clazz, string msg);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern void FatalError (string msg);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int PushLocalFrame (int capacity);
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern IntPtr PopLocalFrame (IntPtr result);
	}
	
	// a .NET exception used by Caffeine to tell the client classes
	// that something went wrong, wrapping an actual Java exception
	public class JNIException : ApplicationException
	{
		JThrowable throwable;
		
		public JNIException (JThrowable throwable)
		{
			this.throwable = throwable;	
		}
		
		public JThrowable JThrowable {
			get {
				return throwable;
			}
		}
	}
}
