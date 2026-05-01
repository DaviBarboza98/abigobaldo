using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("=== CONFIGURAÇÕES ===")]
    public float interactRange = 1.5f;
    public Transform holdPoint;
    public Item heldItem;
    [Range(1f, 20f)] public float throwForce = 10f;
    [Range(1f, 20f)] public float maxThrowForce = 20f;
    [Range(1f, 20f)] public float chargeSpeed = 15f;

    private float currentThrowForce;
    private bool isChargingThrow;

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

        // THROW
        HandleThrow();
    }

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

    void HandleThrow()
    {
        if (heldItem == null) return;

        // Começa a carregar
        if (Input.GetMouseButtonDown(1))
        {
            isChargingThrow = true;
            currentThrowForce = throwForce;
        }

        // Carregando
        if (Input.GetMouseButton(1) && isChargingThrow)
        {
            currentThrowForce += chargeSpeed * Time.deltaTime;
            currentThrowForce = Mathf.Clamp(currentThrowForce, throwForce, maxThrowForce);

            Debug.Log($"[ARREMESSO] Força atual: {currentThrowForce:F2}");
        }

        // Soltou → joga
        if (Input.GetMouseButtonUp(1) && isChargingThrow)
        {
            Debug.Log($"[ARREMESSO] Arremessado, Força final: {currentThrowForce:F2}");

            ThrowItem();
            isChargingThrow = false;
        }
    }

    void ThrowItem()
    {
        Item item = heldItem;

        DetachFromHold(item);
        EnablePhysics(item);

        Vector3 throwDirection = transform.forward;
        throwDirection.y = 0.3f;

        item.rb.AddForce(throwDirection.normalized * currentThrowForce, ForceMode.Impulse);

        heldItem = null;
    }
}