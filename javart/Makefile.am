ASSEMBLY=$(top_srcdir)/lib/javart.dll
noinst_DATA = $(ASSEMBLY)
CLEANFILES = $(ASSEMBLY) rt.xml
PACKAGE = @PACKAGE@
# TODO treat this warning as error
mcs_flags=-nowarn:0114,0108

if WIN32
generated-timestamp:
	mkdir -p src
	CURDIR=`pwd`
	cd $(top_srcdir)/lib/; ./caffeinator.bat `cygpath -w $(JAVAHOME)/jre/lib/rt.jar` `cygpath -w $(CURDIR)/rt.xml` `cygpath -wp $(CURDIR)/src`
	touch generated-timestamp
else
generated-timestamp:
	mkdir -p src
	$(top_srcdir)/lib/caffeinator.sh $(JAVAHOME)/jre/lib/rt.jar rt.xml ./src -Xmx512M
	touch generated-timestamp
endif

# TODO strong sign assembly
if WIN32
$(ASSEMBLY): generated-timestamp sources
	$(MCS) /r:$(top_srcdir)/lib/Caffeine.Jni.dll \
	/t:library $(mcs_flags) /out:$@ `cygpath -wp -f sources`
else
$(ASSEMBLY): generated-timestamp sources
	$(MCS) /r:$(top_srcdir)/lib/Caffeine.Jni.dll \
	/t:library $(mcs_flags) /out:$@ `cat sources`
endif

clean-local:
	rm -fr src
	rm -f rt.xml
	rm -f generated-timestamp
