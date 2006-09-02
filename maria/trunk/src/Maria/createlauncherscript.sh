#!/bin/sh
#
# Script to create Mono application launcher script.
#
# This file is part of Maria.
# Copyright (C) 2006 Thomas Mathys (tom42@users.berlios.de)
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

if test "$#" != "3" ; then
  echo "Wrong arguments"
  echo "Usage : $0 monobinary pkglibdir exename"
  exit 1
fi

monobinary="$1"
pkglibdir="$2"
exename="$3"

echo "#!/bin/sh"
echo exec "$monobinary" "$pkglibdir"/"$exename" '"$@"'

