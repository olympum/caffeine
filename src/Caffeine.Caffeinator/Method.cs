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
	using System.Reflection;
	
	public class Method
	{
		AccessFlag accessFlags;
		string name;
		string signature;
		Class parent;
		Descriptor ret;
		ArrayList arguments;
		
		public Method(Class parent)
		{
			this.parent = parent;
			arguments = new ArrayList();
		}
		
		public AccessFlag AccessFlags {
			set {
				accessFlags = value;
			}
			get {
				return accessFlags;
			}
		}
				
		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}
				
		public string Signature {
			get {
				return signature;
			}
			set {
				if (value == null) {
					throw new ApplicationException("Class format error, null descriptor");
				}
				if (value[0] != '(') {
					throw new ApplicationException("Class format error, wrong descriptor");
				}
				signature = value;
			}
		}
		
		public Class DeclaringType {
			get {
				return parent;
			}
			set {
				parent = value;
			}
		}
		
		public Descriptor Return {
			get {
				return ret;
			}
			set {
				ret = value;
			}
		}
		
		public Descriptor[] Arguments {
			get {
				return (Descriptor[]) arguments.ToArray(typeof(Descriptor));
			}
		}
		
		public void AddArgument(Descriptor des)
		{
			arguments.Add(des);
		}
		
		public bool IsPublic {
			get {
				return (accessFlags & AccessFlag.ACC_PUBLIC) != 0;
			}
		}
		
		public bool IsPrivate {
			get {
				return (accessFlags & AccessFlag.ACC_PRIVATE) != 0;
			}
		}

		public bool IsProtected {
			get {
				return (accessFlags & AccessFlag.ACC_PROTECTED) != 0;
			}
		}
		
		public bool IsStatic {
			get {
				return (accessFlags & AccessFlag.ACC_STATIC) != 0;
			}
		}

		public bool IsFinal {
			get {
				return (accessFlags & AccessFlag.ACC_FINAL) != 0;
			}
		}
		
		public bool IsSynchronized {
			get {
				return (accessFlags & AccessFlag.ACC_SYNCHRONIZED) != 0;
			}
		}
		
		public bool IsNative {
			get {
				return (accessFlags & AccessFlag.ACC_NATIVE) != 0;
			}
		}
		
		public bool IsAbstract {
			get {
				return (accessFlags & AccessFlag.ACC_ABSTRACT) != 0;
			}
		}
		
		public bool IsStrict {
			get {
				return (accessFlags & AccessFlag.ACC_STRICT) != 0;
			}
		}
		
		public MethodAttributes MethodAttributes {
			get {
				MethodAttributes attr;
				
				if (IsPublic) {
					attr = MethodAttributes.Public;
				} else if (IsProtected) {
					attr = MethodAttributes.Family;
				} else if (IsPrivate) {
					attr = MethodAttributes.Private;
				} else { // package -> assembly (C# "internal")
					attr = MethodAttributes.Assembly;
				}
				
				if (IsAbstract) {
					attr |= MethodAttributes.Abstract;
				}
				
				if (IsStatic) {
					attr |= MethodAttributes.Static;
				} else {
					if (IsFinal) {
						attr |= MethodAttributes.Final; 
					} else {
						attr |= MethodAttributes.Virtual;
					}
				}
								
				return attr;
			}
		}
		
		public enum AccessFlag
		{
			ACC_PUBLIC = 0x0001,
			ACC_PRIVATE = 0x0002,
			ACC_PROTECTED = 0x0004,
			ACC_STATIC = 0x0008,
			ACC_FINAL = 0x0010,
			ACC_SYNCHRONIZED = 0x0020,
			ACC_NATIVE = 0x0100,
			ACC_ABSTRACT = 0x0400,
			ACC_STRICT = 0x0800
		}

		public override string ToString()
		{
			string r = ret.ToString() + " " + name;
			r += "(";
			foreach (Descriptor d in arguments) {
				r += d.ToString();
			}
			r += ")";
			return r;
		}
	}
}
