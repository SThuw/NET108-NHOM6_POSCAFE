/**
 * PolyKaFé Self-Order Kiosk JavaScript
 * Designed for High-Resolution Touchscreen & 16:9 Landscape Displays
 * Author: Nguyen Kim Song Thu (Leader - Nhom 6)
 */

// ==========================================
// 1. CAROUSEL TRÌNH CHIẾU 3 BANNER TO FULL MÀN
// ==========================================
function initKioskCarousel() {
    const track = document.getElementById('kiosk-carousel-track');
    if (!track) return;

    const slides = track.querySelectorAll('.kiosk-carousel-slide');
    const dots = document.querySelectorAll('.kiosk-indicator-dot');
    const prevBtn = document.getElementById('carousel-btn-prev');
    const nextBtn = document.getElementById('carousel-btn-next');

    if (!slides.length) return;

    let currentIndex = 0;
    let autoPlayTimer = null;

    function showSlide(index) {
        if (index < 0) index = slides.length - 1;
        if (index >= slides.length) index = 0;
        currentIndex = index;

        slides.forEach((slide, i) => {
            slide.classList.toggle('active', i === currentIndex);
        });

        dots.forEach((dot, i) => {
            dot.classList.toggle('active', i === currentIndex);
        });
    }

    function nextSlide() {
        showSlide(currentIndex + 1);
    }

    function prevSlide() {
        showSlide(currentIndex - 1);
    }

    function startAutoPlay() {
        stopAutoPlay();
        autoPlayTimer = setInterval(nextSlide, 4500);
    }

    function stopAutoPlay() {
        if (autoPlayTimer) clearInterval(autoPlayTimer);
    }

    nextBtn?.addEventListener('click', (e) => {
        e.stopPropagation();
        nextSlide();
        startAutoPlay();
    });

    prevBtn?.addEventListener('click', (e) => {
        e.stopPropagation();
        prevSlide();
        startAutoPlay();
    });

    dots.forEach((dot, i) => {
        dot.addEventListener('click', (e) => {
            e.stopPropagation();
            showSlide(i);
            startAutoPlay();
        });
    });

    track.addEventListener('mouseenter', stopAutoPlay);
    track.addEventListener('mouseleave', startAutoPlay);

    // Bắt đầu chạy tự động
    showSlide(0);
    startAutoPlay();
}

// ==========================================
// 2. ĐỒNG HỒ THỜI GIAN THỰC TRÊN HEADER
// ==========================================
function initKioskClock() {
    const clockEl = document.getElementById('kiosk-live-clock');
    if (!clockEl) return;

    function update() {
        const now = new Date();
        const timeStr = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
        clockEl.textContent = timeStr;
    }
    update();
    setInterval(update, 1000);
}

// ==========================================
// 3. LOGIC ORDER & GIỎ HÀNG KIOSK (ORDER.CSHTML)
// ==========================================
const KioskCart = {
    items: [],
    discountPercent: 0,
    discountCode: '',

    init() {
        const meta = document.getElementById('kiosk-order-meta');
        if (meta) {
            this.discountPercent = parseInt(meta.dataset.discountPercent || '0', 10);
            this.discountCode = meta.dataset.discountCode || '';
        }
        this.bindEvents();
        this.render();
    },

    bindEvents() {
        // Lọc danh mục
        document.querySelectorAll('.kiosk-cat-pill').forEach(pill => {
            pill.addEventListener('click', () => {
                document.querySelectorAll('.kiosk-cat-pill').forEach(p => p.classList.remove('active'));
                pill.classList.add('active');
                const catId = pill.dataset.catId;
                this.filterCategory(catId);
            });
        });

        // Bấm chọn món
        document.querySelectorAll('.kiosk-product-card').forEach(card => {
            card.addEventListener('click', () => {
                const id = card.dataset.productId;
                const name = card.dataset.productName;
                const price = parseFloat(card.dataset.productPrice);
                this.addItem(id, name, price);
            });
        });

        // Nút thanh toán mở modal xác nhận
        document.getElementById('btn-kiosk-checkout')?.addEventListener('click', () => {
            if (this.items.length === 0) return;
            const checkoutModal = document.getElementById('kiosk-checkout-modal');
            if (checkoutModal) checkoutModal.style.display = 'flex';
        });
    },

    filterCategory(catId) {
        document.querySelectorAll('.kiosk-product-card').forEach(card => {
            if (!catId || catId === 'all' || card.dataset.catId === catId) {
                card.style.display = 'flex';
            } else {
                card.style.display = 'none';
            }
        });
    },

    addItem(productId, name, price) {
        const existing = this.items.find(i => i.productId === productId);
        if (existing) {
            if (existing.quantity < 99) existing.quantity++;
        } else {
            this.items.push({ productId, name, price, quantity: 1 });
        }
        this.render();
    },

    changeQty(productId, delta) {
        const item = this.items.find(i => i.productId === productId);
        if (!item) return;
        item.quantity += delta;
        if (item.quantity <= 0) {
            this.items = this.items.filter(i => i.productId !== productId);
        }
        this.render();
    },

    clearCart() {
        this.items = [];
        this.render();
    },

    render() {
        const container = document.getElementById('kiosk-cart-items-list');
        const emptyState = document.getElementById('kiosk-cart-empty');
        const countBadge = document.getElementById('kiosk-cart-count');
        const subtotalEl = document.getElementById('kiosk-subtotal');
        const discountEl = document.getElementById('kiosk-discount');
        const totalEl = document.getElementById('kiosk-total');
        const checkoutBtn = document.getElementById('btn-kiosk-checkout');

        if (!container) return;

        container.innerHTML = '';
        const totalQty = this.items.reduce((sum, i) => sum + i.quantity, 0);
        if (countBadge) countBadge.textContent = totalQty;

        if (this.items.length === 0) {
            if (emptyState) emptyState.style.display = 'block';
            if (checkoutBtn) checkoutBtn.disabled = true;
            if (subtotalEl) subtotalEl.textContent = '0 ₫';
            if (discountEl) discountEl.textContent = '0 ₫';
            if (totalEl) totalEl.textContent = '0 ₫';
            return;
        }

        if (emptyState) emptyState.style.display = 'none';
        if (checkoutBtn) checkoutBtn.disabled = false;

        let subtotal = 0;
        this.items.forEach(item => {
            const lineTotal = item.price * item.quantity;
            subtotal += lineTotal;

            const row = document.createElement('div');
            row.className = 'kiosk-cart-item';
            row.innerHTML = 
                <div class=kiosk-item-info>
                    <div class=kiosk-item-title></div>
                    <div class=kiosk-item-price> ₫</div>
                </div>
                <div class=kiosk-qty-group>
                    <button type=button class=kiosk-qty-btn onclick=KioskCart.changeQty('', -1)>-</button>
                    <span class=kiosk-qty-val></span>
                    <button type=button class=kiosk-qty-btn onclick=KioskCart.changeQty('', 1)>+</button>
                </div>
                <div style=font-weight: 800; min-width: 80px; text-align: right; color: var(--kiosk-primary);>
                     ₫
                </div>
            ;
            container.appendChild(row);
        });

        const discountAmt = Math.round((subtotal * this.discountPercent) / 100);
        const finalTotal = Math.max(0, subtotal - discountAmt);

        if (subtotalEl) subtotalEl.textContent = formatMoney(subtotal) + ' ₫';
        if (discountEl) discountEl.textContent = '-' + formatMoney(discountAmt) + ' ₫ (' + this.discountPercent + '%)';
        if (totalEl) totalEl.textContent = formatMoney(finalTotal) + ' ₫';
    }
};

// ==========================================
// 4. TIỆN ÍCH HỖ TRỢ
// ==========================================
function formatMoney(num) {
    return new Intl.NumberFormat('vi-VN').format(num);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Tự động khởi động khi DOM sẵn sàng
document.addEventListener('DOMContentLoaded', () => {
    initKioskCarousel();
    initKioskClock();
    if (document.getElementById('kiosk-cart-pane')) {
        KioskCart.init();
    }
});
