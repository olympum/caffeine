/*
 * Copyright (C) 2003 Pekka Enberg <penberg@iki.fi>
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
import java.io.IOException;
import java.io.PrintStream;
import java.lang.reflect.Constructor;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.lang.reflect.Modifier;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.ArrayList;
import java.util.Enumeration;
import java.util.HashMap;
import java.util.jar.JarEntry;
import java.util.jar.JarFile;

/**
 * Java API XML generator.
 *
 * Generates API XML description of a Java API.
 */
class JavaApiXmlGenerator {
	private boolean isJavaClass(String filename) {
		/* Ignore inner classes here. The emitClass method can query
		   for them later.  */
		return filename.endsWith(".class") && 
			filename.indexOf('$') < 0;
	}

	private String getClassNameFromFilename(String filename) {
		return filename
			.substring(0, filename.length() - 6)
				.replace(File.separatorChar, '.');
	}
	
	private void emitMethodModifier(PrintStream out, String modifier) {
		out.print( modifier + "=\"true\" ");
	}

	private void emitMethodModifiers(PrintStream out, int modifiers, boolean override) {
		/* The following modifiers are ignored: native, strict,
		   synchronized, transitient, and volatile. The CLR does not
		   need to worry about them as the JVM will already take care
		   of them (eg. thread synchronization). Visiblity modifiers
		   are taken care of elsewhere.  */
		if (Modifier.isStatic(modifiers))
			emitMethodModifier(out, "static");
		if (Modifier.isAbstract(modifiers))
			emitMethodModifier(out, "abstract");
		if (Modifier.isFinal(modifiers))
			emitMethodModifier(out, "final");
		if (override)
			emitMethodModifier(out, "override");
	}

	// native types
	private final static HashMap NATIVE_MAP= new HashMap ();
	static {
		NATIVE_MAP.put("Z", 	"boolean");
		NATIVE_MAP.put("B", 	"byte");
		NATIVE_MAP.put("C", 	"char");
		NATIVE_MAP.put("S", 	"short");
		NATIVE_MAP.put("I", 	"int");
		NATIVE_MAP.put("J",	"long");
		NATIVE_MAP.put("F", 	"float");
		NATIVE_MAP.put("D", 	"double");
	}
	
	private Class getNativeType (Class arrayClass) {
		Class type = (Class) NATIVE_MAP.get (arrayClass);
		return (type != null) ? type : arrayClass.getComponentType ();
	}
	
	private String getTypeString(Class clazz) {
		if (!clazz.isArray())
			return clazz.getName();
		
		Class componentType = clazz.getComponentType();
		int dimensions = 1;
		while (componentType.getComponentType() != null) {
			dimensions++;
			componentType = componentType.getComponentType();
		}
		String typeName = (String) NATIVE_MAP.get(componentType.getName());
		if (typeName == null)
			typeName = componentType.getName();
		
		for (int i = 0; i < dimensions; i++) {
			typeName += "[]";
		}
		
		return typeName;	
	}

	private void emitParentClass(PrintStream out, Class superclass) {
		if (superclass == null)
			return;
		if (Modifier.isPrivate(superclass.getModifiers()))
			return;
			
		out.print(" parent=\"" + getTypeString(superclass) + "\"");
	}

	private void emitImplements(PrintStream out, Class[] interfaces) {
		if (interfaces.length == 0)
			return;
		
		boolean emitted = false;
		
		int i;
		for (i = 0; i < interfaces.length; i++) {
			Class iface = interfaces[i];
			if (!Modifier.isPublic (iface.getModifiers ()))
				continue;
			if (!emitted) {
				out.println("<implements>");
				emitted = true;
			}
			out.println("<interface name=\"" + 
				getTypeString(interfaces[i]) + "\"/>");
		}
		if (emitted)
			out.println("</implements>");
	}

	private void emitConstant(PrintStream out, Field field) {
		Class fieldType = field.getType();

		/*
		 * TODO: object constants are not supported.
		 */
		if (!fieldType.isPrimitive())
			return;

		String value = null;
		try {
			if ("boolean".equals(fieldType.getName())) {
				value = Boolean.toString(field.getBoolean(null));
			} else if ("double".equals(fieldType.getName())
				   || "float".equals(fieldType.getName())) {
				value = Double.toString(field.getDouble(null));
			} else {
				value = Long.toString(field.getLong(null));
			}
		} catch (IllegalAccessException e) {
			throw new RuntimeException(e);
		}
		out.print("<constant ");
		out.print("name=\"" + field.getName() + "\" ");
		out.print("type=\"" + field.getType() + "\" ");
		out.print("value=\"" + value + "\"");
		out.println("/>");
	}

	private void emitConstants(PrintStream out, Field[] fields) {
		for (int i = 0; i < fields.length; i++) {
			int modifiers = fields[i].getModifiers();
			if (Modifier.isPublic(modifiers)
			    && Modifier.isStatic(modifiers)
			    && Modifier.isFinal(modifiers)) {
				emitConstant(out, fields[i]);
			}
		}
	}

	private void emitConstructor(PrintStream out,
				     Constructor constructor) {
		out.println("<constructor>");
		emitMethodParameters(out, constructor.getParameterTypes());
		out.println("</constructor>");
	}

	private void emitConstructors(PrintStream out,
				      Constructor[] constructors) {
		for (int i = 0; i < constructors.length; i++) {
			if (Modifier.isPublic(constructors[i].getModifiers())) {
				emitConstructor(out, constructors[i]);
			}
		}
	}

	private void emitMethodParameters(PrintStream out, Class[] params) {
		if (params.length == 0)
			return;
		out.println("<parameters>");
		for (int i = 0; i < params.length; i++) {
			out.println("<parameter type=\"" + getTypeString(params[i]) + "\"/>");
		}
		out.println("</parameters>");
	}
	
	private void emitThrows(PrintStream out, Class[] exceptionTypes) {
		if (exceptionTypes.length == 0)
			return;
		out.println("<exceptions>");
		final int length = exceptionTypes.length;
		for (int i = 0; i < length; i++) {
			out.println("<throws type=\"" + exceptionTypes[i].getName() + "\"/>");
		}
		out.println("</exceptions>");
	}

	private boolean hasAbstractMethods(Class clazz) {
		Method[] methods = clazz.getMethods();
		for (int i = 0; i < methods.length; i++) {
			if (Modifier.isAbstract(methods[i].getModifiers()))
				return true;
		}
		return false;
	}

	private Class getNonArrayType(Class type) {
		Class ret = type;
		while (ret.isArray())
			ret = ret.getComponentType();
		return ret;
	}

	private void emitMethod(PrintStream out, Method method, boolean overwritten) {
		out.print("<method name=\"" + method.getName() + "\" ");
		emitMethodModifiers(out, method.getModifiers(), overwritten);
		out.println(">");

		Class returnType = method.getReturnType();
		out.print("<return-type ");
		out.print("type=\"" + getTypeString(returnType) + "\"");

		Class nonArrayType = getNonArrayType(returnType);
		if (!nonArrayType.isPrimitive()
		    && (nonArrayType.isInterface()
		        || hasAbstractMethods(nonArrayType)
		        || Modifier.isAbstract(nonArrayType.getModifiers())))
			out.print(" non-instantiable=\"true\"");

		out.println("/>");
		emitMethodParameters(out, method.getParameterTypes());
		emitThrows (out, method.getExceptionTypes());
		out.println("</method>");
	}

	private void emitMethods(PrintStream out, Method[] methods, boolean overwritten) {
		for (int i = 0; i < methods.length; i++) {
			if (Modifier.isPublic(methods[i].getModifiers())) {
				emitMethod(out, methods[i], overwritten);
			}
		}
	}

	private void emitClassModifiers(PrintStream out, ClassInfo clazz) {
		out.print("modifiers=\"");
		out.print(clazz.modifiers);
		out.print("\"");
	}
	
	private void emitClass(PrintStream out, ClassInfo clazz, ClassLoader loader) {
		out.print("<class name=\"");
		out.print(clazz.clazz.getName() + "\"");
		if (clazz.clazz == Object.class) {
			System.out.println("java.lang.Object: emitted as `JObject'.");
			out.print(" parent=\"Caffeine.Jni.JObject\"");
		} else {			
			emitParentClass(out, clazz.superclass);
		}
		out.print(" ");
		emitClassModifiers(out, clazz);
		out.println(">");
		emitImplements(out, clazz.interfaces);

		if (Modifier.isPublic(clazz.clazz.getModifiers()))
			emitConstants(out, clazz.fields);

		emitConstructors(out, clazz.ctors);
		emitMethods(out, clazz.newMethods, false);
		emitMethods(out, clazz.overwrittenMethods, true);
		emitDeclaredClasses (out, clazz.clazz.getDeclaredClasses(), loader);
		out.println("</class>");
	}
	
	private void emitDeclaredClasses(PrintStream out, Class[] classes, ClassLoader loader) {
		final int nClasses = classes.length;
		for (int i = 0; i < nClasses; i++) {
			String className = classes[i].getName ();
			ClassInfo clazz = getClassFromName (className, loader);
			if (Modifier.isPublic(clazz.modifiers) ||
				Modifier.isProtected(clazz.modifiers)) 
				emitClass(out, clazz, loader);
		}
	}

	private static class ClassInfo {
		public Class clazz;
		public Class superclass;
		public Class[] interfaces;
		public Constructor[] ctors;
		public Method[] newMethods;
		public Method[] overwrittenMethods;
		public Field[] fields;
		public int modifiers;

		private ClassInfo(Class clazz,
				  Class superclass,
				  Class[] interfaces,
				  Constructor[] ctors,
				  Method[] newMethods,
				  Method[] overwrittenMethods,
				  Field[] fields,
				  int modifiers) {
			this.clazz = clazz;
			this.superclass = superclass;
			this.interfaces = interfaces;
			this.ctors = ctors;
			this.newMethods = newMethods;
			this.overwrittenMethods = overwrittenMethods;
			this.fields = fields;
			this.modifiers = modifiers;
		}

		public static ClassInfo loadClass(String name, ClassLoader loader)
		    throws ClassNotFoundException {
			/* Reflect everything here so we can catch
			   ClassDefNotFoundError in one place.  */
			Class clazz = loader.loadClass(name.replace('/','.'));
			Class superclass = clazz.getSuperclass();
			Class[] interfaces = clazz.getInterfaces();
			Constructor[] ctors = clazz.getDeclaredConstructors();
			Field[] fields = clazz.getDeclaredFields();

			Method[] nMethods = null;
			Method[] oMethods = null;
			if (superclass != null) {
				Method[] methods = clazz.getDeclaredMethods();				
				Method[] parentMethods = superclass.getMethods();
				ArrayList overwrittenMethods = new ArrayList ();
				ArrayList newMethods = new ArrayList ();
				final int length = methods.length;
				final int pLength = parentMethods.length;
				for (int i = 0; i < length; i++) {
					Method m = methods[i];
					String methodName = m.getName ();
					boolean overwritten = false;
					for (int j = 0; j < pLength; j++) {
						Method p = parentMethods[j];
						String parentMethodName = p.getName ();
						if (methodName.equals(parentMethodName)) {
							Class[] thisTypes = m.getParameterTypes();
							Class[] parentTypes = p.getParameterTypes();
							if (areEqual(thisTypes, parentTypes)) {
								overwritten = true;
								break;
							}
						}
					}
					if (overwritten)
						overwrittenMethods.add (m);
					else
						newMethods.add (m);
				}
				
				oMethods = new Method[overwrittenMethods.size()];
				nMethods = new Method[newMethods.size()];
				overwrittenMethods.toArray (oMethods);
				newMethods.toArray (nMethods);
			} else {
				nMethods = clazz.getMethods();
				oMethods = new Method[0];
			}
			
			int modifiers = clazz.getModifiers ();
			return new ClassInfo(clazz, 
				superclass, 
				interfaces, 
				ctors, 
				nMethods,
				oMethods,
				fields,
				modifiers);
		}
		
		private static boolean areEqual(Class[] thisTypes, Class[] parentTypes) {
			int numberTypes = thisTypes.length;
			if (numberTypes != parentTypes.length)
				return false;
			
			for (int i = 0; i < numberTypes; i++) {
				Class c1 = thisTypes[i];
				Class c2 = parentTypes[i];
				if (c1 != c2)
					return false;
			}
			
			return true;
		}
	}

	private ClassInfo getClassFromName(String name, ClassLoader loader) {
		ClassInfo ret = null;
		try {
			ret = ClassInfo.loadClass(name, loader); 
		} catch (NoClassDefFoundError e) {
			/* Not fatal. We can continue.  */
			System.out.print("Warning: generation failed for " + name);
			System.out.println(". Reason: " + e + ".");
		} catch (Exception e) {
			throw new RuntimeException(e);
		}
		return ret;
	}

	private void emitHeader(PrintStream out) {
		out.println("<?xml version=\"1.0\"?>");
		out.println("<api>");
	}

	private void emitFooter(PrintStream out) {
		out.println("</api>");
	}

	private void printSummary(String filename, int numGenerated, int numFailed) {
		System.out.print(filename + ": ");
		System.out.print(numGenerated + " class definitions generated.");
		if (numFailed > 0)
			System.out.print("  " + numFailed + " failed.");
		System.out.println();
	}

	private boolean isJarEntryJavaClass(JarEntry entry) {
		return !entry.isDirectory() && isJavaClass(entry.getName());
	}

	public void generate(String[] args) throws IOException {
		ClassLoader classLoader = new URLClassLoader(
			new URL[] {
				new File(args[0]).toURL()
			}
		);
		File outFile = new File(args[1]);
		PrintStream out =
			new PrintStream(
				new BufferedOutputStream(
					new FileOutputStream(outFile))
			);
		emitHeader(out);

		int numGenerated = 0;
		int numFailed = 0;

		JarFile jar = new JarFile(args[0]);
		Enumeration entries = jar.entries();
		while (entries.hasMoreElements()) {
			JarEntry entry = (JarEntry) entries.nextElement();
			if (isJarEntryJavaClass(entry)) {
				ClassInfo clazz =
					getClassFromName(
						getClassNameFromFilename(
							entry.getName()),
						classLoader);
				if (clazz != null) {
					emitClass(out, clazz, classLoader);
					numGenerated++;
				} else {
					numFailed++;
				}
			}
		}
		emitFooter(out);
		out.flush();

		printSummary(args[1], numGenerated, numFailed);
	}

	private static void usage() {
		System.out.println("com.olympum.tools.JavaApiXmlGenerator: " +
				   "<jar-file> <api-xml>");
		System.exit(1);
	}

	public static void main(String[] args) {
		if (args.length != 2)
			usage();
		try {
			JavaApiXmlGenerator gen = new JavaApiXmlGenerator();
			gen.generate(args);
		} catch (Exception e) {
			throw new RuntimeException(e);
		}
	}
}
