ASSEMBLY=$(top_srcdir)/lib/caffeinator.jar
noinst_DATA = $(ASSEMBLY)
EXTRA_DIST = $(caffeine_sources)
CLEANFILES = $(ASSEMBLY)

sources = \
	com/olympum/tools/CsJniNetWrapperGenerator.java \
	com/olympum/tools/JavaApiXmlGenerator.java

caffeine_sources = $(addprefix $(srcdir)/, $(sources))
caffeine_build_sources = $(caffeine_sources)

classes:
	mkdir classes

$(ASSEMBLY): $(caffeine_build_sources) classes
	$(JAVAC) -classpath "$(top_srcdir)/lib/dom4j.jar" \
	-d classes $(caffeine_build_sources)
	jar cfv $@ -C classes/ .

clean-local:
	rm -fr classes
