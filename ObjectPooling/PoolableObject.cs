using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shoelace
{
    public class PoolableObject : MonoBehaviour
    {
        public ObjectPool Parent;

        public virtual void OnDisable()
        {
            Parent.ReturnObjectToPool(this);
        }
    }
}
