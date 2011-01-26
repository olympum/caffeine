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
	using System.IO;
	using System.Text;
	
	public class ClassFile
	{
		ConstantPool[] constantPool;
		ushort accessFlags;
		ushort thisClass;
		ushort superClass;
		ushort[] interfaces;
		FieldInfo[] fields;
		MethodInfo[] methods;
		AttributeInfo[] attributes;
		
		public ClassFile(Stream s)
		{
			BigEndianBinaryReader reader = new BigEndianBinaryReader(s);
			/*
				ClassFile {
					u4 magic;
					u2 minor_version;
					u2 major_version;
					u2 constant_pool_count;
					cp_info constant_pool[constant_pool_count-1];
					u2 access_flags;
					u2 this_class;
					u2 super_class;
					u2 interfaces_count;
					u2 interfaces[interfaces_count];
					u2 fields_count;
					field_info fields[fields_count];
					u2 methods_count;
					method_info methods[methods_count];
					u2 attributes_count;
					attribute_info attributes[attributes_count];
				}
			*/
			
			uint magic = reader.ReadUInt32();
			if (magic != 0xCAFEBABE) {
				throw new ApplicationException("Bad magic in class: " +
					magic);
			}
			ushort minorVersion = reader.ReadUInt16();
			ushort majorVersion = reader.ReadUInt16();
			if (majorVersion < 45 || majorVersion > 48) {
				throw new ApplicationException("Unsupported class version (" +
					majorVersion + '.' + minorVersion + ")");
			}
			constantPool = ReadConstantPool(reader);
			accessFlags = reader.ReadUInt16();
			thisClass = reader.ReadUInt16();
			superClass = reader.ReadUInt16();
			interfaces = ReadInterfaces(reader);
			fields = ReadFields(reader);
			methods = ReadMethods(reader);
			attributes = ReadAttributes(reader);
		}
		
		public string Name {
			get {
				return GetClassName(thisClass);
			}
		}
		
		public void BuildClass(Class c, ClassFactory factory)
		{
			// accessFlags
			c.AccessFlags = (Class.AccessFlag) accessFlags;
			// thisClass
			c.InternalName = GetClassName(thisClass);
			// superClass
			c.AddBaseType(factory.LoadClass(GetClassName(superClass)));
			// interfaces
			foreach (ushort iface in interfaces) {
				c.AddBaseType(factory.LoadClass(GetClassName(iface)));
			}
			// fields
			foreach (FieldInfo info in fields) {
				Field f = new Field(c);
				f.AccessFlags = (Field.AccessFlag) info.AccessFlags;
				f.Name = GetUtf8(info.NameIndex);
				f.Signature = GetUtf8(info.DescriptorIndex);
				f.Descriptor = ProcessDescriptor(f.Signature, factory);
				c.AddField(f);
			}
			// methods
			foreach (MethodInfo info in methods) {
				Method m = new Method(c);
				m.AccessFlags = (Method.AccessFlag) info.AccessFlags;
				m.Name = GetUtf8(info.NameIndex);
				m.Signature = GetUtf8(info.DescriptorIndex);
				ProcessMethodDescriptorString(m, m.Signature, factory);
				c.AddMethod(m);
			}
		}
		
		void ProcessMethodDescriptorString(Method m, string value, ClassFactory factory)
		{
			string args = value.Substring(1, value.IndexOf(')') - 1);
			int index = 0;
			while (index < args.Length) {
				string arg = args.Substring(index);
				Descriptor d = ProcessDescriptor (arg, factory);
				if (d.IsBasicType) {
					index++;
				} else if (d.IsObjectType) {
					index += d.Class.InternalName.Length + 2; // Ljava/util/List;
				} else {
					index++; // [
					Descriptor componentType = d;
					while ((componentType = componentType.Component).IsArrayType) {
						index++; // [
					}
					if (componentType.IsBasicType) {
						index++; // [B
					} else {
						index += componentType.Class.InternalName.Length + 2; // Ljava/util/List;
					}
				}
				m.AddArgument(d);
			}
			m.Return = ProcessDescriptor(value.Substring(value.IndexOf(')') + 1), factory);
		}

		const string OBJECT_TYPE = "ObjectType";
		const string ARRAY_TYPE = "ArrayType";
		
		static Hashtable baseTypes;
		
		static ClassFile()
		{
			baseTypes = new Hashtable();
			baseTypes['B'] = "byte";
			baseTypes['C'] = "char";
			baseTypes['D'] = "double";
			baseTypes['F'] = "float";
			baseTypes['I'] = "int";
			baseTypes['J'] = "long";
			baseTypes['L'] = OBJECT_TYPE;
			baseTypes['S'] = "short";
			baseTypes['Z'] = "bool";
			baseTypes['['] = ARRAY_TYPE;
			baseTypes['V'] = "void";
		}
		
		static string GetTypeForDescriptor(string descriptor)
		{
			if (descriptor.Equals(String.Empty)) {
				return "";
			}
			return (string) baseTypes[descriptor[0]];
		}
		
		static string GetTypeForObjectType(string type)
		{
			// Ljava/lang/Object;
			return type.Substring(1, type.IndexOf(';') - 1);
		}

		static Descriptor ProcessDescriptor(string value, ClassFactory factory)
		{
			Descriptor descriptor = new Descriptor();
			string t = GetTypeForDescriptor(value);
			if (ARRAY_TYPE.Equals(t)) {
				descriptor.Component = ProcessDescriptor(value.Substring(1), factory);
				descriptor.IsArrayType = true;
			} else if (OBJECT_TYPE.Equals(t)) {
				string des = GetTypeForObjectType(value);
				descriptor.Class = factory.LoadClass(des);
				descriptor.IsObjectType = true;
			} else {
				descriptor.Class = factory.LoadClass(t);
				descriptor.IsBasicType = true;
			}
			return descriptor;
		}

		string GetClassName(ushort index)
		{
			ConstantPool item = constantPool[index];
			if (item == null) {
				return null; // this is the case for java/lang/Object
			}
			if (item.ConstantType != ConstantType.Class) {
				throw new ApplicationException("Wrong class format");
			}
			return GetUtf8(item.NameIndex);
		}
		
		string GetUtf8(ushort index)
		{
			ConstantPool item = constantPool[index];
			if (item.ConstantType != ConstantType.Utf8) {
				throw new ApplicationException("Wrong class format");
			}
			return item.String;
		}
		
		ushort[] ReadInterfaces(BigEndianBinaryReader reader)
		{
			ushort interfacesCount = reader.ReadUInt16();
			ushort[] interfaces = new ushort[interfacesCount];
			if (interfacesCount == 0) {
				return interfaces;
			}
			for (ushort i = 0; i < interfacesCount; i++) {
				interfaces[i] = reader.ReadUInt16();
			}
			return interfaces;
		}
		
		MethodInfo[] ReadMethods(BigEndianBinaryReader reader)
		{
			ushort methodsCount  = reader.ReadUInt16();
			MethodInfo[] methods = new MethodInfo[methodsCount];
			if (methodsCount == 0) {
				return methods;
			}
			for (ushort i = 0; i < methodsCount; i++) {
				/*
					method_info {
						u2 access_flags;
						u2 name_index;
						u2 descriptor_index;
						u2 attributes_count;
						attribute_info attributes[attributes_count];
					}
				*/
				MethodInfo method;
				method.AccessFlags = reader.ReadUInt16();
				method.NameIndex = reader.ReadUInt16();
				method.DescriptorIndex = reader.ReadUInt16();
				method.Attributes = ReadAttributes(reader);
				methods[i] = method;
			}
			return methods;
		}
		
		FieldInfo[] ReadFields(BigEndianBinaryReader reader)
		{
			ushort fieldsCount = reader.ReadUInt16();
			FieldInfo[] fields = new FieldInfo[fieldsCount];
			if (fieldsCount == 0) {
				return fields;
			}
			for (ushort i = 0; i < fieldsCount; i++) {
				/*
					field_info {
						u2 access_flags;
						u2 name_index;
						u2 descriptor_index;
						u2 attributes_count;
						attribute_info attributes[attributes_count];
					}
				*/
				FieldInfo field;
				field.AccessFlags = reader.ReadUInt16();
				field.NameIndex = reader.ReadUInt16();
				field.DescriptorIndex = reader.ReadUInt16();
				field.Attributes = ReadAttributes(reader);
				fields[i] = field;
			}
			return fields;
		}
		
		AttributeInfo[] ReadAttributes(BigEndianBinaryReader reader)
		{
			/*
				u2 attributes_count;
				attribute_info attributes[attributes_count];
			*/
			ushort attributesCount = reader.ReadUInt16();
			AttributeInfo[] attributes = new AttributeInfo[attributesCount];
			if (attributesCount == 0) {
				return attributes;
			}
			for (ushort i = 0; i < attributesCount; i++) {
				/*
					attribute_info {
						u2 attribute_name_index;
						u4 attribute_length;
						u1 info[attribute_length];
					}
				*/
				AttributeInfo attr;
				attr.AttributeNameIndex = reader.ReadUInt16();
				attr.Bytes = reader.ReadBytes(reader.ReadUInt32());
				attributes[i] = attr;
			}
			return attributes;
		}
		
		ConstantPool[] ReadConstantPool(BigEndianBinaryReader reader)
		{
			ushort constantPoolCount = reader.ReadUInt16();
			ConstantPool[] constantPool = new ConstantPool[constantPoolCount];
			//Console.WriteLine("Reading {0} ConstantPool items", constantPoolCount);
			// JVM Spec 4.1.
			// The value of the constant_pool_count item is equal to the number
			// of entries in the constant_pool table plus one. A constant_pool
			// index is considered valid if it is greater than zero and less
			// than constant_pool_count, with the exception for constants of
			// type long and double noted in 4.4.5.
			for (ushort i = 1; i < constantPoolCount; i++) {
				ConstantType tag = (ConstantType) reader.ReadByte();
				ConstantPool item = new ConstantPool();
				constantPool[i] = item;
				switch (tag) {
					case ConstantType.Class:
						/*
							CONSTANT_Class_info {
								u1 tag;
								u2 name_index;
							}
						*/
						item.ConstantType = ConstantType.Class;
						item.NameIndex = reader.ReadUInt16();
						break;
					case ConstantType.Fieldref:
						/*
							CONSTANT_Fieldref_info {
								u1 tag;
								u2 class_index;
								u2 name_and_type_index;
							}
						*/
						item.ConstantType = ConstantType.Fieldref;
						item.ClassIndex = reader.ReadUInt16();
						item.NameAndTypeIndex = reader.ReadUInt16();
						break;
					case ConstantType.Methodref:
						/*
							CONSTANT_Methodref_info {
								u1 tag;
								u2 class_index;
								u2 name_and_type_index;
							}
						*/
						item.ConstantType = ConstantType.Methodref;
						item.ClassIndex = reader.ReadUInt16();
						item.NameAndTypeIndex = reader.ReadUInt16();
						break;
					case ConstantType.InterfaceMethodref:
						/*
							CONSTANT_InterfaceMethodref_info {
								u1 tag;
								u2 class_index;
								u2 name_and_type_index;
							}
						*/
						item.ConstantType = ConstantType.InterfaceMethodref;
						item.ClassIndex = reader.ReadUInt16();
						item.NameAndTypeIndex = reader.ReadUInt16();
						break;
					case ConstantType.String:
						/*
							CONSTANT_String_info {
								u1 tag;
								u2 string_index;
							}
						*/
						item.ConstantType = ConstantType.String;
						item.StringIndex = reader.ReadUInt16();
						break;
					case ConstantType.Integer:
						/*
							CONSTANT_Integer_info {
								u1 tag;
								u4 bytes;
							}
						*/
						item.ConstantType = ConstantType.Integer;
						item.Integer = reader.ReadInt32();
						break;
					case ConstantType.Float:
						/*
							CONSTANT_Float_info {
								u1 tag;
								u4 bytes;
							}
						*/
						item.ConstantType = ConstantType.Float;
						item.Float = reader.ReadSingle();
						break;
					case ConstantType.Long:
						/*
							CONSTANT_Long_info {
								u1 tag;
								u4 high_bytes;
								u4 low_bytes;
							}
						*/
						item.ConstantType = ConstantType.Long;
						item.Long = reader.ReadInt64();
						// JVM Spec. 4.4.5.
						// All 8-byte constants take up two entries in the
						// constant_pool table of the class file. If a
						// CONSTANT_Long_info or CONSTANT_Double_info structure
						// is the item in the constant_pool table at index n,
						// then the next usable item in the pool is located at
						// index n+2. The constant_pool index n+1 must be valid
						// but is considered unusable.2
						// 2 In retrospect, making 8-byte constants take two
						// constant pool entries was a poor choice.
						++i;
						break;
					case ConstantType.Double:
						/*
							CONSTANT_Double_info {
								u1 tag;
								u4 high_bytes;
								u4 low_bytes;
							}
						*/
						item.ConstantType = ConstantType.Double;
						item.Double = reader.ReadDouble();
						// JVM Spec. 4.4.5.
						// All 8-byte constants take up two entries in the
						// constant_pool table of the class file. If a
						// CONSTANT_Long_info or CONSTANT_Double_info structure
						// is the item in the constant_pool table at index n,
						// then the next usable item in the pool is located at
						// index n+2. The constant_pool index n+1 must be valid
						// but is considered unusable.2
						// 2 In retrospect, making 8-byte constants take two
						// constant pool entries was a poor choice.
						++i;
						break;
					case ConstantType.NameAndType:
						/*
							CONSTANT_NameAndType_info {
								u1 tag;
								u2 name_index;
								u2 descriptor_index;
							}
						*/
						item.ConstantType = ConstantType.NameAndType;
						item.NameIndex = reader.ReadUInt16();
						item.DescriptorIndex = reader.ReadUInt16();
						break;
					case ConstantType.Utf8:
						/*
							CONSTANT_Utf8_info {
								u1 tag;
								u2 length;
								u1 bytes[length];
							}
						*/
						item.ConstantType = ConstantType.Utf8;
						item.String = reader.ReadString(reader.ReadUInt16());
						break;
					default:
						throw new ApplicationException("Wrong ConstantType: " +
							tag);
				}
			}
			return constantPool;
		}
		
		enum ConstantType : byte
		{
			Class = 7,
			Fieldref = 9,
			Methodref = 10,
			InterfaceMethodref = 11,
			String = 8,
			Integer = 3,
			Float = 4,
			Long = 5,
			Double = 6,
			NameAndType = 12,
			Utf8 = 1
		}
		
		// we use the class ConstantPool as a C "union"
		class ConstantPool
		{
			public ConstantType ConstantType;
			public ushort NameIndex;
			public ushort ClassIndex;
			public ushort NameAndTypeIndex;
			public ushort StringIndex;
			public int Integer;
			public float Float;
			public long Long;
			public double Double;
			public ushort DescriptorIndex;
			public string String;
			
			public override string ToString()
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("ConstantType").Append(ConstantType)
					.Append(" NameIndex ").Append(NameIndex)
					.Append(" ClassIndex ").Append(ClassIndex)
					.Append(" NameAndTypeIndex ").Append(NameAndTypeIndex)
					.Append(" StringIndex ").Append(StringIndex)
					.Append(" Integer ").Append(Integer)
					.Append(" Float ").Append(Float)
					.Append(" Long ").Append(Long)
					.Append(" Double ").Append(Double)
					.Append(" DescriptorIndex ").Append(DescriptorIndex)
					.Append(" String ").Append(String);
				return builder.ToString();
			}
		}
		
		struct FieldInfo
		{
			public ushort AccessFlags;
			public ushort NameIndex;
			public ushort DescriptorIndex;
			public AttributeInfo[] Attributes;
		}
		
		struct AttributeInfo
		{
			public ushort AttributeNameIndex;
			public byte[] Bytes;
		}
		
		struct MethodInfo
		{
			public ushort AccessFlags;
			public ushort NameIndex;
			public ushort DescriptorIndex;
			public AttributeInfo[] Attributes;
		}
		
		class BigEndianBinaryReader
		{
			Stream s;
			
			public BigEndianBinaryReader(Stream s)
			{
				this.s = s;
			}
			
			public uint ReadUInt32()
			{
				byte[] bytes = new byte[4];
				bytes[3] = (byte) s.ReadByte();
				bytes[2] = (byte) s.ReadByte();
				bytes[1] = (byte) s.ReadByte();
				bytes[0] = (byte) s.ReadByte();
				return BitConverter.ToUInt32(bytes, 0);
			}
			
			public ushort ReadUInt16()
			{
				byte[] bytes = new byte[2];
				bytes[1] = (byte) s.ReadByte();
				bytes[0] = (byte) s.ReadByte();
				return BitConverter.ToUInt16(bytes, 0);
			}
			
			public long ReadInt64()
			{
				uint lowBits = ReadUInt32();
				uint highBits = ReadUInt32();
				return (long) (lowBits << 32) + highBits;
			}
	
			public double ReadDouble()
			{
				return BitConverter.Int64BitsToDouble(ReadInt64());
			}
			
			public int ReadInt32()
			{
				byte[] bytes = new byte[4];
				bytes[3] = (byte) s.ReadByte();
				bytes[2] = (byte) s.ReadByte();
				bytes[1] = (byte) s.ReadByte();
				bytes[0] = (byte) s.ReadByte();
				return BitConverter.ToInt32(bytes, 0);
			}
			
			public float ReadSingle()
			{
				byte[] bytes = new byte[4];
				bytes[3] = (byte) s.ReadByte();
				bytes[2] = (byte) s.ReadByte();
				bytes[1] = (byte) s.ReadByte();
				bytes[0] = (byte) s.ReadByte();
				return BitConverter.ToSingle(bytes, 0);
			}
			
			public byte ReadByte()
			{
				return (byte) s.ReadByte();
			}
			
			public byte[] ReadBytes(uint length)
			{
				byte[] bytes = new byte[length];
				for (uint i = 0; i < length; i++) {
					bytes[i] = (byte) s.ReadByte();
				}
				return bytes;
			}
			
			public string ReadString(ushort length)
			{
				byte[] utf8 = ReadBytes(length);
				// TODO implement UTF8, right now only ASCII supported
				return System.Text.ASCIIEncoding.ASCII.GetString(utf8, 0, length);
				//return BitConverter.ToString(utf8);
			}
		}
	}
}
