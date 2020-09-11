using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityHelper.Pooling
{
    [Serializable]
    public class Pool
    {
        /// <summary>
        /// The objects that you want to add to the pool
        /// </summary>
        public List<PoolObject> objects_to_pool = new List<PoolObject>();

        /// <summary>
        /// All the objects that were added to the pool
        /// </summary>
        private List<GameObject> pool_objects = new List<GameObject>();

        /// <summary>
        /// Initializes the pool in your scene
        /// </summary>
        public void Start_Pool()
        {
            var pool = new GameObject("Pool");

            foreach (var item in objects_to_pool)
            {
                var sub_parent = new GameObject(item.obj_to_pool.name);

                for (int i = 0; i < item.amount_to_pool; i++)
                {
                    var _obj = UnityEngine.Object.Instantiate(item.obj_to_pool);
                    _obj.transform.SetParent(sub_parent.transform, false);
                    _obj.SetActive(false);
                    pool_objects.Add(_obj);
                }

                sub_parent.transform.SetParent(pool.transform, false);
            }
        }

        /// <summary>
        /// Gets the item from the pool and spawns it at the required position
        /// </summary>
        /// <param name="type">The type of script in the pool that you need to spawn</param>
        /// <param name="position">The position where you want the item to spawn</param>
        /// <returns>A component of the needed type</returns>
        public Component Spawn_Item(Type type, Vector3 position, Quaternion direction)
        {
            foreach (GameObject item in pool_objects)
            {
                Component comp = null;
                item.TryGetComponent(type, out comp);

                if (comp == null)
                    continue;

                if (item.activeSelf)
                    continue;

                item.transform.position = position;
                item.transform.rotation = direction;
                item.SetActive(true);
                return comp;

            }

            return null;
        }

        /// <summary>
        /// Brings the item back to the pool
        /// </summary>
        /// <param name="item">The item to bring back to the pool</param>
        public void Return_Item(GameObject item)
        {
            item.SetActive(false);
            item.transform.position = Vector3.zero;
        }
    }
}