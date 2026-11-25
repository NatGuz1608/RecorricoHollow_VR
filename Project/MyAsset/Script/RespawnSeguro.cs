using UnityEngine;

/// <summary>
/// Monitors the world-space position of an XR grabbable object and automatically
/// resets it to its initial spawn transform if it falls out of bounds or exceeds
/// the maximum allowed distance from its origin.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class RespawnSeguro : MonoBehaviour
{
    [Header("Límites de Distancia")]
    /// <summary>
    /// Maximum distance in meters that the object can move away from its initial spawn point.
    /// </summary>
    [Tooltip("Distancia máxima en metros que el objeto se puede alejar de su punto de origen.")]
    public float distanciaMaxima = 15f;

    /// <summary>
    /// Minimum Y-axis elevation threshold. If the object falls below this height, it will respawn.
    /// </summary>
    [Tooltip("Altura mínima en el eje Y. Si cae por debajo, reaparecerá.")]
    public float limiteCaidaY = -2f;

    /// <summary>
    /// Initial world-space position recorded when the scene loads.
    /// </summary>
    private Vector3 posicionInicial;

    /// <summary>
    /// Initial world-space rotation recorded when the scene loads.
    /// </summary>
    private Quaternion rotacionInicial;

    /// <summary>
    /// Cached reference to the object's <see cref="Rigidbody"/> component.
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// Cached reference to the object's <see cref="UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable"/> component.
    /// </summary>
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;

    /// <summary>
    /// Caches the initial transform coordinates and required physics/XR components on startup.
    /// </summary>
    void Start()
    {
        // Store the exact spawn position and rotation upon scene initialization
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    /// <summary>
    /// Evaluates distance and altitude limits every frame while the object is not held or socketed.
    /// </summary>
    void Update()
    {
        // Do not reset the object while the player is holding it or while it is snapped into an XRSocketInteractor
        if (interactable.isSelected)
        {
            return;
        }

        // Calculate current distance from the initial spawn point while loose in the scene
        float distanciaActual = Vector3.Distance(posicionInicial, transform.position);

        // Trigger respawn if the object rolled beyond the maximum distance or fell below the floor threshold
        if (distanciaActual > distanciaMaxima || transform.position.y < limiteCaidaY)
        {
            RegresarAlInicio();
        }
    }

    /// <summary>
    /// Teleports the object back to its initial position and rotation while clearing residual physics momentum.
    /// </summary>
    public void RegresarAlInicio()
    {
        // Restore original world position and rotation
        transform.position = posicionInicial;
        transform.rotation = rotacionInicial;

        // Reset linear and angular velocities so the object remains stationary upon respawning
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}