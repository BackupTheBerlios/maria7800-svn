/*
 * Maria specific exceptions
 * Copyright (C) 2006 Thomas Mathys (tom42@sourceforge.net)
 *
 * This file is part of Maria.
 *
 * Maria is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Maria is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Maria; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */
using System;

namespace Maria.Core {
	public class MariaCoreException : ApplicationException {
		public MariaCoreException() : base() {}
		public MariaCoreException(string msg) : base(msg) {}
		public MariaCoreException(string msg, Exception inner) : base(msg, inner) {}
	}

	public class InternalErrorException : MariaCoreException {
		public InternalErrorException() : base() {}
		public InternalErrorException(string msg) : base(msg) {}
		public InternalErrorException(string msg, Exception inner) : base(msg, inner) {}
	}
}
