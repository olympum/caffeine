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
namespace Caffeine.Jni.Examples
{
	using System;
	
	public class HelloWorld
	{	
		public static void Main ()
		{
			try {
				Prog.hi ();
				JThrowable.CheckAndThrow ();
				Prog p = new ProgChild ();
				JThrowable.CheckAndThrow ();
				int r = 0;
				r = p.max (34, 15);
				JThrowable.CheckAndThrow ();
				Console.WriteLine ("Max is {0}", r);
				p.foo ();
				int[] arg = new int[2];
				arg[0] = 1;
				arg[1] = 2;
				r = p.foo9 (arg);
				Console.WriteLine ("foo9 is {0}", r);
				int[] array = p.fooC ();
				foreach (int i in array) {
					Console.WriteLine ("fooC {0}", i);
				}
				p.fooA ();
				Prog[] p2 = new Prog[10];
				r = p.fooB (p2);
				Console.WriteLine ("fooB is {0}", r);
				
				//perf test
				for (int i = 0; i < 100000; i++) 
				{
					p.max (34, 15);
				}
				
				string s1 = "hello world";
				string s2 = new String(p.foo10(s1.ToCharArray()));
				Console.WriteLine ("Strings are equals: {0}", s1.Equals(s2));
				
				// throw an exception
				p.maxEx ();
			
			} catch (Exception e) {
				while (e != null) {
					Console.WriteLine (e);
					e = e.InnerException; 
				}
			}
			Console.WriteLine ("Done!");
		}
	}
}
