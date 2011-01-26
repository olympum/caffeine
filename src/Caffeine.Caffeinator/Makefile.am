ASSEMBLY=$(top_srcdir)/lib/caffeinator.exe
SN_KEY=caffeine.snk
noinst_DATA = $(ASSEMBLY)
EXTRA_DIST = $(caffeine_sources) AssemblyInfo.cs.in
CLEANFILES = $(ASSEMBLY) $(SN_KEY)
DISTCLEANFILES = AssemblyInfo.cs
PACKAGE = @PACKAGE@

sources = \
	Class.cs \
	ClassFactory.cs \
	ClassFile.cs \
	Descriptor.cs \
	Field.cs \
	JarFile.cs \
	Main.cs \
	Method.cs \
	PEemitter.cs \
	TypeGenerator.cs

caffeine_sources = $(addprefix $(srcdir)/, $(sources))
caffeine_build_sources = AssemblyInfo.cs $(caffeine_sources)

$(SN_KEY): $(top_srcdir)/$(SN_KEY)
	cp $(top_srcdir)/$(SN_KEY) .

$(ASSEMBLY): $(caffeine_build_sources) $(SN_KEY)
	$(MCS) /r:$(top_srcdir)/lib/ICSharpCode.SharpZipLib.dll \
	/r:$(top_srcdir)/lib/Mono.GetOptions.dll \
	/t:exe /d:DEBUG -g $(mcs_flags) /out:$@ $(caffeine_build_sources)

install-data-local:
	$(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1 ;

uninstall-local:
	$(GACUTIL) /u $$(basename $(ASSEMBLY) .dll) /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1 ;
