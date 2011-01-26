/*
 * Copyright (C) 2003-2004 Pekka Enberg <penberg@iki.fi>
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
package com.olympum.tools;

import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.PrintStream;
import java.lang.reflect.Modifier;
import java.util.Iterator;
import java.util.HashMap;
import java.util.List;

import org.dom4j.Document;
import org.dom4j.Element;
import org.dom4j.io.SAXReader;

public class CsJniNetWrapperGenerator {
	private String getCLRType (String javaType) {
		int literalLengh = javaType.indexOf ('[');
		String literal = javaType; 
		String arrayLiteral = "";
		if (literalLengh != -1) {
			literal = javaType.substring (0, literalLengh);
			arrayLiteral = javaType.substring (literalLengh, 
				javaType.length());
		}
		
		return normalize (literal) + arrayLiteral;
	}
	
	/**
	 * Defines the direct base class and the interfaces implemented by
	 * <code>clazz</code>.
	 * 
	 * @see Section 10.1.2. of the C# language specification.
	 */
	private void emitClassBase(PrintStream out, Element clazz) {
		String parent = clazz.attributeValue("parent");
		Element interfacesRoot = clazz.element("implements");

		if (parent == null && interfacesRoot == null)
			return;

		out.print(" : ");
		if (parent != null) {
			out.print(normalize(parent));
			if (interfacesRoot != null)
				out.print(", ");
		}
		if (interfacesRoot != null) {
			Iterator interfaces = interfacesRoot.elementIterator("interface");
			while (interfaces.hasNext()) {
				Element e = (Element) interfaces.next();
				out.print(normalize(e.attributeValue("name")));
				if (interfaces.hasNext())
					out.print(", ");
			}
		}
	}

	private final static HashMap JVM_BUILT_IN_TYPE_SIGNATURES = new HashMap();
	static {
		JVM_BUILT_IN_TYPE_SIGNATURES.put("boolean", "Z");
		JVM_BUILT_IN_TYPE_SIGNATURES.put("byte",    "B");
		JVM_BUILT_IN_TYPE_SIGNATURES.put("char",    "C");
		JVM_BUILT_IN_TYPE_SIGNATURES.put("double",  "D");
		JVM_BUILT_IN_TYPE_SIGNATURES.put("float",   "F");
		JVM_BUILT_IN_TYPE_SIGNATURES.put("int",     "I");
		JVM_BUILT_IN_TYPE_SIGNATURES.put("long",    "J");
		JVM_BUILT_IN_TYPE_SIGNATURES.put("short",   "S");
		JVM_BUILT_IN_TYPE_SIGNATURES.put("void",    "V");
	}

	private boolean isTypeArray(String type) {
		return type.endsWith("[]");
	}

	private String getBaseType(String arrayType) {
		return arrayType.substring(0, arrayType.length() - 2);
	}

	private String getTypeSignature(String type) {
		boolean isArray = false;
		if (isTypeArray(type)) {
			type = getBaseType(type);
			isArray = true;
		}
		String ret = (String) JVM_BUILT_IN_TYPE_SIGNATURES.get(type);
		if (ret == null) {
			ret = "L" + type + ";";
		}
		if (isArray)
			ret = "[" + ret;
		return ret.replace('.','/');
	}

	private String parameterSignatures(Element method) {
		if (method.element("parameters") == null)
			return "";
		StringBuffer buf = new StringBuffer();
		Iterator parameters = method.element("parameters").elementIterator("parameter");
		while (parameters.hasNext()) {
			Element e = (Element) parameters.next();
			buf.append(getTypeSignature(e.attributeValue("type")));
		}
		return buf.toString();
	}

	/**
	 * Return method JVM type signature for a Java method definition.
	 *
	 * The convention is described here:
	 * http://java.sun.com/docs/books/tutorial/native1.1/implementing/method.html
	 */
	private String getMethodSignature(Element method) {
		return "(" + parameterSignatures(method) + ")" +
		       getTypeSignature(method.element("return-type").attributeValue("type"));
	}

	private String getCtorSignature(Element ctor) {
		return "(" + parameterSignatures(ctor) + ")V";
	}

	private void emitIndented(PrintStream out, String line, int level) {
		for (int i = 0; i < level; i++)
			out.print("\t");
		out.println(line);
	}

	private void emitConstant(PrintStream out, Element constant) {
		String clrType = getCLRType(constant.attributeValue("type"));
		String value = constant.attributeValue("value");
		if (clrType.equals("double")) {
			if ("Infinity".equals(value))
				value = "DotNetSystem.Double.PositiveInfinity";
			else if ("-Infinity".equals(value))
				value = "DotNetSystem.Double.NegativeInfinity";
			else if ("NaN".equals(value))
				value = "DotNetSystem.Double.NaN";
		} else if (clrType.equals("float")) {
			if ("Infinity".equals(value))
				value = "DotNetSystem.Single.PositiveInfinity";
			else if ("-Infinity".equals(value))
				value = "DotNetSystem.Single.NegativeInfinity";
			else if ("NaN".equals(value))
				value = "DotNetSystem.Single.NaN";
		}
		emitIndented(out,
			     "public const "
			     + clrType
			     + " "
			     + constant.attributeValue("name")
			     + " = ("
			     + clrType
			     + ")"
			     + value
			     + ";",
			     2);
	}
	
	private void emitConstants(PrintStream out, Element clazz) {
		List constants = clazz.elements("constant");
		Iterator iter = constants.iterator();
		while (iter.hasNext()) {
			Element e = (Element) iter.next();
			emitConstant(out, e);
		}
	}
	
	/**
	 * Marks whether to generate the java.lang.String to System.String
	 * implicit conversation operators.
	 */
	private static int ctrIndex = -1;

	private void emitCtorAndMethodIds(PrintStream out, Element clazz, String className, boolean isImplementation) {
   		List ctors = clazz.elements("constructor");
		if (!isImplementation) {
			for (int i = 0; i < ctors.size(); i++) {
				Element e = (Element) ctors.get(i);
				emitIndented(out, "readonly static JConstructor ctor" +
					     i + ";", 2);
			}
		}
    
		List methods = clazz.elements("method");
		for (int i = 0; i < methods.size(); i++) {
			Element e = (Element) methods.get(i);
			boolean isAbstract = e.attributeValue("abstract") != null;
			if (!isImplementation || (isImplementation && isAbstract)) {
    			out.println("\t\treadonly static JMethod " +
    				    e.attributeValue("name") + "_mid" +
    				    i + ";");
			}
		}
		out.println();
		
		emitIndented(out, "static " +
			     className +
			     "() {", 2);
		String qualifiedName = clazz.attributeValue("name").replace('.','/');
		emitIndented(out, "clazz = JClass.ForName(\"" +
			     qualifiedName + "\");", 3);
		
		if (!isImplementation) {
			for (int i = 0; i < ctors.size(); i++) {
				Element e = (Element) ctors.get(i);
				String ctrSignature = getCtorSignature(e);
				emitIndented(out,
					     "ctor" + i
					     + " = clazz.GetConstructor("
					     + "\"" + ctrSignature + "\");",
					     3);
				/*
				* Force the creation of the implicit operators between
				* java.lang.String and System.String
				*/
				if ("java/lang/String".equals(qualifiedName) &&
					"([C)V".equals(ctrSignature))
					ctrIndex = i;
			}
		}
		
		for (int i = 0; i < methods.size(); i++) {
			Element e = (Element) methods.get(i);
			boolean isAbstract = e.attributeValue("abstract") != null;
			if (isImplementation && !isAbstract) 
				continue;
			String methodName = e.attributeValue("name");
			boolean isStatic = e.attributeValue("static") != null;
			String callString = isStatic ? "GetStaticMethod" : "GetMethod";
			emitIndented(out,
				     methodName + "_mid" + i
				     + " = clazz." 
				     + callString
				     + "(\""
				     + methodName + "\", \""
				     + getMethodSignature(e)
				     + "\");",
				     3);
		}
		emitIndented(out, "}", 2);
		out.println();
	}

	private void appendMethodParameterList(StringBuffer methodHeader,
					       Element method) {
		if (method.element("parameters") == null)
			return;

		Iterator parameters =
			method.element("parameters").
				elementIterator("parameter");

		int index = 0;
		while (parameters.hasNext()) {
			Element e = (Element) parameters.next();
			methodHeader.append(getCLRType(e.attributeValue("type")));
			methodHeader.append(" arg" + index++);
			if (parameters.hasNext())
				methodHeader.append(", ");
		}
	}

	/** Caffeine JNI.NET API type names for built-ins.  */
	private final static HashMap API_TYPES = new HashMap();
	static {
		API_TYPES.put("bool",    "Boolean");
		API_TYPES.put("byte",    "Byte");
		API_TYPES.put("sbyte",   "Byte");
		API_TYPES.put("char",    "Char");
		API_TYPES.put("double",  "Double");
		API_TYPES.put("float",   "Float");
		API_TYPES.put("int",     "Int");
		API_TYPES.put("long",    "Long");
		API_TYPES.put("short",   "Short");
		API_TYPES.put("void",    "Void");
	}

	/**
	 * Returns a JNI.NET wrapper call code snippet.
	 *
	 * A variable called <code>ret</code> is defined with the proper type
	 * in the generated C# code if return type is not <code>void</code>.
	 */
	private String wrapperCall(Element method,
				   int methodIndex,
				   String returnType,
				   String apiType) {
		StringBuffer call = new StringBuffer();

		if (!returnType.equals("void")) {
			if (!isTypeArray(returnType))
				call.append(returnType + " ret = ");
			else
				call.append("JObject ret = ");
		}

		boolean useCopyCtor = !returnType.equals("void")
				      && !isTypeArray(returnType)
				      && apiType.equals("Object");

		if (useCopyCtor) {
			call.append("new " + returnType + "(");
		}
		call.append(method.attributeValue("name") + "_mid" + methodIndex + ".Call");
		if (!isTypeArray(returnType))
			call.append(apiType);
		else
			call.append("Object");
	
		call.append("(");
		
		call.append((!isMethodStatic (method) ? "this" : "null"));

		int numParams = 0;
		if (method.element("parameters") != null) {
			numParams = method.element("parameters").
					elements("parameter").size();
		}
		for (int i = 0; i < numParams; i++) {
			call.append(", arg" + i);
		}
		call.append(")");

		if (useCopyCtor)
			call.append(")");
		call.append(";");
		
		return call.toString();
	}

	private String getApiType(String returnType) {
		String ret = null;
		if (!isTypeArray(returnType))
			ret = (String) API_TYPES.get(returnType);
		else
			ret = (String) API_TYPES.get(getBaseType(returnType));

		if (ret == null)
			ret = "Object";
		return ret;
	}

	private String interfaceToImplClass(String type) {
		StringBuffer ret = new StringBuffer();
		
		int firstArraySubscript = type.indexOf('[');
		if (firstArraySubscript == -1) {
			ret.append(type);
			ret.append("JNIImpl");
		} else {
			ret.append(type.substring(0, firstArraySubscript));
			ret.append("JNIImpl");
			ret.append(type.substring(firstArraySubscript));
		}
		return ret.toString();
	}

	private void emitMethodWrapperBody(PrintStream out, Element method, int methodIndex) {
		String returnType = getCLRType(method.element("return-type").attributeValue("type"));
		String apiType = getApiType(returnType);
		
		boolean nonInstantiable =
			method.element("return-type").attributeValue("non-instantiable") != null;

		if (nonInstantiable)
			returnType = interfaceToImplClass(returnType);

		emitIndented(out,
			     wrapperCall(method, methodIndex, returnType,
			     		 apiType),
			     3);

		if (!returnType.equals("void")) {
			if (!isTypeArray(returnType)) {
				emitIndented(out, "return ret;", 3);
			} else {
				String arrayType = "J" + apiType + "Array";
				emitIndented(out, arrayType + " array = new " + arrayType + "(ret);", 3);
				emitIndented(out, "return (" + returnType + ") array.Elements;", 3);
			}
		}
	}

	private String getCtorBaseCallParams(Element ctor) {
		if (ctor.element("parameters") == null)
			return "";

		StringBuffer buf = new StringBuffer();
		int numParams = ctor.element("parameters").elements("parameter").size();

		for (int i = 0; i < numParams; i++) {
			buf.append(", arg" + i);
		}
		return buf.toString();
	}

	private void emitCtor(PrintStream out, String className, Element ctor, int index) {
		StringBuffer ctorHeader = new StringBuffer();
		
		ctorHeader.append("public " + className + "(");
		appendMethodParameterList(ctorHeader, ctor);
		ctorHeader.append(") :");

		emitIndented(out, ctorHeader.toString(), 2);
		emitIndented(out, "base(ctor" + index + getCtorBaseCallParams(ctor) + ")", 3);
		emitIndented(out, "{ }", 2);
		out.println();
	}

	private void emitCtors(PrintStream out, Element clazz, String className) {
		List ctors = clazz.elements("constructor");
		for (int i = 0; i < ctors.size(); i++) {
			Element e = (Element) ctors.get(i);
			emitCtor(out, className, e, i);
		}
	}

	private void emitDefaultCtors(PrintStream out, String className, boolean isImplementation) {
		if (!isImplementation) {
    		emitIndented(out,"protected " + className +
    			     "(JConstructor ctr, params object[] args) : " +
    			     "base(ctr, args) { }", 2);
    		out.println();
		}
		emitIndented(out, "public " + className +
			     "(JObject other) : base(other) { }", 2);
		out.println();
	}

	private void emitSpecialStringCtor(PrintStream out, String className) {
	    emitIndented(out, "public static implicit operator string (java.lang.String s) {", 2);
	    emitIndented(out, "return (new JString((JObject)s)).String;", 3);
	    emitIndented(out, "}", 2);
	    emitIndented(out, "", 2);
	    emitIndented(out, "public static implicit operator java.lang.String (string s) {", 2);
	    emitIndented(out, "JString native = new JString(s);", 3);
	    emitIndented(out, "return new java.lang.String((JObject)native);", 3);
	    emitIndented(out, "}", 2);
	    emitIndented(out, "", 2);
	}

	private void emitClassAccessor(PrintStream out, boolean emitNewModifier) {
		String propertySignature = "public static ";
		if (emitNewModifier)
			propertySignature += "new ";
		propertySignature += "JClass JClass {";
		emitIndented(out, propertySignature, 2);
		emitIndented(out, "get {", 3);
		emitIndented(out, "return clazz;", 4);
		emitIndented(out, "}", 3);
		emitIndented(out, "}", 2);
		out.println();
	}

	private boolean isMethodStatic(Element method) {
		return method.attributeValue("static") != null;
	}
	
	private void emitMethod(PrintStream out,
			String type,
			String className, 
			Element method, 
			int methodIndex, 
			boolean isClassFinal,
			boolean isClassInterface,
			boolean forceNonAbstract) {
		StringBuffer methodHeader = new StringBuffer();
		
		boolean isAbstract = method.attributeValue("abstract") != null;
		boolean isFinal = method.attributeValue("final") != null;
		boolean isOverride = method.attributeValue("override") != null;
		boolean isStatic = method.attributeValue("static") != null;

		if (forceNonAbstract) {
			/*
			 * Don't generate non-abstract methods if we were
			 * asked to forcefully generate abstract methods
			 * because the parent class already has an
			 * implementation for them.
			 */
			if (!isAbstract)
				return;
		}

		if (!isClassInterface || (isClassInterface && forceNonAbstract)) {
			methodHeader.append("public ");
			
			if (isStatic)
				methodHeader.append("static ");
			else {
				if (isAbstract && !forceNonAbstract)
					methodHeader.append("abstract ");
				if (isFinal && isOverride)
					methodHeader.append("sealed ");
				if (isOverride || (isAbstract && forceNonAbstract && !isClassInterface))
					methodHeader.append("override ");
				if (!isFinal && !isAbstract && !isOverride && !isClassFinal)
					methodHeader.append("virtual ");
			}
		}
		
		methodHeader.append(getCLRType(method.element("return-type").attributeValue("type")));
		methodHeader.append(" ");
		
		String methodName = normalize(method.attributeValue("name"));
		if (methodName.equals(className)) {
			// TODO document this fix
			// avoid name class
			// member names cannot be the same as their enclosing type
			System.out.println(
				type + ": method `" + methodName
				+ "' renamed to `"
				+ methodName + "_' because it is same as the "
				+ "class name.");
			methodName = methodName + "_";
		}
		methodHeader.append(methodName);
		
		methodHeader.append("(");
		appendMethodParameterList(methodHeader, method);
		methodHeader.append(")");
		
		if (!forceNonAbstract && (isClassInterface || isAbstract)) {
			methodHeader.append(";");
			emitIndented(out, methodHeader.toString(), 2);
			return;
		}
		
		methodHeader.append(" {");
		emitIndented(out, methodHeader.toString(), 2);
		emitMethodWrapperBody(out, method, methodIndex);
		emitIndented(out, "}", 2);
		out.println();
	}
	
	private void emitMethods(PrintStream out,
				 Element clazz,
				 boolean isClassFinal,
				 boolean isClassInterface,
				 boolean forceNonAbstract) {
		String type = normalize(clazz.attributeValue("name"));
		String className = getClassNameFromType(type);

		List methods = clazz.elements("method");
		for (int i = 0; i < methods.size(); i++) {
			Element e = (Element) methods.get(i);
			boolean isAbstract = e.attributeValue("abstract") != null;
			if (!forceNonAbstract || (forceNonAbstract && isAbstract))
				emitMethod(out, type, className, e, i, isClassFinal,
					   isClassInterface, forceNonAbstract);
		}
	}

	private String getPackageNameFromType(String type) {
		int end = type.lastIndexOf('.');
		if (end == -1)
			return null;
		return type.substring(0, end);
	}

	private String getClassNameFromType(String type) {
		int start = type.lastIndexOf('$');
		if (start == -1)
			start = type.lastIndexOf('.');
		return type.substring(start + 1, type.length());
	}
	
	private final static HashMap KEYWORDS = new HashMap ();
	static {
		KEYWORDS.put ("internal",    "internal_");
		KEYWORDS.put ("ref",         "ref_");
		KEYWORDS.put ("event",       "event_");
		KEYWORDS.put ("as",          "as_");
		KEYWORDS.put ("is",          "is_");
		KEYWORDS.put ("lock",        "lock_");
		KEYWORDS.put ("out",         "out_");
		KEYWORDS.put ("params",      "params_");
		KEYWORDS.put ("object",      "object_");
		KEYWORDS.put ("string",      "string_");
		KEYWORDS.put ("boolean",     "bool");
		KEYWORDS.put ("byte",        "sbyte");
	}
	
	private String normalize(String typeOrMethod) {
		if (typeOrMethod == null)
			return null;

		if (typeOrMethod.length() == 0)
			return "";
		
		String mangledType = (String) mangledTypes.get(typeOrMethod);
		if (mangledType != null)
			return mangledType;
		
		String[] tokens = typeOrMethod.split ("\\.");
		final int nTokens = tokens.length;
		String normalizedType = "";
		for (int i = 0; i < nTokens; i++) {
			String replacement = (String) KEYWORDS.get (tokens[i]);
			if (replacement != null)
				normalizedType += replacement;
			else 
				normalizedType += tokens[i];
			if (i < nTokens -1)
				normalizedType += '.';
		}
		
		return normalizedType.replace('$','.');	
	}

	private void emitInterfaceImplementationClass(PrintStream out,
						      Element clazz,
						      boolean isInterface) {
		String origType = clazz.attributeValue("name");
		String baseClassName = getClassNameFromType(normalize(origType));
		String className = getClassNameFromType(normalize(origType) + "JNIImpl");

		if (isInterface)
			emitIndented(out,
				     "public sealed class "
				     + className + " : java.lang.Object, "
				     + baseClassName + " {",
				     1);
		else
			emitIndented(out,
				     "public sealed class "
				     + className + " : " + baseClassName + " {",
				     1);

		emitIndented(out, "readonly static JClass clazz;", 2);
		emitConstants(out, clazz);
		emitCtorAndMethodIds(out, clazz, className, true);
		emitDefaultCtors(out, className, true);
		
		// abstract class implementation uses parent's JClass property
		// interface class implementation defines new JClass property
		if (isInterface)
			emitClassAccessor(out, true);

		int modifiers = Integer.parseInt(clazz.attributeValue("modifiers"));
		emitMethods(out, clazz, Modifier.isFinal(modifiers), isInterface, true);
		emitIndented(out, "}", 1);
	}

	private void emitInnerClasses(PrintStream out, Element parentClass) {
		List declaredClasses = parentClass.elements("class");
		for (int i = 0; i < declaredClasses.size(); i++) {
			Element clazz = (Element) declaredClasses.get(i);
			emitClassDeclaration(out, clazz, parentClass);
		}
	}
	
	private void emitClassDeclaration(PrintStream out, 
			Element clazz, 
			Element parentClass) {		
		String origType = clazz.attributeValue("name");
		
		// Skip anonymous classes.
		String[] tokens = origType.split("\\$");
		if (tokens != null) {
			for (int i = 0; i < tokens.length; i++) {
				String token = tokens[i];
				if (Character.isDigit ((char) token.charAt (0))) {
					System.out.println(origType + ": skipped anonymous class.");
					return;
				}
			}
		}
		
		out.print("\t");
		int modifiers = Integer.parseInt(clazz.attributeValue("modifiers"));
		
		if (parentClass == null)
			// C# language specification: namespaces only
			// have public and internal types
			if (Modifier.isPublic(modifiers))
				out.print("public ");
			else
				out.print("internal ");
		else
			if (Modifier.isPublic(modifiers))
				out.print("public ");
			else if (Modifier.isProtected(modifiers))
				out.print("protected ");
			else
				out.print("internal ");
				
		boolean isInterface = Modifier.isInterface(modifiers);
		boolean isAbstract  = Modifier.isAbstract(modifiers);

		if (isAbstract && !isInterface) 
			out.print("abstract ");

		if (Modifier.isFinal(modifiers))
			out.print("sealed ");

		if (isInterface)
			out.print("interface ");
		else 
			out.print("class ");
			
		String className = getClassNameFromType(normalize(origType));
		
		out.print(className);
		emitClassBase(out, clazz);
		out.println(" {");

		if (!isInterface) {
			emitIndented(out, "readonly static JClass clazz;", 2);
			emitConstants(out, clazz);
			emitCtorAndMethodIds(out, clazz, className, false);

			/*
			 * If we are dealing with java.lang.String, emit
			 * implicit operators
			 */
			if (ctrIndex > 0) {
				emitSpecialStringCtor(out, className);
				ctrIndex = -1;
			}
			emitCtors(out, clazz, className);
			emitDefaultCtors(out, className, false);
			emitClassAccessor(out, !"java.lang.Object".equals(origType));
		}

		emitMethods(out, clazz,
			    Modifier.isFinal(modifiers),
			    Modifier.isInterface(modifiers),
			    false);
		
		if (!isInterface) {
			emitInnerClasses(out, clazz);
			emitIndented(out, "}", 1);
		} else {
			emitIndented(out, "}", 1);
			emitInnerClasses(out, clazz);
		}

		/*
		 * C# interfaces and abstract classes also need a companion
		 * implementation class so we can pass references around.
		 */
		if (isAbstract || isInterface) {
			out.println();
			emitInterfaceImplementationClass(out, clazz, isInterface);
		}
	}

	private void emitWrapperClass(Element clazz, String dir) {
		try {
			int modifiers = Integer.parseInt(clazz.attributeValue("modifiers"));

			if (Modifier.isPrivate(modifiers))
				return;

			String origType = clazz.attributeValue("name");
						
			// Normalize to avoid clash with C# keywords
			String type = normalize(origType);

			File outFile = new File(dir + File.separatorChar + type + ".cs");
			PrintStream out =
				new PrintStream(
					new BufferedOutputStream(
						new FileOutputStream(outFile))
				);

			emitIndented(out, "using DotNetSystem = System;", 0);
			emitIndented(out, "using Caffeine.Jni;", 0);
			out.println();

			String packageName = normalize(getPackageNameFromType(origType));

			if (packageName != null)
				emitIndented(out, "namespace " + packageName + " {", 0);

			emitClassDeclaration(out, clazz, null);

			if (packageName != null)
				emitIndented(out, "}", 0);

			out.flush();
		} catch (Exception e) {
			throw new RuntimeException(e);
		}
	}
						
	private void generate(Element root, String dir) {
		int count = 0;
		try {	
			Iterator iter = root.elementIterator("class");
			while (iter.hasNext()) {
				Element e = (Element) iter.next();
				emitWrapperClass(e, dir);
				count++;
			}
		} catch (Exception e) {
			throw new RuntimeException(e);
		} finally {
			System.out.println(count +
				" proxy classes generated into directory `"
				+ dir + "'.");
		}
	}
	
	private void mangleType(Element clazz, Element parentClass) {
		int parentModifers = 0;
		if (parentClass != null)
			parentModifers = Integer.parseInt(parentClass.attributeValue("modifiers"));
		
		if (Modifier.isInterface(parentModifers)) {
			String typeName = clazz.attributeValue("name");
			String mangledType = normalize(typeName.replace('$','_'));
			System.out.println("Mangled " + typeName + " as " + mangledType);
			mangledTypes.put(typeName, mangledType);
		}
		
		List declaredClasses = clazz.elements("class");
		for (int i = 0; i < declaredClasses.size(); i++) {
			Element e = (Element) declaredClasses.get(i);
			mangleType(e, clazz);
		}
	}
	
	private final HashMap mangledTypes = new HashMap();
	
	private void populateMangleMap(Element root) {
		try {	
			Iterator iter = root.elementIterator("class");
			while (iter.hasNext()) {
				Element e = (Element) iter.next();
				mangleType (e, null);
			}
		} catch (Exception e) {
			throw new RuntimeException(e);
		}	
	}
	
	public void generate(String filename, String dir) {
		try {
			SAXReader reader = new SAXReader();
			Document apiDocument = reader.read(new File(filename));
			Element root = apiDocument.getRootElement();
			if (root != null && root.getName().equals("api")) {
				populateMangleMap(root);
				generate(root, dir);
			} else {
				throw new RuntimeException(
					filename + " does not contain root "
					+ "element `api'.");
			}
		} catch (Exception e) {
			throw new RuntimeException (e);
		}
	}
	
	private static void usage() {
		System.out.println("CsJniNetWrapperGenerator [filename] [output-dir]");
		System.exit(1);
	}

	public static void main(String[] args) {
		if (args.length != 2)
			usage();
		CsJniNetWrapperGenerator gen = new CsJniNetWrapperGenerator();
		gen.generate(args[0], args[1]);
	}
}
