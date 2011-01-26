/* 
 * Copyright (C) 2004 Bruno Fernandez-Ruiz <brunofr@olympum.com>
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
namespace Caffeine.Caffeinator
{
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.IO;
	using ICSharpCode.SharpZipLib.Zip;
	
	public class JarFile
	{
		Hashtable classes;
		
		public JarFile(string jarFileName)
		{
			if (!File.Exists(jarFileName)) {
				throw new ApplicationException(
					"File does not exist: " + jarFileName);
			}
			ZipFile jar = new ZipFile(jarFileName);
			
			// 1st pass: populate Jar classes in the hashtable
			classes = FindClasses(jar);
			// 2nd pass: resolve names from the ConstantPool
			//classes = ResolveConstantPoolNames(classFiles);
			// 3th pass: resolve references to inner classes
			//ResolveInnerClasses(classFiles);
			
			//Debug.Write("Read " + classFiles.Count + " types, out of which ");
			//Debug.WriteLine(classes.Count + " namespace types in " + jarFileName);
		}
		
		public IDictionary Classes {
			get {
				return classes;
			}
		}
		
		private static Hashtable FindClasses(ZipFile jar)
		{
			Hashtable classFiles = new Hashtable();
			foreach (ZipEntry e in jar) {
				if (!e.Name.EndsWith(".class")) {
					continue;
				}
				// drop the .class suffix
				string className = e.Name.Split(new char[] { '.' })[0];
				classFiles[className] = new ClassFile(jar.GetInputStream(e));
				Debug.WriteLine("Read " + className);
			}
			return classFiles;
		}
	}
}
