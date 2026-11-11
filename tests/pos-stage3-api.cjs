async page => {
    const token = await page.locator('input[name=__RequestVerificationToken]').inputValue();
    const headers = { 'X-CSRF-TOKEN': token };
    const results = [];
    const check = (condition, name) => { if (!condition) throw new Error('FAIL: ' + name); results.push(name); };
    const basket = () => ({ requestId: crypto.randomUUID(), orderType: 'pickup', paymentMethod: 'cash', discountCode: 'SALE10', taxPercent: 8,
        items: [{ productId: 'p1', quantity: 2 }, { productId: 'p2', quantity: 1 }] });
    // page.request shares browser cookies and uses absolute URLs when baseURL is not configured.
    const base = new URL(page.url()).origin;
    const send = (action, data, csrf = true) => page.request.post(base + '/Pos/' + action, { data, headers: csrf ? headers : {} });
    check((await send('Checkout', basket(), false)).status() === 400, 'POST without antiforgery rejected');
    const input = basket(); input.total = 1; input.items[0].price = 1; input.discountPercent = 100;
    const first = await send('Checkout', input); check(first.ok(), 'Valid checkout accepted');
    const saved = await first.json();
    check(saved.order.total === 140940, 'Client price, total and discount tampering ignored');
    const retries = await Promise.all(Array.from({ length: 12 }, () => send('Checkout', input)));
    const bodies = await Promise.all(retries.map(r => r.json()));
    check(bodies.every(r => r.order.orderNumber === saved.order.orderNumber && r.nextOrderNumber === saved.nextOrderNumber), '12 HTTP retries keep one order');
    input.note = 'different';
    check((await send('Checkout', input)).status() === 400, 'Changed payload with same key rejected');
    for (const [label, mutate] of [
        ['empty cart', p => p.items = []], ['null cart', p => p.items = null], ['null line', p => p.items = [null]],
        ['negative quantity', p => p.items[0].quantity = -1], ['excess quantity', p => p.items[0].quantity = 100],
        ['fractional quantity', p => p.items[0].quantity = 1.5], ['unknown product', p => p.items[0].productId = 'missing'],
        ['unknown discount', p => p.discountCode = 'FREE'], ['unsupported VAT', p => p.taxPercent = 12],
        ['missing request ID', p => delete p.requestId], ['invalid order type', p => p.orderType = 'x'],
        ['invalid payment', p => p.paymentMethod = 'x'], ['null order type', p => p.orderType = null],
        ['long note', p => p.note = 'a'.repeat(201)]
    ]) {
        const payload = basket(); mutate(payload);
        check((await send('Checkout', payload)).status() === 400, 'Reject ' + label);
    }
    check((await send('Checkout', null)).status() === 400, 'Reject null JSON body');
    const malformed = await page.request.post(base + '/Pos/Checkout', { headers: { ...headers, 'Content-Type': 'application/json' }, data: '{bad' });
    check(malformed.status() === 400, 'Reject malformed JSON');
    for (const payload of [
        { name: ' ', categoryId: 'tea', price: 5 }, { name: 'Bad', categoryId: 'missing', price: 5 },
        { name: 'Bad', categoryId: 'tea', price: -1 }, { name: 'Bad', categoryId: 'tea', price: 1.5 },
        { name: 'Bad', categoryId: 'tea', price: 5, imageUrl: 'javascript:alert(1)' }
    ]) check((await send('CreateProduct', payload)).status() === 400, 'Reject invalid product ' + JSON.stringify(payload));
    const inactive = await send('CreateProduct', { name: 'Kiểm thử ngừng bán ' + Date.now(), categoryId: 'tea', price: 20000, isAvailable: false });
    const inactiveProduct = await inactive.json(); const bad = basket(); bad.items[0].productId = inactiveProduct.id;
    check((await send('Checkout', bad)).status() === 400, 'Cannot checkout unavailable product');
    return { passed: results.length, results };
}
