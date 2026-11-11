"use strict";
(() => {
    const app = document.querySelector("#pos-app");
    if (!app) return;

    const initial = JSON.parse(document.querySelector("#pos-data").textContent);
    const categories = [{ id: "all", name: "Tất cả món" }, ...initial.categories];
    let products = initial.products;
    let visibleProducts = products;
    const productById = new Map(products.map(product => [product.id, product]));
    const state = { category: "all", search: "", discount: 0, discountCode: "", tax: 0,
        orderType: "pickup", payment: "cash", note: "", cart: new Map() };
    let orderNumber = initial.nextOrderNumber;
    let pendingCheckout = null;
    let checkingOut = false;
    let previewing = false;
    let productRequest = 0;
    let productAbort;
    let searchTimer;
    let lastReceipt = null;
    const money = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 });
    const $ = selector => document.querySelector(selector);
    const grid = $("#product-grid");
    const cartItems = $("#cart-items");
    const status = $("#pos-status");
    let notificationTimer;

    function notify(message) {
        window.clearTimeout(notificationTimer);
        status.textContent = message;
        notificationTimer = window.setTimeout(() => { status.textContent = ""; }, 3500);
    }

    function totals() {
        let subtotal = 0;
        let count = 0;
        state.cart.forEach((quantity, id) => { subtotal += productById.get(id).price * quantity; count += quantity; });
        const discount = Math.round(subtotal * state.discount / 100);
        const tax = Math.round((subtotal - discount) * state.tax / 100);
        return { subtotal, discount, tax, total: subtotal - discount + tax, count };
    }

    function setImage(image, product) {
        image.alt = image.classList.contains("cart-thumbnail") ? "" : product.name;
        image.src = product.imageUrl;
    }

    // error không bubble; capture xử lý cả ảnh mới render, không lặp khi ảnh dự phòng lỗi.
    app.addEventListener("error", event => {
        const image = event.target;
        if (!(image instanceof HTMLImageElement) || image.dataset.fallback) return;
        image.dataset.fallback = "true";
        image.src = app.dataset.fallbackImage;
    }, true);

    function renderCategories() {
        const fragment = document.createDocumentFragment();
        categories.forEach(category => {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "category-button";
            button.dataset.category = category.id;
            button.setAttribute("aria-pressed", String(category.id === state.category));
            button.classList.toggle("active", category.id === state.category);
            const label = document.createElement("span");
            label.className = "category-label";
            label.textContent = category.name;
            const count = document.createElement("span");
            count.className = "category-count";
            count.textContent = String(category.id === "all" ? products.length : products.filter(product => product.categoryId === category.id).length);
            const arrow = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            arrow.classList.add("icon");
            arrow.setAttribute("aria-hidden", "true");
            const use = document.createElementNS("http://www.w3.org/2000/svg", "use");
            use.setAttribute("href", "#i-arrow");
            arrow.append(use);
            button.append(label, count, arrow);
            fragment.append(button);
        });
        $("#category-list").replaceChildren(fragment);
        $("#new-category").replaceChildren();
        categories.filter(category => category.id !== "all").forEach(category => {
            const option = new Option(category.name, category.id);
            $("#new-category").add(option);
        });
    }

    function renderProducts() {
        const visible = visibleProducts;
        const fragment = document.createDocumentFragment();
        visible.forEach(product => {
            const card = $("#product-template").content.firstElementChild.cloneNode(true);
            card.dataset.product = product.id;
            card.disabled = !product.isAvailable || Boolean(pendingCheckout) || previewing;
            if (!product.isAvailable) card.title = "Món đang ngừng bán";
            card.setAttribute("aria-label", `Thêm ${product.name}, ${money.format(product.price)} vào giỏ`);
            setImage(card.querySelector("img"), product);
            card.querySelector(".product-name").textContent = product.name + (product.isAvailable ? "" : " · Ngừng bán");
            card.querySelector(".product-name").title = product.name;
            card.querySelector(".product-prices del").textContent = product.originalPrice ? money.format(product.originalPrice) : "";
            card.querySelector(".product-prices strong").textContent = money.format(product.price);
            fragment.append(card);
        });
        grid.replaceChildren(fragment);
        grid.setAttribute("aria-busy", "false");
        $("#product-count").textContent = `${visible.length} món`;
        $("#menu-title").textContent = categories.find(category => category.id === state.category).name;
        $("#no-results").hidden = visible.length > 0;
        updateSelectedProducts();
    }

    function updateSelectedProducts() {
        grid.querySelectorAll("[data-product]").forEach(card => {
            const quantity = state.cart.get(card.dataset.product) || 0;
            card.disabled = !productById.get(card.dataset.product).isAvailable || Boolean(pendingCheckout) || previewing;
            card.classList.toggle("selected", quantity > 0);
            card.querySelector(".product-add").textContent = quantity > 0 ? String(quantity) : "+";
        });
    }

    function renderCart() {
        const focused = document.activeElement;
        const focusedProduct = focused?.closest(".cart-item")?.dataset.product;
        const focusedAction = focused?.dataset.action;
        const fragment = document.createDocumentFragment();
        state.cart.forEach((quantity, id) => {
            const product = productById.get(id);
            const row = $("#cart-template").content.firstElementChild.cloneNode(true);
            row.dataset.product = id;
            setImage(row.querySelector("img"), product);
            row.querySelector("h3").textContent = product.name;
            row.querySelector(".cart-unit-price").textContent = money.format(product.price);
            row.querySelector(".quantity").textContent = String(quantity);
            row.querySelector(".line-total").textContent = money.format(product.price * quantity);
            const decrease = row.querySelector('[data-action="decrease"]');
            const increase = row.querySelector('[data-action="increase"]');
            decrease.disabled = Boolean(pendingCheckout) || previewing;
            increase.disabled = quantity >= 99 || Boolean(pendingCheckout) || previewing;
            row.querySelector('[data-action="remove"]').disabled = Boolean(pendingCheckout) || previewing;
            decrease.setAttribute("aria-label", `Giảm số lượng ${product.name}`);
            increase.setAttribute("aria-label", `Tăng số lượng ${product.name}`);
            row.querySelector('[data-action="remove"]').setAttribute("aria-label", `Xóa ${product.name}`);
            fragment.append(row);
        });
        cartItems.replaceChildren(fragment);
        const amount = totals();
        $("#cart-empty").hidden = amount.count > 0;
        $("#cart-count").textContent = String(amount.count);
        $("#subtotal").textContent = money.format(amount.subtotal);
        $("#total").textContent = money.format(amount.total);
        $("#mobile-total").textContent = money.format(amount.total);
        $("#discount-row").hidden = !state.discount;
        $("#discount-label").textContent = `Giảm giá (${state.discount}%)`;
        $("#discount-amount").textContent = `−${money.format(amount.discount)}`;
        $("#tax-amount").textContent = money.format(amount.tax);
        $("#checkout").disabled = checkingOut || previewing;
        $("#checkout").textContent = checkingOut ? "Đang ghi nhận đơn..." : pendingCheckout ? "Thử xác nhận lại đơn" : "Thanh toán ngay";
        $("#print-receipt").disabled = Boolean(pendingCheckout) || previewing || (!amount.count && !lastReceipt);
        $("#clear-cart").disabled = Boolean(pendingCheckout) || previewing;
        $("#pos-app").querySelectorAll("#order-types button, #discount, #tax-percent, #order-note, input[name=payment]").forEach(el => { el.disabled = Boolean(pendingCheckout) || previewing; });
        updateSelectedProducts();
        if (focusedProduct && focusedAction) {
            const row = cartItems.querySelector(`[data-product="${focusedProduct}"]`);
            const target = row?.querySelector(`[data-action="${focusedAction}"]:not(:disabled)`) || row?.querySelector("button:not(:disabled)") || cartItems.querySelector("button:not(:disabled)");
            (target || $("#product-search")).focus({ preventScroll: true });
        }
    }

    $("#category-list").addEventListener("click", event => {
        const button = event.target.closest("[data-category]");
        if (!button) return;
        state.category = button.dataset.category;
        $("#category-list").querySelectorAll("button").forEach(item => {
            item.classList.toggle("active", item === button);
            item.setAttribute("aria-pressed", String(item === button));
        });
        loadProducts();
        $("#product-scroll").scrollTop = 0;
    });
    $("#product-search").addEventListener("input", event => { state.search = event.target.value; window.clearTimeout(searchTimer); searchTimer = window.setTimeout(loadProducts, 180); });
    $("#reset-search").addEventListener("click", () => {
        state.search = "";
        $("#product-search").value = "";
        $("#category-list [data-category='all']").click();
        $("#product-search").focus();
    });
    grid.addEventListener("click", event => {
        const card = event.target.closest("[data-product]");
        if (pendingCheckout || previewing || !card || !productById.get(card.dataset.product)?.isAvailable) return;
        const id = card.dataset.product;
        const quantity = state.cart.get(id) || 0;
        if (!quantity && state.cart.size >= 100) { notify("Mỗi đơn tối đa 100 món khác nhau."); return; }
        if (quantity >= 99) { notify("Mỗi món tối đa 99 phần trong một đơn."); return; }
        state.cart.set(id, quantity + 1);
        renderCart();
        notify(`Đã thêm ${productById.get(id).name} vào giỏ.`);
    });
    cartItems.addEventListener("click", event => {
        const button = event.target.closest("[data-action]");
        const id = button?.closest("[data-product]")?.dataset.product;
        if (pendingCheckout || previewing || !button || !state.cart.has(id)) return;
        const quantity = state.cart.get(id);
        if (button.dataset.action === "remove") state.cart.delete(id);
        else if (button.dataset.action === "increase" && quantity < 99) state.cart.set(id, quantity + 1);
        else if (button.dataset.action === "decrease") { if (quantity === 1) state.cart.delete(id); else state.cart.set(id, quantity - 1); }
        renderCart();
    });
    $("#order-types").addEventListener("click", event => {
        const button = event.target.closest("[data-order]");
        if (!button) return;
        state.orderType = button.dataset.order;
        $("#order-types").querySelectorAll("button").forEach(item => {
            item.classList.toggle("active", item === button);
            item.setAttribute("aria-pressed", String(item === button));
        });
    });
    $("#discount").addEventListener("change", event => {
        state.discountCode = event.target.value;
        state.discount = initial.discounts.find(d => d.code === state.discountCode)?.percent || 0;
        renderCart();
    });
    $("#tax-percent").addEventListener("change", event => { state.tax = Number(event.target.value); renderCart(); });
    $("#order-note").addEventListener("input", event => { state.note = event.target.value.slice(0, 200); });
    $("#payment-options").addEventListener("change", event => {
        if (["cash", "card", "wallet"].includes(event.target.value)) state.payment = event.target.value;
    });
    $("#clear-cart").addEventListener("click", () => bootstrap.Modal.getOrCreateInstance($("#clear-modal")).show());
    $("#confirm-clear").addEventListener("click", () => {
        if (pendingCheckout || previewing) return;
        resetOrder();
        bootstrap.Modal.getInstance($("#clear-modal")).hide();
        notify("Đã xóa giỏ và đặt lại cấu hình đơn.");
    });
    $("#refresh-pos").addEventListener("click", () => loadProducts(true));
    $("#go-back").addEventListener("click", () => {
        // Không tự chuyển người dùng tới một website khác từ bản xem trước.
        if (document.referrer && new URL(document.referrer).origin === window.location.origin && window.history.length > 1) window.history.back();
        else notify("Bạn đang ở màn hình đầu tiên. Trang quản trị chưa triển khai.");
    });

    async function request(url, data, signal) {
        const controller = new AbortController();
        const abort = () => controller.abort();
        signal?.addEventListener("abort", abort, { once: true });
        if (signal?.aborted) abort();
        const timeoutId = window.setTimeout(abort, 20000);
        try {
            const response = await fetch(url, { method: data ? "POST" : "GET", signal: controller.signal,
                headers: data ? { "Content-Type": "application/json", "X-CSRF-TOKEN": $("input[name=__RequestVerificationToken]").value } : {},
                body: data ? JSON.stringify(data) : undefined });
            const result = await response.json().catch(() => null);
            if (!response.ok) {
                const error = new Error(result?.errors?.join(" ") || result?.message || "Yêu cầu không thành công. Vui lòng tải lại trang nếu phiên đã hết hạn.");
                // Chỉ lỗi từ chối đầu vào chắc chắn chưa ghi nhận đơn. Lỗi 5xx cần thử lại cùng RequestId.
                error.rejected = response.status >= 400 && response.status < 500;
                throw error;
            }
            if (!result) throw new Error("Không đọc được phản hồi máy chủ. Vui lòng thử lại.");
            return result;
        } catch (error) {
            if (signal?.aborted) throw error;
            if (error.name === "AbortError" || error instanceof TypeError)
                throw new Error("Không kết nối được máy chủ hoặc đã quá thời gian chờ. Vui lòng thử lại.");
            throw error;
        } finally {
            window.clearTimeout(timeoutId);
            signal?.removeEventListener("abort", abort);
        }
    }

    async function loadProducts(refreshAll = false) {
        const requestId = ++productRequest;
        productAbort?.abort();
        productAbort = new AbortController();
        grid.setAttribute("aria-busy", "true");
        $("#product-count").textContent = "Đang tải...";
        try {
            if (refreshAll) {
                const all = await request(app.dataset.productsUrl, null, productAbort.signal);
                if (requestId !== productRequest) return;
                products = all;
                all.forEach(p => productById.set(p.id, p));
                renderCategories();
            }
            const url = new URL(app.dataset.productsUrl, location.href);
            if (state.category !== "all") url.searchParams.set("categoryId", state.category);
            url.searchParams.set("search", state.search);
            const found = await request(url, null, productAbort.signal);
            if (requestId !== productRequest) return;
            visibleProducts = found;
            found.forEach(p => productById.set(p.id, p));
            renderProducts();
            renderCart();
        } catch (error) {
            if (error.name === "AbortError" || requestId !== productRequest) return;
            visibleProducts = [];
            renderProducts();
            $("#product-count").textContent = "Không tải được · Bấm Làm mới để thử lại";
            notify(error.message);
        } finally { if (requestId === productRequest) grid.setAttribute("aria-busy", "false"); }
    }

    function orderPayload() {
        return { requestId: crypto.randomUUID(), orderType: state.orderType, paymentMethod: state.payment,
            discountCode: state.discountCode || null, taxPercent: state.tax, note: state.note,
            items: [...state.cart].map(([productId, quantity]) => ({ productId, quantity })) };
    }

    function resetOrder() {
        state.cart.clear();
        state.discount = 0; state.discountCode = ""; state.tax = 0; state.note = "";
        state.orderType = "pickup"; state.payment = "cash";
        $("#discount").value = ""; $("#tax-percent").value = "0"; $("#order-note").value = "";
        $("input[name=payment][value=cash]").checked = true;
        $("#order-types").querySelectorAll("button").forEach(button => {
            button.classList.toggle("active", button.dataset.order === "pickup");
            button.setAttribute("aria-pressed", String(button.dataset.order === "pickup"));
        });
        renderCart();
    }

    function showReceipt(order, committed) {
        const orderNames = { pickup: "Mang đi", delivery: "Giao hàng", dinein: "Tại bàn" };
        const paymentNames = { cash: "Tiền mặt", card: "Thẻ ngân hàng", wallet: "Ví điện tử" };
        const date = new Intl.DateTimeFormat("vi-VN", { dateStyle: "short", timeStyle: "short", timeZone: "Asia/Ho_Chi_Minh" }).format(new Date(order.createdAt));
        $("#receipt-title").textContent = committed ? "Đơn đã ghi nhận" : "Hóa đơn tạm tính";
        $(".receipt-watermark").textContent = committed ? "ĐÃ GHI NHẬN ĐƠN NỘI BỘ" : "TẠM TÍNH · CHƯA CHỐT ĐƠN";
        $("#receipt-meta").textContent = `Đơn ${order.orderNumber} · ${date} · ${orderNames[order.orderType]} · ${paymentNames[order.paymentMethod]}`;
        const fragment = document.createDocumentFragment();
        order.items.forEach(item => {
            const row = document.createElement("div"); row.className = "receipt-line";
            const label = document.createElement("span");
            label.textContent = `${item.quantity} × ${item.productName} (${money.format(item.unitPrice)})`;
            const value = document.createElement("strong"); value.textContent = money.format(item.lineTotal);
            row.append(label, value); fragment.append(row);
        });
        $("#receipt-lines").replaceChildren(fragment);
        const summary = document.createDocumentFragment();
        [["Tạm tính", order.subtotal], [`Giảm giá (${order.discountPercent}%)`, -order.discountAmount],
            [`Thuế VAT (${order.taxPercent}%)`, order.taxAmount], ["Tổng cộng", order.total]].forEach(([label, value], index) => {
            const row = document.createElement("div"); if (index === 3) row.className = "total-row";
            const term = document.createElement("dt"); term.textContent = label;
            const description = document.createElement("dd"); description.textContent = money.format(value);
            row.append(term, description); summary.append(row);
        });
        $("#receipt-summary").replaceChildren(summary);
        $("#receipt-note").textContent = order.note ? `Ghi chú: ${order.note}` : "";
        bootstrap.Modal.getOrCreateInstance($("#receipt-modal")).show();
    }

    $("#checkout").addEventListener("click", async () => {
        if (checkingOut || previewing) return;
        if (!state.cart.size) { notify("Giỏ hàng trống. Vui lòng chọn món trước khi thanh toán."); return; }
        pendingCheckout ||= orderPayload();
        checkingOut = true; renderCart();
        try {
            const result = await request(app.dataset.checkoutUrl, pendingCheckout);
            lastReceipt = result.order;
            orderNumber = result.nextOrderNumber;
            $("#order-number").textContent = `Đơn ${orderNumber}`;
            pendingCheckout = null;
            resetOrder();
            showReceipt(lastReceipt, true);
            notify(`Đã ghi nhận đơn ${lastReceipt.orderNumber}.`);
        } catch (error) {
            if (error.rejected) pendingCheckout = null;
            notify(pendingCheckout ? "Chưa xác nhận được kết quả. Giữ nguyên trang và bấm Thử xác nhận lại đơn để tránh tạo trùng." : error.message);
        } finally { checkingOut = false; renderCart(); }
    });
    $("#print-receipt").addEventListener("click", async () => {
        if (pendingCheckout || previewing) return;
        if (!state.cart.size) { if (lastReceipt) showReceipt(lastReceipt, true); return; }
        previewing = true; renderCart();
        try { showReceipt(await request(app.dataset.previewUrl, orderPayload()), false); }
        catch (error) { notify(error.message); }
        finally { previewing = false; renderCart(); }
    });
    $("#print-preview").addEventListener("click", () => window.print());
    window.addEventListener("beforeunload", event => {
        if (pendingCheckout || state.cart.size) { event.preventDefault(); event.returnValue = ""; }
    });

    $("#add-product-form").addEventListener("submit", async event => {
        event.preventDefault();
        const button = $("#save-product");
        if (button.disabled) return;
        const errorBox = $("#product-error"); errorBox.hidden = true;
        const payload = { name: $("#new-name").value.trim(), categoryId: $("#new-category").value,
            price: Number($("#new-price").value), originalPrice: $("#new-original-price").value === "" ? null : Number($("#new-original-price").value),
            imageUrl: $("#new-image").value.trim() || null, isAvailable: $("#new-available").checked };
        if (!payload.name) { errorBox.textContent = "Vui lòng nhập tên món."; errorBox.hidden = false; return; }
        button.disabled = true; button.textContent = "Đang lưu...";
        try {
            const product = await request(app.dataset.createUrl, payload);
            products.push(product); productById.set(product.id, product);
            state.category = product.categoryId; state.search = ""; $("#product-search").value = "";
            renderCategories();
            event.target.reset();
            bootstrap.Modal.getOrCreateInstance($("#add-product-modal")).hide();
            await loadProducts(); notify("Đã thêm món mới vào thực đơn.");
        } catch (error) { errorBox.textContent = error.message; errorBox.hidden = false; }
        finally { button.disabled = false; button.textContent = "Lưu món"; }
    });
    $("#calculator-form").addEventListener("submit", event => {
        event.preventDefault();
        const first = Number($("#calc-a").value);
        const second = Number($("#calc-b").value);
        const operation = $("#calc-operation").value;
        const output = $("#calc-result");
        if (!Number.isFinite(first) || !Number.isFinite(second) || Math.abs(first) > 1e12 || Math.abs(second) > 1e12) { output.textContent = "Vui lòng nhập số hữu hạn có trị tuyệt đối không quá 1.000.000.000.000."; return; }
        if (operation === "divide" && second === 0) { output.textContent = "Không thể chia cho 0."; return; }
        const result = { add: () => first + second, subtract: () => first - second, multiply: () => first * second, divide: () => first / second }[operation]?.();
        output.textContent = Number.isFinite(result) ? `Kết quả: ${new Intl.NumberFormat("vi-VN", { maximumFractionDigits: 8 }).format(result)}` : "Không thể tính kết quả.";
    });
    renderCategories();
    renderProducts();
    renderCart();
})();
