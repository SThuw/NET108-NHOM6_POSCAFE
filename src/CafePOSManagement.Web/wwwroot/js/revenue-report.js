"use strict";

(() => {
    const root = document.querySelector("#revenue-dashboard");
    const dataElement = document.querySelector("#revenue-report-data");
    if (!root || !dataElement) return;

    let report;
    try {
        report = JSON.parse(dataElement.textContent || "{}");
    } catch {
        return;
    }

    const $ = selector => root.querySelector(selector);
    const vnd = value => new Intl.NumberFormat("vi-VN").format(Math.round(Number(value) || 0)) + " ₫";
    const num = value => new Intl.NumberFormat("vi-VN").format(Math.round(Number(value) || 0));
    const escapeHtml = value => String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
    const charts = {};
    let activeMonthlyRange = "months6";

    const donutCenterPlugin = {
        id: "polykafeDonutCenter",
        afterDraw(chart) {
            if (chart.config.type !== "doughnut" || !chart.chartArea) return;
            const { ctx, chartArea } = chart;
            const centerX = (chartArea.left + chartArea.right) / 2;
            const centerY = (chartArea.top + chartArea.bottom) / 2;
            const options = chart.options.plugins?.polykafeCenter || {};
            ctx.save();
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.font = "600 11px Inter, sans-serif";
            ctx.fillStyle = "#7B6D65";
            ctx.fillText(options.labelText || "Tổng doanh thu", centerX, centerY - 12);
            ctx.font = "800 15px Inter, sans-serif";
            ctx.fillStyle = "#4E342E";
            ctx.fillText(options.totalText || "0 ₫", centerX, centerY + 10);
            ctx.restore();
        }
    };

    if (typeof Chart !== "undefined") {
        Chart.register(donutCenterPlugin);
        Chart.defaults.font.family = 'Inter, "Be Vietnam Pro", -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';
        Chart.defaults.color = "#6E5F57";
    }

    function shortMoney(value) {
        const amount = Number(value) || 0;
        if (amount >= 1_000_000_000) return `${(amount / 1_000_000_000).toFixed(1).replace(".0", "")} tỷ`;
        if (amount >= 1_000_000) return `${(amount / 1_000_000).toFixed(1).replace(".0", "")} tr`;
        if (amount >= 1_000) return `${Math.round(amount / 1_000)}k`;
        return String(Math.round(amount));
    }

    function showToast(message) {
        const toast = document.querySelector("#rev-toast");
        if (!toast) return;
        toast.querySelector("span").textContent = message;
        toast.hidden = false;
        clearTimeout(showToast.timer);
        showToast.timer = setTimeout(() => { toast.hidden = true; }, 3200);
    }

    function setTrendBadge(element, growth) {
        if (!element) return;
        const value = Number(growth) || 0;
        const up = value >= 0;
        element.className = `rev-trend-badge ${up ? "up" : "down"}`;
        element.textContent = `${up ? "+" : ""}${value.toFixed(1)}%`;
    }

    function renderSparkline(key, canvasId, values, lineColor, fillColor) {
        const canvas = document.getElementById(canvasId);
        if (!canvas || typeof Chart === "undefined") return;
        const points = Array.isArray(values) && values.length ? values : [0];
        const context = canvas.getContext("2d");
        const gradient = context.createLinearGradient(0, 0, 0, 46);
        gradient.addColorStop(0, fillColor);
        gradient.addColorStop(1, "rgba(255,255,255,0)");
        charts[key]?.destroy();
        charts[key] = new Chart(context, {
            type: "line",
            data: {
                labels: points.map((_, index) => index + 1),
                datasets: [{ data: points, borderColor: lineColor, backgroundColor: gradient, borderWidth: 2, fill: true, tension: .4, pointRadius: 0 }]
            },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false }, tooltip: { enabled: false } }, scales: { x: { display: false }, y: { display: false } } }
        });
    }

    function renderDailyTrend(trend) {
        const canvas = document.getElementById("chart-daily-trend");
        if (!canvas || typeof Chart === "undefined") return;
        charts.dailyTrend?.destroy();
        charts.dailyTrend = new Chart(canvas.getContext("2d"), {
            type: "line",
            data: {
                labels: trend.labels,
                datasets: [
                    { label: "Doanh thu kỳ này", data: trend.currentRevenue, borderColor: "#4E342E", backgroundColor: "rgba(78,52,46,.12)", borderWidth: 2.5, fill: true, tension: .35, pointRadius: 2, yAxisID: "y" },
                    { label: "Doanh thu kỳ trước", data: trend.previousRevenue, borderColor: "#D99B52", borderDash: [5, 5], borderWidth: 2, tension: .35, pointRadius: 2, yAxisID: "y" },
                    { label: "Số lượng đơn", data: trend.orders, borderColor: "#4F772D", backgroundColor: "#4F772D", borderWidth: 2, tension: .3, pointRadius: 2, yAxisID: "y1" }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: "index", intersect: false },
                plugins: {
                    legend: { display: false },
                    tooltip: { callbacks: { label: context => context.dataset.yAxisID === "y1" ? ` ${context.dataset.label}: ${num(context.parsed.y)} đơn` : ` ${context.dataset.label}: ${vnd(context.parsed.y)}` } }
                },
                scales: {
                    x: { grid: { display: false } },
                    y: { beginAtZero: true, grid: { color: "#F2ECE4" }, ticks: { callback: shortMoney } },
                    y1: { position: "right", beginAtZero: true, grid: { drawOnChartArea: false }, ticks: { callback: value => `${value} đơn` } }
                }
            }
        });
    }

    function renderCategoryDonut(categories, totalRevenue) {
        const legend = $("#rev-category-legend");
        const canvas = document.getElementById("chart-category-donut");
        const items = Array.isArray(categories) ? categories : [];
        const sum = items.reduce((total, item) => total + Number(item.value || 0), 0);
        if (legend) {
            legend.innerHTML = items.length
                ? items.map(item => `<div class="rev-legend-item"><span class="rev-legend-name"><span class="rev-legend-dot" style="background:${escapeHtml(item.color)}"></span>${escapeHtml(item.name)}</span><span class="rev-legend-pct">${sum ? (item.value * 100 / sum).toFixed(1) : "0.0"}%</span></div>`).join("")
                : '<div class="text-muted small">Chưa có dữ liệu danh mục.</div>';
        }
        if (!canvas || typeof Chart === "undefined" || !items.length) return;
        charts.categoryDonut?.destroy();
        charts.categoryDonut = new Chart(canvas.getContext("2d"), {
            type: "doughnut",
            data: { labels: items.map(item => item.name), datasets: [{ data: items.map(item => item.value), backgroundColor: items.map(item => item.color), borderWidth: 2, borderColor: "#fff", hoverOffset: 5 }] },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "68%",
                plugins: {
                    legend: { display: false },
                    polykafeCenter: { totalText: shortMoney(totalRevenue), labelText: "Tổng doanh thu" },
                    tooltip: { callbacks: { label: context => ` ${context.label}: ${vnd(context.parsed)} (${sum ? (context.parsed * 100 / sum).toFixed(1) : "0.0"}%)` } }
                }
            }
        });
    }

    function renderMonthlyComparison() {
        const canvas = document.getElementById("chart-monthly-compare");
        const dataset = report.monthlyComparison?.[activeMonthlyRange];
        if (!canvas || typeof Chart === "undefined" || !dataset) return;
        charts.monthlyCompare?.destroy();
        charts.monthlyCompare = new Chart(canvas.getContext("2d"), {
            type: "bar",
            data: {
                labels: dataset.labels,
                datasets: [
                    { label: "Doanh thu thực tế", data: dataset.actual, backgroundColor: "#4E342E", borderRadius: 6, barPercentage: .72 },
                    { label: "Cùng kỳ năm trước", data: dataset.previous, backgroundColor: "#D9C3B0", borderRadius: 6, barPercentage: .72 }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: "index", intersect: false },
                plugins: { legend: { position: "bottom", labels: { usePointStyle: true, boxWidth: 8 } }, tooltip: { callbacks: { label: context => ` ${context.dataset.label}: ${vnd(context.parsed.y)}` } } },
                scales: { x: { grid: { display: false } }, y: { beginAtZero: true, grid: { color: "#F2ECE4" }, ticks: { callback: shortMoney } } }
            }
        });
    }

    function renderPeakHours(peakHours) {
        const canvas = document.getElementById("chart-peak-hours");
        const orders = peakHours.orders || [];
        const maxOrders = orders.length ? Math.max(...orders) : 0;
        const peakIndex = maxOrders > 0 ? orders.indexOf(maxOrders) : -1;
        const badge = $("#rev-peak-slot-badge");
        if (badge) badge.textContent = peakIndex >= 0 ? `Cao điểm: ${peakHours.labels[peakIndex]} (${num(maxOrders)} đơn)` : "Chưa có dữ liệu khung giờ";
        if (!canvas || typeof Chart === "undefined") return;
        charts.peakHours?.destroy();
        charts.peakHours = new Chart(canvas.getContext("2d"), {
            type: "bar",
            data: {
                labels: peakHours.labels,
                datasets: [{ label: "Doanh thu khung giờ", data: peakHours.revenue, backgroundColor: orders.map((_, index) => index === peakIndex ? "#4E342E" : "#E1C7AE"), borderRadius: 8, borderSkipped: false }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false }, tooltip: { callbacks: { label: context => [` Doanh thu: ${vnd(context.parsed.y)}`, ` Lượng đơn: ${num(orders[context.dataIndex])} đơn`] } } },
                scales: { x: { grid: { display: false } }, y: { beginAtZero: true, grid: { color: "#F2ECE4" }, ticks: { callback: shortMoney } } }
            }
        });
    }

    function renderBreakdowns(channels, payments) {
        $("#rev-channel-pos-bar").style.width = `${channels.posPct}%`;
        $("#rev-channel-kiosk-bar").style.width = `${channels.kioskPct}%`;
        $("#rev-channel-pos-text").textContent = `POS tại quầy: ${channels.posPct}%`;
        $("#rev-channel-kiosk-text").textContent = `Kiosk/không xác định: ${channels.kioskPct}%`;
        $("#rev-pay-cash-bar").style.width = `${payments.cashPct}%`;
        $("#rev-pay-card-bar").style.width = `${payments.cardPct}%`;
        $("#rev-pay-wallet-bar").style.width = `${payments.walletPct}%`;
        $("#rev-pay-cash-text").textContent = `Tiền mặt: ${payments.cashPct}%`;
        $("#rev-pay-card-text").textContent = `Thẻ ngân hàng: ${payments.cardPct}%`;
        $("#rev-pay-wallet-text").textContent = `Ví điện tử: ${payments.walletPct}%`;
    }

    function renderTopProducts(items) {
        const body = $("#rev-top-products-body");
        if (!body) return;
        body.innerHTML = items.length ? items.map(item => `
            <tr>
                <td><span class="rev-rank-badge ${item.rank <= 3 ? `rank-${item.rank}` : ""}">#${item.rank}</span></td>
                <td><div class="rev-prod-cell"><img class="rev-prod-thumb" src="${escapeHtml(item.image)}" alt="${escapeHtml(item.name)}" onerror="this.src='/images/product-fallback.svg'" /><div><div class="rev-prod-name">${escapeHtml(item.name)}</div><div class="rev-prod-cat">${escapeHtml(item.category)}</div></div></div></td>
                <td class="fw-bold text-nowrap">${num(item.cups)} ly</td>
                <td class="rev-share-wrap"><div class="rev-share-track"><div class="rev-share-fill" style="width:${item.share}%"></div></div><small class="text-muted">${item.share}%</small></td>
                <td class="text-end fw-bold text-nowrap">${vnd(item.revenue)}</td>
            </tr>`).join("") : '<tr><td colspan="5" class="text-center text-muted py-4">Chưa có dữ liệu món bán.</td></tr>';
    }

    function renderRecentOrders(items) {
        const body = $("#rev-recent-orders-body");
        if (!body) return;
        body.innerHTML = items.length ? items.map((order, index) => `
            <tr>
                <td class="fw-bold text-nowrap">${escapeHtml(order.code)}</td>
                <td class="text-muted text-nowrap">${escapeHtml(order.time)}</td>
                <td><span class="rev-channel-badge ${order.channel === "POS" ? "pos" : "kiosk"}">${escapeHtml(order.channel)}</span></td>
                <td class="text-nowrap">${escapeHtml(order.payment)}</td>
                <td class="text-end fw-bold text-nowrap">${vnd(order.total)}</td>
                <td class="text-center"><button type="button" class="rev-detail-btn" data-order-index="${index}" aria-label="Xem chi tiết hóa đơn ${escapeHtml(order.code)}"><i class="bi bi-eye" aria-hidden="true"></i></button></td>
            </tr>`).join("") : '<tr><td colspan="6" class="text-center text-muted py-4">Chưa có hóa đơn hoàn thành.</td></tr>';
    }

    function openOrderModal(order) {
        const modal = document.getElementById("rev-order-modal");
        if (!modal || !order || typeof bootstrap === "undefined") return;
        modal.querySelector("#rev-modal-code").textContent = order.code;
        modal.querySelector("#rev-modal-meta").textContent = `${order.time} · ${order.channel} · ${order.payment} · Nhân viên/thiết bị: ${order.cashier}`;
        modal.querySelector("#rev-modal-items").innerHTML = order.items.map(item => `<tr><td>${escapeHtml(item.name)}</td><td class="text-center">${num(item.qty)}</td><td class="text-end">${vnd(item.unitPrice)}</td><td class="text-end fw-semibold">${vnd(item.qty * item.unitPrice)}</td></tr>`).join("");
        modal.querySelector("#rev-modal-subtotal").textContent = vnd(order.subtotal);
        modal.querySelector("#rev-modal-discount").textContent = order.discount > 0 ? `−${vnd(order.discount)}` : "0 ₫";
        modal.querySelector("#rev-modal-total").textContent = vnd(order.total);
        modal.querySelector("#rev-modal-note").textContent = order.note ? `Ghi chú: ${order.note}` : "Không có ghi chú.";
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }

    function renderDashboard() {
        const content = $("#rev-dashboard-content");
        const empty = $("#rev-empty-state");
        const hasData = !report.invalid && Number(report.orderCount) > 0;
        content.hidden = !hasData;
        empty.hidden = hasData;
        $("#rev-export-excel").disabled = !hasData;
        $("#rev-period-text").textContent = report.periodRangeText || "Khoảng thời gian chưa hợp lệ";
        if (!hasData) {
            $("#rev-empty-message").textContent = report.message || "Không tìm thấy hóa đơn đã hoàn thành trong khoảng thời gian này.";
            return;
        }

        const kpis = report.kpis;
        $("#kpi-revenue-value").textContent = vnd(kpis.revenue.value);
        $("#kpi-revenue-sub").textContent = kpis.revenue.sub;
        setTrendBadge($("#kpi-revenue-trend"), kpis.revenue.growth);
        $("#kpi-orders-value").textContent = `${num(kpis.orders.value)} đơn`;
        $("#kpi-orders-sub").textContent = kpis.orders.sub;
        setTrendBadge($("#kpi-orders-trend"), kpis.orders.growth);
        $("#kpi-aov-value").textContent = vnd(kpis.aov.value);
        $("#kpi-aov-sub").textContent = kpis.aov.sub;
        setTrendBadge($("#kpi-aov-trend"), kpis.aov.growth);
        $("#kpi-cups-value").textContent = `${num(kpis.cups.value)} ly`;
        $("#kpi-cups-sub").textContent = kpis.cups.sub;
        setTrendBadge($("#kpi-cups-trend"), kpis.cups.growth);

        renderSparkline("sparkRevenue", "spark-revenue", kpis.revenue.sparkline, "#4E342E", "rgba(78,52,46,.2)");
        renderSparkline("sparkOrders", "spark-orders", kpis.orders.sparkline, "#A16236", "rgba(161,98,54,.2)");
        renderSparkline("sparkAov", "spark-aov", kpis.aov.sparkline, "#D99B52", "rgba(217,155,82,.24)");
        renderSparkline("sparkCups", "spark-cups", kpis.cups.sparkline, "#4F772D", "rgba(79,119,45,.2)");
        renderDailyTrend(report.trend);
        renderCategoryDonut(report.categories, kpis.revenue.value);
        renderBreakdowns(report.channels, report.payments);
        renderMonthlyComparison();
        renderPeakHours(report.peakHours);
        renderTopProducts(report.topProducts);
        renderRecentOrders(report.recentOrders);
    }

    function toIso(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
    }

    function quickRange(key) {
        const today = new Date(`${report.currentDate}T00:00:00`);
        const from = new Date(today);
        if (key === "week") from.setDate(from.getDate() - 6);
        if (key === "month") from.setDate(1);
        if (key === "year") { from.setMonth(0); from.setDate(1); }
        return { from: toIso(from), to: toIso(today) };
    }

    function navigateToRange(range) {
        const url = new URL(window.location.href);
        url.searchParams.set("fromDate", range.from);
        url.searchParams.set("toDate", range.to);
        window.location.assign(url.toString());
    }

    function exportCsv() {
        if (report.invalid || !report.orderCount) return;
        const rows = [
            ["BÁO CÁO THỐNG KÊ DOANH THU - POLYKAFÉ"],
            ["Khoảng thời gian", report.periodRangeText],
            ["Điều kiện", "Chỉ tính hóa đơn Hoàn thành"],
            [],
            ["CHỈ SỐ", "Giá trị", "So với kỳ trước (%)"],
            ["Tổng doanh thu (VNĐ)", report.kpis.revenue.value, report.kpis.revenue.growth],
            ["Số lượng đơn", report.kpis.orders.value, report.kpis.orders.growth],
            ["Doanh thu trung bình mỗi đơn (VNĐ)", report.kpis.aov.value, report.kpis.aov.growth],
            ["Tổng số ly", report.kpis.cups.value, report.kpis.cups.growth],
            [],
            ["Ngày", "Doanh thu kỳ này", "Doanh thu kỳ trước", "Số đơn"]
        ];
        report.trend.labels.forEach((label, index) => rows.push([label, report.trend.currentRevenue[index], report.trend.previousRevenue[index], report.trend.orders[index]]));
        rows.push([], ["TOP SẢN PHẨM", "Danh mục", "Số ly", "Doanh thu"]);
        report.topProducts.forEach(item => rows.push([item.name, item.category, item.cups, item.revenue]));
        const escapeCell = value => `"${String(value ?? "").replaceAll('"', '""')}"`;
        const csv = "\uFEFF" + rows.map(row => row.map(escapeCell).join(",")).join("\r\n");
        const url = URL.createObjectURL(new Blob([csv], { type: "text/csv;charset=utf-8" }));
        const link = document.createElement("a");
        link.href = url;
        link.download = `PolyKafe_DoanhThu_${report.fromDate}_${report.toDate}.csv`;
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
        showToast("Đã xuất báo cáo CSV từ dữ liệu thật.");
    }

    root.querySelectorAll("[data-period]").forEach(button => button.addEventListener("click", () => navigateToRange(quickRange(button.dataset.period))));
    $("#rev-reset-filter")?.addEventListener("click", () => navigateToRange(quickRange("month")));
    $("#rev-empty-reset")?.addEventListener("click", () => navigateToRange(quickRange("month")));
    $("#rev-export-excel")?.addEventListener("click", exportCsv);
    $("#rev-recent-orders-body")?.addEventListener("click", event => {
        const button = event.target.closest("[data-order-index]");
        if (button) openOrderModal(report.recentOrders[Number(button.dataset.orderIndex)]);
    });
    root.querySelectorAll("[data-monthly-range]").forEach(button => button.addEventListener("click", () => {
        activeMonthlyRange = button.dataset.monthlyRange;
        root.querySelectorAll("[data-monthly-range]").forEach(item => item.classList.toggle("active", item === button));
        renderMonthlyComparison();
    }));
    $("#rev-filter-form")?.addEventListener("submit", event => {
        const fromInput = $("#rev-from-date");
        const toInput = $("#rev-to-date");
        fromInput.setCustomValidity(fromInput.value && toInput.value && fromInput.value > toInput.value ? "Ngày bắt đầu không được lớn hơn ngày kết thúc." : "");
        if (!event.currentTarget.checkValidity()) {
            event.preventDefault();
            event.currentTarget.reportValidity();
        }
    });

    renderDashboard();
})();
