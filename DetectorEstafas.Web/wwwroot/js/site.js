"use strict";

(() => {
    const recorderRoot = document.querySelector("[data-audio-recorder]");

    if (!recorderRoot) {
        return;
    }

    const consentimiento = document.getElementById("audio-consentimiento");
    const iniciar = document.getElementById("audio-iniciar-grabacion");
    const detener = document.getElementById("audio-detener-grabacion");
    const tiempo = document.getElementById("audio-tiempo-grabacion");
    const estado = document.getElementById("audio-estado-grabacion");
    const vistaPrevia = document.getElementById("audio-vista-previa");
    const inputAudio = document.getElementById("audio");

    if (!consentimiento || !iniciar || !detener || !tiempo ||
        !estado || !vistaPrevia || !inputAudio) {
        return;
    }

    const duracionMaximaSegundos = 60;
    let flujo = null;
    let contextoAudio = null;
    let fuente = null;
    let procesador = null;
    let fragmentos = [];
    let totalMuestras = 0;
    let frecuenciaMuestreo = 44100;
    let inicioGrabacion = 0;
    let intervalo = null;
    let urlPrevia = null;
    let grabando = false;

    const formatearTiempo = segundos => {
        const minutos = Math.floor(segundos / 60).toString().padStart(2, "0");
        const resto = Math.floor(segundos % 60).toString().padStart(2, "0");
        return `${minutos}:${resto} / 01:00`;
    };

    const actualizarEstado = (mensaje, esError = false) => {
        estado.textContent = mensaje;
        estado.classList.toggle("audio-recorder-status-error", esError);
    };

    const liberarRecursos = async () => {
        if (intervalo) {
            window.clearInterval(intervalo);
            intervalo = null;
        }

        if (procesador) {
            procesador.disconnect();
            procesador.onaudioprocess = null;
            procesador = null;
        }

        if (fuente) {
            fuente.disconnect();
            fuente = null;
        }

        if (flujo) {
            flujo.getTracks().forEach(track => track.stop());
            flujo = null;
        }

        if (contextoAudio) {
            await contextoAudio.close();
            contextoAudio = null;
        }
    };

    const escribirCadena = (vista, desplazamiento, valor) => {
        for (let i = 0; i < valor.length; i += 1) {
            vista.setUint8(desplazamiento + i, valor.charCodeAt(i));
        }
    };

    const crearWav = () => {
        const buffer = new ArrayBuffer(44 + totalMuestras * 2);
        const vista = new DataView(buffer);

        escribirCadena(vista, 0, "RIFF");
        vista.setUint32(4, 36 + totalMuestras * 2, true);
        escribirCadena(vista, 8, "WAVE");
        escribirCadena(vista, 12, "fmt ");
        vista.setUint32(16, 16, true);
        vista.setUint16(20, 1, true);
        vista.setUint16(22, 1, true);
        vista.setUint32(24, frecuenciaMuestreo, true);
        vista.setUint32(28, frecuenciaMuestreo * 2, true);
        vista.setUint16(32, 2, true);
        vista.setUint16(34, 16, true);
        escribirCadena(vista, 36, "data");
        vista.setUint32(40, totalMuestras * 2, true);

        let desplazamiento = 44;
        for (const fragmento of fragmentos) {
            for (let i = 0; i < fragmento.length; i += 1) {
                const muestra = Math.max(-1, Math.min(1, fragmento[i]));
                vista.setInt16(
                    desplazamiento,
                    muestra < 0 ? muestra * 0x8000 : muestra * 0x7fff,
                    true);
                desplazamiento += 2;
            }
        }

        return new Blob([vista], { type: "audio/wav" });
    };

    const detenerGrabacion = async () => {
        if (!grabando) {
            return;
        }

        grabando = false;
        detener.disabled = true;
        iniciar.disabled = !consentimiento.checked;

        await liberarRecursos();

        if (totalMuestras === 0) {
            actualizarEstado("No se detectó audio. Intentá nuevamente.", true);
            return;
        }

        const wav = crearWav();
        const archivo = new File(
            [wav],
            `grabacion-${Date.now()}.wav`,
            { type: "audio/wav", lastModified: Date.now() });

        const transferencia = new DataTransfer();
        transferencia.items.add(archivo);
        inputAudio.files = transferencia.files;

        if (urlPrevia) {
            URL.revokeObjectURL(urlPrevia);
        }

        urlPrevia = URL.createObjectURL(wav);
        vistaPrevia.src = urlPrevia;
        vistaPrevia.hidden = false;
        actualizarEstado(
            "Grabación preparada. Presioná “Transcribir y analizar audio” para continuar.");
    };

    consentimiento.addEventListener("change", () => {
        iniciar.disabled = !consentimiento.checked || grabando;
    });

    inputAudio.addEventListener("change", () => {
        if (inputAudio.files.length > 0) {
            actualizarEstado("Archivo seleccionado. Ya podés transcribirlo y analizarlo.");
        }
    });

    iniciar.addEventListener("click", async () => {
        if (!consentimiento.checked || grabando) {
            return;
        }

        if (!navigator.mediaDevices?.getUserMedia) {
            actualizarEstado(
                "Este navegador no permite grabar audio desde esta página.",
                true);
            return;
        }

        try {
            fragmentos = [];
            totalMuestras = 0;
            tiempo.textContent = formatearTiempo(0);
            vistaPrevia.hidden = true;
            inputAudio.value = "";

            flujo = await navigator.mediaDevices.getUserMedia({
                audio: {
                    channelCount: 1,
                    echoCancellation: true,
                    noiseSuppression: true
                },
                video: false
            });

            const AudioContextClass = window.AudioContext || window.webkitAudioContext;
            contextoAudio = new AudioContextClass();
            frecuenciaMuestreo = contextoAudio.sampleRate;
            fuente = contextoAudio.createMediaStreamSource(flujo);
            procesador = contextoAudio.createScriptProcessor(4096, 1, 1);

            procesador.onaudioprocess = evento => {
                if (!grabando) {
                    return;
                }

                const canal = evento.inputBuffer.getChannelData(0);
                const copia = new Float32Array(canal.length);
                copia.set(canal);
                fragmentos.push(copia);
                totalMuestras += copia.length;
            };

            fuente.connect(procesador);
            procesador.connect(contextoAudio.destination);

            grabando = true;
            inicioGrabacion = Date.now();
            iniciar.disabled = true;
            detener.disabled = false;
            actualizarEstado("Grabando… hablá con claridad.");

            intervalo = window.setInterval(() => {
                const transcurridos = Math.min(
                    duracionMaximaSegundos,
                    Math.floor((Date.now() - inicioGrabacion) / 1000));

                tiempo.textContent = formatearTiempo(transcurridos);

                if (transcurridos >= duracionMaximaSegundos) {
                    void detenerGrabacion();
                }
            }, 250);
        } catch (error) {
            await liberarRecursos();
            grabando = false;
            iniciar.disabled = !consentimiento.checked;
            detener.disabled = true;

            if (error?.name === "NotAllowedError") {
                actualizarEstado(
                    "El permiso de micrófono fue rechazado. Habilitalo en el navegador.",
                    true);
                return;
            }

            actualizarEstado(
                "No fue posible iniciar la grabación desde el micrófono.",
                true);
        }
    });

    detener.addEventListener("click", () => {
        void detenerGrabacion();
    });

    window.addEventListener("beforeunload", () => {
        if (urlPrevia) {
            URL.revokeObjectURL(urlPrevia);
        }

        if (flujo) {
            flujo.getTracks().forEach(track => track.stop());
        }
    });
})();
