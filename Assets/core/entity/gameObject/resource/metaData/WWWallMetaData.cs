﻿using System;

namespace WorldWizards.core.entity.gameObject.resource.metaData
{
    /// <summary>
    /// Metadata required to describe whether a wall of the Tile should have collision or not.
    /// </summary>
    [Serializable]
    public class WWWallMetadata
    {
        public bool north;
        public bool east;
        public bool south;
        public bool west;
        public bool top;
        public bool bottom;

        public WWWallMetadata()
        {
            north = false;
            east = false;
            south = false;
            west = false;
            top = false;
            bottom = false;
        }

        public WWWallMetadata(bool north, bool east, bool south, bool west, bool top, bool bottom)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
            this.top = top;
            this.bottom = bottom;
        }

        /// <summary>
        /// Serializable boolean values are used to make editing metadata in the inspector easier,
        /// but they must be converted to the enum bitflag for use.
        /// </summary>
        /// <returns>WWWalls equivalent to the serializable boolean values set in the inspector.</returns>
        public WWWalls GetWallsEnum()
        {
            WWWalls result = 0;

            if (north)
            {
                result = result | WWWalls.North;
            }
            if (east)
            {
                result = result | WWWalls.East;
            }
            if (south)
            {
                result = result | WWWalls.South;
            }
            if (west)
            {
                result = result | WWWalls.West;
            }
            if (top)
            {
                result = result | WWWalls.Top;
            }
            if (bottom)
            {
                result = result | WWWalls.Bottom;
            }

            return result;
        }
    }
}