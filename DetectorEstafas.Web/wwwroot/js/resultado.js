"use strict";

document.addEventListener("DOMContentLoaded", () => {
    configurarConsultasRdap();
    configurarReporteComunitario();
    configurarDesplazamiento();
    configurarCompartir();
    configurarFeedback();
});

function configurarDesplazamiento() {
    const resultPanel =
        document.getElementById("resultadoAnalisis");

    if (!resultPanel) {
        return;
    }

    resultPanel.scrollIntoView({
        behavior: "smooth",
        block: "start"
    });
}

function configurarCompartir() {
    const shareButton =
        document.getElementById("shareResultButton");

    const shareStatus =
        document.getElementById("shareResultStatus");

    if (!shareButton) {
        return;
    }

    shareButton.addEventListener("click", async () => {
        const risk =
            shareButton.dataset.risk ?? "Sin determinar";

        const score =
            shareButton.dataset.score ?? "0";

        const shareText =
            `Analicé una comunicación sospechosa: ` +
            `riesgo ${risk}, puntuación ${score}/100. ` +
            `El resultado es orientativo.`;

        const shareData = {
            title: "Detector de Estafas",
            text: shareText,
            url: window.location.origin
        };

        try {
            if (navigator.share) {
                await navigator.share(shareData);

                mostrarEstadoCompartir(
                    shareStatus,
                    "Resultado compartido.",
                    false);

                return;
            }

            await navigator.clipboard.writeText(
                `${shareText} ${window.location.origin}`);

            mostrarEstadoCompartir(
                shareStatus,
                "Resultado copiado al portapapeles.",
                false);
        }
        catch (error) {
            if (error?.name === "AbortError") {
                return;
            }

            mostrarEstadoCompartir(
                shareStatus,
                "No se pudo compartir el resultado.",
                true);
        }
    });
}

function configurarFeedback() {
    const feedbackContainer =
        document.getElementById("resultFeedback");

    const feedbackStatus =
        document.getElementById("feedbackStatus");

    if (!feedbackContainer) {
        return;
    }

    const analysisId =
        feedbackContainer.dataset.analysisId;

    const buttons =
        feedbackContainer.querySelectorAll(
            ".feedback-button");

    buttons.forEach(button => {
        button.addEventListener("click", async () => {
            const useful =
                button.dataset.useful === "true";

            await enviarFeedback(
                analysisId,
                useful,
                button,
                buttons,
                feedbackStatus);
        });
    });
}

async function enviarFeedback(
    analysisId,
    useful,
    selectedButton,
    buttons,
    statusElement) {

    const antiforgeryToken =
        document.querySelector(
            'input[name="__RequestVerificationToken"]');

    if (!antiforgeryToken) {
        mostrarEstadoFeedback(
            statusElement,
            "No se pudo validar la solicitud.",
            true);

        return;
    }

    const formData = new FormData();

    formData.append(
        "analisisRegistroId",
        analysisId);

    formData.append(
        "fueUtil",
        useful.toString());

    formData.append(
        "__RequestVerificationToken",
        antiforgeryToken.value);

    buttons.forEach(button => {
        button.disabled = true;
    });

    try {
        const response = await fetch(
            "/Analisis/RegistrarFeedback",
            {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With":
                        "XMLHttpRequest"
                }
            });

        if (!response.ok) {
            throw new Error(
                "No se pudo guardar la respuesta.");
        }

        buttons.forEach(button => {
            button.classList.remove(
                "feedback-button--selected");
        });

        selectedButton.classList.add(
            "feedback-button--selected");

        mostrarEstadoFeedback(
            statusElement,
            "Respuesta registrada. Gracias.",
            false);
    }
    catch {
        mostrarEstadoFeedback(
            statusElement,
            "No se pudo registrar la respuesta.",
            true);
    }
    finally {
        buttons.forEach(button => {
            button.disabled = false;
        });
    }
}

function mostrarEstadoCompartir(
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
    }, 4000);
}

function mostrarEstadoFeedback(
    element,
    message,
    isError) {

    if (!element) {
        return;
    }

    element.textContent = message;

    element.classList.toggle(
        "feedback-status--error",
        isError);

    element.hidden = false;
}
function configurarReporteComunitario() {
    const container =
        document.getElementById("communityReport");

    const categorySelect =
        document.getElementById("communityReportCategory");

    const reportButton =
        document.getElementById("communityReportButton");

    const statusElement =
        document.getElementById("communityReportStatus");

    if (!container ||
        !categorySelect ||
        !reportButton) {
        return;
    }

    reportButton.addEventListener("click", async () => {
        const analysisId =
            container.dataset.analysisId;

        const category =
            categorySelect.value;

        if (!category) {
            mostrarEstadoReporte(
                statusElement,
                "Seleccioná una categoría.",
                true);

            return;
        }

        const antiforgeryToken =
            document.querySelector(
                'input[name="__RequestVerificationToken"]');

        if (!antiforgeryToken) {
            mostrarEstadoReporte(
                statusElement,
                "No se pudo validar la solicitud.",
                true);

            return;
        }

        const formData = new FormData();

        formData.append(
            "analisisRegistroId",
            analysisId);

        formData.append(
            "categoria",
            category);

        formData.append(
            "__RequestVerificationToken",
            antiforgeryToken.value);

        reportButton.disabled = true;
        categorySelect.disabled = true;

        try {
            const response = await fetch(
                "/Analisis/RegistrarReporte",
                {
                    method: "POST",
                    body: formData,
                    headers: {
                        "X-Requested-With":
                            "XMLHttpRequest"
                    }
                });

            if (!response.ok) {
                throw new Error(
                    "No se pudo registrar el reporte.");
            }

            reportButton.textContent =
                "Reporte registrado";

            reportButton.classList.add(
                "community-report-button--registered");

            mostrarEstadoReporte(
                statusElement,
                "Gracias. El reporte anónimo fue registrado.",
                false);
        }
        catch {
            reportButton.disabled = false;
            categorySelect.disabled = false;

            mostrarEstadoReporte(
                statusElement,
                "No se pudo registrar el reporte.",
                true);
        }
    });
}

function mostrarEstadoReporte(
    element,
    message,
    isError) {

    if (!element) {
        return;
    }

    element.textContent = message;

    element.classList.toggle(
        "community-report-status--error",
        isError);

    element.hidden = false;
}
function configurarConsultasRdap() {
    const containers =
        document.querySelectorAll(".js-rdap-result");

    containers.forEach(container => {
        consultarRdap(container);
    });
}

async function consultarRdap(container) {
    const domain =
        container.dataset.domain;

    const statusElement =
        container.querySelector(".js-rdap-status");

    const detailsElement =
        container.querySelector(".js-rdap-details");

    if (!domain ||
        !statusElement ||
        !detailsElement) {
        return;
    }

    try {
        const response = await fetch(
            `/Analisis/ConsultarDominio?dominio=${encodeURIComponent(domain)}`,
            {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

        if (!response.ok) {
            throw new Error(
                "No se pudo consultar el dominio.");
        }

        const data = await response.json();

        statusElement.textContent =
            data.estado ??
            "Consulta completada.";

        const details = [];

        if (data.fechaRegistroUtc) {
            const registrationDate =
                new Date(data.fechaRegistroUtc);

            details.push(
                `Registrado: ${registrationDate.toLocaleDateString("es-AR")}`);
        }

        if (Number.isInteger(data.antiguedadDias)) {
            details.push(
                `Antigüedad: ${data.antiguedadDias} días`);
        }

        if (Array.isArray(data.senales)) {
            data.senales.forEach(signal => {
                details.push(signal);
            });
        }

        if (details.length === 0) {
            return;
        }

        const list =
            document.createElement("ul");

        list.className =
            "rdap-signal-list";

        details.forEach(detail => {
            const item =
                document.createElement("li");

            item.textContent = detail;

            list.appendChild(item);
        });

        detailsElement.replaceChildren(list);
        detailsElement.hidden = false;
    }
    catch {
        statusElement.textContent =
            "El registro oficial no está disponible en este momento.";

        container.classList.add(
            "rdap-result--warning");
    }
}