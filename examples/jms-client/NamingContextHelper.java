import java.util.Properties;

import javax.naming.Context;
import javax.naming.InitialContext;

public class NamingContextHelper {
	public static Context getContext() throws Throwable {
		try {
			Properties p = new Properties();
			p.put(Context.INITIAL_CONTEXT_FACTORY, "org.jnp.interfaces.NamingContextFactory");
			p.put(Context.URL_PKG_PREFIXES, "jboss.naming:org.jnp.interfaces");
			p.put(Context.PROVIDER_URL, "localhost:1099");	
			return new InitialContext(p);
		} catch (Throwable t) {
			t.printStackTrace();
			throw t;
		}
	}
}
