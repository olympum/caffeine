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
	using System.Collections;
	using System.Configuration;
	using System.Reflection;
	using System.Runtime.InteropServices;
	
	public sealed class JavaVM
	{
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern string GetError ();

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int CreateJavaVMDLL (string dllName, JavaVMOption[] options, int nOptions);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int CreateJavaVMAnon (JavaVMOption[] options, int nOptions);
		
		internal JavaVM ()
		{
			ArrayList options = new ArrayList ();
			
			JNIConfiguration config = config = (JNIConfiguration) 
					ConfigurationSettings.GetConfig(
						"caffeine/jni.net");

			// CLASSPATH
			string cpOption = "-Djava.class.path=";
			if (config != null &&
				config.java_class_path.Length > 0) {
				cpOption += config.java_class_path;
			} else {
				// default
				cpOption += ".";
			}
			options.Add (cpOption);

			// LIBPATH
			if (config != null &&
				config.java_library_path.Length > 0) {
				string lpOption = "-Djava.library.path=" +
					config.java_library_path;
				options.Add (lpOption);
			}
			
			// JVM.DLL
			string dllName = config != null ? 
				config.jvm_dll : null;
			
			// OTHER
			string[] otherOptions = config != null ?
				config.java_option : null;
			if (otherOptions != null) {
				foreach (string option in otherOptions) {
					options.Add (option);
				}
			}
						
			PrintVersion ();

			Console.WriteLine("JavaVM Options:");

			JavaVMOption[] vOptions = new JavaVMOption [options.Count];
			for (int i = 0; i < options.Count; i++) {
				vOptions[i].optionString = (string) options[i];
				Console.WriteLine("Option {0}: [{1}]", i, vOptions[i].optionString);
			}

			
			int res;
			
			if (dllName != null) {
				res = CreateJavaVMDLL (dllName, vOptions, vOptions.Length);
			} else {
				res = CreateJavaVMAnon (vOptions, vOptions.Length);
			}
			
			if (res < 0) {
				throw new SystemException (
					"Failed to create Caffeine.Jni.JavaVM: "
					+ GetError () + " (" + res + ").");
			}
			Console.WriteLine("JavaVM Started.");
		}
		
		struct JavaVMOption
		{
			public string optionString;
			char extraInfo;
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int DestroyJavaVM ();
		
		~JavaVM ()
		{
			//DestroyJavaVM ();
		}
		
		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int EnsureLocalCapacity (int capacity);

		[DllImport(JNIEnv.DLL_JAVA)]
		static extern int DetachCurrentThread ();
		
		public static void Detach ()
		{
			DetachCurrentThread ();
		}

		static void PrintVersion ()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			AssemblyName name = assembly.GetName();
			System.Version version =  name.Version;
			string sVersion = version.Major + "." + 
				version.Minor + "." + 
				version.Build + "." + 
				version.Revision;
			string sTitle = null;
			string sProduct = null;
			string sCopyright = null;
			object[] attributes = assembly.GetCustomAttributes(true);
			foreach (object attribute in attributes) {
				AssemblyProductAttribute product = 
					attribute as AssemblyProductAttribute;
				if (product != null) {
					sProduct = product.Product;
					continue;
				}
				AssemblyTitleAttribute title = 
					attribute as AssemblyTitleAttribute;
				if (title != null) {
					sTitle = title.Title;
					continue;
				}
				AssemblyCopyrightAttribute copyright = 
					attribute as AssemblyCopyrightAttribute;
				if (copyright != null) {
					sCopyright = copyright.Copyright;
				}
			}
			Console.WriteLine ("{0} {1}, (c) {2}", 
				sTitle, 
				sVersion, 
				sCopyright);
		}		
	}
}
