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
	
	public class Descriptor
	{
		Descriptor componentType;
		Class c;
		bool isBasicType;
		bool isArrayType;
		bool isObjectType;
		
		public Descriptor()
		{
		}
		
		public Descriptor Component {
			get {
				return componentType;
			}
			set {
				componentType = value;
			}
		}
		
		public Class Class {
			get {
				return c;
			}
			set {
				c = value;
			}
		}
		
		public bool IsBasicType {
			get {
				return isBasicType;
			}
			set {
				isBasicType = value;
			}
		}
		
		public bool IsObjectType {
			get {
				return isObjectType;
			}
			set {
				isObjectType = value;
			}
		}
		
		public bool IsArrayType {
			get {
				return isArrayType;
			}
			set {
				isArrayType = value;
			}
		}
		
		public int GetArrayRank()
		{
			int rank = 0;
			Descriptor tmp = componentType;
			while (tmp != null) {
				tmp = tmp.componentType;
				rank++;
			}
			return rank;
		}
		
		public Class GetElementType()
		{
			Class r = c;
			Descriptor tmp = componentType;
			while (tmp != null) {
				r = tmp.Class;
				tmp = tmp.componentType;
			}
			return r;
		}
		
		public override string ToString()
		{
			if (!IsArrayType) {
				return c.InternalName.ToString();
			} else {
				return componentType.ToString() + "[]";
			}
		}
	}
}
