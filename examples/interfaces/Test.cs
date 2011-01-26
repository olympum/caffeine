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

public class Test : JObject // TODO should be java.lang.Object
{
	readonly static JClass clazz;
	readonly static JConstructor _ctr;
	readonly static JMethod _foo;
	readonly static JMethod _bar;

	static Test () 
	{
		clazz = JClass.ForName ("Test");
		_ctr = clazz.GetConstructor ();
		_foo = clazz.GetMethod ("foo", "(LControllable;)V");
		_bar = clazz.GetMethod ("bar", "()LControllable;");
	}
	
	public Test () : base (_ctr)
	{
	}
	
	protected Test (JConstructor ctr, params object[] args) : base (ctr, args)
	{
	}
	
	protected Test (JObject other) : base (other)
	{
	}
	
	public static JClass JClass {
		get {
			return clazz;
		}
	}

	public virtual void foo (Controllable c) 
	{
		_foo.CallVoid (this, c);
	}
	
	public Controllable bar ()
	{
		Controllable ret = new ControllableJNIImpl (_bar.CallObject(this));
		return ret;
	}
}