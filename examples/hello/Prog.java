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
public class Prog {
	public int BarInt;
	public static int BarIntStatic;
	public int[] BarIntArray;
	public Prog BarObject;
	public Prog[] BarObjectArray;
	
	public Prog () {
		System.out.println ("Prog Constructor");
	}
	
	public int max (int a, int b) {
		return (a > b) ? a : b;
	}
	
	public int maxEx () {
		throw new NullPointerException ();
	}
	
	public static void hi () {
		System.out.println ("Hi there!");
	}
	
	public Prog foo () {
		return new Prog ();
	}
	
	public Prog[] fooA () {
		return new Prog[2];
	}
	
	public int fooB (Prog[] arg) {
		return 1;
	}
	
	public int[] fooC () {
		int[] r = {0, 1, 10, 100};
		return r;
	}
	
	public void foo1 () {
	}
	
	public boolean foo2 () {
		return true;
	}
	
	public boolean[] foo2A () {
		return new boolean[2];
	}
	
	public char foo3 () {
		return 'c';
	}
	
	public byte foo4 () {
		return (byte) 1;
	}
	
	public int foo5 () {
		return 1;
	}
	
	public long foo6 () {
		return 1L;
	}
	
	public float foo7 () {
		return 0.0F;
	}
	
	public double foo8 () {
		return 0.0;
	}
	
	public int foo9 (int[] a) {
		return a[0] + a[1];
	}
	
	public char[] foo10 (char[] r) {
		return r;
	}
}
