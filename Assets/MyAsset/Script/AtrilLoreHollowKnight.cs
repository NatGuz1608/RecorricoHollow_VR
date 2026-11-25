using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections; // Necesario para las Corrutinas

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
[RequireComponent(typeof(AudioSource))]
public class AtrilLoreHollowKnight : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public GameObject cartelDescripcionPersonaje; 
    public MeshRenderer renderizadorBotonRojo; 
    public Material materialBotonReposo;
    public Material materialBotonActivado; 

    [Header("Feedback Sonoro")]
    public AudioClip clipVozPersonaje; 
    public AudioClip sfxClickBoton; 
    // NUEVA REFERENCIA: El Audio Source de la música ambiente
    public AudioSource musicaAmbiente; 

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private AudioSource fuenteAudio;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        fuenteAudio = GetComponent<AudioSource>();
        
        fuenteAudio.spatialBlend = 1.0f; 
        fuenteAudio.playOnAwake = false;
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(AlPresionarBoton);
        interactable.selectExited.AddListener(AlSoltarBoton); 
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(AlPresionarBoton);
        interactable.selectExited.RemoveListener(AlSoltarBoton);
    }

    private void AlPresionarBoton(SelectEnterEventArgs args)
    {
        if (renderizadorBotonRojo != null)
            renderizadorBotonRojo.material = materialBotonActivado; 

        if (cartelDescripcionPersonaje != null)
        {
            bool nuevoEstado = !cartelDescripcionPersonaje.activeSelf;
            cartelDescripcionPersonaje.SetActive(nuevoEstado); 

            fuenteAudio.Stop();
            StopAllCoroutines(); // Detenemos cualquier espera anterior

            if (sfxClickBoton != null)
                fuenteAudio.PlayOneShot(sfxClickBoton);

            if (nuevoEstado)
            {
                // El cartel se ENCIENDE
                if (musicaAmbiente != null)
                {
                    musicaAmbiente.Pause(); // Pausamos la música ambiente
                }

                if (clipVozPersonaje != null)
                {
                    fuenteAudio.clip = clipVozPersonaje;
                    fuenteAudio.Play();
                    // Iniciamos la rutina para esperar a que termine la voz y reanudar la música
                    StartCoroutine(EsperarAudioTermine(clipVozPersonaje.length));
                }
            }
            else
            {
                // El cartel se APAGA
                if (musicaAmbiente != null && !musicaAmbiente.isPlaying)
                {
                    musicaAmbiente.Play(); // Reanudamos la música si se cierra el cartel antes
                }
            }
        }
    }

    private void AlSoltarBoton(SelectExitEventArgs args)
    {
        if (renderizadorBotonRojo != null)
            renderizadorBotonRojo.material = materialBotonReposo; 
    }

    // Corrutina que espera la duración del audio para volver a encender la música
    private IEnumerator EsperarAudioTermine(float duracion)
    {
        yield return new WaitForSeconds(duracion);
        
        // Si el cartel sigue activo después de que termina el audio, reanudamos la música
        if (cartelDescripcionPersonaje.activeSelf && musicaAmbiente != null)
        {
            musicaAmbiente.Play();
        }
    }
}