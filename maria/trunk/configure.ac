dnl Process this file with autoconf to generate a configure script
dnl
dnl Maria configure.ac
dnl Copyright (C) 2006 Thomas Matys (tom42@users.berlios.de)
dnl
dnl This file is part of Maria.
dnl
dnl Maria is free software; you can redistribute it and/or modify
dnl it under the terms of the GNU General Public License as published by
dnl the Free Software Foundation; either version 2 of the License, or
dnl (at your option) any later version.
dnl
dnl Maria is distributed in the hope that it will be useful,
dnl but WITHOUT ANY WARRANTY; without even the implied warranty of
dnl MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
dnl GNU General Public License for more details.
dnl
dnl You should have received a copy of the GNU General Public License
dnl along with Maria; if not, write to the Free Software
dnl Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

AC_INIT([maria], [0.71], [tom42@users.berlios.de])
AC_PREREQ([2.60])
AM_INIT_AUTOMAKE
AM_MAINTAINER_MODE

AC_PROG_INSTALL

dnl Check for pkg-config
AC_PATH_PROG([PKG_CONFIG], [pkg-config], [no])
if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR([You need to install pkg-config])
fi

dnl Check for C# compiler and Mono runtime
AC_PATH_PROG([CSC], [csc], [no])
AC_PATH_PROG([MCS], [mcs], [no])
AC_PATH_PROG([MONO], [mono], [no])
CS="C#"
if test "x$CSC" = "xno" -a "x$MCS" = "xno" ; then
	AC_MSG_ERROR([No $CS Compiler found])
fi
if test "x$MCS" = "xno" ; then
	MCS=$CSC
fi
AC_SUBST([MCS])
if test "x$MONO" = "xno" ; then
	AC_MSG_ERROR([No mono runtime found])
fi

dnl Check for NUnit
NUNIT_REQUIRED_VERSION=2.2.0
PKG_CHECK_MODULES([NUNIT], [nunit-2.2 >= $NUNIT_REQUIRED_VERSION], [NUNIT=yes], [echo no && NUNIT=no])
AC_PATH_PROG([NUNIT_CONSOLE], [nunit-console], [no])
AM_CONDITIONAL(ENABLE_UNIT_TESTS, true)

echo
echo "Summary"
echo "-------"
echo "C# compiler : $MCS"
echo -n "NUnit $NUNIT_REQUIRED_VERSION or higher installed : $NUNIT"
if test "x$NUNIT" = "xno" ; then
	echo -n " (NUnit tests cannot be compiled)"
fi
echo
echo -n "nunit-console : $NUNIT_CONSOLE"
if test "x$NUNIT_CONSOLE" = "xno" ; then
	echo -n " (NUnit tests cannot be compiled)"
fi
echo
echo

AC_CONFIG_FILES([
Makefile
src/Makefile
src/Maria/AssemblyInfo.cs
src/Maria/Makefile
src/Maria.Core/AssemblyInfo.cs
src/Maria.Core/Makefile
src/Maria.Core.Test/Makefile
])
AC_OUTPUT
