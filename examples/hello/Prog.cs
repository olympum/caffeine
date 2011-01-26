/* 
 * Copyright (C) 2003 Bruno Fernandez-Ruiz <brunofr@olympum.com>
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
using java.lang;

public class Prog : java.lang.Object
{
	readonly static JClass clazz;
	readonly static JConstructor _ctr;
	readonly static JMethod _max;
	readonly static JMethod _maxEx;
	readonly static JMethod _hi;
	readonly static JMethod _foo;
	readonly static JMethod _fooA;
	readonly static JMethod _fooB;
	readonly static JMethod _fooC;
	readonly static JMethod _foo9;
	readonly static JMethod _foo10;

	static Prog () 
	{
		clazz = JClass.ForName ("Prog");
		_ctr = clazz.GetConstructor ();
		_max = clazz.GetMethod ("max", "(II)I");
		_maxEx = clazz.GetMethod ("maxEx", "()I");
		_hi = clazz.GetStaticMethod ("hi", "()V");
		_foo = clazz.GetMethod ("foo", "()LProg;");
		_fooA = clazz.GetMethod ("fooA", "()[LProg;");
		_fooB = clazz.GetMethod ("fooB", "([LProg;)I");
		_fooC = clazz.GetMethod ("fooC", "()[I");
		_foo9 = clazz.GetMethod ("foo9", "([I)I");
		_foo10 = clazz.GetMethod ("foo10", "([C)[C");
	}
	
	public Prog () : base (_ctr)
	{
	}
	
	protected Prog (JConstructor ctr, params object[] args) : base (ctr, args)
	{
	}
	
	protected Prog (JObject other) : base (other)
	{
	}
	
	public new static JClass JClass {
		get {
			return clazz;
		}
	}

	public virtual int max (int a, int b) 
	{
		return _max.CallInt (this, a, b);
	}
	
	public virtual int maxEx () 
	{
		return _maxEx.CallInt (this);
	}
	
	public static void hi () 
	{
		_hi.CallVoid (null);
	}
	
	public virtual Prog foo ()
	{
		return new Prog (_foo.CallObject (this));
	}
	
	public virtual Prog[] fooA ()
	{
		JObject o = _fooA.CallObject (this);
		JObjectArray array = new JObjectArray (o);
		JObject[] oArray = (JObject[]) array.Elements;
		int l = array.Length;
		Prog[] r = new Prog[l];
		for (int i = 0; i < l; i++) {
			r[i] = new Prog (oArray[i]);
		}
		return r;
	}
	
	public virtual int fooB (Prog[] arg)
	{
		JObjectArray array = new JObjectArray (arg, Prog.JClass);
		return _fooB.CallInt (this, array);
	}
	
	public virtual int[] fooC ()
	{
		JObject r = _fooC.CallObject (this);
		JIntArray array = new JIntArray (r);
		return (int[]) array.Elements;
	}
	
	public virtual int foo9 (int[] arg)
	{
		return _foo9.CallInt (this, arg);
	}
	
	public virtual char[] foo10 (char[] str) {
		JObject r = _foo10.CallObject(this, str);
		JCharArray array = new JCharArray (r);
		return (char[]) array.Elements;
	}
}

public class ProgChild : Prog
{
}
