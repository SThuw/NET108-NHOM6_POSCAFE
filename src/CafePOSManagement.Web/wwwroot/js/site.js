"use strict";
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
