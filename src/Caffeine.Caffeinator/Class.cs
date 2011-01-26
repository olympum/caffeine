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
	using System.Text;
	using System.Reflection;
	
	public class Class
	{
		Class declaringClass;
		string internalName;   // java/util/Map$Entry		
		string name;           // Entry
		string fqn;            // java.util.Map.Entry
		string ns;             // java.util
		AccessFlag accessFlags;
		ArrayList baseTypes;
		ArrayList fields;
		ArrayList methods;
		ArrayList innerClasses;
		
		public Class()
		{
			baseTypes = new ArrayList();
			fields = new ArrayList();
			methods = new ArrayList();
			innerClasses = new ArrayList();
		}
		
		public Class(string internalName, string fqn, AccessFlag accessFlags) : this()
		{
			this.internalName = internalName;
			this.fqn = fqn;
			this.accessFlags = accessFlags;
		}
		
		public AccessFlag AccessFlags {
			set {
				accessFlags = value;
			}
			get {
				return accessFlags;
			}
		}
		
		public string InternalName {
			get {
				return internalName;
			}
			set {
				internalName = value;
			}
		}
		
		public string FullyQualifiedName {
			get {
				if (fqn == null) {
					// java.lang.Object
					fqn = internalName.Replace('/', '.').Replace('$', '+');
				}
				return fqn;
			}
			set {
				fqn = value;
			}
		}
				
		public string Name {
			get {
				if (name == null) {
					// java/util/Map$Entry -> Entry
					string dottified = FullyQualifiedName;
					int index = dottified.LastIndexOf('+');
					if (index < 0) {
						return dottified;
					}
					name = dottified.Substring(index + 1);
				}
				return name;
			}
			set {
				name = value;
			}
		}
				
		public string NameSpace {
			get {
				if (ns == null) {
					// java/util/Map$Entry -> java.util
					int index = internalName.LastIndexOf('/');
					if (index < 0) {
						return String.Empty;
					}
					ns = internalName.Substring(0, index).Replace('/', '.');
				}
				return ns;
			}
			set {
				ns = value;
			}
		}
		
		public ICollection BaseTypes {
			get {
				return baseTypes;
			}
		}
		
		public ICollection Fields {
			get {
				return fields;
			}
		}
		
		public ICollection Methods {
			get {
				return methods;
			}
		}
		
		public ICollection InnerTypes {
			get {
				return innerClasses;
			}
		}
		
		public Class DeclaringClass {
			set {
				declaringClass = value;
			}
			get {
				return declaringClass;
			}
		}
		
		public void AddBaseType(Class typeName)
		{
			if (typeName == null) {
				return;
			}
			
			if (baseTypes.Contains(typeName)) {
				throw new ApplicationException("Duplicate BaseType: " + typeName);
			}
			baseTypes.Add(typeName);
		}
		
		public void AddInnerClass(Class c)
		{
			if (innerClasses.Contains(c)) {
				// this can happen while processing things like:
				// javax/swing/text/html/HTMLDocument$HTMLReader$TitleAction
				return;
			}			
			innerClasses.Add(c);
			c.DeclaringClass = this;
		}
		
		public void AddField(Field f)
		{
			if (fields.Contains(f)){
				throw new ApplicationException("Duplicate Field: " + f);
			}
			fields.Add(f);
		}
		
		public void AddMethod(Method m)
		{
			if (methods.Contains(m)){
				throw new ApplicationException("Duplicate Method: " + m);
			}
			methods.Add(m);
		}
		
		public bool IsPublic {
			get {
				return (accessFlags & AccessFlag.ACC_PUBLIC) != 0;
			}
		}
		
		public bool IsFinal {
			get {
				return (accessFlags & AccessFlag.ACC_FINAL) != 0;
			}
		}
		
		public bool IsInterface {
			get {
				return (accessFlags & AccessFlag.ACC_INTERFACE) != 0;
			}
		}
		
		public bool IsAbstract {
			get {
				return (accessFlags & AccessFlag.ACC_ABSTRACT) != 0;
			}
		}
		
		public TypeAttributes TypeAttributes {
			get {
				TypeAttributes r;
				
				if (IsPublic) {
					r = TypeAttributes.Public;
				} else {
					r = TypeAttributes.NotPublic;
				}
				
				if (IsInterface) {
					r |= TypeAttributes.Interface;
				} else {
					r |= TypeAttributes.Class;
				}
				
				if (IsFinal) {
					r |= TypeAttributes.Sealed;
				}
				
				if (IsAbstract) {
					r |= TypeAttributes.Abstract;
				}
				
				if (DeclaringClass != null) {
					if (IsPublic) {
						r |= TypeAttributes.NestedPublic;
					} else {
						r |= TypeAttributes.NestedFamily;
					}
				}
				
				// TODO serializable
				// r |= TypeAttributes.Serializable;
				return r;
			}
		}
		
		public enum AccessFlag
		{
			ACC_PUBLIC = 0x0001,
			ACC_FINAL = 0x0010,
			ACC_INTERFACE = 0x0200,
			ACC_ABSTRACT = 0x0400
		}
		
		public override string ToString()
		{
			return Name;
		}
	}
}
