#!/bin/sh
# Run this to generate all the initial makefiles, etc.
# Ripped off from GNOME macros version

srcdir=`dirname $0`
test -z "$srcdir" && srcdir=.

echo Running libtoolize
libtoolize --force --copy
echo Running aclocal
aclocal $ACLOCAL_FLAGS
echo Running automake
automake --add-missing --gnu $am_opt
echo Runing autoconf
autoconf

conf_flags="--enable-maintainer-mode --enable-compile-warnings" #--enable-iso-c

$srcdir/configure $conf_flags "$@"
echo Now type \`make\' to compile $PKG_NAME