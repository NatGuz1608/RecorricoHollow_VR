using UnityEngine;


// Obliga a Unity a requerir estos componentes para evitar errores de referencia
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class RespawnSeguro : MonoBehaviour
{
    [Header("Límites de Distancia")]
    [Tooltip("Distancia máxima en metros que el objeto se puede alejar de su punto de origen.")]
    public float distanciaMaxima = 15f; 
    
    [Tooltip("Altura mínima en el eje Y. Si cae por debajo, reaparecerá.")]
    public float limiteCaidaY = -2f;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;

    void Start()
    {
        // Guardamos el punto de origen exacto al cargar la escena
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        
        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void Update()
    {
        // LA REGLA DE ORO: Si el jugador lo tiene en la mano, o si está encajado 
        // en uno de los XRSocketInteractor de tu GestorAmuletos, no hacemos nada.
        if (interactable.isSelected) 
        {
            return; 
        }

        // Si el objeto está suelto en el mundo, calculamos qué tan lejos está del origen
        float distanciaActual = Vector3.Distance(posicionInicial, transform.position);

        // Si rodó demasiado lejos (distancia máxima) o si atravesó el suelo (límite Y)
        if (distanciaActual > distanciaMaxima || transform.position.y < limiteCaidaY)
        {
            RegresarAlInicio();
        }
    }

    public void RegresarAlInicio()
    {
        // Lo teletransportamos suavemente de vuelta a su sitio original
        transform.position = posicionInicial;
        transform.rotation = rotacionInicial;

        // Limpiamos las fuerzas de las físicas para que no conserve la inercia
        // y se quede completamente quieto al reaparecer.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}