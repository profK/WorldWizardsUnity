using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;


namespace WorldWizards.core.entity.gameObject.resource.metaData
{
    // @author - Brian Keeley-DeBonis bjkeeleydebonis@wpi.edu
    /// <summary>
    /// This component is attached to all WWObjects that are to be created and added to an 
    /// asset bundle. The artist, or person who sets up the prefab needs to configure 
    /// the properties. This component is composed of various pieces of metadata that are relevent to describing a WWObject.
    /// The metadata is subject to change and be extended as more features are added to World Wizards.
    /// WWResourceMetadata is composed of all possible types of metadata.
    /// </summary>
    [Serializable]
    public class WWResourceMetadata : MonoBehaviour
    {
        public WWDoorMetadata doorMetadata;
        public WWObjectMetadata wwObjectMetadata;
        public WWTileMetadata wwTileMetadata;
        public Texture2D Thumbnail;

#if UNITY_EDITOR
        void Reset()
        {
          

            if (Thumbnail != null)
            {
                DestroyImmediate(Thumbnail);
            }
            Debug.Log("Making asset");
            GameObject go = Selection.activeGameObject;
            Texture2D pview = AssetPreview.GetAssetPreview(go);
           
            string thumbPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(go))+
                "/Thumbnails";
            if (!Directory.Exists(thumbPath))
            {
                Directory.CreateDirectory(thumbPath);
            }

            thumbPath = thumbPath.Replace('\\', '/');
            string path = thumbPath + "/" + name + "_thumb.png";
            Debug.Log("Writing PNG to "+path);
            File.WriteAllBytes(path,  pview.EncodeToPNG ());

            Debug.Log("Loading T2D at "+path);
            AssetDatabase.Refresh();
            Thumbnail = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            if (Thumbnail == null)
            {
                Debug.Log("Failed to load "+path);
            }
            else
            {
                Debug.Log("TUmbnail=" + Thumbnail.name);
            }
        }
        
#endif        
    }

   
}