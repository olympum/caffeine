EXECUTABLE = HelloWorld.exe
TARGETS = $(EXECUTABLE) Prog.class
noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS)
EXTRA_DIST = $(build_sources) Prog.java $(EXECUTABLE).config

%.class: %.java
	javac $<

sources = \
	Prog.cs \
	HelloWorld.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

$(EXECUTABLE): $(build_sources)
	$(MCS) /r:$(top_srcdir)/lib/Caffeine.Jni.dll \
	/r:$(top_srcdir)/lib/javart.dll $(mcs_flags) \
	/out:$@ $(build_sources)

run: $(EXECUTABLE)
	export MONO_PATH=$(top_srcdir)/lib/; \
	export LD_LIBRARY_PATH=$(JAVA_HOME)/jre/lib/i386:$(JAVA_HOME)/jre/lib/i386/server:../../glue/.libs/; \
	$(RUNTIME) $(EXECUTABLE)
