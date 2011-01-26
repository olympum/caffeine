/* 
 * Copyright (C) 2003, 2004 Bruno Fernandez-Ruiz <brunofr@olympum.com>
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation files
 * (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software,
 * and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
 * BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
 * ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <assert.h>
#include <jni.h>
#ifdef _JNI_PLATFORM_WIN32
#include <windows.h>
#else
#include <dlfcn.h>
#endif

#define MAX_ERROR_BUFFER_LENGTH 1024
static char error_buffer[MAX_ERROR_BUFFER_LENGTH];

const char * 
GetError()
{
	return error_buffer;
}

void
Error(const char * format, ...)
{
	va_list args;

	va_start(args, format);
#ifdef _JNI_PLATFORM_WIN32
	_vsnprintf(error_buffer, MAX_ERROR_BUFFER_LENGTH, format, args);
#else
	vsnprintf(error_buffer, MAX_ERROR_BUFFER_LENGTH, format, args);
#endif
	va_end(args);
		
	fprintf(stderr, "Error: %s\n", error_buffer);
}

/* the one and only reference to the JVM */
JavaVM *jvm;

#ifdef _JNI_PLATFORM_WIN32
typedef HINSTANCE dllinst_t;
#else
typedef void * dllinst_t;
#endif

dllinst_t
OpenDLL(const char * dllName)
{
#ifdef _JNI_PLATFORM_WIN32
	return LoadLibrary(dllName);
#else
	return dlopen(dllName, RTLD_LAZY);
#endif
}

void *
GetFunction(dllinst_t libVM, const char * name)
{
#ifdef _JNI_PLATFORM_WIN32
	return (void *) GetProcAddress(libVM, name);
#else
	return dlsym(libVM, name);
#endif
}

int
CreateJavaVMDLL(const char *dllName, JavaVMOption * options, int nOptions)
{
	JNIEnv *env;
	jint res = 0;
	JavaVMInitArgs vm_args;
	int (*create_java_vm) ();
	dllinst_t libVM;

	fprintf(stdout, "Using %s as embedded Java VM DLL\n", dllName);
	if (jvm != NULL) {
		/* Only support one JVM per process */
		return 0;
	}

	libVM = OpenDLL(dllName);
	if (libVM == NULL) {
		Error("dlopen() could not open %s", dllName);
		return -2;
	}

        *(void **) (&create_java_vm) = GetFunction(libVM, "JNI_CreateJavaVM");
	if (create_java_vm == NULL) {
		Error("Could not find symbol JNI_CreateJavaVM in %s", dllName);
		return -3;
	}

	vm_args.version = JNI_VERSION_1_2;
	vm_args.options = options;
	vm_args.nOptions = nOptions;
	vm_args.ignoreUnrecognized = JNI_TRUE;

	res = (*create_java_vm) (&jvm, (void **) &env, &vm_args);

	if (res < 0) {
		Error("JNI_CreateJavaVM returned %i", res);
	}

	return res;
}

int
CreateJavaVMAnon(JavaVMOption * options, int nOptions)
{
#ifdef _JNI_PLATFORM_WIN32
	const char *dllName = "jvm.dll";
#else
	const char *dllName = "libjvm.so";
#endif
	return CreateJavaVMDLL(dllName, options, nOptions);
}

jint
DestroyJavaVM()
{
	int r = 0;
	fprintf(stdout, "Destroying JavaVM\n");
	r = (*jvm)->DestroyJavaVM(jvm);
	fprintf(stdout, "Destroyed JavaVM (%d)\n", r);
	return r;
}

/** The JNIEnv is an interface pointer to thread-local data.
 * It is illegal to reuse it across multiple calls from the 
 * CLR (even in non-multithreaded applications, as the finalizer 
 * thread could eventually use a JNIEnv to which is not attached). 
 * Each JNI call via P/Invoke must first obtain the JNIEnv
 * associated with the current thread, or if the current thread 
 * does not have a JNIEnv, it must attach the current thread to 
 * a JNIEnv.
 */
JNIEnv *
GetEnv()
{
	JNIEnv *env;
	jint res;

	assert(jvm != NULL);
	res = (*jvm)->GetEnv(jvm, (void **) &env, JNI_VERSION_1_2);
	if (res == JNI_EVERSION) {
		Error("JRE 1.1 not supported!");
	} else if (res == JNI_EDETACHED) {
		res = (*jvm)->AttachCurrentThread(jvm, (void **) &env, NULL);
		if (res < 0) {
			Error("Can't attach current thread");
		}
	}

	return env;
}

JNIEnv *
AttachCurrentThread()
{
	JNIEnv *env;
	jint res;
	res = (*jvm)->AttachCurrentThread(jvm, (void **) &env, NULL);
	if (res < 0) {
		Error("Can't attach current thread");
	}

	return env;
}

int
DetachCurrentThread()
{
	jint res;
	res = (*jvm)->DetachCurrentThread(jvm);
	if (res < 0) {
		Error("Can't detach current thread");
	}

	return res;
}

/* all the functions below are 1:1 mappings */

jclass
FindClass(const char *name)
{
	JNIEnv *env = GetEnv();
	return (*env)->FindClass(env, name);
}

jmethodID
FromReflectedMethod(jobject method)
{
	JNIEnv *env = GetEnv();
	return (*env)->FromReflectedMethod(env, method);
}

jfieldID
FromReflectedField(jobject field)
{
	JNIEnv *env = GetEnv();
	return (*env)->FromReflectedField(env, field);
}

jobject
ToReflectedMethod(jclass cls, jmethodID methodID, jboolean isStatic)
{
	JNIEnv *env = GetEnv();
	return (*env)->ToReflectedMethod(env, cls, methodID, isStatic);
}

jclass
GetSuperclass(jclass sub)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetSuperclass(env, sub);
}

jboolean
IsAssignableFrom(jclass sub, jclass sup)
{
	JNIEnv *env = GetEnv();
	return (*env)->IsAssignableFrom(env, sub, sup);
}

jobject
ToReflectedField(jclass cls, jfieldID fieldID, jboolean isStatic)
{
	JNIEnv *env = GetEnv();
	return (*env)->ToReflectedField(env, cls, fieldID, isStatic);
}

jint
Throw(jthrowable obj)
{
	JNIEnv *env = GetEnv();
	return (*env)->Throw(env, obj);
}

jint
ThrowNew(jclass clazz, const char *msg)
{
	JNIEnv *env = GetEnv();
	return (*env)->ThrowNew(env, clazz, msg);
}

jthrowable
ExceptionOccurred()
{
	JNIEnv *env = GetEnv();
	return (*env)->ExceptionOccurred(env);
}

void
ExceptionDescribe()
{
	JNIEnv *env = GetEnv();
	(*env)->ExceptionDescribe(env);
}

void
ExceptionClear()
{
	JNIEnv *env = GetEnv();
	(*env)->ExceptionClear(env);
}

void
FatalError(const char *msg)
{
	JNIEnv *env = GetEnv();
	(*env)->FatalError(env, msg);
}

jint
PushLocalFrame(jint capacity)
{
	JNIEnv *env = GetEnv();
	return (*env)->PushLocalFrame(env, capacity);
}

jobject
PopLocalFrame(jobject result)
{
	JNIEnv *env = GetEnv();
	return (*env)->PopLocalFrame(env, result);
}

jobject
NewGlobalRef(jobject lobj)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewGlobalRef(env, lobj);
}

void
DeleteGlobalRef(jobject gref)
{
	JNIEnv *env = GetEnv();
	(*env)->DeleteGlobalRef(env, gref);
}

void
DeleteLocalRef(jobject obj)
{
	JNIEnv *env = GetEnv();
	(*env)->DeleteLocalRef(env, obj);
}

jboolean
IsSameObject(jobject obj1, jobject obj2)
{
	JNIEnv *env = GetEnv();
	return (*env)->IsSameObject(env, obj1, obj2);
}

jobject
NewLocalRef(jobject ref)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewLocalRef(env, ref);
}

jint
EnsureLocalCapacity(jint capacity)
{
	JNIEnv *env = GetEnv();
	return (*env)->EnsureLocalCapacity(env, capacity);
}

jobject
AllocObject(jclass clazz)
{
	JNIEnv *env = GetEnv();
	return (*env)->AllocObject(env, clazz);
}

jobject
NewObject(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewObjectA(env, clazz, methodID, args);
}

jclass
GetObjectClass(jobject obj)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetObjectClass(env, obj);
}

jboolean
IsInstanceOf(jobject obj, jclass clazz)
{
	JNIEnv *env = GetEnv();
	return (*env)->IsInstanceOf(env, obj, clazz);
}

jmethodID
GetMethodID(jclass clazz, const char *name, const char *sig)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetMethodID(env, clazz, name, sig);
}

jobject
CallObjectMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallObjectMethodA(env, obj, methodID, args);
}

jboolean
CallBooleanMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallBooleanMethodA(env, obj, methodID, args);
}

jbyte
CallByteMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallByteMethodA(env, obj, methodID, args);
}

jchar
CallCharMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallCharMethodA(env, obj, methodID, args);
}

jshort
CallShortMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallShortMethodA(env, obj, methodID, args);
}

jint
CallIntMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallIntMethodA(env, obj, methodID, args);
}

jlong
CallLongMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallLongMethodA(env, obj, methodID, args);
}

jfloat
CallFloatMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallFloatMethodA(env, obj, methodID, args);
}

jdouble
CallDoubleMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallDoubleMethodA(env, obj, methodID, args);
}

void
CallVoidMethod(jobject obj, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	(*env)->CallVoidMethodA(env, obj, methodID, args);
}

jobject
CallNonvirtualObjectMethod(jobject obj, jclass clazz,
			   jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualObjectMethodA(env, obj, clazz, methodID,
						   args);
}

jboolean
CallNonvirtualBooleanMethod(jobject obj,
			    jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualBooleanMethodA(env, obj, clazz,
						    methodID, args);
}

jbyte
CallNonvirtualByteMethod(jobject obj, jclass clazz,
			 jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualByteMethodA(env, obj, clazz, methodID,
						 args);
}

jchar
CallNonvirtualCharMethod(jobject obj, jclass clazz,
			 jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualCharMethodA(env, obj, clazz, methodID,
						 args);
}

jshort
CallNonvirtualShortMethod(jobject obj, jclass clazz,
			  jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualShortMethodA(env, obj, clazz, methodID,
						  args);
}

jint
CallNonvirtualIntMethod(jobject obj, jclass clazz,
			jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualIntMethodA(env, obj, clazz, methodID,
						args);
}

jlong
CallNonvirtualLongMethod(jobject obj, jclass clazz,
			 jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualLongMethodA(env, obj, clazz, methodID,
						 args);
}

jfloat
CallNonvirtualFloatMethod(jobject obj, jclass clazz,
			  jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualFloatMethodA(env, obj, clazz, methodID,
						  args);
}

jdouble
CallNonvirtualDoubleMethod(jobject obj, jclass clazz,
			   jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallNonvirtualDoubleMethodA(env, obj, clazz, methodID,
						   args);
}

void
CallNonvirtualVoidMethod(jobject obj, jclass clazz,
			 jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	(*env)->CallNonvirtualVoidMethodA(env, obj, clazz, methodID,
						 args);
}

jfieldID
GetFieldID(jclass clazz, const char *name, const char *sig)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetFieldID(env, clazz, name, sig);
}

jobject
GetObjectField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetObjectField(env, obj, fieldID);
}

jboolean
GetBooleanField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetBooleanField(env, obj, fieldID);
}

jbyte
GetByteField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetByteField(env, obj, fieldID);
}

jchar
GetCharField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetCharField(env, obj, fieldID);
}

jshort
GetShortField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetShortField(env, obj, fieldID);
}

jint
GetIntField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetIntField(env, obj, fieldID);
}

jlong
GetLongField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetLongField(env, obj, fieldID);
}

jfloat
GetFloatField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetFloatField(env, obj, fieldID);
}

jdouble
GetDoubleField(jobject obj, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetDoubleField(env, obj, fieldID);
}

void
SetObjectField(jobject obj, jfieldID fieldID, jobject val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetObjectField(env, obj, fieldID, val);
}

void
SetBooleanField(jobject obj, jfieldID fieldID, jboolean val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetBooleanField(env, obj, fieldID, val);
}

void
SetByteField(jobject obj, jfieldID fieldID, jbyte val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetByteField(env, obj, fieldID, val);
}

void
SetCharField(jobject obj, jfieldID fieldID, jchar val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetCharField(env, obj, fieldID, val);
}

void
SetShortField(jobject obj, jfieldID fieldID, jshort val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetByteField(env, obj, fieldID, val);
}

void
SetIntField(jobject obj, jfieldID fieldID, jint val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetIntField(env, obj, fieldID, val);
}

void
SetLongField(jobject obj, jfieldID fieldID, jlong val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetLongField(env, obj, fieldID, val);
}

void
SetFloatField(jobject obj, jfieldID fieldID, jfloat val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetFloatField(env, obj, fieldID, val);
}

void
SetDoubleField(jobject obj, jfieldID fieldID, jdouble val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetDoubleField(env, obj, fieldID, val);
}

jmethodID
GetStaticMethodID(jclass clazz, const char *name, const char *sig)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticMethodID(env, clazz, name, sig);
}

jobject
CallStaticObjectMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticObjectMethodA(env, clazz, methodID, args);
}

jboolean
CallStaticBooleanMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticBooleanMethodA(env, clazz, methodID, args);
}

jbyte
CallStaticByteMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticByteMethodA(env, clazz, methodID, args);
}

jchar
CallStaticCharMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticCharMethodA(env, clazz, methodID, args);
}

jshort
CallStaticShortMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticShortMethodA(env, clazz, methodID, args);
}

jint
CallStaticIntMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticIntMethodA(env, clazz, methodID, args);
}

jlong
CallStaticLongMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticLongMethodA(env, clazz, methodID, args);
}

jfloat
CallStaticFloatMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticFloatMethodA(env, clazz, methodID, args);
}

jdouble
CallStaticDoubleMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	return (*env)->CallStaticDoubleMethodA(env, clazz, methodID, args);
}

void
CallStaticVoidMethod(jclass clazz, jmethodID methodID, jvalue * args)
{
	JNIEnv *env = GetEnv();
	(*env)->CallStaticVoidMethodA(env, clazz, methodID, args);
}

jfieldID
GetStaticFieldID(jclass clazz, const char *name, const char *sig)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticFieldID(env, clazz, name, sig);
}

jobject
GetStaticObjectField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticObjectField(env, clazz, fieldID);
}

jboolean
GetStaticBooleanField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticBooleanField(env, clazz, fieldID);
}

jbyte
GetStaticByteField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticByteField(env, clazz, fieldID);
}

jchar
GetStaticCharField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticCharField(env, clazz, fieldID);
}

jshort
GetStaticShortField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticShortField(env, clazz, fieldID);
}

jint
GetStaticIntField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticIntField(env, clazz, fieldID);
}

jlong
GetStaticLongField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticLongField(env, clazz, fieldID);
}

jfloat
GetStaticFloatField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticFloatField(env, clazz, fieldID);
}

jdouble
GetStaticDoubleField(jclass clazz, jfieldID fieldID)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStaticDoubleField(env, clazz, fieldID);
}

void
SetStaticObjectField(jclass clazz, jfieldID fieldID, jobject value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticObjectField(env, clazz, fieldID, value);
}

void
SetStaticBooleanField(jclass clazz, jfieldID fieldID, jboolean value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticBooleanField(env, clazz, fieldID, value);
}

void
SetStaticByteField(jclass clazz, jfieldID fieldID, jbyte value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticByteField(env, clazz, fieldID, value);
}

void
SetStaticCharField(jclass clazz, jfieldID fieldID, jchar value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticCharField(env, clazz, fieldID, value);
}

void
SetStaticShortField(jclass clazz, jfieldID fieldID, jshort value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticShortField(env, clazz, fieldID, value);
}

void
SetStaticIntField(jclass clazz, jfieldID fieldID, jint value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticIntField(env, clazz, fieldID, value);
}

void
SetStaticLongField(jclass clazz, jfieldID fieldID, jlong value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticLongField(env, clazz, fieldID, value);
}

void
SetStaticFloatField(jclass clazz, jfieldID fieldID, jfloat value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticFloatField(env, clazz, fieldID, value);
}

void
SetStaticDoubleField(jclass clazz, jfieldID fieldID, jdouble value)
{
	JNIEnv *env = GetEnv();
	(*env)->SetStaticDoubleField(env, clazz, fieldID, value);
}

jstring
NewString(const jchar * unicode, jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewString(env, unicode, len);
}

jsize
GetStringLength(jstring str)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStringLength(env, str);
}

const jchar *
GetStringChars(jstring str, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStringChars(env, str, isCopy);
}

void
ReleaseStringChars(jstring str, const jchar * chars)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseStringChars(env, str, chars);
}

jstring
NewStringUTF(const char *utf)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewStringUTF(env, utf);
}

jsize
GetStringUTFLength(jstring str)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStringUTFLength(env, str);
}

const char *
GetStringUTFChars(jstring str, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStringUTFChars(env, str, isCopy);
}

void
ReleaseStringUTFChars(jstring str, const char *chars)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseStringUTFChars(env, str, chars);
}

jsize
GetArrayLength(jarray array)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetArrayLength(env, array);
}

jobjectArray
NewObjectArray(jsize len, jclass clazz, jobject init)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewObjectArray(env, len, clazz, init);
}

jobject
GetObjectArrayElement(jobjectArray array, jsize index)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetObjectArrayElement(env, array, index);
}

void
SetObjectArrayElement(jobjectArray array, jsize index, jobject val)
{
	JNIEnv *env = GetEnv();
	(*env)->SetObjectArrayElement(env, array, index, val);
}

jbooleanArray
NewBooleanArray(jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewBooleanArray(env, len);
}

jbyteArray
NewByteArray(jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewByteArray(env, len);
}

jcharArray
NewCharArray(jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewCharArray(env, len);
}

jshortArray
NewShortArray(jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewShortArray(env, len);
}

jintArray
NewIntArray(jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewIntArray(env, len);
}

jlongArray
NewLongArray(jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewLongArray(env, len);
}

jfloatArray
NewFloatArray(jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewFloatArray(env, len);
}

jdoubleArray
NewDoubleArray(jsize len)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewDoubleArray(env, len);
}

jboolean *
GetBooleanArrayElements(jbooleanArray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetBooleanArrayElements(env, array, isCopy);
}

jbyte *
GetByteArrayElements(jbyteArray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetByteArrayElements(env, array, isCopy);
}

jchar *
GetCharArrayElements(jcharArray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetCharArrayElements(env, array, isCopy);
}

jshort *
GetShortArrayElements(jshortArray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetShortArrayElements(env, array, isCopy);
}

jint *
GetIntArrayElements(jintArray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetIntArrayElements(env, array, isCopy);
}

jlong *
GetLongArrayElements(jlongArray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetLongArrayElements(env, array, isCopy);
}

jfloat *
GetFloatArrayElements(jfloatArray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetFloatArrayElements(env, array, isCopy);
}

jdouble *
GetDoubleArrayElements(jdoubleArray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetDoubleArrayElements(env, array, isCopy);
}

void
ReleaseBooleanArrayElements(jbooleanArray array, jboolean * elems, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseBooleanArrayElements(env, array, elems, mode);
}

void
ReleaseByteArrayElements(jbyteArray array, jbyte * elems, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseByteArrayElements(env, array, elems, mode);
}

void
ReleaseCharArrayElements(jcharArray array, jchar * elems, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseCharArrayElements(env, array, elems, mode);
}

void
ReleaseShortArrayElements(jshortArray array, jshort * elems, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseShortArrayElements(env, array, elems, mode);
}

void
ReleaseIntArrayElements(jintArray array, jint * elems, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseIntArrayElements(env, array, elems, mode);
}

void
ReleaseLongArrayElements(jlongArray array, jlong * elems, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseLongArrayElements(env, array, elems, mode);
}

void
ReleaseFloatArrayElements(jfloatArray array, jfloat * elems, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseFloatArrayElements(env, array, elems, mode);
}

void
ReleaseDoubleArrayElements(jdoubleArray array, jdouble * elems, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseDoubleArrayElements(env, array, elems, mode);
}

void
GetBooleanArrayRegion(jbooleanArray array, jsize start,
		      jsize len, jboolean * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetBooleanArrayRegion(env, array, start, len, buf);
}

void
GetByteArrayRegion(jbyteArray array, jsize start, jsize len, jbyte * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetByteArrayRegion(env, array, start, len, buf);
}

void
GetCharArrayRegion(jcharArray array, jsize start, jsize len, jchar * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetCharArrayRegion(env, array, start, len, buf);
}

void
GetShortArrayRegion(jshortArray array, jsize start, jsize len, jshort * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetShortArrayRegion(env, array, start, len, buf);
}

void
GetIntArrayRegion(jintArray array, jsize start, jsize len, jint * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetIntArrayRegion(env, array, start, len, buf);
}

void
GetLongArrayRegion(jlongArray array, jsize start, jsize len, jlong * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetLongArrayRegion(env, array, start, len, buf);
}

void
GetFloatArrayRegion(jfloatArray array, jsize start, jsize len, jfloat * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetFloatArrayRegion(env, array, start, len, buf);
}

void
GetDoubleArrayRegion(jdoubleArray array, jsize start, jsize len, jdouble * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetDoubleArrayRegion(env, array, start, len, buf);
}

void
SetBooleanArrayRegion(jbooleanArray array, jsize start,
		      jsize len, jboolean * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->SetBooleanArrayRegion(env, array, start, len, buf);
}

void
SetByteArrayRegion(jbyteArray array, jsize start, jsize len, jbyte * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->SetByteArrayRegion(env, array, start, len, buf);
}

void
SetCharArrayRegion(jcharArray array, jsize start, jsize len, jchar * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->SetCharArrayRegion(env, array, start, len, buf);
}

void
SetShortArrayRegion(jshortArray array, jsize start, jsize len, jshort * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->SetShortArrayRegion(env, array, start, len, buf);
}

void
SetIntArrayRegion(jintArray array, jsize start, jsize len, jint * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->SetIntArrayRegion(env, array, start, len, buf);
}

void
SetLongArrayRegion(jlongArray array, jsize start, jsize len, jlong * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->SetLongArrayRegion(env, array, start, len, buf);
}

void
SetFloatArrayRegion(jfloatArray array, jsize start, jsize len, jfloat * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->SetFloatArrayRegion(env, array, start, len, buf);
}

void
SetDoubleArrayRegion(jdoubleArray array, jsize start, jsize len, jdouble * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->SetDoubleArrayRegion(env, array, start, len, buf);
}

jint
MonitorEnter(jobject obj)
{
	JNIEnv *env = GetEnv();
	return (*env)->MonitorEnter(env, obj);
}

jint
MonitorExit(jobject obj)
{
	JNIEnv *env = GetEnv();
	return (*env)->MonitorExit(env, obj);
}

void
GetStringRegion(jstring str, jsize start, jsize len, jchar * buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetStringRegion(env, str, start, len, buf);
}

void
GetStringUTFRegion(jstring str, jsize start, jsize len, char *buf)
{
	JNIEnv *env = GetEnv();
	(*env)->GetStringUTFRegion(env, str, start, len, buf);
}

void *
GetPrimitiveArrayCritical(jarray array, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetPrimitiveArrayCritical(env, array, isCopy);
}

void
ReleasePrimitiveArrayCritical(jarray array, void *carray, jint mode)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleasePrimitiveArrayCritical(env, array, carray, mode);
}

const jchar *
GetStringCritical(jstring string, jboolean * isCopy)
{
	JNIEnv *env = GetEnv();
	return (*env)->GetStringCritical(env, string, isCopy);
}

void
ReleaseStringCritical(jstring string, const jchar * cstring)
{
	JNIEnv *env = GetEnv();
	(*env)->ReleaseStringCritical(env, string, cstring);
}

jweak
NewWeakGlobalRef(jobject obj)
{
	JNIEnv *env = GetEnv();
	return (*env)->NewWeakGlobalRef(env, obj);
}

void
DeleteWeakGlobalRef(jweak ref)
{
	JNIEnv *env = GetEnv();
	(*env)->DeleteWeakGlobalRef(env, ref);
}

jboolean
ExceptionCheck()
{
	JNIEnv *env = GetEnv();
	return (*env)->ExceptionCheck(env);
}
