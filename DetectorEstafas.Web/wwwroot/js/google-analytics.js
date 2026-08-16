"use strict";

document.addEventListener("DOMContentLoaded", () => {
    const measurementElement =
        document.querySelector(
            'meta[name="google-analytics-measurement-id"]');

    const measurementId =
        measurementElement?.content?.trim();

    if (!measurementId) {
        return;
    }

    window.dataLayer = window.dataLayer || [];

    window.gtag = function () {
        window.dataLayer.push(arguments);
    };

    // Comportamiento general:
    // Analytics habilitado.
    // Publicidad todavía deshabilitada.
    window.gtag("consent", "default", {
        analytics_storage: "granted",
        ad_storage: "denied",
        ad_user_data: "denied",
        ad_personalization: "denied"
    });

    // EEE + Reino Unido + Suiza:
    // Analytics espera decisión de consentimiento.
    window.gtag("consent", "default", {
        analytics_storage: "denied",
        ad_storage: "denied",
        ad_user_data: "denied",
        ad_personalization: "denied",
        wait_for_update: 500,
        region: [
            "AT", "BE", "BG", "HR", "CY", "CZ",
            "DK", "EE", "FI", "FR", "DE", "GR",
            "HU", "IE", "IT", "LV", "LT", "LU",
            "MT", "NL", "PL", "PT", "RO", "SK",
            "SI", "ES", "SE",
            "IS", "LI", "NO",
            "GB", "CH"
        ]
    });

    window.gtag("js", new Date());
    window.gtag("config", measurementId);

    const script =
        document.createElement("script");

    script.async = true;

    script.src =
        `https://www.googletagmanager.com/gtag/js?id=${encodeURIComponent(measurementId)}`;

    document.head.appendChild(script);
});