using UnityEngine;


namespace WorldWizards.core.manager
{
    // @author - Brian Keeley-DeBonis bjkeeleydebonis@wpi.edu
    /// <summary>
    /// This MonoBehavior needs to execute before everything else in World Wizards.
    /// It is responsible for setting up the Manager Registry.
    /// </summary>
    public class WWMain : MonoBehaviour
    {
        private void Awake()
        {
            // setup the manager registry
            ManagerRegistry setupManagerRegistry = ManagerRegistry.Instance; // touching Instance, starts up the Manager Registry
            /// This should be moved to be part of the TileMap code
            //ResourceLoader.LoadResources();
        }
    }
}