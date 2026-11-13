"use strict";
window.addEventListener("pageshow", event => {
    if (event.persisted) window.location.reload();
});
document.querySelectorAll("[data-image-fallback]").forEach(image => {
    image.addEventListener("error", () => {
        if (!image.src.endsWith("/images/product-fallback.svg")) image.src = "/images/product-fallback.svg";
    });
});
const imageInput = document.querySelector("[data-image-preview-input]");
imageInput?.addEventListener("input", () => {
    const preview = document.querySelector("[data-image-preview]");
    preview.src = imageInput.value.trim() || preview.dataset.originalSrc;
});
document.querySelectorAll("[data-management-modal]").forEach(dialog => {
    dialog.addEventListener("hidden.bs.modal", () => window.location.assign(dialog.dataset.returnUrl));
    bootstrap.Modal.getOrCreateInstance(dialog).show();
});
document.querySelectorAll("[data-disable-on-submit]").forEach(form => {
    form.addEventListener("submit", () => {
        if (!form.checkValidity()) return;
        const button = form.querySelector("button[type=submit]");
        button.disabled = true;
        button.textContent = button.dataset.savingLabel || "Đang lưu...";
    });
});
{
    const clocks = document.querySelectorAll("[data-local-clock]");
    if (clocks.length) {
        const formatter = new Intl.DateTimeFormat("vi-VN", {
            timeZone: "Asia/Ho_Chi_Minh", day: "2-digit", month: "2-digit",
            year: "numeric", hour: "2-digit", minute: "2-digit", hour12: false
        });
        const updateClock = () => {
            const now = new Date();
            clocks.forEach(clock => { clock.dateTime = now.toISOString(); clock.textContent = formatter.format(now); });
        };
        updateClock();
        window.setInterval(updateClock, 30_000);
    }
}
