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
	
	public class Field
	{
		AccessFlag accessFlags;
		string name;
		string signature;
		Class parent;
		Descriptor descriptor;
		
		public Field(Class parent)
		{
			this.parent = parent;
		}
		
		public AccessFlag AccessFlags {
			set {
				accessFlags = value;
			}
			get {
				return accessFlags;
			}
		}
		
		public Descriptor Descriptor {
			get {
				return descriptor;
			}
			set {
				descriptor = value;
			}
		}
		
		public string Signature {
			get {
				return signature;
			}
			set {
				signature = value;
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
		
		public Class DeclaringType {
			get {
				return parent;
			}
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
		
		public bool IsVolatile {
			get {
				return (accessFlags & AccessFlag.ACC_VOLATILE) != 0;
			}
		}
		
		public bool IsTransient {
			get {
				return (accessFlags & AccessFlag.ACC_TRANSIENT) != 0;
			}
		}
		
		public enum AccessFlag : ushort
		{
			ACC_PUBLIC = 0x0001,
			ACC_PRIVATE = 0x0002,
			ACC_PROTECTED = 0x0004,
			ACC_STATIC = 0x0008,
			ACC_FINAL = 0x0010,
			ACC_VOLATILE = 0x0040,
			ACC_TRANSIENT = 0x0080
		}
		
		public override string ToString()
		{
			return descriptor + " " + name;
		}
	}
}
