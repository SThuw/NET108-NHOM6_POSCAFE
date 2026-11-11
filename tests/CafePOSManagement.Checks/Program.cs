using System.ComponentModel.DataAnnotations;
using CafePOSManagement.Web.Models.ViewModels;
using CafePOSManagement.Web.Services.Implementations;

var products = new ProductService();
var orders = new OrderService(products, TimeProvider.System);
var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name); passed++;
}
void Reject(Action action, string name)
{
    try { action(); }
    catch (ValidationException) { Check(true, name); return; }
    throw new Exception("FAIL: " + name + " did not reject");
}
CheckoutOrderDto Basket() => new()
{
    RequestId = Guid.NewGuid(), DiscountCode = "SALE10", TaxPercent = 8,
    Items = [new() { ProductId = "p1", Quantity = 2 }, new() { ProductId = "p2", Quantity = 1 }]
};
Check(products.GetProducts(search: "  CA PHE SUA DA ").Single().Id == "p1", "Accent-insensitive search including d-stroke");
Check(products.GetProducts("espresso").Count == 3, "Category filter");
Check(products.GetProducts("tea", "ca phe").Count == 0, "Combined category and search");
Check(products.GetProducts(search: "   ").Count == 12, "Whitespace search");
var preview = orders.Preview(Basket());
Check(preview.Subtotal == 145000 && preview.DiscountAmount == 14500 && preview.TaxAmount == 10440 && preview.Total == 140940, "Server money: discount before tax");
Check(orders.GetNextOrderNumber() == "#004", "Preview does not consume order number");
var input = Basket();
var submissions = Enumerable.Range(0, 20).Select(_ => Task.Run(() => orders.Checkout(input))).ToArray();
Task.WaitAll(submissions);
Check(submissions.Select(t => t.Result.OrderNumber).Distinct().Single() == "#004" && orders.GetNextOrderNumber() == "#005", "20 concurrent retries create one order");
input.Note = "Changed";
Reject(() => orders.Checkout(input), "Idempotency key cannot be reused with different payload");
var separate = Enumerable.Range(0, 20).Select(_ => Task.Run(() => orders.Checkout(Basket()))).ToArray();
Task.WaitAll(separate);
Check(separate.Select(t => t.Result.OrderNumber).Distinct().Count() == 20, "Concurrent independent checkouts have unique numbers");
var invalid = Basket(); invalid.Items.Clear(); Reject(() => orders.Checkout(invalid), "Empty cart");
invalid = Basket(); invalid.Items[0].Quantity = 0; Reject(() => orders.Checkout(invalid), "Zero quantity");
invalid = Basket(); invalid.Items[0].Quantity = -1; Reject(() => orders.Checkout(invalid), "Negative quantity");
invalid = Basket(); invalid.Items[0].Quantity = 100; Reject(() => orders.Checkout(invalid), "Quantity upper limit");
invalid = Basket(); invalid.Items[0].ProductId = "missing"; Reject(() => orders.Checkout(invalid), "Missing product");
invalid = Basket(); invalid.Items.Add(invalid.Items[0]); Reject(() => orders.Checkout(invalid), "Duplicate product lines");
invalid = Basket(); invalid.Items = null!; Reject(() => orders.Checkout(invalid), "Null cart");
invalid = Basket(); invalid.Items.Add(null!); Reject(() => orders.Checkout(invalid), "Null line");
invalid = Basket(); invalid.DiscountCode = "FREE"; Reject(() => orders.Checkout(invalid), "Unknown discount");
invalid = Basket(); invalid.TaxPercent = 99; Reject(() => orders.Checkout(invalid), "Unconfigured tax");
invalid = Basket(); invalid.OrderType = "unknown"; Reject(() => orders.Checkout(invalid), "Invalid order type");
invalid = Basket(); invalid.PaymentMethod = "bitcoin"; Reject(() => orders.Checkout(invalid), "Invalid payment method");
invalid = Basket(); invalid.Note = new string('x', 201); Reject(() => orders.Checkout(invalid), "Note length");
invalid = Basket(); invalid.RequestId = Guid.Empty; Reject(() => orders.Checkout(invalid), "Missing request ID");
CreateProductDto NewProduct() => new() { Name = "  Món kiểm thử  ", CategoryId = "tea", Price = 5 };
var newProduct = products.Create(NewProduct());
Check(newProduct.Name == "Món kiểm thử" && newProduct.ImageUrl == "/images/product-fallback.svg", "New product trimmed with fallback");
Reject(() => products.Create(NewProduct()), "Duplicate product blocked");
var badProduct = NewProduct(); badProduct.Name = " "; Reject(() => products.Create(badProduct), "Blank name");
badProduct = NewProduct(); badProduct.Price = 1.5m; Reject(() => products.Create(badProduct), "Fractional VND rejected");
badProduct = NewProduct(); badProduct.Price = -5; Reject(() => products.Create(badProduct), "Negative price rejected");
badProduct = NewProduct(); badProduct.OriginalPrice = 4; Reject(() => products.Create(badProduct), "Original price below selling price");
badProduct = NewProduct(); badProduct.CategoryId = "missing"; Reject(() => products.Create(badProduct), "Invalid category");
badProduct = NewProduct(); badProduct.ImageUrl = "javascript:alert(1)"; Reject(() => products.Create(badProduct), "Unsafe image scheme");
var rounded = orders.Preview(new() { RequestId = Guid.NewGuid(), Items = [new() { ProductId = newProduct.Id, Quantity = 1 }], DiscountCode = "SALE10", TaxPercent = 10 });
Check(rounded.DiscountAmount == 1 && rounded.TaxAmount == 0 && rounded.Total == 4, "Half-dong rounding away from zero");
var unavailable = products.Create(new() { Name = "Ngừng bán", CategoryId = "tea", Price = 10000, IsAvailable = false });
invalid = Basket(); invalid.Items[0].ProductId = unavailable.Id; Reject(() => orders.Checkout(invalid), "Unavailable product rejected");
Console.WriteLine($"ALL {passed} CHECKS PASSED");
if (args.Contains("--lab3") || args.Contains("--database")) Lab3DalChecks.Run();
if (args.Contains("--database")) await Stage4Checks.RunAsync();
