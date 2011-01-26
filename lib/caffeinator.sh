#!/bin/sh
CURDIR=`dirname $0`
CLASSPATH=$CURDIR/caffeinator.jar:$CURDIR/dom4j.jar
java -classpath "$CLASSPATH" com.olympum.tools.JavaApiXmlGenerator $1 $2
java $4 -classpath "$CLASSPATH" com.olympum.tools.CsJniNetWrapperGenerator $2 $3
