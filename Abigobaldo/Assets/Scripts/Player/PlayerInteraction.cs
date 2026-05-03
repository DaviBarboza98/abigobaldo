using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("=== CONFIGURAÇÕES ===")]
    public float interactRange = 1.5f;
    public Transform holdPoint;
    public Item heldItem;

    [Header("=== ARREMESSO ===")]
    public float throwForce = 10f;

    [Header("=== DEPÓSITO (ENTREGA NO CUBO) ===")]
    public string depositTag = "DepositCube";   // Tag do cubo específico
    public GameObject spawnPrefab;              // Prefab que será criado em cima
    public float spawnHeightOffset = 1f;        // Altura acima do cubo
    [Range(0f, 1f)]
    public float spawnTowardPlayer = 0.3f;      // 0 = centro do cubo | 1 = no player
    public KeyCode depositKey = KeyCode.F;      // Tecla para depositar

    void Update()
    {
        // INTERAÇÃO (pegar/soltar)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem == null)
                TryPickup();
            else
                DropItem();
        }

        // ARREMESSO (clique direito → lança na hora)
        if (Input.GetMouseButtonDown(1) && heldItem != null)
        {
            ThrowItem();
        }

        // DEPÓSITO (tecla F → entrega item no cubo específico)
        if (Input.GetKeyDown(depositKey) && heldItem != null)
        {
            TryDepositOnCube();
        }
    }

    void TryDepositOnCube()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);

        Transform closestCube = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(depositTag)) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCube = hit.transform;
            }
        }

        if (closestCube != null)
        {
            DepositItem(closestCube);
        }
        else
        {
            Debug.Log("[DEPÓSITO] Nenhum cubo de depósito por perto.");
        }
    }

    void DepositItem(Transform targetCube)
    {
        // 1️⃣ Remove o item da mão (destrói)
        DetachFromHold(heldItem);
        Destroy(heldItem.gameObject);
        heldItem = null;

        // 2️⃣ Calcula a posição BASE em cima do cubo
        Vector3 spawnPosition = targetCube.position;
        spawnPosition.y += (targetCube.localScale.y / 2f) + spawnHeightOffset;

        // 3️⃣ 🆕 Move a posição um pouco em direção ao player (mantendo a altura)
        Vector3 directionToPlayer = transform.position - targetCube.position;
        directionToPlayer.y = 0; // Ignora altura para não puxar pra baixo/cima
        spawnPosition += directionToPlayer * spawnTowardPlayer;

        // 4️⃣ Instancia o prefab
        if (spawnPrefab != null)
        {
            Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"[DEPÓSITO] Item entregue! Prefab criado em {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("[DEPÓSITO] Nenhum prefab definido no Inspector!");
        }
    }

    // ===================================================================
    // MÉTODOS ORIGINAIS (sem alterações)
    // ===================================================================
    void EnablePhysics(Item item)
    {
        item.rb.isKinematic = false;
        item.rb.useGravity = true;
        item.col.enabled = true;
    }

    void DisablePhysics(Item item)
    {
        item.rb.velocity = Vector3.zero;
        item.rb.angularVelocity = Vector3.zero;

        item.rb.isKinematic = true;
        item.rb.useGravity = false;
        item.col.enabled = false;
    }

    void AttachToHold(Item item)
    {
        item.followTarget = holdPoint;
        item.transform.SetParent(holdPoint);
    }

    void DetachFromHold(Item item)
    {
        item.followTarget = null;
        item.transform.SetParent(null);
    }

    void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);

        Item closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Item")) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = hit.GetComponent<Item>();
            }
        }

        if (closestItem != null)
        {
            PickupItem(closestItem);
        }
    }

    void PickupItem(Item item)
    {
        heldItem = item;

        DisablePhysics(item);
        AttachToHold(item);
    }

    void DropItem()
    {
        DetachFromHold(heldItem);
        EnablePhysics(heldItem);

        heldItem = null;
    }

    void ThrowItem()
    {
        Item item = heldItem;

        DetachFromHold(item);
        EnablePhysics(item);

        Vector3 throwDirection = transform.forward;
        throwDirection.y = 0.3f;

        item.rb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);

        Debug.Log($"[ARREMESSO] Arremessado com força: {throwForce}");

        heldItem = null;
    }
}