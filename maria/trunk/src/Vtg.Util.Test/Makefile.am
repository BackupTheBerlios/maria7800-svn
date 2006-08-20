# Process this file with automake to get a Makefile
#
# Copyright (C) 2006 Thomas Mathys (tom42@users.berlios.de)
#
# This file is part of Vtg.Util.
#
# Vtg.Util is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# Vtg.Util is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with Vtg.Util; if not, write to the Free Software
# Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

TESTS = $(top_srcdir)/runtests.sh
TESTS_ENVIRONMENT = \
testassembly=Vtg.Util.Test.dll \
MONO_PATH=$(dir $(OWN_ASSEMBLIES))

check_SCRIPTS = Vtg.Util.Test.dll
EXTRA_DIST = $(Vtg_Util_Test_sources) $(Vtg_Util_Test_sources_in)
CLEANFILES = Vtg.Util.Test.dll Vtg.Util.Test.dll.mdb TestResult.xml

Vtg_Util_Test_sources_in =
Vtg_Util_Test_generated_sources = $(Vtg_Util_Test_sources_in:.in=)
Vtg_Util_Test_sources = \
ArgumentCheckTest.cs

OWN_ASSEMBLIES = \
$(top_builddir)/src/Vtg.Util/Vtg.Util.dll

OWN_ASSEMBLY_REFERENCES = $(addprefix -r:, $(OWN_ASSEMBLIES))

Vtg_Util_Test_build_sources = $(addprefix $(srcdir)/, $(Vtg_Util_Test_sources) $(Vtg_Util_Test_generated_sources))

Vtg.Util.Test.dll : $(Vtg_Util_Test_build_sources)
	$(MCS) -out:$@ -target:library $(NUNIT_LIBS) $(OWN_ASSEMBLY_REFERENCES) $^
