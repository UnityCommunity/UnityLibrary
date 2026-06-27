// Here is a simple script which must be places instead of Image (beneath the mask object)
// https://discussions.unity.com/t/ui-inverse-mask/590229/5

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UnityLibrary.UI
{
    public class InvertedMaskImage : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material result = base.materialForRendering;
                result.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return result;
            }
        }
    }
}
