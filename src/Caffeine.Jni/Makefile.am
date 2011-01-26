ASSEMBLY=$(top_srcdir)/lib/Caffeine.Jni.dll
SN_KEY=caffeine.snk
noinst_DATA = $(ASSEMBLY)
EXTRA_DIST = $(caffeine_sources) AssemblyInfo.cs.in
CLEANFILES = $(ASSEMBLY) $(SN_KEY)
DISTCLEANFILES = AssemblyInfo.cs
PACKAGE = @PACKAGE@

sources = \
	JNIEnv.cs \
	JNISectionHandler.cs \
	JArray.cs \
	JBooleanArray.cs \
	JByteArray.cs \
	JCharArray.cs \
	JClass.cs \
	JConstructor.cs \
	JDoubleArray.cs \
	JField.cs \
	JFloatArray.cs \
	JIntArray.cs \
	JLongArray.cs \
	JMember.cs \
	JMethod.cs \
	JObject.cs \
	JObjectArray.cs \
	JShortArray.cs \
	JString.cs \
	JThrowable.cs \
	JValue.cs \
	JWeak.cs \
	JavaVM.cs

caffeine_sources = $(addprefix $(srcdir)/, $(sources))
caffeine_build_sources = $(caffeine_sources) AssemblyInfo.cs

$(SN_KEY): $(top_srcdir)/$(SN_KEY)
	cp $(top_srcdir)/$(SN_KEY) .

$(ASSEMBLY): $(caffeine_build_sources) $(SN_KEY)
	$(MCS) /t:library $(mcs_flags) /out:$@ $(caffeine_build_sources)
