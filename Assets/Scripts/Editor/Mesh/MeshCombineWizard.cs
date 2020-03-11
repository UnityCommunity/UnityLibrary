// from https://forum.unity.com/threads/mesh-combine-wizard-free-unity-tool-source-code.444483/#post-5575042

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace UnityLibrary
{
  public class MeshCombineWizard : ScriptableWizard
  {
      public GameObject parentOfObjectsToCombine;

      [MenuItem("E.S. Tools/Mesh Combine Wizard")]
      static void CreateWizard()
      {
          ScriptableWizard.DisplayWizard<MeshCombineWizard>("Mesh Combine Wizard");
      }

      void OnWizardCreate()
      {
          if (parentOfObjectsToCombine == null) return;

          Vector3 originalPosition = parentOfObjectsToCombine.transform.position;
          parentOfObjectsToCombine.transform.position = Vector3.zero;

          MeshFilter[] meshFilters = parentOfObjectsToCombine.GetComponentsInChildren<MeshFilter>();
          Dictionary<Material, List<MeshFilter>> materialToMeshFilterList = new Dictionary<Material, List<MeshFilter>>();
          List<GameObject> combinedObjects = new List<GameObject>();

          for (int i = 0; i < meshFilters.Length; i++)
          {
              var materials = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
              if (materials == null) continue;
              if (materials.Length > 1)
              {
                  parentOfObjectsToCombine.transform.position = originalPosition;
                  Debug.LogError("Objects with multiple materials on the same mesh are not supported. Create multiple meshes from this object's sub-meshes in an external 3D tool and assign separate materials to each.");
                  return;
              }
              var material = materials[0];
              if (materialToMeshFilterList.ContainsKey(material)) materialToMeshFilterList[material].Add(meshFilters[i]);
              else materialToMeshFilterList.Add(material, new List<MeshFilter>() { meshFilters[i] });
          }


          // NC 03/2020 changes the loop to create meshes smaller than 695536 vertices
          foreach (var entry in materialToMeshFilterList)
          {
              // get list of each meshes order by number of vertices
              List<MeshFilter> meshesWithSameMaterial = entry.Value.OrderByDescending(mf => mf.sharedMesh.vertexCount).ToList();
              // split into bins of 65536 vertices max
              int nrMeshes = meshesWithSameMaterial.Count;
              while (nrMeshes > 0)
              {
                  string materialName = entry.Key.ToString().Split(' ')[0];
                  List<MeshFilter> meshBin = new List<MeshFilter>();
                  meshBin.Add(meshesWithSameMaterial[0]);
                  meshesWithSameMaterial.RemoveAt(0);
                  nrMeshes--;
                  // add meshes in bin until 65536 vertices is reached
                  for (int i = 0; i < meshesWithSameMaterial.Count; i++)
                  {
                      if (meshBin.Sum(mf => mf.sharedMesh.vertexCount) + meshesWithSameMaterial[i].sharedMesh.vertexCount < 65536)
                      {
                          meshBin.Add(meshesWithSameMaterial[i]);
                          meshesWithSameMaterial.RemoveAt(i);
                          i--;
                          nrMeshes--;
                      }
                  }

                  // merge this bin
                  CombineInstance[] combine = new CombineInstance[meshBin.Count];
                  for (int i = 0; i < meshBin.Count; i++)
                  {
                      combine[i].mesh = meshBin[i].sharedMesh;
                      combine[i].transform = meshBin[i].transform.localToWorldMatrix;
                  }
                  Mesh combinedMesh = new Mesh();
                  combinedMesh.CombineMeshes(combine);

                  // save the mesh
                  materialName += "_" + combinedMesh.GetInstanceID();
                  if (meshBin.Count > 1)
                  {
                      AssetDatabase.CreateAsset(combinedMesh, "Assets/CombinedMeshes_" + materialName + ".asset"); ;
                  }

                  // assign the mesh to a new go
                  string goName = (materialToMeshFilterList.Count > 1) ? "CombinedMeshes_" + materialName : "CombinedMeshes_" + parentOfObjectsToCombine.name;
                  GameObject combinedObject = new GameObject(goName);
                  var filter = combinedObject.AddComponent<MeshFilter>();
                  if (meshBin.Count > 1)
                  {
                      filter.sharedMesh = combinedMesh;
                  }
                  else
                  {
                      filter.sharedMesh = meshBin[0].sharedMesh; // the original mesh
                  }
                  var renderer = combinedObject.AddComponent<MeshRenderer>();
                  renderer.sharedMaterial = entry.Key;
                  combinedObjects.Add(combinedObject);
              }
          }

          GameObject resultGO = null;
          if (combinedObjects.Count > 1)
          {
              resultGO = new GameObject("CombinedMeshes_" + parentOfObjectsToCombine.name);
              foreach (var combinedObject in combinedObjects) combinedObject.transform.parent = resultGO.transform;
          }
          else
          {
              resultGO = combinedObjects[0];
          }

          Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/" + resultGO.name + ".prefab");
          PrefabUtility.ReplacePrefab(resultGO, prefab, ReplacePrefabOptions.ConnectToPrefab);

          parentOfObjectsToCombine.SetActive(false);
          parentOfObjectsToCombine.transform.position = originalPosition;
          resultGO.transform.position = originalPosition;
      }
  }
}
