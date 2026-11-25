using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 

public class GestorAmuletos : MonoBehaviour
{
    [Header("Zonas de Encaje")]
    public XRSocketInteractor socket1;
    public XRSocketInteractor socket2;
    public XRSocketInteractor socket3;

    [Header("Recompensa / Feedback")]
    public GameObject textoEspecial; 
    public AudioClip sfxCompletado; // Sonido extra opcional al colocar los 3

    private int elementosColocados = 0;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (textoEspecial != null) 
        {
            textoEspecial.SetActive(false); // Oculta el texto al iniciar la escena
        }
    }

    void OnEnable()
    {
        // Suscribirse a los eventos de colocar y quitar objetos
        socket1.selectEntered.AddListener(AlObjetoColocado);
        socket2.selectEntered.AddListener(AlObjetoColocado);
        socket3.selectEntered.AddListener(AlObjetoColocado);

        socket1.selectExited.AddListener(AlObjetoRetirado);
        socket2.selectExited.AddListener(AlObjetoRetirado);
        socket3.selectExited.AddListener(AlObjetoRetirado);
    }

    void OnDisable()
    {
        // Limpiar las suscripciones para evitar fugas de memoria
        socket1.selectEntered.RemoveListener(AlObjetoColocado);
        socket2.selectEntered.RemoveListener(AlObjetoColocado);
        socket3.selectEntered.RemoveListener(AlObjetoColocado);

        socket1.selectExited.RemoveListener(AlObjetoRetirado);
        socket2.selectExited.RemoveListener(AlObjetoRetirado);
        socket3.selectExited.RemoveListener(AlObjetoRetirado);
    }

    private void AlObjetoColocado(SelectEnterEventArgs args)
    {
        elementosColocados++;
        VerificarCompletado();
    }

    private void AlObjetoRetirado(SelectExitEventArgs args)
    {
        elementosColocados--;
        if (textoEspecial != null) 
        {
            textoEspecial.SetActive(false); // Oculta el texto si el jugador retira un elemento
        }
    }

    private void VerificarCompletado()
    {
        if (elementosColocados >= 3)
        {
            if (textoEspecial != null) textoEspecial.SetActive(true);
            
            if (sfxCompletado != null && audioSource != null)
            {
                audioSource.PlayOneShot(sfxCompletado);
            }
        }
    }
}