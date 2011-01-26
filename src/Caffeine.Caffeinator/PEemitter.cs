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
	using System.IO;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Threading;
	
	public class PEemitter
	{
		AppDomain appDomain;
		AssemblyName asmName;
		AssemblyBuilder asmBuilder;
		ModuleBuilder modBuilder;
		TypeBuilder currType;
		
		public PEemitter() {
		}
		
		public void BeginModule(string fileName)
		{
			FileInfo f = new FileInfo(fileName);
			appDomain = Thread.GetDomain();
			asmName = new AssemblyName();
			asmName.Name = f.Name;
			asmBuilder = appDomain.DefineDynamicAssembly(
				asmName,
				AssemblyBuilderAccess.RunAndSave,
				f.DirectoryName
				);
			modBuilder = asmBuilder.DefineDynamicModule(
				asmName.Name,
				asmName.Name);
		}
		
		public void EndModule()
		{
			try {
				asmBuilder.Save(asmName.Name);
			} catch (Exception e) {
				throw new ApplicationException("Could not save: " +
				asmName, e);
			}
		}
		
		public TypeBuilder BeginType(string typeName, TypeAttributes attr)
		{
			currType = modBuilder.DefineType(typeName, attr);
			return currType;
		}
		
		public void EndType()
		{
			currType.CreateType();
		}
		
		public TypeBuilder CurrentType {
			get {
				return currType;
			}
		}
		
		public ModuleBuilder ModuleBuilder {
			get {
				return modBuilder;
			}
		}
	}
}
