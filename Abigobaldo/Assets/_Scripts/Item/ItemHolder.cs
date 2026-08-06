using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    [Header("Arremesso")]
    [SerializeField] private float dropDistance = 1f;
    [SerializeField] private float throwForce = 8f;

    [Header("Retração na Parede")]
    [SerializeField] private Transform cameraTransform;      // Arraste a Main Camera aqui
    [SerializeField] private LayerMask obstacleLayers;       // Camadas que contam como parede (ex: Default)
    [SerializeField] private float itemRadius = 0.15f;       // Espessura/volume do item para checagem
    [SerializeField] private float retractionSpeed = 12f;    // Velocidade com que ele recua/volta
    [SerializeField] private float minDistanceOffset = 0.1f; // Distância mínima para não colar na parede

    private Item currentItem;
    private Vector3 defaultLocalPosition;

    public Item CurrentItem => currentItem;

    private void Awake()
    {
        // Salva a posição original do ItemHolder relativa à Câmera/Player
        defaultLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        HandleWallRetraction();
    }

    private void HandleWallRetraction()
    {
        if (cameraTransform == null)
            return;

        // Posição desejada do ItemHolder sem obstáculos
        Vector3 targetWorldPos = cameraTransform.TransformPoint(defaultLocalPosition);
        Vector3 direction = targetWorldPos - cameraTransform.position;
        float maxDistance = direction.magnitude;

        // Se estiver segurando um item e colidir com uma parede no caminho...
        if (!IsEmpty() && Physics.SphereCast(cameraTransform.position, itemRadius, direction.normalized, out RaycastHit hit, maxDistance, obstacleLayers))
        {
            // Calcula o ponto limite para o item não atravessar
            float safeDistance = Mathf.Max(0.1f, hit.distance - minDistanceOffset);
            Vector3 targetLocalPos = new Vector3(defaultLocalPosition.x, defaultLocalPosition.y, safeDistance);

            // Suaviza a aproximação
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * retractionSpeed);
        }
        else
        {
            // Se o caminho estiver livre, volta para a posição normal
            transform.localPosition = Vector3.Lerp(transform.localPosition, defaultLocalPosition, Time.deltaTime * retractionSpeed);
        }
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public bool TryPickUp(Item item)
    {
        if (item == null)
            return false;

        if (currentItem != null)
            return false;

        if (!item.CanBeHeld)
            return false;

        currentItem = item;

        item.PickUp(transform);

        return true;
    }

    public Item RemoveItem()
    {
        if (currentItem == null)
            return null;

        Item item = currentItem;

        currentItem = null;

        return item;
    }

    public bool DropItem()
    {
        if (currentItem == null)
            return false;

        Item item = RemoveItem();

        Vector3 position =
            transform.position +
            transform.forward * dropDistance;

        item.Drop(position);

        return true;
    }

    public bool ThrowItem()
    {
        if (currentItem == null)
            return false;

        if (!currentItem.CanBeThrown)
            return false;

        Item item = RemoveItem();

        Vector3 position =
            transform.position +
            transform.forward * dropDistance;

        item.Throw(
            position,
            transform.forward,
            throwForce
        );

        return true;
    }
}