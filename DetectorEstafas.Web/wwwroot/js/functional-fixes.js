"use strict";

document.addEventListener("DOMContentLoaded", () => {
    normalizarHistorialDeFormularios();
    integrarCapturaEnAnalizador();
    configurarSelectorArchivoCaptura();
    configurarSelectorTipoContenido();
    configurarCambioTipoContenido();
    mostrarErrorDeCarga();
    configurarCompartirResultadoCorregido();
});

function textoInterfaz(espanol, ingles) {
    return (document.documentElement.lang || "es")
        .toLowerCase()
        .startsWith("en")
            ? ingles
            : espanol;
}

function normalizarHistorialDeFormularios() {
    if (!window.history || !window.history.replaceState) {
        return;
    }

    const hayRespuestaDeFormulario =
        document.getElementById("resultadoAnalisis") ||
        document.querySelector(".capture-error") ||
        document.querySelector(".capture-result") ||
        document.querySelector(".audio-result");

    if (!hayRespuestaDeFormulario) {
        return;
    }

    const url = new URL(window.location.href);

    window.history.replaceState(
        { detectorEstafasResultado: true },
        "",
        `${url.pathname}${url.search}${url.hash}`);
}

function integrarCapturaEnAnalizador() {
    const captureCard = Array.from(
        document.querySelectorAll(".capture-card")
    ).find(card => !card.classList.contains("audio-card"));

    const analyzerCard = document.querySelector(".analyzer-card");
    const tabsContainer = document.getElementById("contentTypeTabs");

    if (!captureCard || !analyzerCard || !tabsContainer) {
        return;
    }

    captureCard.classList.add("capture-card--integrated");
    captureCard.hidden = true;
    analyzerCard.appendChild(captureCard);

    if (tabsContainer.querySelector('[data-capture-tab="true"]')) {
        return;
    }

    const captureTab = document.createElement("button");
    captureTab.type = "button";
    captureTab.className = "content-type-tab";
    captureTab.dataset.captureTab = "true";
    captureTab.setAttribute("role", "tab");
    captureTab.setAttribute("aria-selected", "false");
    captureTab.textContent = textoInterfaz("Captura", "Screenshot");

    tabsContainer.appendChild(captureTab);
}

function configurarSelectorArchivoCaptura() {
    const input = document.getElementById("captura");

    if (!input || input.dataset.customFilePicker === "true") {
        return;
    }

    input.dataset.customFilePicker = "true";
    input.classList.add("visually-hidden");
    input.tabIndex = -1;

    const picker = document.createElement("div");
    picker.className = "input-group capture-file-picker";

    const button = document.createElement("button");
    button.type = "button";
    button.id = "capturaElegirArchivo";
    button.className = "btn btn-outline-secondary";
    button.textContent = textoInterfaz(
        "Elegir archivo",
        "Choose file");

    const fileName = document.createElement("span");
    fileName.id = "capturaNombreArchivo";
    fileName.className = "form-control text-secondary";
    fileName.setAttribute("role", "status");
    fileName.setAttribute("aria-live", "polite");
    fileName.textContent = textoInterfaz(
        "No se eligió ningún archivo",
        "No file selected");

    button.setAttribute(
        "aria-describedby",
        fileName.id);

    button.addEventListener("click", () => {
        input.click();
    });

    input.addEventListener("change", () => {
        const selectedFile = input.files?.[0];

        fileName.textContent = selectedFile?.name ?? textoInterfaz(
            "No se eligió ningún archivo",
            "No file selected");
    });

    picker.append(button, fileName);
    input.insertAdjacentElement("afterend", picker);
}

function configurarSelectorTipoContenido() {
    const tipoSelect = document.getElementById("Tipo");
    const contenido = document.getElementById("Contenido");
    const tabsContainer = document.getElementById("contentTypeTabs");
    const label = document.getElementById("contentInputLabel");
    const helpText = document.getElementById("contentHelpText");
    const analyzerForm = tabsContainer?.closest("form");
    const captureCard = document.querySelector(".capture-card--integrated");

    if (!tipoSelect || !contenido || !tabsContainer || !label || !analyzerForm) {
        return;
    }

    const tabs = Array.from(
        tabsContainer.querySelectorAll(".content-type-tab")
    );

    const captureTabIndex = tabs.findIndex(
        tab => tab.dataset.captureTab === "true"
    );

    function aplicarModoVisual(mode) {
        contenido.classList.toggle(
            "content-field--compact",
            mode === "singleline"
        );

        contenido.rows = mode === "singleline" ? 2 : 4;
    }

    function limpiarValidacionContenido() {
        const validation = document.querySelector(
            '[data-valmsg-for="Contenido"]'
        );

        if (validation) {
            validation.textContent = "";
            validation.classList.remove("field-validation-error");
            validation.classList.add("field-validation-valid");
        }
    }

    function activarTab(index, clearContent) {
        const tab = tabs[index];

        if (!tab) {
            return;
        }

        const isCapture = tab.dataset.captureTab === "true";

        tabs.forEach((item, itemIndex) => {
            const isActive = itemIndex === index;

            item.classList.toggle("is-active", isActive);
            item.setAttribute("aria-selected", isActive ? "true" : "false");
        });

        analyzerForm.classList.toggle("capture-mode-active", isCapture);

        if (captureCard) {
            captureCard.hidden = !isCapture;
        }

        if (isCapture) {
            limpiarValidacionContenido();
            (
                document.getElementById("capturaElegirArchivo") ??
                document.getElementById("captura")
            )?.focus();
            return;
        }

        const selectIndex = Number(tab.dataset.tabIndex ?? index);
        tipoSelect.selectedIndex = selectIndex;

        label.textContent = tab.dataset.label ?? textoInterfaz(
            "Contenido sospechoso",
            "Suspicious content");

        contenido.placeholder =
            tab.dataset.placeholder ?? textoInterfaz(
                "Ingresá el contenido a analizar.",
                "Enter the content you want to analyze.");

        aplicarModoVisual(tab.dataset.mode ?? "multiline");

        if (helpText) {
            helpText.textContent =
                selectIndex === 1
                    ? textoInterfaz(
                        "Pegá una URL completa para evaluar señales técnicas y de riesgo.",
                        "Paste a full URL to evaluate technical and risk signals.")
                    : selectIndex === 2
                        ? textoInterfaz(
                            "Ingresá el número principal que querés revisar.",
                            "Enter the main phone number you want to review.")
                        : selectIndex === 3
                            ? textoInterfaz(
                                "Describí la llamada con el mayor detalle posible.",
                                "Describe the call in as much detail as possible.")
                            : textoInterfaz(
                                "Analizá primero el contenido principal y luego revisá el resultado.",
                                "Analyze the main content first, then review the result.");
        }

        if (clearContent) {
            contenido.value = "";
            limpiarValidacionContenido();
        }

        contenido.focus();
    }

    tabs.forEach((tab, index) => {
        tab.addEventListener("click", () => {
            activarTab(index, true);
        });
    });

    const hayRespuestaCaptura =
        captureCard?.querySelector(".capture-error, .capture-result") != null;

    if (hayRespuestaCaptura && captureTabIndex >= 0) {
        activarTab(captureTabIndex, false);
        return;
    }

    activarTab(tipoSelect.selectedIndex >= 0 ? tipoSelect.selectedIndex : 0, false);
}

function configurarCambioTipoContenido() {
    const tipo = document.getElementById("Tipo");
    const contenido = document.getElementById("Contenido");

    if (!tipo || !contenido) {
        return;
    }

    tipo.addEventListener("change", () => {
        contenido.value = "";

        const validation = document.querySelector(
            '[data-valmsg-for="Contenido"]'
        );

        if (validation) {
            validation.textContent = "";
            validation.classList.remove("field-validation-error");
            validation.classList.add("field-validation-valid");
        }

        contenido.focus();
    });
}

function mostrarErrorDeCarga() {
    const error = document.querySelector(".capture-error");

    if (!error) {
        return;
    }

    error.setAttribute("tabindex", "-1");

    window.requestAnimationFrame(() => {
        error.scrollIntoView({
            behavior: "smooth",
            block: "center"
        });

        error.focus({ preventScroll: true });
    });
}

function configurarCompartirResultadoCorregido() {
    const shareButton = document.getElementById("shareResultButton");
    const shareStatus = document.getElementById("shareResultStatus");

    if (!shareButton) {
        return;
    }

    shareButton.addEventListener(
        "click",
        async event => {
            event.preventDefault();
            event.stopImmediatePropagation();

            const analysisContainer =
                document.getElementById("resultFeedback") ??
                document.getElementById("communityReport");

            const analysisId = analysisContainer?.dataset.analysisId;
            const antiforgeryToken = document.querySelector(
                'input[name="__RequestVerificationToken"]');

            if (!analysisId || !antiforgeryToken) {
                mostrarEstadoCompartirCorregido(
                    shareStatus,
                    "No se pudo generar el vínculo del resultado.",
                    true);
                return;
            }

            shareButton.disabled = true;

            try {
                const formData = new FormData();
                formData.append("analisisRegistroId", analysisId);
                formData.append(
                    "__RequestVerificationToken",
                    antiforgeryToken.value);

                const response = await fetch(
                    "/resultado-compartido/crear",
                    {
                        method: "POST",
                        body: formData,
                        headers: {
                            "X-Requested-With": "XMLHttpRequest"
                        }
                    });

                if (!response.ok) {
                    throw new Error("No se pudo crear el vínculo.");
                }

                const data = await response.json();

                if (!data.url) {
                    throw new Error("La respuesta no incluyó un vínculo.");
                }

                const risk = shareButton.dataset.risk ?? "Sin determinar";
                const score = shareButton.dataset.score ?? "0";
                const shareText =
                    `Analicé una comunicación sospechosa: ` +
                    `riesgo ${risk}, puntuación ${score}/100. ` +
                    `El resultado es orientativo.`;

                const shareData = {
                    title: "Detector de Estafas",
                    text: shareText,
                    url: data.url
                };

                if (navigator.share) {
                    await navigator.share(shareData);
                    mostrarEstadoCompartirCorregido(
                        shareStatus,
                        "Resultado compartido.",
                        false);
                    return;
                }

                await navigator.clipboard.writeText(
                    `${shareText} ${data.url}`);

                mostrarEstadoCompartirCorregido(
                    shareStatus,
                    "Vínculo del resultado copiado al portapapeles.",
                    false);
            }
            catch (error) {
                if (error?.name === "AbortError") {
                    return;
                }

                mostrarEstadoCompartirCorregido(
                    shareStatus,
                    "No se pudo compartir el resultado.",
                    true);
            }
            finally {
                shareButton.disabled = false;
            }
        },
        true);
}

function mostrarEstadoCompartirCorregido(
    element,
    message,
    isError) {

    if (!element) {
        return;
    }

    element.textContent = message;
    element.classList.toggle(
        "share-result-status--error",
        isError);
    element.hidden = false;

    window.setTimeout(() => {
        element.hidden = true;
    }, 5000);
}
