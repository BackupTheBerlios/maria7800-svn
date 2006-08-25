# Process this file with automake to get a Makefile
#
# Copyright (C) 2006 Thomas Mathys (tom42@users.berlios.de)
#
# This file is part of Maria.
#
# Maria is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# Maria is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with Maria; if not, write to the Free Software
# Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

TESTS = $(top_srcdir)/runtests.sh
TESTS_ENVIRONMENT = \
rt_top_builddir=$(top_builddir) \
rt_assembly=Maria.Core.Test.dll

check_SCRIPTS = Maria.Core.Test.dll
EXTRA_DIST = $(Maria_Core_Test_sources) $(Maria_Core_Test_sources_in)
CLEANFILES = Maria.Core.Test.dll Maria.Core.Test.dll.mdb TestResult.xml

Maria_Core_Test_sources_in =
Maria_Core_Test_generated_sources = $(Maria_Core_Test_sources_in:.in=)
Maria_Core_Test_sources = \
AddressSpaceTest.cs \
GameSettingsTest.cs \
M6502DASMTest.cs \
MD5Test.cs \
NullDeviceTest.cs \
NUnitTestTest.cs \
RAM6116Test.cs

OWN_ASSEMBLIES = \
$(top_builddir)/src/Maria.Core/Maria.Core.dll

OWN_ASSEMBLY_REFERENCES = $(addprefix -r:, $(OWN_ASSEMBLIES))

Maria_Core_Test_build_sources = $(addprefix $(srcdir)/, $(Maria_Core_Test_sources) $(Maria_Core_Test_generated_sources))

Maria.Core.Test.dll : $(Maria_Core_Test_build_sources)
	$(MCS) -out:$@ -target:library $(NUNIT_LIBS) $(OWN_ASSEMBLY_REFERENCES) $^
