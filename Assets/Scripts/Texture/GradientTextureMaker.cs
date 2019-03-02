// returns gradient Texture2D (size=256x1)

using UnityEngine;

namespace UnityLibrary
{
    public static class GradientTextureMaker
    {
        const int width = 256;
        const int height = 1;

        public static Texture2D Create(Color[] colors, TextureWrapMode textureWrapMode = TextureWrapMode.Clamp, FilterMode filterMode = FilterMode.Point, bool isLinear = false, bool hasMipMap = false)
        {
            if (colors == null || colors.Length == 0)
            {
                Debug.LogError("No colors assigned");
                return null;
            }

            int length = colors.Length;
            if (colors.Length > 8)
            {
                Debug.LogWarning("Too many colors! maximum is 8, assigned: " + colors.Length);
                length = 8;
            }

            // build gradient from colors
            var colorKeys = new GradientColorKey[length];
            var alphaKeys = new GradientAlphaKey[length];

            float steps = length - 1f;
            for (int i = 0; i < length; i++)
            {
                float step = i / steps;
                colorKeys[i].color = colors[i];
                colorKeys[i].time = step;
                alphaKeys[i].alpha = colors[i].a;
                alphaKeys[i].time = step;
            }

            // create gradient
            Gradient gradient = new Gradient();
            gradient.SetKeys(colorKeys, alphaKeys);

            // create texture
            Texture2D outputTex = new Texture2D(width, height, TextureFormat.ARGB32, false, isLinear);
            outputTex.wrapMode = textureWrapMode;
            outputTex.filterMode = filterMode;

            // draw texture
            for (int i = 0; i < width; i++)
            {
                outputTex.SetPixel(i, 0, gradient.Evaluate((float)i / (float)width));
            }
            outputTex.Apply(false);

            return outputTex;
        } // BuildGradientTexture

    } // class
} // namespcae
