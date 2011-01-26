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
using System;
using Caffeine.Jni;
//using java.lang;

// TODO: extend java.lang.Object
public abstract class Template : JObject 
{
	readonly static JClass clazz;
	readonly static JMethod _getInstance;
	readonly static JMethod _foo;

	static Template () 
	{
		clazz = JClass.ForName ("Template");
		_getInstance = clazz.GetStaticMethod ("getInstance", "()LTemplate;");
		_foo = clazz.GetMethod ("foo", "(LTemplate;)V");
	}
	
	protected Template (JObject other) : base (other)
	{
	}
	
	public static JClass JClass {
		get {
			return clazz;
		}
	}

	public static Template getInstance () 
	{
		Template ret = 
			new TemplateJNIImpl (_getInstance.CallObject (null));
		return ret;
	}
	
	public void foo (Template t) {
		_foo.CallVoid (this, t);
	}
	
	public abstract void bar ();	
}

public class TemplateJNIImpl : Template
{
	readonly static JClass clazz;
	readonly static JMethod _bar;

	static TemplateJNIImpl () 
	{
		clazz = JClass.ForName ("Template");
		_bar = clazz.GetMethod ("bar", "()V");
	}
	
	public TemplateJNIImpl (JObject other) : base (other)
	{
	}
	
	public override void bar ()
	{
		_bar.CallVoid (this);
	}
}
