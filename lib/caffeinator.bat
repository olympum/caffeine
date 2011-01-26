set CURDIR=.
set CLASSPATH=%CURDIR%\caffeinator.jar;%CURDIR%\dom4j.jar
java -classpath "%CLASSPATH%" com.olympum.tools.JavaApiXmlGenerator %1 %2
java -classpath "%CLASSPATH%" com.olympum.tools.CsJniNetWrapperGenerator %2 %3
