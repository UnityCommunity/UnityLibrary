using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// AssetBundle cache checker & loader with caching
// worsk by loading .manifest file from server and parsing hash string from it

namespace UnityLibrary
{
    public class AssetBundleLoader : MonoBehaviour
    {
        public string assetBundleURL = "http://localhost/bundle";

        void Start()
        {
            //StartCoroutine(DownloadAndCache(assetBundleURL));
        }

        /// <summary>
        /// load assetbundle manifest, check hash, load actual bundle with hash parameter to use caching
        /// instantiate gameobject
        /// </summary>
        /// <param name="bundleURL">full url to assetbundle file</param>
        /// <param name="assetName">optional parameter to access specific asset from assetbundle</param>
        /// <returns></returns>
        IEnumerator DownloadAndCache(string bundleURL, string assetName = "")
        {
            // Wait for the Caching system to be ready
            while (!Caching.ready)
            {
                yield return null;
            }

            // if you want to always load from server, can clear cache first
            //        Caching.CleanCache();

            // get current bundle hash from server, random value added to avoid caching
            UnityWebRequest www = UnityWebRequest.Get(bundleURL + ".manifest?r=" + (Random.value * 9999999));
            Debug.Log("Loading manifest:" + bundleURL + ".manifest");

            // wait for load to finish
            yield return www.Send();

            // if received error, exit
            if (www.isNetworkError == true)
            {
                Debug.LogError("www error: " + www.error);
                www.Dispose();
                www = null;
                yield break;
            }

            // create empty hash string
            Hash128 hashString = (default(Hash128));// new Hash128(0, 0, 0, 0);

            // check if received data contains 'ManifestFileVersion'
            if (www.downloadHandler.text.Contains("ManifestFileVersion"))
            {
                // extract hash string from the received data, TODO should add some error checking here
                var hashRow = www.downloadHandler.text.ToString().Split("\n".ToCharArray())[5];
                hashString = Hash128.Parse(hashRow.Split(':')[1].Trim());

                if (hashString.isValid == true)
                {
                    // we can check if there is cached version or not
                    if (Caching.IsVersionCached(bundleURL, hashString) == true)
                    {
                        Debug.Log("Bundle with this hash is already cached!");
                    } else
                    {
                        Debug.Log("No cached version founded for this hash..");
                    }
                } else
                {
                    // invalid loaded hash, just try loading latest bundle
                    Debug.LogError("Invalid hash:" + hashString);
                    yield break;
                }

            } else
            {
                Debug.LogError("Manifest doesn't contain string 'ManifestFileVersion': " + bundleURL + ".manifest");
                yield break;
            }

            // now download the actual bundle, with hashString parameter it uses cached version if available
            www = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL + "?r=" + (Random.value * 9999999), hashString, 0);

            // wait for load to finish
            yield return www.Send();

            if (www.error != null)
            {
                Debug.LogError("www error: " + www.error);
                www.Dispose();
                www = null;
                yield break;
            }

            // get bundle from downloadhandler
            AssetBundle bundle = ((DownloadHandlerAssetBundle)www.downloadHandler).assetBundle;

            GameObject bundlePrefab = null;

            // if no asset name is given, take the first/main asset
            if (assetName == "")
            {
                bundlePrefab = (GameObject)bundle.LoadAsset(bundle.GetAllAssetNames()[0]);
            } else
            { // use asset name to access inside bundle
                bundlePrefab = (GameObject)bundle.LoadAsset(assetName);
            }

            // if we got something out
            if (bundlePrefab != null)
            {

                // instantiate at 0,0,0 and without rotation
                Instantiate(bundlePrefab, Vector3.zero, Quaternion.identity);

                /*
                // fix pink shaders, NOTE: not always needed..
                foreach (Renderer r in go.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    // FIXME: creates multiple materials, not good
                    var material = Shader.Find(r.material.shader.name);
                    r.material.shader = null;
                    r.material.shader = material;
                }*/
            }

            www.Dispose();
            www = null;

            // try to cleanup memory
            Resources.UnloadUnusedAssets();
            bundle.Unload(false);
            bundle = null;
        }
    }
}
