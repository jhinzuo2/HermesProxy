/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

public static class MathFunctions
{
    public static ulong MakePair64(uint l, uint h)
    {
        return (ulong)l | ((ulong)h << 32);
    }

    public static uint Pair64_HiPart(ulong x)
    {
        return (uint)((x >> 32) & 0x00000000FFFFFFFF);
    }

    public static uint Pair64_LoPart(ulong x)
    {
        return (uint)(x & 0x00000000FFFFFFFF);
    }
}
