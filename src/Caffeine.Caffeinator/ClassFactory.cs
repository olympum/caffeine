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
	using System.Diagnostics;
	using System.Collections;
	
	public class ClassFactory
	{
		// the classes to generate
		Hashtable classFiles;
		// all loaded classes
		Hashtable classes;
		// references to other assemblies
		string[] references;
		
		public ClassFactory(Hashtable classFiles, string[] references)
		{
			this.classFiles = classFiles;
			this.references = references;
			classes = new Hashtable();
			
			// Populate basic types
			Class.AccessFlag flag = Class.AccessFlag.ACC_PUBLIC;
			classes["byte"] = new Class("B", "byte", flag);
			classes["char"] = new Class("C", "char", flag);
			classes["double"] = new Class("D", "double", flag);
			classes["float"] = new Class("F", "float", flag);
			classes["int"] = new Class("I", "int", flag);
			classes["long"] = new Class("J", "long", flag);
			classes["short"] = new Class("S", "short", flag);
			classes["bool"] = new Class("Z", "bool", flag);
			classes["void"] = new Class("V", "void", flag);
		}
		
		public Class LoadClass(string name)
		{
			if (name == null) {
				return null;
			}
			if (classes.Contains(name)) {
				return (Class) classes[name];
			}
			Class c = new Class();
			c.InternalName = name;
			classes[name] = c;
			ClassFile cf = (ClassFile) classFiles[name];
			if (cf != null) {
				// If a type is not resolved (e.g. a class
				// a missing, or in another jar), we will get
				// a null class file at this stage.
				// We will just continue building the tree
				// during the emit phase, we will find whether
				// the type is available in one of the user
				// provided assemblies.
				cf.BuildClass(c, this);
			}
			return c;
		}
			
		public void ResolveInnerClasses()
		{
			foreach (DictionaryEntry entry in classes) {
				string className = (string) entry.Key;
				int index = className.LastIndexOf('$');
				if (index < 0) {
					continue;
				}
				string enclosingName = className.Substring(0, index);
				Class enclosing = (Class) classes[enclosingName];
				if (enclosing == null) {
					Console.WriteLine("Class not found (perhaps an incomplete Jar): " + enclosingName);
					continue;
				}
				Class enclosed = (Class) classes[className];
				enclosing.AddInnerClass(enclosed);				
			}
		}
		
	}
}
