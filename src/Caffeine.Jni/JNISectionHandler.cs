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
	using System.IO;
	using System.Text;
	using System.Xml;
	
	public sealed class JNISectionHandler : IConfigurationSectionHandler
	{
		const string JAVA_CLASS_PATH = "java.class.path";
		const string JAVA_LIBRARY_PATH = "java.library.path";
		const string JVM_DLL = "jvm.dll";
		const string JAVA_OPTION = "java.option";
		
		public object Create (object parent, 
			object configContext, 
			XmlNode section)
		{
			StringBuilder java_class_path = new StringBuilder ();
			StringBuilder java_library_path = new StringBuilder ();
			string jvm_dll = null;
			ArrayList options = new ArrayList ();			
						
			foreach (XmlNode node in section.ChildNodes) {
				if (node.NodeType != XmlNodeType.Element) {
					continue;
				}
				string nn = node.Name;
				string value = GetStringValue(node,
					"value",
					false);
				if (nn.Equals (JAVA_CLASS_PATH) &&
					value.Length > 0) {
					java_class_path.Append (value).Append (Path.PathSeparator);
				} else if (nn.Equals (JAVA_LIBRARY_PATH) &&
					value.Length > 0) {
					java_library_path.Append (value).Append (Path.PathSeparator);
				} else if (nn.Equals (JVM_DLL) &&
					value.Length > 0) {				
					jvm_dll = value;
				} else if (nn.Equals (JAVA_OPTION) &&
					value.Length > 0) {
					options.Add (value);
				} else {
					// ignore
				}
			}
			
			JNIConfiguration conf = new JNIConfiguration (
				java_class_path.ToString (),
				java_library_path.ToString (),
				jvm_dll,
				(string[]) options.ToArray (typeof (string)));
			return conf;
		}
		
		string GetStringValue(XmlNode _node, 
			string _attribute, 
			bool required)
		{
			XmlNode a = _node.Attributes.RemoveNamedItem(_attribute);
			if (a==null) {
				if (required) {
					throw new ConfigurationException(
						"Attribute required: " + 
						_attribute);
				} else {
					Console.WriteLine (
						"{0} not specified, will use defaults", 
						_attribute);
					return System.String.Empty;
				}
			}
			return a.Value;
		}
	}
	
	internal class JNIConfiguration
	{
		public readonly string java_class_path;
		public readonly string java_library_path;
		public readonly string jvm_dll;
		public readonly string[] java_option;
		
		internal JNIConfiguration (string cp,
			string libpath,
			string dll,
			string[] options)
		{
			java_class_path = cp;
			java_library_path = libpath;
			jvm_dll = dll;
			java_option = options;
		}
	}
}
