if ENABLE_MONODOC
TARGETS = caffeine-docs.zip caffeine-docs.tree caffeine-docs.source
DOCDIR = `$(MONODOC) --get-sourcesdir`
else
TARGETS =
DOCDIR = $(datadir)
endif

noinst_DATA = $(TARGETS)

ASSEMBLER = $(MONODOC) --assemble
UPDATER = $(MONODOC) --update

DIRS = src/Caffeine.Jni

caffeine-docs.zip caffeine-docs.tree: $(srcdir)/en/*/*.xml $(srcdir)/en/*.xml
	$(ASSEMBLER) --ecma $(srcdir)/en -o caffeine-docs

update:
	for i in $(DIRS); do                                    \
		$(UPDATER) ../$$i/*.dll -o ./en -f || exit 1;   \
	done

CLEANFILES = caffeine-docs.zip caffeine-docs.tree

EXTRA_DIST =                    \
	caffeine-docs.source

dist-hook:
	mkdir -p $(distdir)/en
	cp $(srcdir)/en/*.xml $(distdir)/en/
	mkdir -p $(distdir)/en/Caffeine.Jni
	cp $(srcdir)/en/Caffeine.Jni/*.xml $(distdir)/en/Caffeine.Jni/
