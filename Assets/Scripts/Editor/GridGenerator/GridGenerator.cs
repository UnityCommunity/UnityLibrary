using UnityEngine;
namespace UnityLibrary
{
    public class GridGenerator : MonoBehaviour
    {
        [SerializeField]
        private GameObject prefabToPlace;

        [SerializeField]
        private uint sizeX = 2;

        [SerializeField]
        private uint sizeY = 2;

        [SerializeField]
        private Vector2 offset = Vector2.one;

        public GameObject PrefabToPlace { get { return prefabToPlace; } }
        public Vector2 Offset { get { return offset; } }

        public uint SizeX { get { return sizeX; } }
        public uint SizeY { get { return sizeY; } }
    }
}