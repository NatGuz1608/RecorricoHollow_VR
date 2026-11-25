using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 
using UnityEngine.XR.Interaction.Toolkit.Interactables; 
using TMPro; 
using System.Collections; 

public class GestorMinijuegoCristales : MonoBehaviour
{
    [Header("Cristales (Para iniciar el juego)")]
    public XRGrabInteractable cristal1;
    public XRGrabInteractable cristal2;
    public XRGrabInteractable cristal3;

    [Header("Zonas de Encaje (Altar)")]
    public XRSocketInteractor socket1;
    public XRSocketInteractor socket2;
    public XRSocketInteractor socket3;

    [Header("Interfaz de Usuario (Canvas)")]
    public TextMeshProUGUI textoTemporizador;
    public TextMeshProUGUI textoContador;
    public GameObject pantallaVictoria;
    public GameObject pantallaDerrota;

    [Header("Configuración del Juego")]
    public float tiempoMaximoEnMinutos = 5f; 
    public float tiempoOcultarPaneles = 30f; 
    
    private float tiempoRestante;
    private bool juegoActivo = false;
    private bool minijuegoIniciado = false; 
    private int cristalesColocados = 0;

    void Start()
    {
        tiempoRestante = tiempoMaximoEnMinutos * 60f;
        cristalesColocados = 0;
        
        juegoActivo = false; 
        minijuegoIniciado = false;

        if (pantallaVictoria != null) pantallaVictoria.SetActive(false);
        if (pantallaDerrota != null) pantallaDerrota.SetActive(false);
        
        if (textoTemporizador != null) textoTemporizador.gameObject.SetActive(false);
        if (textoContador != null) textoContador.gameObject.SetActive(false);
        
        ActualizarTextosUI();

        // Ejecuta la reactivación XR un fotograma después de que la escena anterior termine de destruirse
        StartCoroutine(ReactivarSistemaXREnEscena());
    }

    private IEnumerator ReactivarSistemaXREnEscena()
    {
        // Espera 1 fotograma para que el OnDisable() de la escena "menu" termine por completo
        yield return null;

        // 1. Forzar el encendido de todos los InputActionAssets en memoria (XRI Default Input Actions y Simulador)
        InputActionAsset[] actionAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
        foreach (var asset in actionAssets)
        {
            if (asset != null)
            {
                asset.Enable();
            }
        }

        // 2. Reactivar cualquier InputActionManager presente en la escena Principal
        InputActionManager[] inputManagers = FindObjectsByType<InputActionManager>(FindObjectsSortMode.None);
        foreach (var im in inputManagers)
        {
            im.enabled = false;
            im.enabled = true;
            im.EnableInput();
        }

        // 3. Obtener el XRInteractionManager propio de la escena Principal
        XRInteractionManager managerPrincipal = FindFirstObjectByType<XRInteractionManager>();
        if (managerPrincipal != null)
        {
            managerPrincipal.enabled = true;
        }

        // 4. Reiniciar el XR Origin de la escena Principal para refrescar el XRInputModalityManager y los mandos
        if (Camera.main != null)
        {
            GameObject xrOriginRoot = Camera.main.transform.root.gameObject;
            xrOriginRoot.SetActive(false);
            xrOriginRoot.SetActive(true);
        }

        // 5. Reconectar todos los Interactors (Near-Far, Poke, Teleport, Sockets) al Manager de Principal
        XRBaseInteractor[] interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in interactors)
        {
            if (managerPrincipal != null)
            {
                interactor.interactionManager = managerPrincipal;
            }
            interactor.enabled = false;
            interactor.enabled = true;
        }

        // 6. Reconectar todos los Interactables (Cristales, Atriles, etc.) al Manager de Principal
        XRBaseInteractable[] interactables = FindObjectsByType<XRBaseInteractable>(FindObjectsSortMode.None);
        foreach (var interactable in interactables)
        {
            if (managerPrincipal != null)
            {
                interactable.interactionManager = managerPrincipal;
            }
            interactable.enabled = false;
            interactable.enabled = true;
        }

        // 7. Refrescar el simulador en DontDestroyOnLoad para que recupere las entradas de teclado/ratón
        MonoBehaviour[] todosLosScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in todosLosScripts)
        {
            if (script != null && script.GetType().Name.Contains("Simulator"))
            {
                script.enabled = false;
                script.enabled = true;
            }
        }

        Debug.Log("Sistema XR en escena Principal sincronizado y activo.");
    }

    void OnEnable()
    {
        if (cristal1 != null) cristal1.selectEntered.AddListener(AlCristalAgarrado);
        if (cristal2 != null) cristal2.selectEntered.AddListener(AlCristalAgarrado);
        if (cristal3 != null) cristal3.selectEntered.AddListener(AlCristalAgarrado);

        socket1.selectEntered.AddListener(AlCristalColocado);
        socket2.selectEntered.AddListener(AlCristalColocado);
        socket3.selectEntered.AddListener(AlCristalColocado);

        socket1.selectExited.AddListener(AlCristalRetirado);
        socket2.selectExited.AddListener(AlCristalRetirado);
        socket3.selectExited.AddListener(AlCristalRetirado);
    }

    void OnDisable()
    {
        if (cristal1 != null) cristal1.selectEntered.RemoveListener(AlCristalAgarrado);
        if (cristal2 != null) cristal2.selectEntered.RemoveListener(AlCristalAgarrado);
        if (cristal3 != null) cristal3.selectEntered.RemoveListener(AlCristalAgarrado);

        socket1.selectEntered.RemoveListener(AlCristalColocado);
        socket2.selectEntered.RemoveListener(AlCristalColocado);
        socket3.selectEntered.RemoveListener(AlCristalColocado);

        socket1.selectExited.RemoveListener(AlCristalRetirado);
        socket2.selectExited.RemoveListener(AlCristalRetirado);
        socket3.selectExited.RemoveListener(AlCristalRetirado);
    }

    void Update()
    {
        if (!juegoActivo) return;

        tiempoRestante -= Time.deltaTime; 

        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            PerderJuego(); 
        }

        ActualizarTextosUI();
    }

    private void IniciarJuego()
    {
        if (!minijuegoIniciado)
        {
            minijuegoIniciado = true;
            juegoActivo = true; 
            
            if (textoTemporizador != null) textoTemporizador.gameObject.SetActive(true);
            if (textoContador != null) textoContador.gameObject.SetActive(true);
        }
    }

    private void AlCristalAgarrado(SelectEnterEventArgs args)
    {
        IniciarJuego();
    }

    private void AlCristalColocado(SelectEnterEventArgs args)
    {
        IniciarJuego();

        cristalesColocados++;
        ActualizarTextosUI();
        
        if (cristalesColocados >= 3 && tiempoRestante > 0)
        {
            GanarJuego();
        }
    }

    private void AlCristalRetirado(SelectExitEventArgs args)
    {
        cristalesColocados--;
        ActualizarTextosUI();
    }

    private void ActualizarTextosUI()
    {
        if (textoContador != null) 
            textoContador.text = $"Cristales: {cristalesColocados} / 3";

        if (textoTemporizador != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTemporizador.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    private void GanarJuego()
    {
        juegoActivo = false; 
        OcultarHUD(); 

        if (pantallaVictoria != null) 
        {
            pantallaVictoria.SetActive(true);
            StartCoroutine(OcultarPantallaDespuesDeTiempo(pantallaVictoria, tiempoOcultarPaneles)); 
        }
    }

    private void PerderJuego()
    {
        juegoActivo = false; 
        OcultarHUD(); 

        if (pantallaDerrota != null) 
        {
            pantallaDerrota.SetActive(true);
            StartCoroutine(OcultarPantallaDespuesDeTiempo(pantallaDerrota, tiempoOcultarPaneles)); 
        }
    }

    private void OcultarHUD()
    {
        if (textoTemporizador != null) textoTemporizador.gameObject.SetActive(false);
        if (textoContador != null) textoContador.gameObject.SetActive(false);
    }

    private IEnumerator OcultarPantallaDespuesDeTiempo(GameObject pantallaActiva, float tiempoEspera)
    {
        yield return new WaitForSeconds(tiempoEspera);
        
        if (pantallaActiva != null)
        {
            pantallaActiva.SetActive(false);
        }
    }
}

