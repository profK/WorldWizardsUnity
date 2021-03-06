using UnityEngine;
using WorldWizards.core.entity.coordinate;
using WorldWizards.core.entity.coordinate.utils;
using WorldWizards.core.entity.gameObject;
using WorldWizards.core.manager;

namespace WorldWizards.core.controller.builder
{
    // @author - Brian Keeley-DeBonis bjkeeleydebonis@wpi.edu
    /// <summary>
    /// A simple controller for scaling and moving a Grid Collider up and down in the Builder.
    /// </summary>
    public class GridController : MonoBehaviour
    {
        [SerializeField] private GameObject grid;
        [SerializeField] private GameObject gridCursor;
        

        public GameObject Cursor
        {
            get { return gridCursor; }
        }

        public double TileWidth
        {
            get { return transform.localScale.x * 0.01; }
        }

        public double TileDepth
        {
            get { return transform.localScale.z * 0.01; }
        }

        private Coordinate _currentGridPos;

        public Coordinate CursorLocation
        {
            get { return _currentGridPos; }
            set
            {
                _currentGridPos = value;
                Cursor.transform.position = CoordinateHelper.WWCoordToUnityCoord(
                    _currentGridPos);
            }

        }

        public int GridHeight
        {
            get { return height; }
        }


        private int height;
        private GameObject playerReferenceScale;
        [SerializeField] private Material gridMat;
        private readonly string gridMatMainTexture = "_MainTex";
        private readonly string gridMatViewDistance = "_ViewDistance";
        private float viewDistance = 10; // the amount of coordinate spaces the user can see
        private float scaleMultiplier = 1000;

        /// <summary>
        /// Basic setup
        /// </summary>
        private void Start()
        {
            _currentGridPos = CoordinateHelper.UnityCoordToWWCoord(Cursor.transform.position);
            // playerReferenceScale = Instantiate(Resources.Load("Prefabs/PlayerScale")) as GameObject;
            RefreshGrid();
        }

        private void SetGridScale()
        {
            var tileLengthScale = CoordinateHelper.tileLengthScale;
            var baseTileLenghtScale = CoordinateHelper.baseTileLength;
            var textureScale = scaleMultiplier * baseTileLenghtScale;
            var scale = Vector3.one * tileLengthScale * scaleMultiplier;
            scale.y = 1;
            grid.transform.localScale = scale;
            gridMat.SetTextureScale(gridMatMainTexture, new Vector2(textureScale, textureScale));
            // adjust the material's view distance as well based on the world scale
            var viewDistanceScaled = viewDistance * CoordinateHelper.GetTileScale();
            gridMat.SetFloat(gridMatViewDistance, viewDistanceScaled);

        }

        /// <summary>
        /// Get the grid collider.
        /// </summary>
        /// <returns> The Grid's Collider</returns>
        public Collider GetGridCollider()
        {
            return GetComponent<Collider>();
        }


        /// <summary>
        /// Refresh the grid position and scale.
        /// </summary>
        public void RefreshGrid()
        {
            float yPos = height * CoordinateHelper.GetTileScale();
            grid.transform.position = new Vector3(0, yPos, 0);
            Coordinate c = CoordinateHelper.UnityCoordToWWCoord(grid.transform.position);
            // set the scale too
            SetGridScale();
            /* playerReferenceScale.transform.position = new Vector3(0,
                 yPos,
                 0);*/
        }

        /// <summary>
        /// Set the height to a specific index and refresh.
        /// </summary>
        public void SetHeightAndRefresh(int height)
        {
            this.height = height;
            RefreshGrid();
        }

        /// <summary>
        /// Move the grid up one step.
        /// </summary>
        public void StepUp()
        {
            height++;
            RefreshGrid();
        }

        /// <summary>
        /// Move the grid down one step.
        /// </summary>
        public void StepDown()
        {
            height--;
            RefreshGrid();
        }


        public void RotateSelectedTileCW()
        {
        }

        public void RotateSelectedTileCCW()
        {
            throw new System.NotImplementedException();
        }
    }
}
