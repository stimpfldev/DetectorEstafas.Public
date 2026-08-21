"use strict";

document.addEventListener("DOMContentLoaded", () => {
    configurarCambioTipoContenido();
    mostrarErrorDeCarga();
    configurarCompartirResultadoCorregido();
});

function configurarCambioTipoContenido() {
    const tipo = document.getElementById("Tipo");
    const contenido = document.getElementById("Contenido");

    if (!tipo || !contenido) {
        return;
    }

    tipo.addEventListener("change", () => {
        contenido.value = "";

        const validation = document.querySelector(
            '[data-valmsg-for="Contenido"]');

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
