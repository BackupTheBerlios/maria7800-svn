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

Maria_Coredir = $(pkglibdir)
Maria_Core_SCRIPTS = Maria.Core.dll
EXTRA_DIST = $(Maria_Core_sources) $(Maria_Core_sources_in)
CLEANFILES = Maria.Core.dll Maria.Core.dll.mdb

Maria_Core_sources_in = AssemblyInfo.cs.in
Maria_Core_generated_sources = $(Maria_Core_sources_in:.in=)
Maria_Core_sources = \
AddressSpace.cs \
Cart.cs \
CartA2K.cs \
CartA4K.cs \
Cart7808.cs \
Cart7816.cs \
Cart7832.cs \
CartType.cs \
CommandLine.cs \
Controller.cs \
EMU7800App.cs \
Exceptions.cs \
FontRenderer.cs \
GameSettings.cs \
GlobalSettings.cs \
IAddressable.cs \
IDevice.cs \
IHost.cs \
InputAdapter.cs \
M6502.cs \
M6502DASM.cs \
Machine.cs \
Machine2600.cs \
Machine7800.cs \
MachineType.cs \
MariaTables.cs \
MD5.cs \
NullDevice.cs \
NUnitTest.cs \
PIA.cs \
PlaybackInputAdapter.cs \
RAM6116.cs \
RecordInputAdapter.cs \
ROMProperties.cs \
SDLHost.cs \
TIA.cs \
TIASound.cs \
TIATables.cs

OWN_ASSEMBLIES = \
$(top_builddir)/src/Vtg.Util/Vtg.Util.dll

OWN_ASSEMBLY_REFERENCES = $(addprefix -r:, $(OWN_ASSEMBLIES))

Maria_Core_build_sources = $(addprefix $(srcdir)/, $(Maria_Core_sources) $(Maria_Core_generated_sources))

Maria.Core.dll : $(Maria_Core_build_sources)
	$(MCS) -out:$@ -target:library $(OWN_ASSEMBLY_REFERENCES) $^
