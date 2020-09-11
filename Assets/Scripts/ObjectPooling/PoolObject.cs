using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityHelper.Pooling
{
    [Serializable]
    public class PoolObject
    {
        /// <summary>
        /// The gameobject that you want to spawn
        /// </summary>
        public GameObject obj_to_pool;
        /// <summary>
        /// The amount of objects that you want to spawn
        /// </summary>
        public int amount_to_pool;
    }
}