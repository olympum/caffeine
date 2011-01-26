public class Base
{
	public class Inner {
	}
}

public class Derived : Base
{
	new public class Inner : Base.Inner
	{
	}
}
