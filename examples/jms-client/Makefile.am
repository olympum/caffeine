EXECUTABLE = JMSSender.exe
ASSEMBLY = jms.dll
JAR_FILE = jms.jar
TARGETS = $(EXECUTABLE) $(ASSEMBLY)
noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS) $(JAR_FILE) generated-stamp
EXTRA_DIST = $(csharp_build_sources) $(java_build_sources) $(EXECUTABLE).config javax.jms.jar

csharp_sources = \
	JMSSender.cs

java_sources = \
	NamingContextHelper.java

csharp_build_sources = $(addprefix $(srcdir)/, $(csharp_sources))
java_build_sources =  $(addprefix $(srcdir)/, $(java_sources))
assembly_references = \
	/r:$(top_srcdir)/lib/Caffeine.Jni.dll \
	/r:$(top_srcdir)/lib/javart.dll 

$(EXECUTABLE): $(csharp_build_sources) $(ASSEMBLY)
	$(MCS) $(assembly_references) /r:$(ASSEMBLY) $(mcs_flags) /out:$@ $(csharp_build_sources)

if WIN32
$(ASSEMBLY): generated-stamp
	$(MCS) $(mcs_flags) $(assembly_references) /t:library /out:$@ `cygpath -wp generated/*.cs`
else
$(ASSEMBLY): generated-stamp
	$(MCS) $(mcs_flags) $(assembly_references) /t:library /out:$@ generated/*.cs
endif

run: $(EXECUTABLE)
	export MONO_PATH=$(top_srcdir)/lib; \
	export LD_LIBRARY_PATH=$(JAVA_HOME)/jre/lib/i386:$(JAVA_HOME)/jre/lib/i386/server:../../glue/.libs/; \
	$(RUNTIME) $(EXECUTABLE)

if WIN32
generated-stamp: generated $(JAR_FILE)
	CURDIR=`pwd`
	cd $(top_srcdir)/lib/; ./caffeinator.bat `cygpath -wp $(CURDIR)/$(JAR_FILE)` `cygpath -wp $(CURDIR)/$(JAR_FILE).xml` `cygpath -wp $(CURDIR)/generated`
	rm -f generated/*XA*
	touch generated-stamp
else
generated-stamp: generated $(JAR_FILE)
	$(top_srcdir)/lib/caffeinator.sh $(JAR_FILE) $(JAR_FILE).xml generated
	rm -f generated/*XA*
	touch generated-stamp
endif

$(JAR_FILE): $(java_build_sources) .build
	cp javax.jms.jar jms.jar
	$(JAVAC) -d .build $(java_build_sources)
	$(JAR) uf $@ -C .build . > /dev/null

generated:
	mkdir generated

.build:
	mkdir .build

clean-local:
	rm -rf .build
	rm -rf generated
	rm -f $(JAR_FILE).xml
