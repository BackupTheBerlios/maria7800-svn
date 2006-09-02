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

mariadir = $(pkglibdir)
maria_SCRIPTS = maria.exe
dist_maria_DATA = romproperties.csv
bin_SCRIPTS = maria

EXTRA_DIST = $(maria_sources) $(maria_sources_in) createlauncherscript.sh
CLEANFILES = maria maria.exe maria.exe.mdb Tao.Sdl.dll

maria_sources_in = AssemblyInfo.cs.in
maria_generated_sources = $(maria_sources_in:.in=)
maria_sources = \
EmuTraceListener.cs \
Maria.cs

maria_build_sources = $(addprefix $(srcdir)/, $(maria_sources) $(maria_generated_sources))

OWN_ASSEMBLIES = $(top_srcdir)/tao/Tao.Sdl.dll
OWN_ASSEMBLY_REFERENCES = $(addprefix -r:,$(OWN_ASSEMBLIES))

maria.exe : $(maria_build_sources)
	$(MCS) -out:$@ $(OWN_ASSEMBLY_REFERENCES) $^
	cp $(OWN_ASSEMBLIES) .

maria : $(srcdir)/createlauncherscript.sh
	sh $(srcdir)/createlauncherscript.sh "$(MONO)" "$(pkglibdir)" "$(PACKAGE_NAME)".exe > $@
