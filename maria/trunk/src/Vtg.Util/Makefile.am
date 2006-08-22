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

Vtg_Utildir = $(pkglibdir)
Vtg_Util_SCRIPTS = Vtg.Util.dll
EXTRA_DIST = $(Vtg_Util_sources) $(Vtg_Util_sources_in)
CLEANFILES = Vtg.Util.dll Vtg.Util.dll.mdb

Vtg_Util_sources_in = AssemblyInfo.cs.in
Vtg_Util_generated_sources = $(Vtg_Util_sources_in:.in=)
Vtg_Util_sources = \
ArgumentCheck.cs

Vtg_Util_build_sources = $(addprefix $(srcdir)/, $(Vtg_Util_sources) $(Vtg_Util_generated_sources))

Vtg.Util.dll : $(Vtg_Util_build_sources)
	$(MCS) -out:$@ -target:library $^
	cp $@ $(top_srcdir)/rundir
