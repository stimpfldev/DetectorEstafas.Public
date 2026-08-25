(() => {
    const forms = document.querySelectorAll("form[data-commercial-form='true']");

    for (const form of forms) {
        form.addEventListener("submit", async event => {
            if (event.defaultPrevented) {
                return;
            }

            if (typeof window.jQuery !== "undefined" &&
                typeof window.jQuery(form).valid === "function" &&
                !window.jQuery(form).valid()) {
                return;
            }

            event.preventDefault();

            const errorBox = form.querySelector("[data-commercial-error]");
            const submitButton = form.querySelector("button[type='submit']");
            const originalText = submitButton?.textContent;

            if (errorBox) {
                errorBox.textContent = "";
                errorBox.hidden = true;
            }

            if (submitButton) {
                submitButton.disabled = true;
                submitButton.textContent = "Procesando...";
            }

            try {
                const response = await fetch(form.action || window.location.href, {
                    method: "POST",
                    body: new FormData(form),
                    headers: {
                        "X-Requested-With": "XMLHttpRequest",
                        "Accept": "application/json"
                    },
                    credentials: "same-origin"
                });

                let payload = null;

                try {
                    payload = await response.json();
                } catch {
                    payload = null;
                }

                if (!response.ok || !payload?.redirectUrl) {
                    throw new Error(
                        payload?.mensaje ||
                        "No fue posible completar la operación.");
                }

                window.location.assign(payload.redirectUrl);
            } catch (error) {
                if (errorBox) {
                    errorBox.textContent =
                        error instanceof Error
                            ? error.message
                            : "No fue posible completar la operación.";
                    errorBox.hidden = false;
                }
            } finally {
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.textContent = originalText || "Continuar";
                }
            }
        });
    }
})();
