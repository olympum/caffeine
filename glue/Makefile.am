lib_LTLIBRARIES = libjninet.la

libjninet_la_LDFLAGS = -module -avoid-version -no-undefined

libjninet_la_SOURCES = 	\
	jvm.c

INCLUDES = -I ${JAVAHOME}/include/ -I ${JAVAHOME}/include/${JNI_PLATFORM}/

CLEANFILES = lib*.a lib*.dll
