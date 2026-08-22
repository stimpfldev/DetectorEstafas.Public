"use strict";

document.addEventListener("DOMContentLoaded", () => {
    configurarSelectorTipoContenido();
    configurarCambioTipoContenido();
    mostrarErrorDeCarga();
});

function configurarSelectorTipoContenido() {
    const tipoSelect = document.getElementById("Tipo");
    const contenido = document.getElementById("Contenido");
    const tabsContainer = document.getElementById("contentTypeTabs");
    const label = document.getElementById("contentInputLabel");
    const helpText = document.getElementById("contentHelpText");

    if (!tipoSelect || !contenido || !tabsContainer || !label) {
        return;
    }

    const tabs = Array.from(
        tabsContainer.querySelectorAll(".content-type-tab")
    );

    function aplicarModoVisual(mode) {
        contenido.classList.toggle(
            "content-field--compact",
            mode === "singleline"
        );

        contenido.rows = mode === "singleline" ? 2 : 4;
    }

    function activarTab(index, clearContent) {
        const tab = tabs[index];

        if (!tab) {
            return;
        }

        tabs.forEach((item, itemIndex) => {
            const isActive = itemIndex === index;

            item.classList.toggle("is-active", isActive);
            item.setAttribute("aria-selected", isActive ? "true" : "false");
        });

        tipoSelect.selectedIndex = index;

        label.textContent = tab.dataset.label ?? "Contenido sospechoso";
        contenido.placeholder =
            tab.dataset.placeholder ?? "Ingresá el contenido a analizar.";

        aplicarModoVisual(tab.dataset.mode ?? "multiline");

        if (helpText) {
            helpText.textContent =
                index === 1
                    ? "Pegá una URL completa para evaluar señales técnicas y de riesgo."
                    : index === 2
                        ? "Ingresá el número principal que querés revisar."
                        : index === 3
                            ? "Describí la llamada con el mayor detalle posible."
                            : "Analizá primero el contenido principal y luego revisá el resultado.";
        }

        if (clearContent) {
            contenido.value = "";

            const validation = document.querySelector(
                '[data-valmsg-for="Contenido"]'
            );

            if (validation) {
                validation.textContent = "";
                validation.classList.remove("field-validation-error");
                validation.classList.add("field-validation-valid");
            }
        }

        contenido.focus();
    }

    tabs.forEach((tab, index) => {
        tab.addEventListener("click", () => {
            activarTab(index, true);
        });
    });

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