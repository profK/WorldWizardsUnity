﻿using System.Collections.Generic;
using UnityEngine;
using WorldWizards.core.entity.coordinate;
using WorldWizards.core.entity.gameObject;
using WorldWizards.core.entity.gameObject.utils;
using WorldWizards.core.manager;

namespace WorldWizards.core.experimental
{
    public class TerrainGenerator
    {
        public static List<Coordinate> CreateTerrainFromImage(Texture2D heightmap)
        {
            var coordinates = new List<Coordinate>();
            var maxHeight = 10;
            for (var x = 0; x < heightmap.width; x++)
            for (var y = 0; y < heightmap.height; y++)
            {
                var height = (int) (heightmap.GetPixel(x, y).r * maxHeight);
                var c = new Coordinate(x, height, y);
                coordinates.Add(c);
                
                var wwTransform = new WWTransform(c, 0);

                WWObjectData parentData = WWObjectFactory.CreateNew(wwTransform, "white");
                WWObject parentObj = WWObjectFactory.Instantiate(parentData);
                ManagerRegistry.Instance.GetAnInstance<SceneGraphManager>().Add(parentObj);
                

                while (height > 0)
                {
                    height--;
                    c = new Coordinate(x, height, y);
                    coordinates.Add(c);
                    wwTransform = new WWTransform(c, 0);
                    
                    WWObjectData childData = WWObjectFactory.CreateNew(wwTransform, "white");

                    WWObject childObj = WWObjectFactory.Instantiate(childData);
                    ManagerRegistry.Instance.GetAnInstance<SceneGraphManager>().Add(childObj);

                    var children = new List<WWObject>();
                    children.Add(childObj);
                    parentObj.AddChildren(children);
                    parentObj = childObj;
                }
            }
            return coordinates;
        }
    }
}