"use strict";
window.addEventListener("pageshow", event => {
    if (event.persisted) window.location.reload();
});
{
    const sidebar = document.querySelector("#app-sidebar");
    const toggle = document.querySelector("[data-sidebar-toggle]");
    const desktop = window.matchMedia("(min-width: 1181px)");
    const setOpen = open => {
        if (!sidebar || !toggle) return;
        document.body.classList.toggle("sidebar-open", open);
        toggle.setAttribute("aria-expanded", String(open));
        if (open) sidebar.querySelector("[data-sidebar-close]")?.focus();
    };
    toggle?.addEventListener("click", () => setOpen(!document.body.classList.contains("sidebar-open")));
    document.querySelectorAll("[data-sidebar-close]").forEach(button => button.addEventListener("click", () => {
        setOpen(false);
        toggle?.focus();
    }));
    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && document.body.classList.contains("sidebar-open")) {
            setOpen(false);
            toggle?.focus();
        }
    });
    desktop.addEventListener("change", event => { if (event.matches) setOpen(false); });
}
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
{
    const urlParams = new URLSearchParams(window.location.search);
    const tabParam = urlParams.get("tab");
    const hash = window.location.hash;
    const tabTarget = tabParam ? `#tab-${tabParam}` : (hash || null);
    if (tabTarget) {
        const triggerEl = document.querySelector(`[data-bs-target="${tabTarget}"]`);
        if (triggerEl) {
            const tab = bootstrap.Tab.getOrCreateInstance(triggerEl);
            tab.show();
        }
    }
}
document.querySelectorAll("[data-password-toggle]").forEach(toggleBtn => {
    toggleBtn.addEventListener("click", () => {
        const targetInput = document.querySelector(toggleBtn.dataset.passwordToggle);
        if (!targetInput) return;
        const isPassword = targetInput.type === "password";
        targetInput.type = isPassword ? "text" : "password";
        toggleBtn.setAttribute("aria-label", isPassword ? "Ẩn mật khẩu" : "Hiện mật khẩu");
        const eyeIcon = toggleBtn.querySelector(".icon-eye");
        const eyeOffIcon = toggleBtn.querySelector(".icon-eye-off");
        if (eyeIcon && eyeOffIcon) {
            eyeIcon.hidden = isPassword;
            eyeOffIcon.hidden = !isPassword;
        }
    });
});
