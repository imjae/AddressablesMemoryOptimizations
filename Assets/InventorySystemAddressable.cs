using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InventorySystemAddressable : MonoBehaviour
{
    public AssetReferenceGameObject[] inventoryItems;
    public Transform[] spawnPositions;
    Dictionary<int, List<GameObject>> spawnedInstantiateObjects = new Dictionary<int, List<GameObject>>();
    Dictionary<int, List<GameObject>> spawnedAssetObject = new Dictionary<int, List<GameObject>>();
    public void SpawnItem(int itemNumber)
    {
        Debug.Log("Spawning item " + itemNumber);
        if (!spawnedInstantiateObjects.ContainsKey(itemNumber))
        {
            spawnedInstantiateObjects.Add(itemNumber, new List<GameObject>());
        }

        if (spawnedInstantiateObjects[itemNumber].Count > 0)
        {
            Vector3 randomPos = new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-1.5f, 1.5f), 0);
            Vector3 targetPos = spawnPositions[itemNumber].position + randomPos;
            StartCoroutine(WaitForSpawnComplete(Addressables.LoadAssetAsync<GameObject>(inventoryItems[itemNumber]), targetPos, spawnPositions[itemNumber].rotation, itemNumber));
        }
        else
        {
            StartCoroutine(WaitForSpawnComplete(Addressables.LoadAssetAsync<GameObject>(inventoryItems[itemNumber]), spawnPositions[itemNumber].position, spawnPositions[itemNumber].rotation, itemNumber));
        }
    }



    IEnumerator WaitForSpawnComplete(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> op, Vector3 position, Quaternion rotation, int itemNumber)
    {
        while (op.IsDone == false)
        {
            yield return op;
        }

        OnSpawnComplete(op, itemNumber, position, rotation);
    }

    public void DespawnItem(int itemNumber)
    {
        if (spawnedInstantiateObjects.TryGetValue(itemNumber, out var value))
        {
            foreach (var entry in value)
            {
                Destroy(entry);
            }

            value.Clear();
        }
        else
        {
            return;
        }

        if (spawnedAssetObject.TryGetValue(itemNumber, out var value2))
        {
            foreach (var entry in value2)
            {
                Addressables.ReleaseInstance(entry);
            }

            value.Clear();
        }
        else
        {
            return;
        }
    }

    void OnSpawnComplete(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> handle, int itemNumber, Vector3 position, Quaternion rotation)
    {
        var ob = GameObject.Instantiate(handle.Result, position, rotation);
        if (spawnedInstantiateObjects.TryGetValue(itemNumber, out var value))
        {
            value.Add(ob);
        }
        else
        {
            spawnedInstantiateObjects.Add(itemNumber, new List<GameObject>() { ob });
        }

        if (spawnedAssetObject.TryGetValue(itemNumber, out var value2))
        {
            value2.Add(handle.Result);
        }
        else
        {
            spawnedAssetObject.Add(itemNumber, new List<GameObject>() { handle.Result });
        }
    }

    public void SpawnAll(int amount)
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            for (int j = 0; j < amount; j++)
            {
                SpawnItem(i);
            }
        }
    }
}
