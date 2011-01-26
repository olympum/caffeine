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
	using Mono.GetOptions;
	
	class MainClass
	{
		public static void Main(string[] args)
		{
			try {
				CaffeOptions opt = CaffeOptions.Instance;
				opt.ProcessArgs();
				
				if (opt.Verbose) {
					Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
					Debug.AutoFlush = true;
				}
				
				if (opt.RemainingArguments.Length == 0) {
					opt.DoUsage();
					throw new ApplicationException("Jar files to be processed not specified");
				}
				
				string assemblyName = opt.Output;
				if (assemblyName == null) {
					assemblyName = opt.RemainingArguments[0];
					FileInfo info = new FileInfo(assemblyName);
					if (!info.Exists) {
						throw new ApplicationException("File does not exist: " +
							assemblyName);
					}
					int index = assemblyName.LastIndexOf(".jar");
					if (index < 0) {
						index = assemblyName.LastIndexOf(".zip");
					}
					if (index > 0) {
						assemblyName = assemblyName.Substring(0, index);
					}
					assemblyName += ".dll";
					Console.WriteLine("Output assembly: {0}" +
						" (Use /out switch to specify assembly name)",
						assemblyName);
				}
				
				// 1st pass: load class files in each Jar into a hashtable				
				Hashtable classFiles = new Hashtable();
				foreach (string file in opt.RemainingArguments) {
					// try/catch purposely to allow processing to continue
					try {
						JarFile j = new JarFile(file);
						foreach (DictionaryEntry entry in j.Classes) {
							// WARNING: silently overwrites existing class
							classFiles[entry.Key] = entry.Value;
						}
					} catch (ApplicationException e) {
						Console.WriteLine("Error while processing {0}: {1}", 
							file, e.Message);
					}
				}
				
				// 2nd pass: load classes from class files and assembly references
				ClassFactory factory = new ClassFactory(classFiles, opt.References);
				
				foreach (ClassFile cf in classFiles.Values) {
					factory.LoadClass(cf.Name);
				}
				
				// 3rd pass: dereference inner types
				factory.ResolveInnerClasses();
				
				// Actual generation
				PEemitter emitter = new PEemitter();
				TypeGenerator bld = new TypeGenerator(
					emitter,
					factory);
				emitter.BeginModule(assemblyName);
				
				// 1st pass: generate all type builders
				// (in order to avoid cyclic references)	
				foreach (string classFile in classFiles.Keys) {
					bld.AddType(factory.LoadClass(classFile));
				}
				
				Console.WriteLine("Generating types");
				// 2nd pass: generate type implementation
				foreach (string classFile in classFiles.Keys) {
					Class c = factory.LoadClass(classFile);
					bld.Generate(c);
				}
				
				Console.WriteLine("Baking types");
				// 3rd pass: "bake" types (inner order matters)
				bld.EndTypes();
				
				// 4th save assembly
				emitter.EndModule();
			} catch (Exception e) 
			{
				Console.WriteLine(e);
				Console.WriteLine("Error: {0}", e.Message);
				System.Environment.Exit(1);
			}
		}
	}
	
	public class CaffeOptions : Options
	{
		public static readonly CaffeOptions Instance = new CaffeOptions();
		
		string[] args;
		
		private CaffeOptions() 
		{
			string[] tmp = System.Environment.GetCommandLineArgs();
			args = new string[tmp.Length -1];
			if (args.Length > 0) {
				System.Array.Copy(tmp, 1, args, 0, args.Length);
			}
		}
		
		public void ProcessArgs()
		{
			base.ProcessArgs(args);
		}
				
		[Option("Print detailed progress information", 'v', "verbose")]
		public bool Verbose = false;
		
		[Option(-1, "Reference an assembly", 'r', "reference")]
		public string[] References = new string[] {"rt.dll"};
		
		[Option("Assembly name", 'o', "out")]
		public string Output;
	}
}
